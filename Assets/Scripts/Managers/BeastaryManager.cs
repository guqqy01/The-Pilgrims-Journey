using GameManagerCalls;
using Map;
using SimpleAudioManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Units;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace Monsters
{
    public class BeastaryManager : MonoBehaviour, ISaveLoad, IOnCompletedLevel
    {
        public static BeastaryManager Instance { private set; get; }

        [SerializeField]
        private List<MonsterSO> monsters;
        public List<MonsterSO> Monsters => monsters;

        public string SaveFileName => "BeastaryManagerSave";

        public SerializableDictionary<MonsterSO, BeastaryItem> beastaryItems = new SerializableDictionary<MonsterSO, BeastaryItem>();

        public Dictionary<MonsterSO, int> monstersInLevelBuffer = new Dictionary<MonsterSO, int>();

        public int[] NumberOfEncountedMonstersToUpgradeLockState = new int[4] { 1, 1, 1, 1 };

        public delegate void UpdateUI();
        public UpdateUI updateUI;

        private void Awake()
        {
            Instance = this;

            beastaryItems = new SerializableDictionary<MonsterSO, BeastaryItem>();

            for (int i = 0; i < monsters.Count; i++)
            {
                beastaryItems.Add(monsters[i], new BeastaryItem(monsters[i]));
            }
        }

        public BeastaryItem GetBeasaryItemFromMonstersIndex(int index)
        {
            return beastaryItems[Monsters[index]];
        }

        public void BufferMonster(MonsterSO monster)
        {
            if (monstersInLevelBuffer.ContainsKey(monster))
            {
                monstersInLevelBuffer[monster]++;
            }
            else
            {
                monstersInLevelBuffer.Add(monster, 1);
            }
        }

        public void Unitdamaged(MonsterSO monster, OnHitInfo hitInfo)
        {
            if (monster == null)
                return;

            if (hitInfo.unit == null)
                return;


            if (beastaryItems.ContainsKey(monster))
                beastaryItems[monster].Unitdamaged(hitInfo.unit.UnitSO, hitInfo.TotalDamage);
            else


            if (hitInfo.killed)
            {
                beastaryItems[monster].UnitKilled(hitInfo.unit.UnitSO);
            }
        }

        public void ReleaseBuffer()
        {
            monstersInLevelBuffer.Clear();
        }

        public void Save()
        {
            SaveManager.IfFileExistsDelete(SaveFileName);
            SaveData saveData = new SaveData(beastaryItems);
            SaveProfile<SaveData> saveProfile = new SaveProfile<SaveData>(SaveFileName, saveData);
            SaveManager.Save(saveProfile);
        }

        public void Load()
        {
            if (SaveManager.TryLoad(SaveFileName, out SaveProfile<SaveData> saveProfile))
            {
                beastaryItems = saveProfile.saveData.beastaryItems;

                updateUI?.Invoke();
            }
        }

        public void OnLevelCompleted()
        {
            foreach (KeyValuePair<MonsterSO, int> kvp in monstersInLevelBuffer)
            {
                beastaryItems[kvp.Key].encountered += kvp.Value;
            }

            updateUI?.Invoke();
        }

        [Serializable]
        private record SaveData : SaveProfileData
        {
            public SerializableDictionary<MonsterSO, BeastaryItem> beastaryItems;

            public SaveData(SerializableDictionary<MonsterSO, BeastaryItem> beastaryItems)
            {
                this.beastaryItems = beastaryItems;
            }
        }


    }


}