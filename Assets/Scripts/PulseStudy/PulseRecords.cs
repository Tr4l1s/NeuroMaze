using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NeuroMaze.Pulse
{
    [Serializable]
    public class PulseRecord
    {
        public string measurementId, participantCode, sessionId, phase, startedUtc, completedUtc;
        public string scene, source, status, reason, algorithmVersion, deviceModel, operatingSystem, cameraInfo;
        public bool clinicallyValidated = false;
        public double bpm, signalCorrelation, durationSeconds, samplingFps;
        public int sampleCount;
        public string signalFile;
        public string safeZoneId, windowReasons;
        public double startBpm, middleBpm, endBpm, recordingSeconds;
        public bool startValid, middleValid, endValid, completeWindowCoverage;
        public int validWindowCount, correctAnswers;
    }

    public static class PulseRecords
    {
        static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
        public static string DirectoryPath { get { return Path.Combine(Application.persistentDataPath, "PulseStudy"); } }

        public static bool ValidCode(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > 24) return false;
            foreach (char c in value)
                if (!(c >= 'A' && c <= 'Z') && !(c >= '0' && c <= '9') && c != '-' && c != '_') return false;
            return true;
        }

        public static void Save(PulseRecord record, IList<CameraSample> samples)
        {
            Directory.CreateDirectory(DirectoryPath);
            // Write a separate immutable record per attempt. A failed attempt has no BPM.
            record.signalFile = record.measurementId + "_signal.csv";
            var signal = new StringBuilder("seconds,red,green,blue,red_clipped_fraction,green_clipped_fraction,contact_detected,luminance,luminance_clipped_fraction\n");
            foreach (var sample in samples)
                signal.AppendFormat(Invariant, "{0:F6},{1:F5},{2:F5},{3:F5},{4:F5},{5:F5},{6},{7:F5},{8:F5}\n",
                    sample.t, sample.r, sample.g, sample.b, sample.clipR, sample.clipG, PulseSignal.HasContact(sample) ? 1 : 0, sample.y, sample.clipY);
            WriteAtomic(Path.Combine(DirectoryPath, record.signalFile), signal.ToString());
            WriteAtomic(Path.Combine(DirectoryPath, record.measurementId + ".json"), JsonUtility.ToJson(record, true));
        }

        static void WriteAtomic(string path, string value)
        {
            string temp = path + ".tmp";
            File.WriteAllText(temp, value, new UTF8Encoding(false));
            File.Move(temp, path);
        }

        public static List<PulseRecord> Load()
        {
            var records = new List<PulseRecord>();
            if (!Directory.Exists(DirectoryPath)) return records;
            foreach (string path in Directory.GetFiles(DirectoryPath, "*.json"))
            {
                // Never silently drop corrupt research records from an export.
                var record = JsonUtility.FromJson<PulseRecord>(File.ReadAllText(path));
                if (record == null || string.IsNullOrEmpty(record.measurementId))
                    throw new InvalidDataException("Kayıt okunamadı: " + Path.GetFileName(path));
                records.Add(record);
            }
            return records.OrderBy(r => r.startedUtc, StringComparer.Ordinal).ToList();
        }

        public static string ExportZip()
        {
            var records = Load();
            if (records.Count == 0) throw new InvalidOperationException("Henüz dışa aktarılacak ölçüm yok.");
            string destination = Path.Combine(Application.persistentDataPath, "NeuroMaze_Nabiz_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff", Invariant) + ".zip");
            using (var file = new FileStream(destination, FileMode.CreateNew))
            using (var zip = new ZipArchive(file, ZipArchiveMode.Create))
            {
                var csv = new StringBuilder("measurement_id,participant_code,session_id,phase,started_utc,completed_utc,scene,source,status,bpm,signal_correlation,valid_signal_seconds,sampling_fps,sample_count,algorithm_version,clinically_validated,device_model,operating_system,camera_info,reason,signal_file\n");
                foreach (var r in records)
                {
                    csv.AppendLine(Row(r.measurementId, r.participantCode, r.sessionId, r.phase, r.startedUtc, r.completedUtc,
                        r.scene, r.source, r.status, r.status == "accepted_unvalidated" ? r.bpm.ToString("F2", Invariant) : "",
                        r.signalCorrelation.ToString("F4", Invariant), r.durationSeconds.ToString("F3", Invariant), r.samplingFps.ToString("F3", Invariant),
                        r.sampleCount.ToString(Invariant), r.algorithmVersion, "false", r.deviceModel, r.operatingSystem, r.cameraInfo, r.reason, r.signalFile));
                    Add(zip, "records/" + r.measurementId + ".json", File.ReadAllText(Path.Combine(DirectoryPath, r.measurementId + ".json")));
                    Add(zip, "signals/" + r.signalFile, File.ReadAllText(Path.Combine(DirectoryPath, r.signalFile)));
                }
                Add(zip, "measurements.csv", csv.ToString());
                var visits = new StringBuilder("measurement_id,participant_code,session_id,scene,safe_zone_id,started_utc,source,start_bpm,middle_bpm,mean_available_windows_bpm,end_bpm,valid_window_count,complete_window_coverage,accepted_signal_seconds,recording_seconds,correct_answers,status,window_reasons,clinically_validated\n");
                foreach (var r in records.Where(r => r.phase == "safe_zone"))
                    visits.AppendLine(Row(r.measurementId,r.participantCode,r.sessionId,r.scene,r.safeZoneId,r.startedUtc,r.source,
                        r.startValid?r.startBpm.ToString("F2",Invariant):"",r.middleValid?r.middleBpm.ToString("F2",Invariant):"",
                        r.validWindowCount>0?r.bpm.ToString("F2",Invariant):"",r.endValid?r.endBpm.ToString("F2",Invariant):"",
                        r.validWindowCount.ToString(),r.completeWindowCoverage?"true":"false",r.durationSeconds.ToString("F3",Invariant),
                        r.recordingSeconds.ToString("F3",Invariant),r.correctAnswers.ToString(),r.status,r.windowReasons,"false"));
                Add(zip,"safe_zone_visits.csv",visits.ToString());
                var summary = new StringBuilder("participant_code,session_id,source,phase,accepted_count,point_measurement_mean_bpm,min_bpm,max_bpm,clinically_validated\n");
                foreach (var group in records.Where(r => r.status == "accepted_unvalidated").GroupBy(r => new { r.participantCode, r.sessionId, r.source, r.phase }))
                    summary.AppendLine(Row(group.Key.participantCode, group.Key.sessionId, group.Key.source, group.Key.phase,
                        group.Count().ToString(Invariant), group.Average(r => r.bpm).ToString("F2", Invariant), group.Min(r => r.bpm).ToString("F2", Invariant), group.Max(r => r.bpm).ToString("F2", Invariant), "false"));
                Add(zip, "phase_summary.csv", summary.ToString());
                Add(zip, "OKU.txt", "NeuroMaze deneysel kamera PPG kayıtları\n" +
                    "Bu yazılım ve sinyal eşikleri klinik olarak doğrulanmamıştır. Sinyal kontrolünden geçmek ölçüm doğruluğunu kanıtlamaz.\n" +
                    "safe_zone_visits.csv: her 30 saniyelik güvenli alan ziyareti ayrı satır. android-game-ppg-2 için ilk 3 sn parmak yerleştirme, başlangıç 3–12 sn, orta 12–21 sn, bitiş 21–30 sn; her aralık içinde en az 8 saniye kesintisiz kaliteli sinyal gerektirir. Eski android-game-ppg-1 aralıkları 1–10, 10–20, 20–30 sn idi. Ortalama yalnızca geçen pencerelerin süre ağırlıklı BPM ortalamasıdır. valid_window_count<3 ise tüm ölçüm aralığını temsil etmez.\n" +
                    "camera_external_light_experimental: cihazda flaş yok; harici ışık kullanımı beyan edilen deneysel ölçüm.\n" +
                    "camera_ambient_light_experimental: cihazda flaş yok; ek ışık kullanımı beyan edilmedi, ortam ışığıyla deneysel ölçüm.\n" +
                    "camera_builtin_torch_experimental: kameranın kendi flaşıyla deneysel ölçüm.\n" +
                    "Farklı kaynaklar ve farklı katılımcı/oturumlar ayrı özetlenir. baseline=başlangıç; interim=ara; end=bitiş.\n" +
                    "phase_summary.csv aynı aşamadaki kabul edilen noktasal ölçümlerin aritmetik ortalamasıdır. Oyun boyunca sürekli veya zamana göre ağırlıklı ortalama DEĞİLDİR.\n" +
                    "Başarısız/iptal edilen denemelerde BPM boş bırakılır; ortalamaya katılmaz. Hiçbir değer normal/anormal veya stres tanısı olarak yorumlanmaz.\n" +
                    "Her deneme benzersiz kimlikle saklanır. Yeniden alınan ölçümler eski ölçümün üzerine yazılmaz.\n" +
                    "signals: kamera zaman damgalı merkez görüntü alanı RGB ve parlaklık ortalamaları; fotoğraf/video içermez. Kabul edilen süre, ilk 3 saniyelik yerleşme sonrasındaki 25 saniyelik kesintisiz sinyaldir.\n" +
                    "Algorithm: " + PulseSignal.Version + "; 30 Hz yeniden örnekleme, 1.5 s yerel ortalama çıkarma, otokorelasyon; 40–210 BPM edinim aralığı (normal aralık değildir).\n" +
                    "Kayıtlarda cihaz modeli, işletim sistemi, örnekleme hızı, yöntem ve başarısız denemeler bulunur. Cihazı referans ölçümle doğrulamadan bu verileri doğrulanmış araştırma ölçümü olarak sunmayın.\n");
            }
            return destination;
        }

        static string Row(params string[] cells) { return string.Join(",", cells.Select(c => "\"" + (c ?? "").Replace("\"", "\"\"") + "\"")); }
        static void Add(ZipArchive zip, string name, string text)
        {
            var entry = zip.CreateEntry(name, System.IO.Compression.CompressionLevel.Fastest);
            using (var writer = new StreamWriter(entry.Open(), new UTF8Encoding(true))) writer.Write(text);
        }
    }
}
