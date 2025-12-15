using UnityEngine;
using System;

namespace Heros
{
    public abstract class HeroAttackSO : ScriptableObject
    {
        public abstract Type HeroAttackType { get; }

        public Sprite icon;

        [TextArea]
        public string description;

        [BuffType(true)]
        public Buff damage;
        [BuffType(false)]
        public Buff attackSpeed;
        [BuffType(false)]
        [Tooltip("critical percentage, value 100 => 100% chance to crit")]
        public Buff criticalChance;
        [BuffType(true)]
        [Tooltip("critical damage multiplier, value 100 => +100% damage increase")]
        public Buff criticalDamageMultiplier;

        [BuffType(false)]
        public Buff knockback;

        [BuffType(false)]
        public Buff size;

        public float findTargetDistance;

        public LayerMask enemyLayerMask;
    }
}

