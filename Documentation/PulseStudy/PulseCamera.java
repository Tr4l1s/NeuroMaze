package com.neuromaze.pulse;

import android.Manifest;
import android.app.Activity;
import android.app.Application;
import android.content.Context;
import android.content.pm.PackageManager;
import android.graphics.ImageFormat;
import android.hardware.camera2.*;
import android.hardware.camera2.params.StreamConfigurationMap;
import android.media.Image;
import android.media.ImageReader;
import android.os.Bundle;
import android.os.Handler;
import android.os.HandlerThread;
import android.util.Range;
import android.util.Size;
import org.json.JSONArray;
import org.json.JSONObject;
import java.nio.ByteBuffer;
import java.util.Collections;

/** Camera2 acquisition only: no invented BPM, no image storage or network traffic. */
public final class PulseCamera implements Application.ActivityLifecycleCallbacks {
    private final Object lock = new Object();
    private JSONArray pending = new JSONArray();
    private volatile String state = "idle", error = "", info = "";
    private volatile boolean stopped = true, hasTorch;
    private HandlerThread thread;
    private Handler handler;
    private CameraDevice camera;
    private CameraCaptureSession session;
    private ImageReader reader;
    private Activity activity;
    private int dropped;
    private long firstTimestamp;

    public void start(Activity owner) {
        if (!stopped) return;
        activity = owner;
        stopped = false; state = "opening"; error = ""; info = ""; hasTorch = false;
        synchronized (lock) { pending = new JSONArray(); dropped = 0; }
        firstTimestamp = 0;
        owner.getApplication().registerActivityLifecycleCallbacks(this);
        thread = new HandlerThread("NeuroMazePulseCamera"); thread.start();
        handler = new Handler(thread.getLooper());
        handler.post(() -> open(owner));
        // Native timeout also closes the camera if Unity stops polling.
        handler.postDelayed(() -> { if (!stopped) fail("Ölçüm zaman aşımına uğradı"); }, 65000);
    }

    private void open(Activity owner) {
        try {
            if (stopped) return;
            if (owner.checkSelfPermission(Manifest.permission.CAMERA) != PackageManager.PERMISSION_GRANTED)
                throw new IllegalStateException("Kamera izni verilmedi");
            CameraManager manager = (CameraManager) owner.getSystemService(Context.CAMERA_SERVICE);
            String selected = null;
            CameraCharacteristics characteristics = null;
            for (String id : manager.getCameraIdList()) {
                CameraCharacteristics c = manager.getCameraCharacteristics(id);
                Integer facing = c.get(CameraCharacteristics.LENS_FACING);
                if (facing != null && facing == CameraCharacteristics.LENS_FACING_BACK) {
                    if (selected == null || Boolean.TRUE.equals(c.get(CameraCharacteristics.FLASH_INFO_AVAILABLE))) {
                        selected = id; characteristics = c;
                        if (Boolean.TRUE.equals(c.get(CameraCharacteristics.FLASH_INFO_AVAILABLE))) break;
                    }
                }
            }
            if (selected == null) throw new IllegalStateException("Arka kamera bulunamadı");
            hasTorch = Boolean.TRUE.equals(characteristics.get(CameraCharacteristics.FLASH_INFO_AVAILABLE));
            StreamConfigurationMap map = characteristics.get(CameraCharacteristics.SCALER_STREAM_CONFIGURATION_MAP);
            Size[] sizes = map == null ? null : map.getOutputSizes(ImageFormat.YUV_420_888);
            if (sizes == null || sizes.length == 0) throw new IllegalStateException("Kamera görüntü biçimi desteklenmiyor");
            Size chosen = sizes[0];
            for (Size size : sizes)
                if (Math.abs(size.getWidth() * size.getHeight() - 320 * 240) < Math.abs(chosen.getWidth() * chosen.getHeight() - 320 * 240)) chosen = size;
            reader = ImageReader.newInstance(chosen.getWidth(), chosen.getHeight(), ImageFormat.YUV_420_888, 3);
            reader.setOnImageAvailableListener(this::frame, handler);
            Range<Integer>[] ranges = characteristics.get(CameraCharacteristics.CONTROL_AE_AVAILABLE_TARGET_FPS_RANGES);
            Range<Integer> fps = null;
            if (ranges != null) for (Range<Integer> range : ranges)
                if (range.getUpper() >= 30 && range.getLower() <= 30 &&
                    (fps == null || range.getUpper() - range.getLower() < fps.getUpper() - fps.getLower())) fps = range;
            final Range<Integer> targetFps = fps;
            info = "camera=" + selected + ";size=" + chosen + ";torch=" + hasTorch + ";fps=" + fps;
            manager.openCamera(selected, new CameraDevice.StateCallback() {
                @Override public void onOpened(CameraDevice device) {
                    if (stopped) { device.close(); return; }
                    camera = device;
                    try {
                        CaptureRequest.Builder request = device.createCaptureRequest(CameraDevice.TEMPLATE_PREVIEW);
                        request.addTarget(reader.getSurface());
                        request.set(CaptureRequest.CONTROL_MODE, CaptureRequest.CONTROL_MODE_AUTO);
                        request.set(CaptureRequest.CONTROL_AE_MODE, CaptureRequest.CONTROL_AE_MODE_ON);
                        request.set(CaptureRequest.CONTROL_AF_MODE, CaptureRequest.CONTROL_AF_MODE_OFF);
                        request.set(CaptureRequest.FLASH_MODE, hasTorch ? CaptureRequest.FLASH_MODE_TORCH : CaptureRequest.FLASH_MODE_OFF);
                        if (targetFps != null) request.set(CaptureRequest.CONTROL_AE_TARGET_FPS_RANGE, targetFps);
                        device.createCaptureSession(Collections.singletonList(reader.getSurface()), new CameraCaptureSession.StateCallback() {
                            @Override public void onConfigured(CameraCaptureSession configured) {
                                if (stopped) { configured.close(); return; }
                                session = configured;
                                try { session.setRepeatingRequest(request.build(), null, handler); state = "running"; }
                                catch (Exception e) { fail(e.toString()); }
                            }
                            @Override public void onConfigureFailed(CameraCaptureSession failed) { fail("Kamera oturumu açılamadı"); }
                        }, handler);
                    } catch (Exception e) { fail(e.toString()); }
                }
                @Override public void onDisconnected(CameraDevice device) { device.close(); fail("Kamera bağlantısı kesildi"); }
                @Override public void onError(CameraDevice device, int code) { device.close(); fail("Kamera hatası: " + code); }
            }, handler);
        } catch (Exception e) { fail(e.toString()); }
    }

    private void frame(ImageReader source) {
        Image image = null;
        try {
            image = source.acquireLatestImage();
            if (image == null || stopped) return;
            if (firstTimestamp == 0) firstTimestamp = image.getTimestamp();
            double t = (image.getTimestamp() - firstTimestamp) / 1e9;
            Image.Plane[] planes = image.getPlanes();
            ByteBuffer yPlane = planes[0].getBuffer(), uPlane = planes[1].getBuffer(), vPlane = planes[2].getBuffer();
            int count = 0, clipR = 0, clipG = 0, clipY = 0;
            double rTotal = 0, gTotal = 0, bTotal = 0, yTotal = 0;
            // Center half of the image. Explicit row/pixel strides support vendor layouts.
            for (int y = image.getHeight() / 4; y < image.getHeight() * 3 / 4; y += 4) {
                for (int x = image.getWidth() / 4; x < image.getWidth() * 3 / 4; x += 4) {
                    int yy = (yPlane.get(y * planes[0].getRowStride() + x * planes[0].getPixelStride()) & 255) - 16;
                    int u = (uPlane.get((y / 2) * planes[1].getRowStride() + (x / 2) * planes[1].getPixelStride()) & 255) - 128;
                    int v = (vPlane.get((y / 2) * planes[2].getRowStride() + (x / 2) * planes[2].getPixelStride()) & 255) - 128;
                    double r = clamp(1.164 * yy + 1.596 * v);
                    double g = clamp(1.164 * yy - 0.392 * u - 0.813 * v);
                    double b = clamp(1.164 * yy + 2.017 * u);
                    rTotal += r; gTotal += g; bTotal += b;
                    double luma = clamp(1.164 * yy);
                    yTotal += luma;
                    if (luma <= 3 || luma >= 250) clipY++;
                    if (r >= 250) clipR++; if (g >= 250) clipG++;
                    count++;
                }
            }
            if (count == 0) return;
            JSONObject sample = new JSONObject();
            sample.put("t", t); sample.put("r", rTotal / count); sample.put("g", gTotal / count); sample.put("b", bTotal / count);
            sample.put("clipR", (double)clipR / count); sample.put("clipG", (double)clipG / count);
            sample.put("y", yTotal / count); sample.put("clipY", (double)clipY / count);
            synchronized (lock) {
                if (pending.length() < 600) pending.put(sample);
                else dropped++;
            }
        } catch (Exception e) { if (!stopped) fail("Görüntü okunamadı: " + e); }
        finally { if (image != null) image.close(); }
    }

    private static double clamp(double value) { return Math.max(0, Math.min(255, value)); }

    public String poll() {
        synchronized (lock) {
            try {
                JSONObject payload = new JSONObject();
                payload.put("state", state); payload.put("error", error); payload.put("info", info);
                payload.put("torch", hasTorch); payload.put("dropped", dropped); payload.put("samples", pending);
                String result = payload.toString(); pending = new JSONArray(); return result;
            } catch (Exception e) { return "{\"state\":\"error\",\"error\":\"Kamera verisi okunamadı\"}"; }
        }
    }

    public static String exportFile(Activity owner, String path) throws Exception {
        java.io.File file = new java.io.File(path);
        if (android.os.Build.VERSION.SDK_INT < 29)
            return "ZIP uygulama klasörüne kaydedildi: " + path + ". USB ile alınabilir.";
        android.content.ContentResolver resolver = owner.getContentResolver();
        android.content.ContentValues values = new android.content.ContentValues();
        values.put(android.provider.MediaStore.Downloads.DISPLAY_NAME, file.getName());
        values.put(android.provider.MediaStore.Downloads.MIME_TYPE, "application/zip");
        values.put(android.provider.MediaStore.Downloads.RELATIVE_PATH, android.os.Environment.DIRECTORY_DOWNLOADS + "/NeuroMaze");
        values.put(android.provider.MediaStore.Downloads.IS_PENDING, 1);
        android.net.Uri uri = resolver.insert(android.provider.MediaStore.Downloads.EXTERNAL_CONTENT_URI, values);
        if (uri == null) throw new java.io.IOException("İndirilenler klasöründe dosya oluşturulamadı");
        try {
            try (java.io.InputStream input = new java.io.FileInputStream(file);
                 java.io.OutputStream output = resolver.openOutputStream(uri)) {
                if (output == null) throw new java.io.IOException("Dışa aktarma dosyası açılamadı");
                byte[] buffer = new byte[8192]; int count;
                while ((count = input.read(buffer)) > 0) output.write(buffer, 0, count);
            }
            values.clear(); values.put(android.provider.MediaStore.Downloads.IS_PENDING, 0);
            resolver.update(uri, values, null, null);
            return "ZIP kaydedildi: Dosyalarım > İndirilenler > NeuroMaze > " + file.getName();
        } catch (Exception e) { resolver.delete(uri, null, null); throw e; }
    }

    private void fail(String message) { error = message; state = "error"; stop(); }

    public void stop() {
        if (stopped) return;
        stopped = true;
        if (!state.equals("error")) state = "stopped";
        if (activity != null) activity.getApplication().unregisterActivityLifecycleCallbacks(this);
        Handler worker = handler;
        if (worker != null) worker.post(() -> {
            try { if (session != null) { session.stopRepeating(); session.close(); } } catch (Exception ignored) { }
            session = null;
            try { if (camera != null) camera.close(); } catch (Exception ignored) { }
            camera = null;
            try { if (reader != null) reader.close(); } catch (Exception ignored) { }
            reader = null;
            if (thread != null) thread.quitSafely();
        });
    }

    @Override public void onActivityPaused(Activity a) { if (a == activity) stop(); }
    @Override public void onActivityDestroyed(Activity a) { if (a == activity) stop(); }
    @Override public void onActivityCreated(Activity a, Bundle b) { }
    @Override public void onActivityStarted(Activity a) { }
    @Override public void onActivityResumed(Activity a) { }
    @Override public void onActivityStopped(Activity a) { }
    @Override public void onActivitySaveInstanceState(Activity a, Bundle b) { }
}
