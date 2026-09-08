using System;
using System.Collections.Generic;
using System.Linq;

namespace NeuroMaze.Pulse
{
    public sealed class GamePulseWindow
    {
        public double From, To;
        public PulseEstimate Estimate;
    }
    public static class GamePulseWindows
    {
        public const string Version = "android-game-ppg-2";
        // Fixed game-time intervals. Never substitute a later good sample for a missing baseline.
        // The first three seconds allow the player to place a finger on the camera.
        public static GamePulseWindow Analyze(IList<CameraSample> samples, int index)
        {
            double from = 3 + index * 9;
            double to = from + 9;
            var data = samples.Where(s => s.t >= from && s.t < to).ToList();
            var longest=new List<CameraSample>();var run=new List<CameraSample>();
            foreach(var sample in data)
            {
                bool gap=run.Count>0 && (sample.t<=run[run.Count-1].t || sample.t-run[run.Count-1].t>0.25);
                if(gap || !PulseSignal.HasContact(sample)) {if(run.Count>longest.Count) longest=new List<CameraSample>(run);run.Clear();}
                if(PulseSignal.HasContact(sample)) run.Add(sample);
            }
            if(run.Count>longest.Count) longest=run;
            return new GamePulseWindow { From=from, To=to, Estimate=PulseSignal.Analyze(longest,8) };
        }
        public static double Mean(GamePulseWindow[] windows)
        {
            var valid=windows.Where(w=>w.Estimate.Valid).ToArray();
            double seconds=valid.Sum(w=>w.Estimate.Seconds);
            return seconds>0 ? valid.Sum(w=>w.Estimate.Bpm*w.Estimate.Seconds)/seconds : 0;
        }
    }
}
