using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tiles;
using static GameManager;

namespace Relics
{
    public class RelicManager : MonoBehaviour, ISaveLoad, ISingletonToBeInitiatedFromRoot
    {
        public string SaveFileName => "RelicManagerSave";

        public static RelicManager Instance { private set; get; }

        private List<Relic> relics = new List<Relic>();
        public List<Relic> Relics => relics;
        private List<Relic> relicsToRemove = new List<Relic>();

        public List<RelicSO> allRelics;
        [SerializeField]
        private List<RelicSO> startRelics;

        public delegate void UpdateUI();
        public UpdateUI updateUI;

        public void InitiateSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }

            if (TilePlayerManager.Instance != null)
                TilePlayerManager.Instance.onPlay += OnPlayRelicEffects;

            onCompleteLevel += AfterPlayRelicEffects;
            onSave += Save;
            onLoad += Load;
            onDelete += Delete;
            onSetup += Setup;
            onClear += Clear;
        }

        public void AfterPlayRelicEffects()
        {
            foreach (Relic relic in relics)
            {
                foreach (RelicEffect effect in relic.relicEffects)
                {
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

        public void AddRelic(RelicSO relicSO)
        {
            Relic relic = new Relic(relicSO);
            relics.Add(relic);

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

        public void Setup()
        {
            foreach(RelicSO relicSO in startRelics)
            {
                AddRelic(relicSO);
            }
        }

        private void Delete()
        {
            SaveManager.IfFileExistsDelete(SaveFileName);
        }

        public void Save()
        {
            SaveManager.IfFileExistsDelete(SaveFileName);
            SaveData saveData = new SaveData(relics);
            SaveProfile<SaveData> saveProfile = new SaveProfile<SaveData>(SaveFileName, saveData);
            SaveManager.Save(saveProfile);
        }

        public void Load()
        {
            SaveData saveData = SaveManager.Load<SaveData>(SaveFileName).saveData;
            relics = saveData.relics;

            updateUI?.Invoke();
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