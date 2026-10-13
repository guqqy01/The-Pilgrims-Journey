using Units;

namespace Quests
{
    // =====================================================================
    //  Keep casualties low
    // =====================================================================

    public class CasualtyLimitQuest : Quest
    {
        private readonly int maxCasualties;
        private int casualties;

        public CasualtyLimitQuest(int maxCasualties) => this.maxCasualties = maxCasualties;

        public override string Title => "Leave No One Behind";
        public override string Description => $"Lose no more than {maxCasualties} pilgrims.";
        public override string ProgressText => $"{casualties}/{maxCasualties} lost";

        public override void OnUnitDied(Unit unit)
        {
            casualties++;
            NotifyChanged();

            if (casualties > maxCasualties)
                Fail();
        }

        public override void OnLevelEnd() => Complete();
    }
}