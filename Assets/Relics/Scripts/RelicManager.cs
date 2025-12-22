using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tiles;
using static GameManager;
using GameManagerCalls;

namespace Relics
{
    public class RelicManager : Singleton<RelicManager>, ISaveLoad, IOnCompletedLevel, IOnSave, IOnLoadAfterMapPointSceneLoad, IOnDelete, IOnSetupNewGame, IOnClear
    {
        public string SaveFileName => "RelicManagerSave";


        private List<Relic> relics = new List<Relic>();
        public List<Relic> Relics => relics;
        private readonly List<Relic> relicsToRemove = new List<Relic>();

        [SerializeField]
        private List<RelicSO> startRelics;

        public delegate void UpdateUI();
        public UpdateUI updateUI;

        public override void InitiateSingleton()
        {
            base.InitiateSingleton();

            if (TilePlayerManager.Instance != null)
                TilePlayerManager.Instance.onPlay += OnPlayRelicEffects;
        }

        public bool TryGetRelic(RelicSO relicSO, out Relic relic)
        {
            foreach(Relic iteratingRelic in Relics)
            {
                if(iteratingRelic.relicSO == relicSO)
                {
                    relic = iteratingRelic;
                    return true;
                }
            }

            relic = null;
            return false;
        }

        public void OnLevelCompleted ()
        {
            foreach (Relic relic in relics)
            {
                foreach (RelicEffect effect in relic.relicEffects)
                {
                    if(effect != null)
                        effect.AfterPlayEffect();
                }
            }

            foreach(Relic relic in relics)
            {
                foreach (RelicEffect effect in relic.relicEffects)
                {
                    effect.Reset();
                }
            }

            if (relicsToRemove.Count <= 0)
                return;

            foreach(Relic relic in relicsToRemove)
            {
                relics.Remove(relic);
            }

            relicsToRemove.Clear();

            updateUI?.Invoke();
        }

        public void OnPlayRelicEffects()
        {
            foreach (Relic relic in relics)
            {
                foreach (RelicEffect effect in relic.relicEffects)
                {
                    effect.OnPlayEffect();
                }
            }
        }

        public void AddRelic(RelicSO relicSO, int level = 1)
        {
            bool relicAlreadyInManager = false;
            foreach(Relic relic in Relics)
            {
                if(relic.relicSO == relicSO)
                {
                    relic.Level += level;
                    relicAlreadyInManager = true;
                    break;
                }
            }
            
            if (!relicAlreadyInManager)
            {
                Relic newRelic = new Relic(relicSO, level);
                relics.Add(newRelic);
            }
            
            updateUI?.Invoke();
        }

        public void RemoveRelicAfterRound(Relic relic)
        {
            relicsToRemove.Add(relic);
        }

        public void RemoveRelic(Relic relic)
        {
            relics.Remove(relic);
            
            updateUI?.Invoke();
        }

        public void Clear()
        {
            relics.Clear();

            updateUI?.Invoke();
        }

        public void SetupNewGame()
        {
            foreach(RelicSO relicSO in startRelics)
            {
                AddRelic(relicSO);
            }
        }

        public void SetupContinueGameAfterMapPointLoad()
        {
            
        }

        public void Delete()
        {
            SaveManager.IfFileExistsDelete(SaveFileName);
        }

        public void Save()
        {
            SaveManager.IfFileExistsDelete(SaveFileName);
            SaveData saveData = new SaveData(relics);
            SaveProfile<SaveData> saveProfile = new SaveProfile<SaveData>(SaveFileName, saveData);
            SaveManager.SaveOver(saveProfile);
        }

        public void LoadAfterMapPointSceneLoad()
        {
            if(SaveManager.TryLoad(SaveFileName, out SaveProfile<SaveData> outSaveProfile))
            {
                SaveData saveData = outSaveProfile.saveData;

                relics = saveData.relics;

                foreach(Relic relic in relics)
                {
                    relic.Init(relic.relicSO, relic.Level);
                }

                updateUI?.Invoke();
            }
        }

       

        [Serializable]
        private record SaveData : SaveProfileData
        {
            public List<Relic> relics;

            public SaveData(List<Relic> relics)
            {
                this.relics = relics;
            }
        }
    }
}