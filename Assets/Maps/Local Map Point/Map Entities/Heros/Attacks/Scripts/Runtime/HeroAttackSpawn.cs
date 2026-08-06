using UnityEngine;

namespace Heros.Attacks
{
    // Backwards-compatible wrapper: specific hero contexts remain supported by deriving from the generalized AttackSpawn
    public abstract class HeroAttackSpawn<TInitContext, TStatsContext> : AttackSpawn<TInitContext, TStatsContext>
        where TInitContext : HeroAttackInitContext
        where TStatsContext : AttackStatsContext
    {
    }
}