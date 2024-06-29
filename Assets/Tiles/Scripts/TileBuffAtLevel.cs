using NUnit.Framework.Constraints;
using System;

[Serializable]
public class TileBuffAtLevel
{
    public Buff spawnRateBuff = new Buff(UnitStatsEnum.SpawnRate);
    public Buff movementBuff = new Buff(UnitStatsEnum.MovementSpeed);
    public Buff healthBuff = new Buff(UnitStatsEnum.Health);
    public Buff shieldBuff = new Buff(UnitStatsEnum.Shield);
    public Buff shieldCooldownBuff = new Buff(UnitStatsEnum.ShieldCooldown);
    public Buff shieldRegenerationBuff = new Buff(UnitStatsEnum.ShieldRegeneration);

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