using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using NeuroMaze.Pulse;

static class Program
{
    static int tests;
    static void Check(bool condition, string name)
    { tests++; if (!condition) throw new Exception("FAIL: " + name); Console.WriteLine("PASS: " + name); }
    static List<CameraSample> Wave(double bpm, double fps = 30, double seconds = 28, double noise = 0.15)
    {
        var data = new List<CameraSample>(); var random = new Random(730);
        for (int i = 0; i <= (int)(seconds * fps); i++)
        {
            double t = i / fps;
            double value = 3 * Math.Sin(2 * Math.PI * bpm / 60 * t) + noise * (random.NextDouble() - 0.5);
            data.Add(new CameraSample { t = t, r = (float)(170 + value), g = (float)(65 + value), b = 25 });
        }
        return data;
    }
    static void Main()
    {
        foreach (double bpm in new[] { 42.0, 50, 60, 72, 100, 120, 145, 160, 180, 205 })
        {
            var e = PulseSignal.Analyze(Wave(bpm));
            Check(e.Valid && Math.Abs(e.Bpm - bpm) < 2, "periodic " + bpm + " BPM -> " + e.Bpm.ToString("F2") + " " + e.Reason);
        }
        Check(!PulseSignal.Analyze(new List<CameraSample>()).Valid, "empty rejected");
        Check(!PulseSignal.Analyze(Wave(80, 30, 10)).Valid, "short signal rejected");
        Check(!PulseSignal.Analyze(Wave(80, 10)).Valid, "low camera FPS rejected");
        var flat = Wave(80); foreach (var s in flat) { s.r = 170; s.g = 65; }
        Check(!PulseSignal.Analyze(flat).Valid, "flat illumination rejected");
        var noise = Wave(80); var rng = new Random(222); foreach (var s in noise) { s.r = 170 + (float)rng.NextDouble()*10; s.g = 65 + (float)rng.NextDouble()*10; }
        Check(!PulseSignal.Analyze(noise).Valid, "random noise rejected");
        var gap = Wave(80); gap.RemoveRange(120, 30);
        Check(!PulseSignal.Analyze(gap).Valid, "missing frames rejected");
        var removal = Wave(80); removal[250].r = 10;
        Check(!PulseSignal.Analyze(removal).Valid, "finger removal rejected");
        var saturated = Wave(80); foreach (var s in saturated) { s.clipR = 0.9f; s.r = 255; }
        var green = PulseSignal.Analyze(saturated);
        Check(green.Valid && green.Channel == "green" && Math.Abs(green.Bpm - 80) < 2, "green fallback when red saturated");
        foreach (var s in saturated) s.clipG = 0.9f;
        Check(!PulseSignal.Analyze(saturated).Valid, "both channels saturated rejected");
        var contradictory = Wave(80); var redWave = Wave(120);
        for (int i = 0; i < contradictory.Count; i++) contradictory[i].r = redWave[i].r;
        Check(!PulseSignal.Analyze(contradictory).Valid, "conflicting channels rejected");
        var transient = Wave(80); transient[350].r += 60; transient[350].g += 60;
        Check(!PulseSignal.Analyze(transient).Valid, "dominant motion spike rejected");
        var variable = Wave(80); var fast = Wave(120);
        for (int i = 300; i < variable.Count; i++) { variable[i].r = fast[i].r; variable[i].g = fast[i].g; }
        Check(!PulseSignal.Analyze(variable).Valid, "inconsistent windows rejected");
        var redOnly = Wave(85); foreach (var s in redOnly) { s.g = 0; s.b = 0; }
        var redEstimate = PulseSignal.Analyze(redOnly);
        Check(redEstimate.Valid && Math.Abs(redEstimate.Bpm - 85) < 2, "red-only transmitted light is not falsely rejected as no finger");
        var lumaOnly = Wave(95);
        foreach (var s in lumaOnly) { s.y = (s.r - 170) + 70; s.r = 255; s.g = 0; s.b = 0; s.clipR = 1; }
        var lumaEstimate = PulseSignal.Analyze(lumaOnly);
        Check(lumaEstimate.Valid && lumaEstimate.Channel == "luminance" && Math.Abs(lumaEstimate.Bpm - 95) < 2, "native luma retains pulse when display RGB is clipped");
        foreach (var s in lumaOnly) s.clipY = 1;
        Check(!PulseSignal.Analyze(lumaOnly).Valid, "clipped luma cannot bypass signal rejection");
        Check(PulseRecords.ValidCode("K001") && !PulseRecords.ValidCode("Murat Ali") && !PulseRecords.ValidCode("../K001"), "participant code validation");
        Save("K001", "session1", "baseline", 80, "camera_external_light_experimental", "accepted_unvalidated");
        Save("K001", "session1", "baseline", 100, "camera_external_light_experimental", "accepted_unvalidated");
        Save("K001", "session1", "baseline", 150, "camera_builtin_torch_experimental", "accepted_unvalidated");
        Save("K001", "session1", "end", 110, "camera_external_light_experimental", "accepted_unvalidated");
        Save("K002", "session2", "baseline", 120, "camera_external_light_experimental", "accepted_unvalidated");
        Save("K001", "session1", "baseline", 0, "camera_external_light_experimental", "rejected");
        Check(PulseRecords.Load().Count == 6, "all attempts persist including rejected");
        using (var zip = ZipFile.OpenRead(PulseRecords.ExportZip()))
        {
            string summary; using (var reader = new StreamReader(zip.GetEntry("phase_summary.csv").Open())) summary = reader.ReadToEnd();
            Check(summary.Contains("\"K001\",\"session1\",\"camera_external_light_experimental\",\"baseline\",\"2\",\"90.00\""), "baseline mean excludes rejection and other source/participant/phase");
            Check(summary.Contains("\"K002\",\"session2\"") && summary.Contains("\"end\",\"1\",\"110.00\""), "participant and end phase summaries separate");
            string csv; using (var reader = new StreamReader(zip.GetEntry("measurements.csv").Open())) csv = reader.ReadToEnd();
            Check(csv.Contains("\"rejected\",\"\""), "failed BPM exported blank");
            Check(zip.Entries.Count == 16, "ZIP contains raw signal and metadata for every attempt plus CSVs and readme");
            Check(csv.Contains("\"false\""), "unvalidated status exported");
        }
        foreach(var rate in new[]{50.0,80.0,120.0,180.0})
        {
            var data=Wave(rate,30,30);
            for(int i=0;i<3;i++) {var w=GamePulseWindows.Analyze(data,i);Check(w.Estimate.Valid && Math.Abs(w.Estimate.Bpm-rate)<3,"game window "+i+" at "+rate+" BPM: "+w.Estimate.Reason);}
        }
        var lateData=Wave(80,30,30);lateData.RemoveAll(s=>s.t<12);
        var lateFirst=GamePulseWindows.Analyze(lateData,0);
        var lateEnd=GamePulseWindows.Analyze(lateData,2);
        Check(!lateFirst.Estimate.Valid && lateEnd.Estimate.Valid,"late contact cannot fabricate initial BPM from later data");
        var missing=Wave(80,30,30);missing.RemoveAll(s=>s.t>13 && s.t<14);
        Check(!GamePulseWindows.Analyze(missing,1).Estimate.Valid,"game middle window rejects acquisition gap");
        var full=new[]{GamePulseWindows.Analyze(Wave(80,30,30),0),GamePulseWindows.Analyze(Wave(100,30,30),1),GamePulseWindows.Analyze(Wave(120,30,30),2)};
        Check(GamePulseWindows.Mean(full)>99 && GamePulseWindows.Mean(full)<102,"game mean uses accepted nonoverlapping windows");
        var visit=new PulseRecord {measurementId=Guid.NewGuid().ToString("N"),participantCode="K003",sessionId="s3",phase="safe_zone",safeZoneId="Lab1/Zone1",status="accepted_unvalidated",startValid=false,endValid=true,endBpm=90,bpm=90,validWindowCount=1,correctAnswers=2};
        PulseRecords.Save(visit,Wave(90));
        using(var zip=ZipFile.OpenRead(PulseRecords.ExportZip()))
        using(var reader=new StreamReader(zip.GetEntry("safe_zone_visits.csv").Open()))
        {var csv=reader.ReadToEnd();Check(csv.Contains("\"Lab1/Zone1\"") && csv.Contains("\"\",\"\",\"90.00\",\"90.00\",\"1\",\"false\""),"partial visit exports missing start blank and incomplete coverage");}
        Console.WriteLine(tests + " signal tests passed. Synthetic tests do not establish device accuracy.");
        var encounter=new EnemyEncounterClock(30);
        Check(!encounter.Advance(15,false,20) && encounter.SpawnRemaining==15,"enemy absent before initial 30 seconds");
        Check(!encounter.Advance(60,true,20) && encounter.SpawnRemaining==15,"safe zone freezes initial spawn countdown");
        Check(!encounter.Advance(14,false,20) && encounter.Advance(1,false,20),"enemy resumes remaining initial delay after exit");
        Check(!encounter.Advance(30,true,20),"spawned enemy remains hidden throughout safe zone");
        Check(!encounter.Advance(10,false,20) && encounter.ReturnRemaining==10,"existing scene respawn grace applies after exit");
        Check(!encounter.Advance(10,false,20) && encounter.Advance(0,false,20),"enemy returns after configured exit delay");
        Check(!encounter.Advance(60,true,20) && !encounter.Advance(0,false,20) && encounter.ReturnRemaining==20,"reentry protects player and renews exit grace");
        Console.WriteLine(tests + " total checks passed.");
    }
    static void Save(string participant, string session, string phase, double bpm, string source, string status)
    {
        PulseRecords.Save(new PulseRecord { measurementId = Guid.NewGuid().ToString("N"), participantCode = participant, sessionId = session,
            phase = phase, bpm = bpm, source = source, status = status, startedUtc = DateTime.UtcNow.ToString("O"), reason = "Test, not patient data" }, Wave(80));
    }
}
