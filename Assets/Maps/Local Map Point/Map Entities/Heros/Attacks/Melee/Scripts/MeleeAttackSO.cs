using System;
using System.Collections.Generic;
using UnityEngine;
using Heros;
namespace Attacks.Melee
{
    [CreateAssetMenu(menuName = "Attack/Melee")]
    public class MeleeAttackSO : AttackSO<BaseWeaponStatsAtLevel>
    {
        public override Type HeroAttackType => typeof(MeleeAttack);

        public MeleeAttackSpawn meleeAttackSpawn;

        public bool destroyProjectiles;

        public override Buff GetBuffAtIndex(StatTypes heroStats, int index)
        {
            return null;
        }
    }
}

