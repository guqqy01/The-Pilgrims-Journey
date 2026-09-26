using System;
using System.Collections.Generic;
using TPJ.Gameplay;
using UnityEngine;
namespace Attacks
{
    public abstract class BaseAttackSO : ScriptableObject, ICardable, ITileCardType, IDescriptable
    {
        public abstract Type AttackType { get; }
        public abstract int MaxLevel { get; }

        public Sprite icon;

        [TextArea]
        public string description;

        public float findTargetDistance;

        public abstract Buff GetBuff(StatTypes heroStats, int level);

        public abstract AttackSpawnBase AttackSpawn { get; }

        [SerializeField]
        private int actionPointCost;
        public int ActionPointCost => actionPointCost;

        [SerializeField]
        private TileCardType tileCardType;
        public TileCardType TileCardType => tileCardType;

        public string Title => name;

        public string MainDescription => description;

        public string ShortDescription => description;

        public Sprite Sprite => icon;

        public List<ISubDescriptionData> GetSubDescriptionData => new List<ISubDescriptionData>();

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

                fromBuff ??= new Buff(BuffType.Additional | BuffType.Multiplier | BuffType.Base, isInt);
                toBuff ??= new Buff(BuffType.Additional | BuffType.Multiplier | BuffType.Base, isInt);

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

        public CardBase CreateCard()
        {
            return new AttackCard(this);
        }
    }
}

