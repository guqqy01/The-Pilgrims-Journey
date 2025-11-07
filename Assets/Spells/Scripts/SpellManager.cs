using GlobalMap;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tiles;
using Unity.VisualScripting;
using GameManagerCalls;
using LocalMap;

namespace Spells
{
    public class SpellManager : MonoBehaviour, ISaveLoad, IOnCompletedLevel, IOnSave, IOnLoad, IOnClear
    {
        public static SpellManager Instance { get; private set; }

        public List<Spell> Spells { private set; get; } = new List<Spell>();

        public string SaveFileName => "SpellManagerSave";

        public List<GameObject> spawnedObjects;

        private Spell activeSpell;

        private Camera cam;

        private LocalMapGenerator localMapGenerator;

        private List<MapTile> highlightedTiles = new List<MapTile>();

        public delegate void UpdateUI();
        public UpdateUI updateUI;

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            cam = Camera.main;

            localMapGenerator = LocalMapGenerator.Instance;
        }

        public void Clear()
        {
            DestorySpawnedObjects();

            Spells.Clear();
        }

        private void RefreshSpells()
        {
            DestorySpawnedObjects();

            activeSpell = null;

            foreach (Spell spell in Spells)
            {
                spell.Reset();
            }

            updateUI?.Invoke();
        }

        private void DestorySpawnedObjects()
        {
            foreach (GameObject obj in spawnedObjects)
            {
                if (obj == null)
                    continue;

                Destroy(obj);
            }

            spawnedObjects.Clear();
        }

        // Update is called once per frame
        void Update()
        {
            if (cam == null)
                cam = Camera.main;

            if (localMapGenerator == null)
                localMapGenerator = LocalMapGenerator.Instance;

            HideHighlight();

            ShowHighlight();

            if (Input.GetMouseButtonDown(0))
            {
                UseSpell();
            }
        }

        public void AddSpell(SpellSO spellSO, int amountOfUses)
        {
            if (spellSO == null)
                return;

            for (int i = 0; i < Spells.Count; i++)
            {
                if (Spells[i].spellSO == spellSO)
                {
                    Spells[i].MaxUses += amountOfUses;
                    return;
                }
            }

            Spell spell = new Spell(spellSO, amountOfUses);

            if (spell.spellSO.spellEffects != null)
                spell.spellSO.spellEffects.ForEach(x => x.Reset());

            Spells.Add(spell);

            updateUI?.Invoke();
        }

        public void RemoveSpell(SpellSO spellSO, int amountOfUses)
        {
            foreach (Spell spell in Spells)
            {
                if (spell.spellSO == spellSO)
                {
                    spell.MaxUses -= amountOfUses;

                    if (spell.MaxUses <= 0)
                        Spells.Remove(spell);

                    updateUI?.Invoke();
                    return;
                }
            }
        }

        private void HideHighlight()
        {
            if (highlightedTiles == null || highlightedTiles.Count == 0)
                return;

            for (int i = 0; i < highlightedTiles.Count; i++)
            {
                if (highlightedTiles[i] != null)
                    highlightedTiles[i].Highlighted = false;
            }

            highlightedTiles.Clear();
        }

        public void ShowHighlight()
        {
            if (activeSpell == null || activeSpell.spellSO == null)
                return;

            Vector2Int position = ((Vector2)cam.ScreenToWorldPoint(Input.mousePosition)).V2ToV2Int();

            bool canUseSpell = activeSpell.spellSO.CanUseSpell(position);

            foreach (SpellEffect effect in activeSpell.spellSO.spellEffects)
            {
                List<Vector2Int> v2IntList = effect.GetPositions(position);

                foreach (Vector2Int v2Int in v2IntList)
                {
                    if (localMapGenerator.Tiles != null && v2Int.x >= 0 && v2Int.x < localMapGenerator.Tiles.GetLength(0) && v2Int.y >= 0 && v2Int.y < localMapGenerator.Tiles.GetLength(1))
                    {
                        bool[,] boolArray = new bool[3, 3];

                        for (int i = 0; i < boolArray.GetLength(0); i++)
                        {
                            for (int j = 0; j < boolArray.GetLength(1); j++)
                            {
                                boolArray[i, j] = !v2IntList.Contains(v2Int + new Vector2Int(i, j) - new Vector2Int(1, 1));
                            }
                        }

                        localMapGenerator.Tiles[v2Int.x, v2Int.y].HighlightWithColor(canUseSpell ? activeSpell.spellSO.highlightColor : activeSpell.spellSO.invalidUseColor, boolArray);
                        highlightedTiles.Add(localMapGenerator.Tiles[v2Int.x, v2Int.y]);
                    }

                }
            }
        }

        public void UseSpell()
        {
            if (!TilePlayerManager.Instance.IsPlaying)
                return;

            if (activeSpell == null || activeSpell.numberOfUses <= 0)
                return;

            if (InputManager.Instance.MouseOverUI)
                return;

            Vector2Int position = ((Vector2)cam.ScreenToWorldPoint(Input.mousePosition)).V2ToV2Int();

            if (!activeSpell.spellSO.CanUseSpell(position))
                return;

            foreach (SpellEffect spellEffect in activeSpell.spellSO.spellEffects)
            {
                if (spellEffect != null)
                    spellEffect.Effect(position);
            }

            activeSpell.numberOfUses--;
            activeSpell = null;

            updateUI?.Invoke();
        }

        public void ActivateSpell(Spell spell)
        {
            if (!TilePlayerManager.Instance.IsPlaying)
                return;

            activeSpell = spell;
        }

        public void Save()
        {
            SaveManager.IfFileExistsDelete(SaveFileName);
            SaveData saveData = new SaveData(Spells);
            SaveProfile<SaveData> saveProfile = new SaveProfile<SaveData>(SaveFileName, saveData);
            SaveManager.SaveOver(saveProfile);
        }

        public void Load()
        {
            if (SaveManager.TryLoad(SaveFileName, out SaveProfile<SaveData> outSaveProfile))
            {
                SaveData saveData = outSaveProfile.saveData;

                Spells = saveData.Spells;
                updateUI?.Invoke();
            }
            else
            {
                Debug.LogWarning($"No save data found for {SaveFileName}");
                return;
            }
        }

        public void OnLevelCompleted()
        {
            RefreshSpells();
        }



        [Serializable]
        private record SaveData : SaveProfileData
        {
            public List<Spell> Spells;

            public SaveData(List<Spell> Spells)
            {
                this.Spells = Spells;
            }
        }
    }

}
