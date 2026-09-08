using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.SceneManagement;

namespace NeuroMaze.Pulse
{
    // Capture runs alongside the existing quiz; it never changes Time.timeScale.
    public sealed class GamePulseMeasurement : MonoBehaviour
    {
        public bool Active { get; private set; }
        GameManager manager;
        QuizManager quiz;
        AndroidJavaObject nativeCamera;
        readonly List<CameraSample> samples=new List<CameraSample>();
        readonly GamePulseWindow[] windows=new GamePulseWindow[3];
        PulseRecord record;
        float started, nextPoll, duration;
        double clockOffset=double.NaN;
        string cameraError="", contact="Kamera hazırlanıyor…";
        bool awaitingPermission;
        int sleepTimeout;

        public void Begin(GameManager owner, QuizManager questions, string zoneId, float seconds)
        {
            if(Active) return;
            manager=owner; quiz=questions; duration=seconds;
            samples.Clear(); for(int i=0;i<3;i++) windows[i]=null;
            cameraError=""; clockOffset=double.NaN; started=Time.realtimeSinceStartup;
            Active=true; sleepTimeout=Screen.sleepTimeout; Screen.sleepTimeout=SleepTimeout.NeverSleep;
            var study=AndroidPulseStudy.Instance; study.EnsureGameSession();
            record=new PulseRecord { measurementId=Guid.NewGuid().ToString("N"), participantCode=study.ParticipantCode,
                sessionId=study.SessionId, phase="safe_zone", safeZoneId=zoneId, startedUtc=DateTime.UtcNow.ToString("O"),
                scene=SceneManager.GetActiveScene().name, algorithmVersion=GamePulseWindows.Version,
                source=study.UsesExternalLight?"camera_external_light_experimental":"camera_ambient_light_experimental",
                deviceModel=SystemInfo.deviceModel, operatingSystem=SystemInfo.operatingSystem };
#if UNITY_ANDROID && !UNITY_EDITOR
            if(!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                awaitingPermission=true;
                var callbacks=new PermissionCallbacks();
                callbacks.PermissionDenied += _ => { if(Active) {awaitingPermission=false; cameraError="Kamera izni verilmedi";} };
                Permission.RequestUserPermission(Permission.Camera,callbacks);
            }
            else OpenCamera();
#else
            cameraError="Kamera ölçümü Android cihazda yapılır";
#endif
        }

        void OpenCamera()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                nativeCamera=new AndroidJavaObject("com.neuromaze.pulse.PulseCamera");
                using(var unity=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using(var activity=unity.GetStatic<AndroidJavaObject>("currentActivity")) nativeCamera.Call("start",activity);
            }
            catch(Exception e) { cameraError=e.Message; StopCamera(); }
#endif
        }

        void Update()
        {
            if(!Active) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            if(awaitingPermission && Permission.HasUserAuthorizedPermission(Permission.Camera)) {awaitingPermission=false;OpenCamera();}
#endif
            float elapsed=Time.realtimeSinceStartup-started;
            if(Time.realtimeSinceStartup>=nextPoll) {nextPoll=Time.realtimeSinceStartup+0.1f;Poll();}
            for(int i=0;i<3;i++) if(windows[i]==null && elapsed>=12+i*9) windows[i]=GamePulseWindows.Analyze(samples,i);
            string first=Value(windows[0]), last=Value(windows[2]);
            string live=windows[1]!=null?Value(windows[1]):first;
            if(manager!=null) manager.SetGamePulseText("Nabız ölçülüyor • "+Mathf.Max(0,Mathf.CeilToInt(duration-elapsed))+" sn\n"+
                (elapsed<3?"Parmağınızı kameraya yerleştirin • hazırlık":cameraError.Length>0?cameraError:contact)+"\nBaşlangıç: "+first+"  •  Son pencere: "+live+" BPM (deneysel)");
            if(elapsed>=duration) Complete();
        }

        void Poll()
        {
            if(nativeCamera==null) return;
            try
            {
                var data=JsonUtility.FromJson<AndroidPulseStudy.CameraPayload>(nativeCamera.Call<string>("poll"));
                if(data==null) return;
                record.cameraInfo=data.info;
                if(data.torch) record.source="camera_builtin_torch_experimental";
                if(data.state=="error" || data.state=="stopped" || data.dropped>0)
                {cameraError=data.dropped>0?"Kamera örnek kaybı":string.IsNullOrEmpty(data.error)?"Kamera durdu":data.error;StopCamera();return;}
                if(data.samples==null || data.samples.Length==0) return;
                // Map native timestamps onto this safe-zone visit, including nativeCamera permission/open delay.
                if(double.IsNaN(clockOffset)) clockOffset=Time.realtimeSinceStartup-started-data.samples[data.samples.Length-1].t;
                foreach(var s in data.samples)
                {
                    s.t+=clockOffset;
                    if(s.t>=0 && s.t<duration) samples.Add(s);
                    contact=PulseSignal.HasContact(s)?"Parmak algılandı. Sabit tutup soruları cevaplayın.":"Parmağınızı arka kameraya tutun; sabit kalın.";
                }
            }
            catch(Exception e) {cameraError=e.Message;StopCamera();}
        }

        public void Complete()
        {
            if(!Active) return;
            Poll(); Active=false; awaitingPermission=false; StopCamera(); Screen.sleepTimeout=sleepTimeout;
            float elapsed=Time.realtimeSinceStartup-started;
            for(int i=0;i<3;i++)
                windows[i]=elapsed>=12+i*9-0.1f?GamePulseWindows.Analyze(samples,i):new GamePulseWindow {From=3+i*9,To=12+i*9,Estimate=new PulseEstimate {Reason="Ölçüm süresi tamamlanmadı"}};
            record.completedUtc=DateTime.UtcNow.ToString("O");
            record.sampleCount=samples.Count; record.correctAnswers=quiz!=null?quiz.correctAnswerCount:0;
            record.startValid=windows[0].Estimate.Valid;record.middleValid=windows[1].Estimate.Valid;record.endValid=windows[2].Estimate.Valid;
            record.startBpm=record.startValid?windows[0].Estimate.Bpm:0;
            record.middleBpm=record.middleValid?windows[1].Estimate.Bpm:0;
            record.endBpm=record.endValid?windows[2].Estimate.Bpm:0;
            record.validWindowCount=windows.Count(w=>w.Estimate.Valid);
            record.bpm=GamePulseWindows.Mean(windows);
            record.durationSeconds=windows.Where(w=>w.Estimate.Valid).Sum(w=>w.Estimate.Seconds);
            record.recordingSeconds=Math.Min(duration,elapsed);
            record.completeWindowCoverage=record.validWindowCount==3;
            record.windowReasons=string.Join(" | ",windows.Select(w=>w.From+"-"+w.To+"s: "+w.Estimate.Reason));
            record.status=record.validWindowCount>0?"accepted_unvalidated":"rejected";
            record.reason=cameraError.Length>0?cameraError:record.completeWindowCoverage?"Üç pencere sinyal kontrolünü geçti; deneysel ölçüm":"Eksik sinyal: "+record.validWindowCount+"/3 pencere kullanılabilir";
            if(samples.Count>1) record.samplingFps=(samples.Count-1)/(samples[samples.Count-1].t-samples[0].t);
            record.signalCorrelation=record.validWindowCount>0?windows.Where(w=>w.Estimate.Valid).Average(w=>w.Estimate.Correlation):0;
            string result="Başlangıç: "+Value(windows[0])+"  •  Ort.: "+(record.validWindowCount>0?record.bpm.ToString("F1"):"—")+"  •  Bitiş: "+Value(windows[2])+" BPM\n"+
                "Deneysel • "+record.validWindowCount+"/3 pencere • "+record.correctAnswers+" doğru cevap";
            try {PulseRecords.Save(record,samples);} catch(Exception e) {result+="\nKAYIT HATASI: "+e.Message;}
            if(manager!=null) manager.SetGamePulseResult(result);
            if(AndroidPulseStudy.Instance!=null) AndroidPulseStudy.Instance.RefreshGameHistory();
            Debug.Log("PULSE_GAME_COMPLETE "+record.measurementId+";windows="+record.validWindowCount+";samples="+samples.Count);
        }
        static string Value(GamePulseWindow w) {return w!=null&&w.Estimate.Valid?w.Estimate.Bpm.ToString("F1"):"—";}
        void StopCamera() {try {if(nativeCamera!=null) nativeCamera.Call("stop");} catch(Exception) {} finally {if(nativeCamera!=null) nativeCamera.Dispose();nativeCamera=null;} }
        void OnApplicationPause(bool paused) {if(paused) Complete();}
        void OnDestroy() {Complete();}
    }
}

