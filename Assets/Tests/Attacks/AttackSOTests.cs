using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Attacks;
using System;

public class AttackSOTests
{
    // Minimal concrete generic attack SO for testing
    public class TestGenericAttackSO : AttackSO<AttackSpawnBase, BaseWeaponStatsAtLevel>
    {
        public override Type AttackType => typeof(TestAttack);

        public override Buff GetBuffAtIndex(StatTypes stat, int index)
        {
            return null;
        }
    }

    [Test]
    public void GetBuff_ReturnsCorrectBuffForDamage()
    {
        var so = ScriptableObject.CreateInstance<TestGenericAttackSO>();
        so.WeaponStatsAtLevel = new List<BaseWeaponStatsAtLevel> { new BaseWeaponStatsAtLevel() };

        // Provide a simple damage buff
        so.WeaponStatsAtLevel[0].damage = new Buff(BuffType.Additional | BuffType.Multiplier | BuffType.Base, true);
        so.WeaponStatsAtLevel[0].damage.BaseBuff = 5;

        Assert.AreEqual(1, so.MaxLevel);

        var buff = so.GetBuff(StatTypes.Damage, 1);
        Assert.IsNotNull(buff);

        var outOfRange = so.GetBuff(StatTypes.Damage, 2);
        Assert.IsNull(outOfRange);
    }

    [Test]
    public void GetStatsDescription_IncludesDamage()
    {
        var so = ScriptableObject.CreateInstance<TestGenericAttackSO>();
        so.WeaponStatsAtLevel = new List<BaseWeaponStatsAtLevel> { new BaseWeaponStatsAtLevel() };
        so.WeaponStatsAtLevel[0].damage = new Buff(BuffType.Additional | BuffType.Multiplier | BuffType.Base, true);
        so.WeaponStatsAtLevel[0].damage.BaseBuff = 10;

        string desc = so.GetStatsDescription(1);

        Assert.IsNotNull(desc);
        Assert.IsTrue(desc.Contains("Damage"));
    }
}
