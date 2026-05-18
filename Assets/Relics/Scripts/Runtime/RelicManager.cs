using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tiles;
using static GameManager;
using GameManagerCalls;
using System.Linq;

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

        public override void InitiateFromRootAtAwake()
        {
            base.InitiateFromRootAtAwake();

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
                        effect.AfterPlayEffect(relic);
                }
            }

            foreach(Relic relic in relics)
            {
                foreach (RelicEffect effect in relic.relicEffects)
                {
                    effect.Reset(relic);
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
                    effect.OnPlayEffect(relic);
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
            foreach(RelicEffect effect in relic.relicEffects)
            {
                effect.OnRemoveRelic(relic);
            }

            relics.Remove(relic);
            

            updateUI?.Invoke();
        }

        public void Clear()
        {
            relics.Clear();

            updateUI?.Invoke();
        }

        public void OnSetupNewGame()
        {
            foreach(RelicSO relicSO in startRelics)
            {
                AddRelic(relicSO);
            }
        }

        public void Delete()
        {
            SaveManager.IfFileExistsDelete(SaveFileName);
        }

        public void Save()
        {
            SaveManager.IfFileExistsDelete(SaveFileName);
            SaveData saveData = new SaveData(relics.Select(x => new RelicSave(x.relicSO, x.Level)).ToList());
            SaveProfile<SaveData> saveProfile = new SaveProfile<SaveData>(SaveFileName, saveData);
            SaveManager.SaveOver(saveProfile);
        }

        public void LoadAfterMapPointSceneLoad()
        {
            if(SaveManager.TryLoad(SaveFileName, out SaveProfile<SaveData> outSaveProfile))
            {
                SaveData saveData = outSaveProfile.saveData;

                List<RelicSave> relicsSaves = saveData.relicSaves;

                if(relicsSaves == null)
                {
                    Debug.LogError($"Relic manager Relic Saves null, load failed");
                    return;
                }

                foreach(RelicSave relicSave in relicsSaves)
                {
                    AddRelic(relicSave.relicSO, relicSave.level);
                }

                updateUI?.Invoke();
            }
            else
            {
                Debug.LogWarning($"Relic Manager save failed");
            }
        }


        [Serializable]
        private class SaveData : SaveProfileData
        {
            public List<RelicSave> relicSaves;

            public SaveData(List<RelicSave> relics)
            {
                this.relicSaves = relics;
            }
        }

        [Serializable]
        private class RelicSave 
        {
            public RelicSO relicSO;
            public int level;

            public RelicSave(RelicSO relicSO, int level)
            {
                this.relicSO = relicSO;
                this.level = level;
            }
        }
    }
}