using GameManagerCalls;
using GlobalMap;
using LocalMap;
using Spells;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spells
{
    public class SpellManager : Singleton<SpellManager>, ISaveLoad, IOnCompletedLevel, IOnSave, IOnLoad, IOnClear
    {
        public List<Spell> Spells { private set; get; } = new List<Spell>();

        public string SaveFileName => "SpellManagerSave";

        private const string _timeManagmentReference = "SpellManager";

        public List<GameObject> spawnedObjects;

        private Spell activeSpell;

        public Spell ActiveSpell => activeSpell;

        private Camera cam;

        private LocalMapGenerator localMapGenerator;

        public delegate void UpdateUI();
        public UpdateUI updateUI;

        [SerializeField]
        private float timeSlowDownScale = 0.1f;
        
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
            EndSlowTimeEffect();
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

            MapTileHighlightManager.Instance.HideHighlight();

            ShowHighlight();

            if (InputManager.Instance.Actions.Spell_Casting.Cast.WasPressedThisFrame())
            {
                CastSpell();
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
                spell.spellSO.spellEffects.ForEach(x => x?.Reset());
            
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

        public void ShowHighlight()
        {
            if (activeSpell == null || activeSpell.spellSO == null)
                return;

            List<MapTileHighlightManager.HighlightInfo> highlightInfos = new List<MapTileHighlightManager.HighlightInfo>();

            Vector2Int position = PointerManager.Instance.PointerWorldPosition.V2ToV2Int();
            bool canUseSpell = activeSpell.spellSO.CanUseSpell(position);
            Color color = canUseSpell ? activeSpell.spellSO.highlightColor : activeSpell.spellSO.invalidUseColor;

            foreach (SpellEffect effect in activeSpell.spellSO.spellEffects)
            {
                MapTileHighlightManager.HighlightInfo highlightInfo = 
                    new MapTileHighlightManager.HighlightInfo(color, effect.GetPositions(position));

                highlightInfos.Add(highlightInfo);
            }

            MapTileHighlightManager.Instance.ShowHighlight(highlightInfos);
        }

        public void CastSpell()
        {
            if (!LocalMapLevelCompletedManager.Instance.CurrentlyPlayingLocalLevel)
                return;

            if (activeSpell == null || activeSpell.numberOfUses <= 0)
                return;

            Vector2Int position = PointerManager.Instance.PointerWorldPosition.V2ToV2Int();

            if (!activeSpell.spellSO.CanUseSpell(position))
                return;

            foreach (SpellEffect spellEffect in activeSpell.spellSO.spellEffects)
            {
                if (spellEffect != null)
                    spellEffect.Effect(position);
            }

            activeSpell.numberOfUses--;
            activeSpell = null;
            EndSlowTimeEffect();
            updateUI?.Invoke();
        }

        public void ActivateSpell(Spell spell)
        {
            if (!LocalMapLevelCompletedManager.Instance.CurrentlyPlayingLocalLevel)
                return;

            if (spell != null)
                BeginSlowTimeEffect();
            else if (activeSpell != null)
                EndSlowTimeEffect();

            activeSpell = spell;
        }

        private void BeginSlowTimeEffect()
        {
            TimeManager.Instance.AddTimeScaleReference(_timeManagmentReference, timeSlowDownScale);
            LocalCameraMovementManager.Instance.CameraLocked = false;
            InputManager.Instance.Actions.Player.Disable();
            InputManager.Instance.Actions.Spell_Casting.Enable();
        }

        private void EndSlowTimeEffect()
        {
            TimeManager.Instance.RemoveTimeScaleReference(_timeManagmentReference);
            LocalCameraMovementManager.Instance.CameraLocked = true;
            InputManager.Instance.Actions.Player.Enable();
            InputManager.Instance.Actions.Spell_Casting.Disable();
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
        private class SaveData : SaveProfileData
        {
            public List<Spell> Spells;

            public SaveData(List<Spell> Spells)
            {
                this.Spells = Spells;
            }
        }
    }

}
