using UnityEngine;
using System.Collections.Generic;
using Relics;
using Unity.VisualScripting;
using System.Linq;
using Attacks;

namespace LevelingSystem
{
    public class LevelingUpRewardManager : Singleton<LevelingUpRewardManager>
    {
        public delegate void OnShowUI(Dictionary<ILevelingUpReward, int> rewards);
        public OnShowUI onShowUI;

        [SerializeField] private WeightedChanceList<RelicSO> unitStatRelics;
        [SerializeField] private WeightedChanceList<RelicSO> heroStatRelics;
        [SerializeField] private WeightedChanceList<BaseAttackSO> heroWeaponRelicsTest;

        private int unitWeight = 1;
        private int heroWeight = 0;
        private int weaponWeight = 1;

        public Dictionary<ILevelingUpReward, int> Rewards { get; private set; } = new();

        private const string pauseReference = "LevelingUpReward";

        protected override void CleanUp()
        {
            onShowUI = null;
        }

        private enum RewardPool
        {
            Unit,
            Hero,
            Weapon
        }

        private RewardPool GetRandomPool()
        {
            WeightedChanceList<RewardPool> pools = new();
            pools.Add(RewardPool.Unit, unitWeight);
            pools.Add(RewardPool.Hero, heroWeight);
            pools.Add(RewardPool.Weapon, weaponWeight);

            RewardPool selected = pools.GetRandom();

            if (selected == RewardPool.Unit) unitWeight = 0;
            if (selected == RewardPool.Hero) heroWeight = 0;
            if (selected == RewardPool.Weapon) weaponWeight = 0;

            unitWeight++;
            //heroWeight++;
            weaponWeight++;

            return selected;
        }

        public void SetupRewardChoices()
        {
            Rewards.Clear();

            RewardPool pool = GetRandomPool();
            int breaker = 100;
            while (!IsValidPool(pool) && breaker > 0)
            {
                pool = GetRandomPool();
                breaker--;
            }

            if (!IsValidPool(pool))
                return;

            breaker = 100;

            while (Rewards.Count < OverallStatsManager.Instance.AmountOfRelicToChooseFrom && breaker > 0)
            {
                ILevelingUpReward reward = pool switch
                {
                    RewardPool.Unit => new RelicReward(unitStatRelics.GetRandom()),
                    RewardPool.Hero => new RelicReward(heroStatRelics.GetRandom()),
                    RewardPool.Weapon => new HeroWeaponReward(heroWeaponRelicsTest.GetRandom()),
                    _ => null
                };

                breaker--;

                if (!IsRewardValid(reward))
                    continue;

                int amount = pool == RewardPool.Weapon ? 1 : GetAmountOfLevelsGained();
                Rewards.Add(reward, amount);
            }

            onShowUI?.Invoke(Rewards);
            TimeManager.Instance.PauseTime(pauseReference);
        }

        private bool IsValidPool(RewardPool pool)
        {
            List<ILevelingUpReward> rewardPool  = pool switch
            {
                RewardPool.Unit => unitStatRelics.weightedList.Select(x => new RelicReward(x.item) as ILevelingUpReward).ToList(),
                RewardPool.Hero => heroStatRelics.weightedList.Select(x => new RelicReward(x.item) as ILevelingUpReward).ToList(),
                RewardPool.Weapon => heroWeaponRelicsTest.weightedList.Select(x => new HeroWeaponReward(x.item) as ILevelingUpReward).ToList(),
                _ => null
            };

            bool hasValidReward = false;

            foreach (ILevelingUpReward reward in rewardPool)
            {
                if (IsRewardValid(reward))
                {
                    hasValidReward = true;
                }
            }

            return hasValidReward;
        }

        private bool IsRewardValid(ILevelingUpReward reward)
        {
            if(reward == null)
            {
                return false;
            }

            if (Rewards.Keys.Select(x => x.Source).Contains(reward.Source))
            {
                return false;
            }

            if(reward is HeroWeaponReward weaponReward)
            {
                int currentWeaponLevel = HeroAttacksManager.Instance.GetAttackLevel(weaponReward.AttackSO);
                if (currentWeaponLevel >= weaponReward.AttackSO.MaxLevel)
                    return false;
            }

            return true;
        }

        public void SelectReward(ILevelingUpReward reward)
        {
            reward.Apply(Rewards[reward]);
            TimeManager.Instance.RemovePauseRequest(pauseReference);
        }
        private int GetAmountOfLevelsGained()
        {
            //luck is where on the xAxis the standard deviation curve lies, its it Mathf.Log10'd to stop exponential boosts.
            float luck = Mathf.Log10(Mathf.Max(OverallStatsManager.Instance.Luck, 0) + 1);

            //Standard deviation on the curve
            float standardDiv = 2f;

            //random position on the curve, within 0 and standardDiv * (Sqrt(2 * PI)).
            float y = Random.Range(Mathf.Epsilon, (1f / (standardDiv * Mathf.Sqrt(2 * Mathf.PI))) - Mathf.Epsilon);

            //Calculate one side of the curve;
            float standardDivCurve = standardDiv * (Mathf.Sqrt(2f)) * Mathf.Sqrt(Mathf.Log10(1f / (standardDiv * Mathf.Sqrt(2 * Mathf.PI) * y)));

            //boolean of wether to sample sample from the left or right side of the curve.
            int rightSide = Random.Range(0, 2);

            int value;

            if (rightSide >= 1)
            {
                int rightSideOfCurve = Mathf.RoundToInt(luck + standardDivCurve);
                value = rightSideOfCurve;
            }
            else
            {
                int leftSideOfCurve = Mathf.RoundToInt(luck - standardDivCurve);
                value = leftSideOfCurve;
            }

            //Return at least one level increase.
            return Mathf.Max(value, 1);
        }
    }

    public class HeroWeaponReward : ILevelingUpReward
    {
        public BaseAttackSO AttackSO { get; }
        public ScriptableObject Source => AttackSO;
        public Sprite Sprite => AttackSO.icon;

        public string Name => AttackSO.name;

        public string Description(int levelsIncrease)
        {
            string heroAttackDescription = AttackSO.description;

            if (HeroAttacksManager.Instance == null)
                return heroAttackDescription;

            int currentAttackLevel = HeroAttacksManager.Instance.GetAttackLevel(AttackSO);

            heroAttackDescription = heroAttackDescription.AddStringWithCarriageReturn(GetWeaponAttackStats(currentAttackLevel), 2);

            return heroAttackDescription;
        }

        private string GetWeaponAttackStats(int currentAttackLevel)
        {
            return AttackSO.GetComparedStatsDescription(currentAttackLevel, currentAttackLevel + 1);
        }

        public HeroWeaponReward(BaseAttackSO relicSO)
        {
            AttackSO = relicSO;
        }

        public void Apply(int amount)
        {
            HeroAttacksManager.Instance.LevelUpOrAddAttack(AttackSO);
        }
    }


    public interface ILevelingUpReward
    {
        public ScriptableObject Source { get; }
        public Sprite Sprite { get; }
        public string Name { get; }
        public string Description(int levelsIncrease);
        void Apply(int amount);
    }
}

