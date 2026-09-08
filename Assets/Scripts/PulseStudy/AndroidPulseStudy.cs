using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace NeuroMaze.Pulse
{
    public sealed class AndroidPulseStudy : MonoBehaviour
    {
        [Serializable] public class CameraPayload
        {
            public string state, error, info;
            public bool torch;
            public int dropped;
            public CameraSample[] samples;
        }

        public static AndroidPulseStudy Instance { get; private set; }
        public bool IsOpen { get { return panel != null && panel.activeSelf; } }
        public bool IsMeasuring { get { return measuring; } }
        public string SessionId { get; private set; } = "";
        public string ParticipantCode { get; private set; } = "";
        public string LastResult { get; private set; } = "Henüz ölçüm yok";
        public bool UsesExternalLight { get { return externalLight != null && externalLight.isOn; } }
        public void EnsureGameSession()
        {
            if (!string.IsNullOrEmpty(SessionId)) return;
            codeInput.text = "P" + DateTime.Now.ToString("yyyyMMddHHmmss");
            NewSession();
        }
        public void RefreshGameHistory() { RefreshHistory(); }

        GameObject panel;
        RectTransform safeRoot;
        Text status, sessionLabel, summaryLabel, historyLabel;
        InputField codeInput;
        Toggle externalLight;
        Button startButton, interimButton, endButton, newSessionButton, exportButton, closeButton, cancelButton;
        Font font;
        readonly List<Behaviour> pausedControls = new List<Behaviour>();
        readonly List<CameraSample> allSamples = new List<CameraSample>();
        readonly List<CameraSample> continuous = new List<CameraSample>();
        PulseRecord attempt;
        AndroidJavaObject nativeCamera;
        bool measuring, cameraStarted, waitingPermission;
        double contactSince = -1, lastSample = -1;
        float attemptStarted, lastFrameReceived, savedTimeScale, nextPoll;
        int previousSleepTimeout;
        string phase = "baseline";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
#if UNITY_ANDROID || UNITY_EDITOR
            if (Instance == null) new GameObject("AndroidPulseStudy").AddComponent<AndroidPulseStudy>();
#endif
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildUI();
            gameObject.AddComponent<TabletGameLayout>();
            SceneManager.sceneLoaded += SceneLoaded;
            EnsureEventSystem();
        }

        void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureEventSystem();
            if (IsOpen) PauseControls();
        }

        void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            // Scene-local fallback; subsequent scenes can supply their own EventSystem.
            new GameObject("PulseEventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        public void OpenPanel(string requestedPhase = null)
        {
            var gameCapture = GetComponent<GamePulseMeasurement>();
            if (gameCapture != null && gameCapture.Active) return;
            if (requestedPhase != null) phase = requestedPhase;
            if (IsOpen) return;
            savedTimeScale = Time.timeScale;
            panel.SetActive(true);
            PauseControls();
            Time.timeScale = 0;
            RefreshHistory();
            SetStatus("Ölçüm güvenli alanlarda sorularla birlikte otomatik başlar. Bu ekran oyuncu kodu ve oyun kayıtları içindir.");
        }

        void PauseControls()
        {
            foreach (var control in FindObjectsByType<FPSJoystickController>(FindObjectsSortMode.None)) Pause(control);
            foreach (var control in FindObjectsByType<PauseMenuController>(FindObjectsSortMode.None)) Pause(control);
        }

        void Pause(Behaviour control)
        {
            if (control.enabled && !pausedControls.Contains(control)) { pausedControls.Add(control); control.enabled = false; }
        }

        public void ClosePanel()
        {
            if (measuring) Finish(false, "Kullanıcı ölçümü iptal etti", "cancelled");
            if (!IsOpen) return;
            panel.SetActive(false);
            Time.timeScale = savedTimeScale;
            foreach (var control in pausedControls) if (control != null) control.enabled = true;
            pausedControls.Clear();
        }

        void Update()
        {
            if (!measuring) return;
            if (Time.realtimeSinceStartup - attemptStarted > 60)
            { Finish(false, "60 saniye içinde yeterli sinyal alınamadı", "rejected"); return; }
#if UNITY_ANDROID && !UNITY_EDITOR
            if (waitingPermission)
            {
                if (Permission.HasUserAuthorizedPermission(Permission.Camera)) { waitingPermission = false; StartCamera(); }
                return;
            }
#endif
            if (!cameraStarted || Time.realtimeSinceStartup < nextPoll) return;
            nextPoll = Time.realtimeSinceStartup + 0.1f;
            try
            {
                CameraPayload payload = JsonUtility.FromJson<CameraPayload>(nativeCamera.Call<string>("poll"));
                if (payload == null) throw new InvalidOperationException("Kamera verisi okunamadı");
                attempt.cameraInfo = payload.info;
                if (payload.state == "error" || payload.state == "stopped")
                { Finish(false, payload.error ?? "Kamera durdu", "rejected"); return; }
                if (payload.state == "running")
                {
                    attempt.source = payload.torch ? "camera_builtin_torch_experimental" :
                        externalLight.isOn ? "camera_external_light_experimental" : "camera_ambient_light_experimental";
                }
                if (payload.dropped > 0) { Finish(false, "Kamera verisi işlenirken örnek kaybı oluştu", "rejected"); return; }
                if (payload.samples != null)
                    foreach (var sample in payload.samples)
                    {
                        if (!measuring) break;
                        lastFrameReceived = Time.realtimeSinceStartup;
                        ProcessSample(sample);
                    }
                if (measuring && Time.realtimeSinceStartup - lastFrameReceived > 10)
                    Finish(false, "Kameradan görüntü gelmiyor", "rejected");
            }
            catch (Exception e) { Finish(false, "Kamera hatası: " + e.Message, "rejected"); }
        }

        void LateUpdate()
        {
            if (IsOpen) Time.timeScale = 0;
            Rect safe = Screen.safeArea;
            if (Screen.width > 0 && Screen.height > 0)
            {
                safeRoot.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
                safeRoot.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
            }
        }

        void ProcessSample(CameraSample sample)
        {
            allSamples.Add(sample);
            if (lastSample >= 0 && (sample.t <= lastSample || sample.t - lastSample > 0.25))
            { continuous.Clear(); contactSince = -1; }
            lastSample = sample.t;
            if (!PulseSignal.HasContact(sample))
            {
                continuous.Clear(); contactSince = -1;
                SetStatus("Parmak sinyali bulunamadı. Arka kamerayı parmağınızla kapatın; aydınlık ortamda sabit tutun. Fazla bastırmayın.");
                return;
            }
            if (contactSince < 0) contactSince = sample.t;
            double settled = sample.t - contactSince;
            if (settled < 3)
            { SetStatus("Parmak algılandı. Sabit tutun… " + (3 - settled).ToString("F0") + " sn"); return; }
            continuous.Add(sample);
            double seconds = sample.t - continuous[0].t;
            SetStatus(PhaseName(phase) + " ölçülüyor • " + Math.Min(25, seconds).ToString("F0") + "/25 sn\nParmağı ve ışığı sabit tutun. Rahatsızlık veya ısınma olursa iptal edin.");
            if (seconds < PulseSignal.MinimumSeconds) return;
            var estimate = PulseSignal.Analyze(continuous);
            attempt.bpm = estimate.Valid ? estimate.Bpm : 0;
            attempt.signalCorrelation = estimate.Correlation;
            attempt.durationSeconds = estimate.Seconds;
            attempt.samplingFps = estimate.Fps;
            attempt.cameraInfo += ";signal_channel=" + estimate.Channel;
            Finish(estimate.Valid, estimate.Reason, estimate.Valid ? "accepted_unvalidated" : "rejected");
        }

        void NewSession()
        {
            string code = codeInput.text.Trim().ToUpperInvariant();
            if (!PulseRecords.ValidCode(code))
            { SetStatus("İsim yerine K001 gibi bir kod yazın. En fazla 24 harf/rakam; tire ve alt çizgi kullanılabilir."); return; }
            ParticipantCode = code;
            SessionId = Guid.NewGuid().ToString("N");
            codeInput.text = code;
            LastResult = "Henüz ölçüm yok";
            sessionLabel.text = "Katılımcı: " + code + "   •   Oturum: " + SessionId.Substring(0, 8);
            RefreshHistory();
            SetStatus("Oyuncu hazır. Oyuna dönün; güvenli alanlarda sorularla birlikte ölçüm otomatik başlar. Aynı oyuncuda oturumu değiştirmeyin.");
        }

        void Begin(string requestedPhase)
        {
            if (measuring) return;
            if (string.IsNullOrEmpty(SessionId) || codeInput.text.Trim().ToUpperInvariant() != ParticipantCode)
            { SetStatus("Önce katılımcı kodunu yazıp Yeni oturum düğmesine basın."); return; }
            phase = requestedPhase;
            allSamples.Clear(); continuous.Clear(); contactSince = -1; lastSample = -1;
            attempt = new PulseRecord {
                measurementId = Guid.NewGuid().ToString("N"), participantCode = ParticipantCode, sessionId = SessionId,
                phase = phase, startedUtc = DateTime.UtcNow.ToString("O"), scene = SceneManager.GetActiveScene().name,
                source = "camera_unknown", algorithmVersion = PulseSignal.Version,
                deviceModel = SystemInfo.deviceModel, operatingSystem = SystemInfo.operatingSystem
            };
            attemptStarted = lastFrameReceived = Time.realtimeSinceStartup;
            previousSleepTimeout = Screen.sleepTimeout;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            measuring = true; SetBusy(true);
            SetStatus("Kamera hazırlanıyor…");
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                waitingPermission = true;
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionDenied += _ => { if (measuring) Finish(false, "Kamera izni reddedildi. Uygulama ayarlarından izin verin.", "rejected"); };
                Permission.RequestUserPermission(Permission.Camera, callbacks);
                return;
            }
            StartCamera();
#else
            Finish(false, "Gerçek ölçüm için Android cihaz gerekli. Editörde örnek veya yapay BPM üretilmez.", "rejected");
#endif
        }

        void StartCamera()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                nativeCamera = new AndroidJavaObject("com.neuromaze.pulse.PulseCamera");
                using (var unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unity.GetStatic<AndroidJavaObject>("currentActivity")) nativeCamera.Call("start", activity);
                cameraStarted = true; nextPoll = 0;
            }
            catch (Exception e) { Finish(false, "Kamera başlatılamadı: " + e.Message, "rejected"); }
#endif
        }

        void Finish(bool accepted, string reason, string outcome)
        {
            if (!measuring) return;
            measuring = false; waitingPermission = false; cameraStarted = false;
            try { if (nativeCamera != null) nativeCamera.Call("stop"); }
            catch (Exception e) { Debug.LogWarning("Pulse camera stop: " + e.Message); }
            finally { if (nativeCamera != null) nativeCamera.Dispose(); nativeCamera = null; }
            Screen.sleepTimeout = previousSleepTimeout;
            attempt.completedUtc = DateTime.UtcNow.ToString("O");
            attempt.status = outcome; attempt.reason = reason; attempt.sampleCount = allSamples.Count;
            if (attempt.samplingFps <= 0 && allSamples.Count > 1)
            {
                double span = allSamples[allSamples.Count - 1].t - allSamples[0].t;
                if (span > 0) attempt.samplingFps = (allSamples.Count - 1) / span;
            }
            if (!accepted) attempt.bpm = 0;
            bool saved = false;
            try { PulseRecords.Save(attempt, allSamples); saved = true; }
            catch (Exception e) { reason += "\nKAYIT HATASI: " + e.Message + ". Bu deneme dosyaya kaydedilemedi."; }
            LastResult = accepted ? PhaseName(phase) + ": " + attempt.bpm.ToString("F1") + " BPM (deneysel)" : "Ölçüm alınamadı";
            SetBusy(false);
            RefreshHistory();
            SetStatus(LastResult + "\n" + reason + (saved ? "\nDeneme kaydedildi." : ""));
            // This study mode deliberately records only; it does not change enemy speed
            // or infer stress from an unvalidated heart-rate estimate.
        }

        void Export()
        {
            try
            {
                string path = PulseRecords.ExportZip();
#if UNITY_ANDROID && !UNITY_EDITOR
                using (var cls = new AndroidJavaClass("com.neuromaze.pulse.PulseCamera"))
                using (var unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unity.GetStatic<AndroidJavaObject>("currentActivity"))
                    SetStatus(cls.CallStatic<string>("exportFile", activity, path));
#else
                SetStatus("ZIP kaydedildi: " + path);
#endif
            }
            catch (Exception e) { SetStatus("Dışa aktarma tamamlanamadı: " + e.Message); }
        }

        void RefreshHistory()
        {
            try
            {
                var all = PulseRecords.Load();
                var current = all.Where(r => r.sessionId == SessionId).ToList();
                var visits = current.Where(r => r.phase == "safe_zone").ToList();
                if (visits.Count > 0)
                {
                    var last = visits[visits.Count - 1];
                    summaryLabel.text = "Son alan • Başlangıç: " + (last.startValid ? last.startBpm.ToString("F1") : "—") +
                        " • Ort.: " + (last.validWindowCount > 0 ? last.bpm.ToString("F1") : "—") +
                        " • Bitiş: " + (last.endValid ? last.endBpm.ToString("F1") : "—") + " BPM\n" +
                        last.validWindowCount + "/3 pencere • Deneysel • " + last.correctAnswers + " doğru cevap";
                    historyLabel.text = "Oyuncunun son güvenli alanları:\n" + string.Join("\n", visits.AsEnumerable().Reverse().Take(3).Select(r =>
                        r.scene + " • " + (r.validWindowCount > 0 ? r.bpm.ToString("F1") + " BPM" : "sinyal yok") + " • " + r.validWindowCount + "/3 pencere")) +
                        "\nBu oturumda " + visits.Count + " alan kaydı.";
                    return;
                }
                var lines = new List<string>();
                foreach (var p in new[] { "baseline", "interim", "end" })
                {
                    var values = current.Where(r => r.phase == p && r.status == "accepted_unvalidated").ToList();
                    // Do not combine built-in-torch and external-light recordings.
                    if (values.Count == 0) lines.Add(PhaseName(p) + ": —");
                    else foreach (var source in values.GroupBy(r => r.source))
                        lines.Add(PhaseName(p) + ": " + source.Average(r => r.bpm).ToString("F1") + " BPM (n=" + source.Count() + ", " + (source.Key.Contains("external") ? "harici ışık" : source.Key.Contains("ambient") ? "ortam ışığı" : "flaş") + ")");
                }
                summaryLabel.text = string.Join("   |   ", lines) + "\nBunlar noktasal ölçüm ortalamalarıdır; oyun boyunca sürekli ortalama değildir.";
                var latest = current.AsEnumerable().Reverse().Take(4).Select(r => PhaseName(r.phase) + " • " +
                    (r.status == "accepted_unvalidated" ? r.bpm.ToString("F1") + " BPM • deneysel" : "sonuç yok / " + r.status));
                historyLabel.text = "Bu oturumun son denemeleri:\n" + string.Join("\n", latest) + "\nCihazda toplam " + all.Count + " deneme kayıtlı.";
            }
            catch (Exception e) { historyLabel.text = "Kayıt okuma hatası: " + e.Message; }
        }

        public int CurrentParticipantAverage()
        {
            if (string.IsNullOrEmpty(SessionId)) return 0;
            try
            {
                var values = PulseRecords.Load().Where(r => r.sessionId == SessionId && r.status == "accepted_unvalidated").ToList();
                if (values.Count == 0 || values.Select(r => r.source).Distinct().Count() != 1) return 0;
                return (int)Math.Round(values.Average(r => r.bpm));
            }
            catch { return 0; }
        }

        void SetBusy(bool busy)
        {
            startButton.interactable = interimButton.interactable = endButton.interactable = !busy;
            newSessionButton.interactable = exportButton.interactable = !busy;
            codeInput.interactable = externalLight.interactable = !busy;
            cancelButton.gameObject.SetActive(busy);
            closeButton.GetComponentInChildren<Text>().text = busy ? "İptal et ve oyuna dön" : "Oyuna dön";
        }

        static string PhaseName(string p) { return p == "baseline" ? "Başlangıç" : p == "end" ? "Bitiş" : "Ara"; }
        void SetStatus(string message) { status.text = message; }
        void OnApplicationPause(bool paused) { if (paused && measuring && !waitingPermission) Finish(false, "Uygulama arka plana alındı", "cancelled"); }
        void OnApplicationQuit() { if (measuring) Finish(false, "Uygulama kapatıldı", "cancelled"); }
        void OnDestroy()
        {
            if (Instance != this) return;
            if (measuring) Finish(false, "Ölçüm ekranı kapatıldı", "cancelled");
            SceneManager.sceneLoaded -= SceneLoaded;
            Instance = null;
        }

        void BuildUI()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvasObject = new GameObject("PulseStudyCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.GetComponent<Canvas>().sortingOrder = 32000;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 800); scaler.matchWidthOrHeight = 0;
            safeRoot = Rect("SafeArea", canvasObject.transform, 0, 0, 0, 0);
            Stretch(safeRoot);
            var launcher = ButtonAt(safeRoot, "NABIZ / KAYIT", 0, 12, 200, 48, () => OpenPanel());
            var launchRect = launcher.GetComponent<RectTransform>(); launchRect.anchorMin = launchRect.anchorMax = new Vector2(0.5f, 1); launchRect.pivot = new Vector2(0.5f, 1);
            panel = new GameObject("PulsePanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(safeRoot, false); Stretch(panel.GetComponent<RectTransform>());
            panel.GetComponent<Image>().color = new Color(0.025f, 0.06f, 0.1f, 0.99f);
            var content = Rect("Content", panel.transform, 0, 0, 1120, 700);
            content.anchorMin = content.anchorMax = content.pivot = new Vector2(0.5f, 0.5f);
            TextAt(content, "Oyuncu • Oyun içi nabız kayıtları", 0, 0, 1080, 48, 32, FontStyle.Bold);
            TextAt(content, "Deneysel kamera ölçümü — referans cihazla doğrulanmadı.", 0, 50, 1080, 32, 22);
            TextAt(content, "Katılımcı kodu (K001 gibi; isim yazmayın)", 0, 98, 640, 30, 21);
            codeInput = InputAt(content, 0, 135, 280, 48);
            newSessionButton = ButtonAt(content, "Yeni oturum", 300, 135, 220, 48, NewSession);
            sessionLabel = TextAt(content, "Önce katılımcı koduyla bir oturum açın.", 0, 195, 1080, 30, 21);

            var toggleRoot = Rect("ExternalLight", content, 0, 235, 1080, 42);
            externalLight = toggleRoot.gameObject.AddComponent<Toggle>();
            var background = Rect("Box", toggleRoot, 0, 2, 32, 32).gameObject.AddComponent<Image>(); background.color = Color.white;
            var mark = Rect("Check", background.transform, 6, 6, 20, 20).gameObject.AddComponent<Image>(); mark.color = new Color(0.02f, 0.62f, 0.6f);
            externalLight.targetGraphic = background; externalLight.graphic = mark; externalLight.isOn = false;
            TextAt(toggleRoot, "Ek telefon ışığı kullanıyorum (kapalıysa yalnızca ortam ışığı)", 45, 0, 1035, 38, 21);
            startButton = ButtonAt(content, "1  Başlangıç ölç", 0, 285, 270, 54, () => Begin("baseline"));
            interimButton = ButtonAt(content, "2  Ara ölçüm", 290, 285, 270, 54, () => Begin("interim"));
            endButton = ButtonAt(content, "3  Bitiş ölç", 580, 285, 270, 54, () => Begin("end"));
            cancelButton = ButtonAt(content, "İptal", 870, 285, 210, 54, () => Finish(false, "Kullanıcı iptal etti", "cancelled"));
            cancelButton.gameObject.SetActive(false);
            startButton.gameObject.SetActive(false);
            interimButton.gameObject.SetActive(false);
            endButton.gameObject.SetActive(false);
            TextAt(content, "Güvenli alana gir → 30 sn soru + nabız → oyuna devam", 0, 285, 1080, 54, 24, FontStyle.Bold);
            status = TextAt(content, "Oyundan önce oyuncu kodunu girin. Her güvenli alanda başlangıç, ortalama ve bitiş otomatik kaydedilir. Eksik sinyal boş bırakılır.", 0, 355, 1080, 112, 23);
            summaryLabel = TextAt(content, "", 0, 475, 1080, 70, 19);
            historyLabel = TextAt(content, "", 0, 550, 780, 130, 18);
            exportButton = ButtonAt(content, "Kayıtları ZIP aktar", 800, 555, 280, 52, Export);
            closeButton = ButtonAt(content, "Oyuna dön", 800, 625, 280, 52, ClosePanel);
            panel.SetActive(false);
        }

        RectTransform Rect(string name, Transform parent, float x, float y, float width, float height)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(width, height); return rect;
        }
        static void Stretch(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; }
        Text TextAt(Transform parent, string value, float x, float y, float width, float height, int size, FontStyle style = FontStyle.Normal)
        {
            var text = Rect("Text", parent, x, y, width, height).gameObject.AddComponent<Text>();
            text.font = font; text.text = value; text.fontSize = size; text.fontStyle = style;
            text.color = new Color(0.92f, 0.97f, 1); text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }
        Button ButtonAt(Transform parent, string label, float x, float y, float width, float height, UnityEngine.Events.UnityAction action)
        {
            var rect = Rect(label, parent, x, y, width, height);
            var image = rect.gameObject.AddComponent<Image>(); image.color = new Color(0.04f, 0.5f, 0.52f);
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image; button.onClick.AddListener(action);
            var text = TextAt(rect, label, 5, 0, width - 10, height, 22, FontStyle.Bold); text.alignment = TextAnchor.MiddleCenter;
            return button;
        }
        InputField InputAt(Transform parent, float x, float y, float width, float height)
        {
            var rect = Rect("ParticipantCode", parent, x, y, width, height);
            var image = rect.gameObject.AddComponent<Image>(); image.color = Color.white;
            var input = rect.gameObject.AddComponent<InputField>(); input.targetGraphic = image;
            input.textComponent = TextAt(rect, "", 12, 6, width - 24, height - 12, 24);
            input.textComponent.color = new Color(0.03f, 0.08f, 0.12f);
            input.characterLimit = 24; input.lineType = InputField.LineType.SingleLine;
            var placeholder = TextAt(rect, "K001", 12, 6, width - 24, height - 12, 24); placeholder.color = Color.gray;
            input.placeholder = placeholder; return input;
        }
    }
}
