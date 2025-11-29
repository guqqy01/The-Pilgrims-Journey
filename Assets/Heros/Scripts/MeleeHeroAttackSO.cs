using UnityEngine;
using System;

namespace Heros
{
    [CreateAssetMenu(menuName = "Hero Attack/Melee")]
    public class MeleeHeroAttackSO : HeroAttackSO
    {
        public override Type HeroAttackType => typeof(MeleeHeroAttack);

        public MeleeAttackSpawn meleeAttackSpawn;
    }
}

