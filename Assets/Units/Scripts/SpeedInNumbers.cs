namespace Units
{
    using static UnitSpecialAbility;
    public class SpeedInNumbers : UnitSpecialAbility
    {
        public float BaseSpeedBuff = 0.1f;
        public float SpeedLevelIncrement = 0.1f;
        public float Range = 2f;

        public float SpeedBuff => (int)Rarity * (BaseSpeedBuff + SpeedLevelIncrement * Level);

        public Rarity Rarity { get; set; }

        public int Level { get; set; } = 0;
        public int MaxLevel { get; set; } = 5;

        public OnLevelUp OnLevelUpEvent { get; set; }

    }
}

