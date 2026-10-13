using System.Linq;
using Units;
namespace Quests
{
    public class SafePassageQuest : Quest
    {
        private readonly int required;
        private int passed;
        private int died;

        public SafePassageQuest(int required) => this.required = required;

        public override string Title => "Safe Passage";
        public override string Description => $"Lead {required} pilgrims safely to the end of the path.";
        public override string ProgressText => $"{passed}/{required} arrived";
        public override string FailureReason => "Enough of your pilgrims did not survive the journey";
        protected override void OnBegin()
        {
            if (required <= 0)
                Complete();
        }

        public override void OnUnitPassed(int totalPassed)
        {
            passed = totalPassed;
            NotifyChanged();

            if (passed >= required)
                Complete();
        }
        public override void Tick(float deltaTime)
        {
            var director = Context.Director;
            if (director == null || !director.SpawnPhaseFinished)
                return;

            var units = UnitSpawningManager.Instance;
            if (units != null && units.SpawnedUnits.Any(u => u != null))
                return;

            // Spawning is over, nobody is left walking, and we didn't make the quota.
            if (passed < required)
                Fail();
        }
        public override void OnUnitDied(Unit unit)
        {
            died++;

            // Not enough pilgrims left in the level to ever reach the requirement.
            if (Context.Plan.TotalUnits - died < required)
                Fail();
        }

        public override void OnLevelEnd()
        {
            if (passed >= required) Complete();
            else Fail();
        }
    }
}