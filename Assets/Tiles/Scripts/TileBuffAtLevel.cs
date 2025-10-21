using NUnit.Framework.Constraints;
using System;

[Serializable]
public class TileBuffAtLevel
{
    public Buff spawnRateBuff = new Buff(false);
    public Buff movementBuff = new Buff(false);
    public Buff healthBuff = new Buff(true);
    public Buff shieldBuff = new Buff(true);
    public Buff shieldCooldownBuff = new Buff(false);
    public Buff shieldRegenerationBuff = new Buff(false);

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