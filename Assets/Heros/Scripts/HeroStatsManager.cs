using UnityEngine;

namespace Heros
{
    
    public class HeroStatsManager : Singleton<HeroStatsManager>
    {
        [Header("Stats")]
        public Buff health;
        public Buff shield;
        public Buff shieldCooldown;
        public Buff shieldRegeneration;

        public Buff movementSpeed;


        public Buff damageBuff;
        public Buff attackSpeed;

        public Buff numberOfAttacks;
        public Buff criticalChance;
        public Buff criticalDamage;
        public Buff attackSize;

        [Header("Other")]
        public Hero hero;


    }
}

