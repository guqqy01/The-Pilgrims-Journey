using System;
using System.Collections.Generic;
using UnityEngine;
using Heros;
namespace Attacks.Melee
{
    [CreateAssetMenu(menuName = "Attack/Melee")]
    public class MeleeAttackSO : AttackSO<MeleeAttackSpawn, BaseWeaponStatsAtLevel>
    {
        public override Type AttackType => typeof(MeleeAttack);

        public bool destroyProjectiles;

        public override Buff GetBuffAtIndex(StatTypes heroStats, int index)
        {
            return null;
        }
    }
}

