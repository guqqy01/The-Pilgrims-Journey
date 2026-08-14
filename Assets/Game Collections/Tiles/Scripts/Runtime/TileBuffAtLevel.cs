using NUnit.Framework.Constraints;
using System;
using Units;

[Serializable]
public class TileBuffAtLevel
{
    public Buff spawnRateBuff = new Buff(BuffType.Additional | BuffType.Multiplier | BuffType.Base, false);
    public Buff movementBuff = new Buff(BuffType.Additional | BuffType.Multiplier | BuffType.Base, false);
    public Buff healthBuff = new Buff(BuffType.Additional | BuffType.Multiplier | BuffType.Base, true);
    public Buff shieldBuff = new Buff(BuffType.Additional | BuffType.Multiplier | BuffType.Base, true);
    public Buff shieldCooldownBuff = new Buff(BuffType.Additional | BuffType.Multiplier | BuffType.Base, false);
    public Buff shieldRegenerationBuff = new Buff(BuffType.Additional | BuffType.Multiplier | BuffType.Base, false);

    public Buff GetBuff(UnitStatsEnum stat)
    {
        switch (stat)
        {
            case UnitStatsEnum.Health:
                return healthBuff;
            case UnitStatsEnum.Shield:
                return shieldBuff;
            case UnitStatsEnum.SpawnRate:
                return spawnRateBuff;
            case UnitStatsEnum.MovementSpeed:
                return movementBuff;
            case UnitStatsEnum.ShieldRegeneration:
                return shieldRegenerationBuff;
            case UnitStatsEnum.ShieldCooldown:
                return shieldCooldownBuff;
        }

        return null;
    }
}