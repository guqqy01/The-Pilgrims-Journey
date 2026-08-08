using GameManagerCalls;
using Helper.Wait;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Heros;
namespace Attacks
{
    public class HeroAttacksManager : Singleton<HeroAttacksManager>, IOnClear
    {
        private Hero ActiveHero => HeroManager.Instance.ActiveHero;

        public string SaveFileName => "HeroAttackManager";

        [SerializeField]
        private Dictionary<BaseAttackSO, BaseAttack> attacks = new Dictionary<BaseAttackSO, BaseAttack>();

        public delegate void OnAddHeroAttack(BaseAttack heroAttack);
        public OnAddHeroAttack onAddHeroAttack;

        public delegate void OnRemoveHeroAttack(BaseAttack heroAttack);
        public OnAddHeroAttack onRemoveHeroAttack;

        public delegate void OnHeroAttackLevelChange(BaseAttack heroAttack);
        public OnHeroAttackLevelChange onHeroAttackLevelChange;

        private bool loading = false;

        private void Update()
        {
            if(attacks.Count != 0)
                return;
        }

        private void OnDestroy()
        {
            onAddHeroAttack = null;
            onRemoveHeroAttack = null;
            onHeroAttackLevelChange = null;
        }

        public bool HasHeroAttack(BaseAttackSO heroAttackSO)
        {
            return attacks.ContainsKey(heroAttackSO);
        }

        public void AddHeroAttack(BaseAttackSO heroAttackSO)
        {
            // If hero or stats are not ready, defer adding until they are
            if (HeroManager.Instance == null || HeroManager.Instance.ActiveHero == null || HeroManager.Instance.Stats == null)
            {
                Debug.Log($"HeroAttacksManager.AddHeroAttack: delaying AddHeroAttack for {heroAttackSO.name} until hero and stats ready");
                WaitForHelper.WaitForConditionThenPerformAction(() => HeroManager.Instance != null && HeroManager.Instance.ActiveHero != null && HeroManager.Instance.Stats != null,
                                                                 () => AddHeroAttack(heroAttackSO));
                return;
            }

            if (attacks.ContainsKey(heroAttackSO))
                return;

            if (!typeof(BaseAttack).IsAssignableFrom(heroAttackSO.HeroAttackType))
            {
                Debug.LogError($"Type {heroAttackSO.HeroAttackType} is not a HeroAttack!");
                return;
            }

            var attack = (BaseAttack)ActiveHero.gameObject.AddComponent(heroAttackSO.HeroAttackType);

            // Guard: if Stats or HeroSO missing, schedule setup after they become available
            if (HeroManager.Instance == null || HeroManager.Instance.HeroSO == null || HeroManager.Instance.Stats == null)
            {
                Debug.LogWarning($"HeroAttacksManager.AddHeroAttack: Deferring Setup for {heroAttackSO.name} because HeroSO/Stats not ready");
                WaitForHelper.WaitForConditionThenPerformAction(() => HeroManager.Instance != null && HeroManager.Instance.HeroSO != null && HeroManager.Instance.Stats != null,
                                                                 () => attack.Setup(HeroManager.Instance.HeroSO, heroAttackSO));
            }
            else
            {
                attack.Setup(HeroManager.Instance.HeroSO, heroAttackSO);
            }

            attacks.Add(heroAttackSO, attack);

            onAddHeroAttack?.Invoke(attack);
        }

        

        public void RemoveHeroAttack(BaseAttackSO heroAttackSO)
        {
            if (attacks.TryGetValue(heroAttackSO, out BaseAttack heroAttack))
            {
                onRemoveHeroAttack?.Invoke(heroAttack);

                Destroy(heroAttack);

                attacks.Remove(heroAttackSO);
            }
        }
        
        public int GetHeroAttackLevel(BaseAttackSO heroAttackSO)
        {
            if (attacks.TryGetValue(heroAttackSO, out BaseAttack heroAttack))
            {
                return heroAttack.Level;
            }
            return 0;
        }

        public void LevelUpOrAddHeroAttack(BaseAttackSO heroAttackSO)
        {
            int heroAttackLevel = GetHeroAttackLevel(heroAttackSO);

            if (heroAttackLevel == 0)
            {
                AddHeroAttack(heroAttackSO);
            }
            else
            {
                SetHeroAttackLevel(heroAttackSO, heroAttackLevel + 1);
            }
        }

        public void SetHeroAttackLevel(BaseAttackSO heroAttackSO, int level)
        {
            if(!attacks.ContainsKey(heroAttackSO))
            {
                AddHeroAttack(heroAttackSO);
            }

            if (attacks.TryGetValue(heroAttackSO, out BaseAttack heroAttack))
            {
                heroAttack.SetAttackLevel(level);

                onHeroAttackLevelChange?.Invoke(heroAttack);
            }
        }

        public void Save()
        {
            if (loading)
                return;

            SaveManager.IfFileExistsDelete(SaveFileName);
            SaveData saveData = new SaveData(attacks.Values.ToList());
            SaveProfile<SaveData> saveProfile = new SaveProfile<SaveData>(SaveFileName, saveData);
            SaveManager.SaveOver(saveProfile);
        }

        public void Load()
        {
            if (SaveManager.TryLoad(SaveFileName, out SaveProfile<SaveData> outSaveProfile))
            {
                loading = true;

                SaveData saveData = outSaveProfile.saveData;

                List<HeroAttackInfo> heroAttackInfo = saveData.heroAttackData;

                // Wait for hero instance AND stats to be ready before applying saved attacks
                WaitForHelper.WaitForConditionThenPerformAction(() => HeroManager.Instance != null && 
                                                                    HeroManager.Instance.ActiveHero != null && 
                                                                    HeroManager.Instance.ActiveHero.gameObject != null &&
                                                                    HeroManager.Instance.Stats != null,
                                                                        () => LoadAfterHeroExists(heroAttackInfo));
            }
            else
            {
                Debug.LogWarning($"No save data found for {SaveFileName}");
                return;
            }
        }

        private void LoadAfterHeroExists(List<HeroAttackInfo> heroAttackInfo)
        {
            if (HeroManager.Instance.ActiveHero == null)
            {
                Debug.Log("It still null even after all that");
                return;
            }

            foreach (HeroAttackInfo info in heroAttackInfo)
            {
                SetHeroAttackLevel(info.HeroAttackSO, info.Level);
            }

            loading = false;
        }

        public void Clear()
        {
            List<BaseAttackSO> heroAttackSOs = attacks.Keys.ToList();
            foreach (BaseAttackSO heroAttackSO in heroAttackSOs)
            {
                RemoveHeroAttack(heroAttackSO);
            }
        }

        [Serializable]
        public class SaveData : SaveProfileData
        {
            public List<HeroAttackInfo> heroAttackData;

            public SaveData(List<BaseAttack> heroAttacks)
            {
                heroAttackData = heroAttacks.Select(ha => new HeroAttackInfo(ha.BaseAttackSO, ha.Level)).ToList();
            }
        }
    }

    [Serializable]
    public class HeroAttackInfo
    {
        public BaseAttackSO HeroAttackSO;
        public int Level;
        public HeroAttackInfo(BaseAttackSO heroAttackSO, int level)
        {
            HeroAttackSO = heroAttackSO;
            Level = level;
        }
    }
}

