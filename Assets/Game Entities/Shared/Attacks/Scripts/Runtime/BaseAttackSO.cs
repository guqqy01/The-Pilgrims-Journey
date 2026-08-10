using System;
using UnityEngine;
namespace Attacks
{
    public abstract class BaseAttackSO : ScriptableObject
    {
        public abstract Type AttackType { get; }
        public abstract int MaxLevel { get; }

        public Sprite icon;

        [TextArea]
        public string description;

        public float findTargetDistance;

        public abstract Buff GetBuff(StatTypes heroStats, int level);

        public abstract AttackSpawnBase AttackSpawn { get; }


        public string GetStatsDescription(int level)
        {
            string str = "";
            foreach (StatTypes stat in Enum.GetValues(typeof(StatTypes)))
            {
                Buff buff = GetBuff(stat, level);

                if (buff == null)
                    continue;

                string buffDescription = buff.GetSimpleDescription();

                if (String.IsNullOrEmpty(buffDescription))
                    continue;

                string buffStr = stat.ToString().AddSpacesBeforeCapsRegex() + ": " + buffDescription;

                buffStr.AppendLineToTarget(ref str);
            }

            return str;
        }

        public string GetComparedStatsDescription(int fromLevel, int toLevel)
        {
            string str = "";

            foreach (StatTypes stat in Enum.GetValues(typeof(StatTypes)))
            {
                Buff fromBuff = GetBuff(stat, fromLevel);
                Buff toBuff = GetBuff(stat, toLevel);

                if (fromBuff == null && toBuff == null)
                    continue;

                bool isInt = fromBuff?.IsInt ?? toBuff?.IsInt ?? false;

                fromBuff ??= new Buff(isInt);
                toBuff ??= new Buff(isInt);

                string buffDescription = fromLevel == 0 ? toBuff.GetSignedSimpleDescription() : Buff.SimpleCompareString(fromBuff, toBuff);

                if (string.IsNullOrEmpty(buffDescription))
                    continue;

                string buffStr =
                    stat.ToString().AddSpacesBeforeCapsRegex() +
                    ": " +
                    buffDescription;

                buffStr.AppendLineToTarget(ref str);
            }

            return str;
        }
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

