using UnityEngine;
using System.Collections.Generic;


namespace Heros
{
    public class HeroAttacksManager : MonoBehaviour
    {
        [SerializeField]
        private List<HeroAttack> attacks;
    }


    public abstract class HeroAttackSO : ScriptableObject
    {
        public int damage;
        public float attackSpeed;
        public float knockback;
        public float findTargetDistance;
    }

    public class MeleeHeroAttackSO : HeroAttackSO
    {
        public MeleeAttackSpawn meleeAttackSpawn;
    }

    public class ProjectileHeroAttackSO : HeroAttackSO
    {
        public Projectile projectilePrefab;

        public float projectileSpeed;

        public bool projectileHoming;
    }
}

