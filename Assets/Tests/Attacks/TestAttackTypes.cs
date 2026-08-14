using System;
using UnityEngine;
using Attacks;

// Minimal Test Attack SO that points to TestAttack type.
public class TestAttackSO : BaseAttackSO
{
    public override Type AttackType => typeof(TestAttack);

    public override int MaxLevel => 3;

    public override Buff GetBuff(StatTypes heroStats, int level)
    {
        // Return a neutral buff (zero) so SetAttackLevel can be called safely.
        return new Buff(BuffType.Additional | BuffType.Multiplier | BuffType.Base, true);
    }

    public override AttackSpawnBase AttackSpawn => null;
}

// Minimal concrete attack used only for tests.
public class TestAttack : BaseAttack
{
    private BaseAttackSO _so;

    public int ProtectedAttackCalls { get; private set; }

    public override BaseAttackSO BaseAttackSO => _so;

    public override void Setup<TBaseAttackSO>(TBaseAttackSO attackSO, LayerMask hitLayerMask)
    {
        _so = attackSO as BaseAttackSO;

        // Provide minimal stats so Update/ProtectedAttack logic isn't blocked by nulls.
        damage = new BuffableStat(1);
        attackSpeed = new BuffableStat(1f);
        criticalChance = new BuffableStat(0f);
        criticalDamageMultiplier = new BuffableStat(1f);
        size = new BuffableStat(1f);
        knockback = new BuffableStat(0f);
        amountOfHits = new BuffableStat(1);
    }

    protected override bool ProtectedAttack()
    {
        ProtectedAttackCalls++;
        return true;
    }

    protected override void InheritLevelSetup() { }
}

// Simple concrete AttackSpawn used if needed by future tests.
public class TestAttackSpawn : AttackSpawnBase
{
    public bool SpawnRaised { get; private set; }

    public override void Init<TAttackInit, TAttackStats>(TAttackInit init, TAttackStats stats)
    {
        SpawnRaised = true;
        RaiseOnSpawn();
    }
}
