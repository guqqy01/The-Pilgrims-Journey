using System;
using System.Collections.Generic;
using UnityEngine;

namespace Heros.Attacks.Melee
{
    [CreateAssetMenu(menuName = "Hero Attack/Melee")]
    public class MeleeHeroAttackSO : HeroAttackSO<BaseWeaponStatsAtLevel>
    {
        public override Type HeroAttackType => typeof(MeleeHeroAttack);

        public MeleeAttackSpawn meleeAttackSpawn;

        public bool destroyProjectiles;

        public override Buff GetBuffAtIndex(HeroStats heroStats, int index)
        {
            return null;
        }
    }
}

