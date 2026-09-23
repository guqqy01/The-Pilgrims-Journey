using System;
using UnityEngine;
using UnityEngine.U2D.Animation;
using Attacks;
using System.Collections.Generic;
using Equipment;
using Tiles;
using TPJ.Gameplay;

namespace Heros
{
    [CreateAssetMenu()]
    public class HeroSO : BuffableAttackEntitySO, ITileable, IDescriptable, ITileCardType
    {
        [TextArea()]
        public string Description;

        public Sprite icon;

        public Hero prefab;

        [Header("Hero Only Stats")]
        [BuffType(false)]
        public Buff Luck;
        [BuffType(false)]
        public Buff ExperienceGain;
        [Space]
        public BaseAttackSO startingAttack;

        public SpriteLibraryAsset spriteLibrary;

        public List<EquipmentSO> startingEquipment;

        public Sprite Sprite => icon;

        public string Title => name;

        public string MainDescription => Description;
        public string ShortDescription => Description;

        public List<ISubDescriptionData> GetSubDescriptionData => null;

        private TileCardType tileCardType;
        public TileCardType TileCardType => tileCardType;

        protected override Buff InheritGetBuff(StatTypes stat)
        {
            return stat switch
            {
                StatTypes.Luck => Luck,
                StatTypes.ExperienceGain => ExperienceGain,
                _ => base.InheritGetBuff(stat),
            };
        }

        public string GetHeroStatsString()
        {
            string str = "";
            foreach (StatTypes stat in Enum.GetValues(typeof(StatTypes)))
            {
                Buff buff = GetBuff(stat);

                if (buff == null)
                    continue;

                string buffDescription;

                if (stat == StatTypes.Health || 
                    stat == StatTypes.Shield || 
                    stat == StatTypes.MovementSpeed || 
                    stat == StatTypes.ShieldCooldown ||
                    stat == StatTypes.ShieldRegeneration)
                {
                    buffDescription = buff.GetSimpleDescription();
                }
                else
                {
                    buffDescription = buff.GetSignedSimpleDescription();
                }

                if (String.IsNullOrEmpty(buffDescription))
                    continue;

                string buffStr = stat.ToString().AddSpacesBeforeCapsRegex() + ": " + buffDescription;

                buffStr.AppendLineToTarget(ref str);
            }

            return str;
        }
    }
}
