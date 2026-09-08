using System;
using System.Collections.Generic;

namespace NeuroMaze.Pulse
{
    [Serializable]
    public class CameraSample
    {
        public double t;
        public float r, g, b, y, clipR, clipG, clipY;
    }

    public sealed class PulseEstimate
    {
        public bool Valid;
        public double Bpm, Correlation, Fps, Seconds;
        public string Channel = "", Reason = "Sinyal bekleniyor";
    }

    // Experimental contact-PPG estimator. Quality checks are NOT clinical validation.
    // Camera timestamps, rather than Unity frame times, drive all calculations.
    public static class PulseSignal
    {
        public const string Version = "android-ppg-acf-3";
        public const double MinimumSeconds = 25;
        const double Rate = 30;

        public static bool HasContact(CameraSample s)
        {
            // Transmitted external light can be almost entirely red. A minimum green
            // value would falsely report no finger even with a usable red/luma signal.
            return s.r > 80 && s.r > s.g + 15 && s.r > s.b + 15;
        }

        public static PulseEstimate Analyze(IList<CameraSample> data, double minimumSeconds = MinimumSeconds)
        {
            var result = new PulseEstimate();
            if (data.Count < 2) return result;
            result.Seconds = data[data.Count - 1].t - data[0].t;
            if (result.Seconds <= 0) return result;
            result.Fps = (data.Count - 1) / result.Seconds;
            if (result.Seconds < minimumSeconds) { result.Reason = minimumSeconds + " saniyelik kesintisiz sinyal gerekli"; return result; }
            if (result.Fps < 15) { result.Reason = "Kamera örnekleme hızı yetersiz"; return result; }
            for (int i = 1; i < data.Count; i++)
                if (data[i].t <= data[i - 1].t || data[i].t - data[i - 1].t > 0.25)
                { result.Reason = "Kamera örneklerinde kesinti var"; return result; }
            foreach (var s in data)
                if (!HasContact(s)) { result.Reason = "Parmak teması kesildi"; return result; }

            var red = AnalyzeChannel(data, 0);
            var green = AnalyzeChannel(data, 1);
            var luma = AnalyzeChannel(data, 2);
            if (!red.Valid && !green.Valid && !luma.Valid)
            {
                result.Channel = "none";
                result.Reason = "Kullanılabilir nabız sinyali yok. Parlaklık: " + luma.Reason;
                return result;
            }
            // YUV luma is taken before display RGB clipping. This helps with strongly
            // red transmission, but does not bypass clipping or periodicity checks.
            var chosen = green.Valid ? green : luma.Valid ? luma : red;
            var channels = new[] { red, green, luma };
            foreach (var channel in channels)
            {
                if (chosen.Valid && channel.Valid && Math.Abs(chosen.Bpm - channel.Bpm) > 8)
                { result.Reason = "Sinyal kanalları uyuşmuyor; ölçümü tekrarlayın"; return result; }
            }
            chosen.Fps = result.Fps;
            chosen.Seconds = result.Seconds;
            return chosen;
        }

        static PulseEstimate AnalyzeChannel(IList<CameraSample> samples, int channel)
        {
            var result = new PulseEstimate { Channel = channel == 0 ? "red" : channel == 1 ? "green" : "luminance" };
            double clipping = 0;
            foreach (var s in samples) clipping += channel == 0 ? s.clipR : channel == 1 ? s.clipG : s.clipY;
            if (clipping / samples.Count > 0.1)
            { result.Reason = "Görüntü aşırı aydınlık; sinyal doygun"; return result; }
            int n = (int)Math.Floor((samples[samples.Count - 1].t - samples[0].t) * Rate) + 1;
            var raw = new double[n];
            int pos = 0;
            for (int i = 0; i < n; i++)
            {
                double t = samples[0].t + i / Rate;
                while (pos + 1 < samples.Count - 1 && samples[pos + 1].t < t) pos++;
                var a = samples[pos]; var b = samples[pos + 1];
                double f = Math.Max(0, Math.Min(1, (t - a.t) / (b.t - a.t)));
                raw[i] = Value(a, channel) * (1 - f) + Value(b, channel) * f;
            }
            // Remove slow illumination drift; short smoothing limits high-frequency noise.
            var detrended = new double[n];
            for (int i = 0; i < n; i++)
            {
                double sum = 0; int count = 0;
                for (int j = Math.Max(0, i - 22); j <= Math.Min(n - 1, i + 22); j++) { sum += raw[j]; count++; }
                detrended[i] = raw[i] - sum / count;
            }
            var filtered = new double[n - 60]; // discard filter edges (one second each).
            double energy = 0;
            for (int i = 0; i < filtered.Length; i++)
            {
                int k = i + 30;
                filtered[i] = (detrended[k - 1] + 2 * detrended[k] + detrended[k + 1]) / 4;
                energy += filtered[i] * filtered[i];
            }
            double rms = Math.Sqrt(energy / filtered.Length);
            if (rms < 0.12) { result.Reason = "Nabız sinyali çok zayıf"; return result; }
            // Reject a large transient that dominates the otherwise small waveform.
            double max = 0;
            foreach (var value in filtered) max = Math.Max(max, Math.Abs(value));
            if (max / rms > 6) { result.Reason = "Hareket veya ışık değişimi algılandı"; return result; }

            double bpm, score;
            if (!Period(filtered, 0, filtered.Length, out bpm, out score))
            { result.Reason = "Düzenli nabız sinyali bulunamadı"; return result; }
            // Require agreement between disjoint windows, not just a repeating artifact
            // across the whole recording. This still cannot prove physiological accuracy.
            int window = filtered.Length / 3;
            for (int i = 0; i < 3; i++)
            {
                double localBpm, localScore;
                if (!Period(filtered, i * window, window, out localBpm, out localScore) ||
                    localScore < 0.65 || Math.Abs(localBpm - bpm) > Math.Max(8, bpm * 0.08))
                { result.Reason = "Ölçüm boyunca sinyal tutarlı değil"; return result; }
            }
            if (score < 0.7) { result.Reason = "Sinyal kalitesi yetersiz"; return result; }
            result.Valid = true; result.Bpm = bpm; result.Correlation = score;
            result.Reason = "Sinyal kontrolü geçti; doğruluk cihaz karşılaştırması gerektirir";
            return result;
        }

        static double Value(CameraSample sample, int channel) { return channel == 0 ? sample.r : channel == 1 ? sample.g : sample.y; }

        static bool Period(double[] signal, int start, int length, out double bpm, out double score)
        {
            bpm = 0; score = 0;
            // Acquisition range 40–210 BPM; not a normal/abnormal classification.
            int low = (int)Math.Floor(60 * Rate / 210);
            int high = (int)Math.Ceiling(60 * Rate / 40);
            var corr = new double[high + 2];
            for (int lag = low - 1; lag <= high + 1; lag++)
            {
                double ab = 0, aa = 0, bb = 0;
                for (int i = lag; i < length; i++)
                { double a = signal[start + i], b = signal[start + i - lag]; ab += a * b; aa += a * a; bb += b * b; }
                corr[lag] = aa > 0 && bb > 0 ? ab / Math.Sqrt(aa * bb) : 0;
            }
            double best = 0;
            for (int lag = low; lag <= high; lag++)
                if (corr[lag] > corr[lag - 1] && corr[lag] >= corr[lag + 1]) best = Math.Max(best, corr[lag]);
            if (best < 0.65) return false;
            // Earliest strong local maximum avoids mistaking two periods for one.
            for (int lag = low; lag <= high; lag++)
            {
                if (corr[lag] < Math.Max(0.65, best * 0.92) || corr[lag] <= corr[lag - 1] || corr[lag] < corr[lag + 1]) continue;
                double denom = corr[lag - 1] - 2 * corr[lag] + corr[lag + 1];
                double correction = Math.Abs(denom) > 1e-10 ? 0.5 * (corr[lag - 1] - corr[lag + 1]) / denom : 0;
                correction = Math.Max(-0.5, Math.Min(0.5, correction));
                double value = 60 * Rate / (lag + correction);
                if (value < 40 || value > 210) continue;
                bpm = value; score = corr[lag]; return true;
            }
            return false;
        }
    }
}
