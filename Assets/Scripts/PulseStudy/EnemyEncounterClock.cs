using System;
namespace NeuroMaze.Pulse
{
    public sealed class EnemyEncounterClock
    {
        public double SpawnRemaining { get; private set; }
        public double ReturnRemaining { get; private set; }
        public bool Spawned { get; private set; }
        bool wasProtected;
        public EnemyEncounterClock(double spawnDelay) { SpawnRemaining=Math.Max(0,spawnDelay); }
        public bool Advance(double seconds,bool protectedNow,double returnDelay)
        {
            seconds=Math.Max(0,seconds);
            if(protectedNow) {wasProtected=true;return false;}
            if(wasProtected) {wasProtected=false;if(Spawned) ReturnRemaining=Math.Max(0,returnDelay);}
            if(!Spawned)
            {
                SpawnRemaining=Math.Max(0,SpawnRemaining-seconds);
                if(SpawnRemaining>0) return false;
                Spawned=true;
            }
            if(ReturnRemaining>0) {ReturnRemaining=Math.Max(0,ReturnRemaining-seconds);return false;}
            return true;
        }
    }
}
