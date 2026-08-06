using UnityEngine;
using Units;
using Monsters;
using Heros.Attacks;

public class MeleeAttackSpawn : HeroAttackSpawn<HeroAttackInitContext, AttackStatsContext>
{
    // Kept simple: the base class handles stats, audio and lifecycle.

    public void Init(int damage, float criticalChance, float criticalDamageMultiplier, bool destroyProjectiles, float knockbackMultiplier = 0f)
    {
        // For melee swipes we usually want a single instant hit.
        var statsContext = new AttackStatsContext(damage, criticalChance, criticalDamageMultiplier, knockbackMultiplier, 0f, destroyProjectiles, 1, false);
        Init(new HeroAttackInitContext(), statsContext);
    }

    protected override void FixedUpdateMovement()
    {
        // Melee swipe is stationary; no movement required here.
    }

    protected override void InheritInit(HeroAttackInitContext initContext)
    {
        // No specialized init required for melee swipe.
    }

    protected override void InheritSetupStats(AttackStatsContext statsContext)
    {
        // Base.SetupStats already applied the stats; nothing additional here.
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        // Allow base to handle projectile destruction and hit counting, then apply immediate damage for melee on enter.
        base.OnTriggerEnter2D(collision);

        // Apply damage immediately on trigger enter (so melee is responsive)
        HandleColliderHits(collision);
    }
}
