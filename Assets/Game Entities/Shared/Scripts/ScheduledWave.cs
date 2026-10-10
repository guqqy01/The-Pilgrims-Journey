using Monsters;

namespace Spawning
{
    public readonly struct ScheduledWave
    {
        public readonly float StartTime;
        public readonly WaveMonsterSpawn Spawn;
        public readonly int Multiplier;

        public ScheduledWave(float startTime, WaveMonsterSpawn spawn, int multiplier)
        {
            StartTime = startTime;
            Spawn = spawn;
            Multiplier = multiplier;
        }
    }
}