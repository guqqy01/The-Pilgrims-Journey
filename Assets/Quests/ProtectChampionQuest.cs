using Units;

namespace Quests
{
    // =====================================================================
    //  Escort a buffed champion
    // =====================================================================

    public class ProtectChampionQuest : Quest
    {
        private readonly UnitSO unitSO;
        private readonly float buffPercent;
        private readonly float scale;
        private readonly float spawnAtSeconds;

        private Unit champion;
        private bool spawned;

        public ProtectChampionQuest(UnitSO unitSO, float buffPercent, float scale, float spawnAtSeconds)
        {
            this.unitSO = unitSO;
            this.buffPercent = buffPercent;
            this.scale = scale;
            this.spawnAtSeconds = spawnAtSeconds;
        }

        public override string Title => "Escort the Champion";
        public override string Description => "A champion joins the march. Keep it alive until it reaches the end of the path.";
        public override string ProgressText => spawned ? "Marching" : "Not yet arrived";

        public override void Tick(float deltaTime)
        {
            if (spawned || Context.Director.Elapsed < spawnAtSeconds)
                return;

            // If this returns null (e.g. path not ready) it simply retries next frame.
            champion = UnitSpawningManager.Instance.SpawnUnit(unitSO, buffPercent, scale);
            if (champion == null)
                return;

            spawned = true;
            NotifyChanged();
        }

        public override void OnUnitDied(Unit unit)
        {
            if (spawned && unit == champion)
                Fail();
        }

        public override void OnUnitEscaped(Unit unit)
        {
            if (spawned && unit == champion)
                Complete();
        }
    }
}