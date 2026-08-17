using GameManagerCalls;
using Helper.Wait;
using Heros;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Attacks
{
    public class HeroAttacksManager : AttacksManager, IOnClear, IToBeInitiatedFromRoot
    {
        public string SaveFileName => "HeroAttackManager";

        private bool loading = false;

        private static readonly SingletonHelper<HeroAttacksManager> _singleton = new();
        public static HeroAttacksManager Instance => _singleton.Instance;
        public static bool HasInstance => _singleton.HasInstance;

        public static void WhenReady(Action<HeroAttacksManager> onReady) => _singleton.WhenReady(onReady);

        public void InitiateFromRootAtAwake()
        {
            if (!_singleton.TryClaim(this, () => Destroy(gameObject)))
                return;
        }

        protected override GameObject GetGameObjectToHaveAttack()
        {
            return HeroManager.Instance.ActiveHero.gameObject;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _singleton.Release(this);
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
                SetAttackLevel(info.HeroAttackSO, info.Level);
            }

            loading = false;
        }

        public void Clear()
        {
            List<BaseAttackSO> heroAttackSOs = attacks.Keys.ToList();
            foreach (BaseAttackSO heroAttackSO in heroAttackSOs)
            {
                RemoveAttack(heroAttackSO);
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

