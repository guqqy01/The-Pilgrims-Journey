using System;
using System.Collections.Generic;
using UnityEngine;
using Heros;
namespace Attacks
{
    public abstract class AttackSO<TStatsAtLevel> : BaseAttackSO where TStatsAtLevel : BaseWeaponStatsAtLevel
    {
        public List<TStatsAtLevel> WeaponStatsAtLevel;
        public override int MaxLevel => WeaponStatsAtLevel.Count;

        public AttackSpawnBase attackPrefab;

        public abstract Buff GetBuffAtIndex(StatTypes stat, int index);

        public override Buff GetBuff(StatTypes stat, int level)
        {
            int index = level - 1;

            if (index < 0 || index >= WeaponStatsAtLevel.Count)
                return null;

            Buff buff = stat switch
            {
                StatTypes.Damage => WeaponStatsAtLevel[index].damage,
                StatTypes.AttackSpeed => WeaponStatsAtLevel[index].attackSpeed,
                StatTypes.CriticalChance => WeaponStatsAtLevel[index].criticalChance,
                StatTypes.CriticalDamageMultiplier => WeaponStatsAtLevel[index].criticalDamageMultiplier,
                StatTypes.Knockback => WeaponStatsAtLevel[index].knockback,
                StatTypes.AttackSize => WeaponStatsAtLevel[index].size,
                
                _ => null,
            };

            if (buff != null)
                return buff;

            return GetBuffAtIndex(stat, index);
        }
    }


    [Serializable]
    public class ProjectileWeaponStatsAtLevel : BaseWeaponStatsAtLevel
    {
        [BuffType(true)]
        public Buff numberOfProjectiles;
        [BuffType(false)]
        public Buff projectileSpeed;
        [BuffType(true)]
        public Buff amountOfHits;
        [BuffType(false)]
        public Buff projectileTurnSpeed;
    }

    [Serializable]
    public class BaseWeaponStatsAtLevel
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
        [BuffType(false)]
        public Buff knockback;
        [BuffType(false)]
        public Buff size;
    }

    [Serializable]
    public class OrbitalWeaponStatsAtLevel : BaseWeaponStatsAtLevel
    {
        [BuffType(true)]
        public Buff numberOfOrbitals;
        [BuffType(false)]
        public Buff orbitalSpeed;
    }

    [Serializable]
    public class SpotWeaponStatsAtLevel : BaseWeaponStatsAtLevel
    {
        [BuffType(false)]
        public Buff spawnAccuracy;
        [BuffType(false)]
        public Buff lifeTime;
        [BuffType(false)]
        public Buff hitRefreshRate;
    }
    /*
    [CustomEditor(typeof(HeroAttackSO), true)]
    public class HeroAttackEditor : Editor
    {
        private GUIStyle richLabel;

        private void EnsureStyles()
        {
            if (richLabel == null)
            {
                richLabel = new GUIStyle(EditorStyles.label)
                {
                    richText = true,
                    wordWrap = true
                };
            }
        }

        public override void OnInspectorGUI()
        {
            EnsureStyles();

            HeroAttackSO heroAttackSO = (HeroAttackSO)target;
            DrawDefaultInspector();
            if (heroAttackSO.WeaponStats != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.Space();

                for (int i = 0; i < heroAttackSO.WeaponStats.Count; i++)
                {
                    EditorGUILayout.LabelField($"Level {i + 1} Stats:");
                    BaseWeaponStatsAtLevel stats = heroAttackSO.WeaponStats[i];
                    BaseWeaponStatsAtLevel previousStats = (i>0) ? heroAttackSO.WeaponStats[i - 1] : null;
                    DrawBuff("Damage", stats.damage, previousStats?.damage);
                    DrawBuff("Attack Speed", stats.attackSpeed, previousStats?.attackSpeed);
                    DrawBuff("Critical Chance", stats.criticalChance, previousStats?.criticalChance);
                    DrawBuff("Critical Damage Multiplier", stats.criticalDamageMultiplier, previousStats?.criticalDamageMultiplier);
                    DrawBuff("Knockback", stats.knockback, previousStats?.knockback);
                    DrawBuff("Size", stats.size, previousStats?.size);
                    if (stats is ProjectileWeaponStatsAtLevel projectileStats)
                    {
                        ProjectileWeaponStatsAtLevel previousProjectileStats = null;
                        if(previousStats != null && previousStats is ProjectileWeaponStatsAtLevel)
                        {
                            previousProjectileStats = previousStats as ProjectileWeaponStatsAtLevel;
                        }
                        DrawBuff("Number of Projectiles", projectileStats.numberOfProjectiles, previousProjectileStats?.numberOfProjectiles);
                        DrawBuff("Projectile Speed", projectileStats.projectileSpeed, previousProjectileStats?.projectileSpeed);
                        DrawBuff("Projectile Turn Speed", projectileStats.projectileTurnSpeed, previousProjectileStats?.projectileTurnSpeed);
                    }

                    if(i < heroAttackSO.WeaponStats.Count - 1)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                        EditorGUILayout.Space();
                    }
                }
            }
        }

        public void DrawBuff(string label, Buff buff, Buff previousBuff)
        {
            if (buff == null)
                return;

            EnsureStyles();

            string description;

            if (previousBuff == null)
            {
                // No comparison available (level 1)
                description = buff.GetDescription();
                if (string.IsNullOrEmpty(description))
                    return;

                description = $"<color=#AAAAAA>{description}</color>";
            }
            else
            {
                description = Buff.CompareStringColoredEditor(previousBuff, buff);
                if (string.IsNullOrEmpty(description))
                    return;
            }

            string formatted =
                $"<b>{label}</b>:\n\t{description.Replace("\r\n", "\n\t")}";

            EditorGUILayout.LabelField(formatted, richLabel);
        }


        public int GetAmountOfLines(string str)
        {
            int lineCount = 1;
            foreach (char c in str)
            {
                if (c == '\n')
                {
                    lineCount++;
                }
            }
            return lineCount;
        }
    }*/
}

