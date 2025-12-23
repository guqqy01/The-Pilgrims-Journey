using UnityEngine;
using System;
using NUnit.Framework.Internal;
using System.Collections.Generic;
using UnityEditor;
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

        public Buff GetBuff(HeroStats heroStat)
        {
            return heroStat switch
            {
                HeroStats.Damage => damage,
                HeroStats.AttackSpeed => attackSpeed,
                HeroStats.CriticalChance => criticalChance,
                HeroStats.CriticalDamageMultiplier => criticalDamageMultiplier,
                HeroStats.Knockback => knockback,
                HeroStats.AttackSize => size,
                _ => null,
            };
        }
    }
    [Serializable]
    public class WeaponStats
    {
        public List<WeaponStatsAtLevel> weaponStatsAtLevel = new List<WeaponStatsAtLevel>();
    }

    [Serializable]
    public class ProjectileWeaponStatsAtLevel
    {
        [BuffType(true)]
        public Buff numberOfProjectiles;
        [BuffType(false)]
        public Buff projectileSpeed;
        [BuffType(false)]
        public Buff projectileTurnSpeed;
    }
    [Serializable]
    public class WeaponStatsAtLevel
    {
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
    }

    public class HeroAttackWindow : EditorWindow
    {
        private HeroAttackSO _heroAttakSO;

        [MenuItem("Window/Weapon Stats Window")]
        static void Init()
        {
            HeroAttackWindow window = GetWindow<HeroAttackWindow>();

            window.Show();
        }

        static void Init(HeroAttackSO heroAttakSO)
        {
            HeroAttackWindow window = GetWindow<HeroAttackWindow>();

            window._heroAttakSO = heroAttakSO;
        }

        private void OnGUI()
        {
            
        }
    }
}

