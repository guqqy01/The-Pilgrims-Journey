using UnityEngine;
using System.Collections.Generic;
using Relics;
using Heros;
using Unity.VisualScripting;
using System.Linq;
namespace LevelingSystem
{
    public class LevelingUpRewardManager : Singleton<LevelingUpRewardManager>
    {
        public delegate void OnShowUI(Dictionary<ILevelingUpReward, int> rewards);
        public OnShowUI onShowUI;

        [SerializeField] private WeightedChanceList<RelicSO> unitStatRelics;
        [SerializeField] private WeightedChanceList<RelicSO> heroStatRelics;
        [SerializeField] private WeightedChanceList<HeroAttackSO> heroWeaponRelics;

        private int unitWeight = 1;
        private int heroWeight = 0;
        private int weaponWeight = 1;

        public Dictionary<ILevelingUpReward, int> Rewards { get; private set; } = new();

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

        public void SetupRelicsChoices()
        {
            Rewards.Clear();

            RewardPool pool = GetRandomPool();

            int breaker = 100;

            while (Rewards.Count < OverallStatsManager.Instance.AmountOfRelicToChooseFrom && breaker > 0)
            {
                ILevelingUpReward reward = pool switch
                {
                    RewardPool.Unit => new RelicReward(unitStatRelics.GetRandom()),
                    RewardPool.Hero => new RelicReward(heroStatRelics.GetRandom()),
                    RewardPool.Weapon => new HeroWeaponReward(heroWeaponRelics.GetRandom()),
                    _ => null
                };

                breaker--;

                if (!IsRewardValid(reward))
                    continue;

                int amount = pool == RewardPool.Weapon ? 1 : GetAmountOfLevelsGained();
                Rewards.Add(reward, amount);
            }

            onShowUI?.Invoke(Rewards);
            Time.timeScale = 0f;
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
                int currentWeaponLevel = HeroAttacksManager.Instance.GetHeroAttackLevel(weaponReward.HeroAttackSO);
                if (currentWeaponLevel >= weaponReward.HeroAttackSO.MaxLevel)
                    return false;
            }

            return true;
        }

        public void SelectReward(ILevelingUpReward reward)
        {
            reward.Apply(Rewards[reward]);
            Time.timeScale = 1f;
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

    public class RelicReward : ILevelingUpReward
    {
        public RelicSO RelicSO { get; }

        public ScriptableObject Source => RelicSO;

        public RelicReward(RelicSO relicSO)
        {
            RelicSO = relicSO;
        }

        public void Apply(int amount)
        {
            if (RelicManager.Instance.TryGetRelic(RelicSO, out Relic relic))
            {
                relic.Level += amount;
            }
            else
            {
                RelicManager.Instance.AddRelic(RelicSO, amount);
            }
        }
    }

    public class HeroWeaponReward : ILevelingUpReward
    {
        public HeroAttackSO HeroAttackSO { get; }

        public ScriptableObject Source => HeroAttackSO;

        public HeroWeaponReward(HeroAttackSO relicSO)
        {
            HeroAttackSO = relicSO;
        }

        public void Apply(int amount)
        {
            HeroAttacksManager.Instance.LevelUpOrAddHeroAttack(HeroAttackSO);
        }
    }


    public interface ILevelingUpReward
    {
        ScriptableObject Source { get; }
        void Apply(int amount);
    }
}

