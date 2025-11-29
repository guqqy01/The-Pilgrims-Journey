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

        public int damage;
        public float attackSpeed;
        public float knockback;
        public float findTargetDistance;

        public LayerMask enemyLayerMask;
    }
}

