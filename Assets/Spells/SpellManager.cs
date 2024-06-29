using Map;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tiles;
using Unity.VisualScripting;

public class SpellManager : MonoBehaviour, ISaveLoad
{
    public static SpellManager Instance {  get; private set; }

    public List<SpellSO> startSpells;
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

        GameManager.onCompleteLevel += RefreshSpells;
        GameManager.onSave += Save;
        GameManager.onLoad += Load;
        GameManager.onClear += Clear;
        GameManager.onSetup += Setup;
    }

    private void OnDisable()
    {
        GameManager.onCompleteLevel -= RefreshSpells;
        GameManager.onSave -= Save;
        GameManager.onLoad -= Load;
        GameManager.onClear -= Clear;
        GameManager.onSetup -= Setup;
    }

    private void Clear()
    {
        DestorySpawnedObjects();

        Spells.Clear();
    }

    public void Setup()
    {
        for (int i = 0; i < startSpells.Count; i++)
        {
            Spells.Add(new Spell(startSpells[i]));
        }

        updateUI?.Invoke();
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

    public void AddSpell(SpellSO spell)
    {
        for (int i = 0; i < Spells.Count; i++)
        {
            if (Spells[i].spellSO == spell)
            {
                Spells[i].MaxUses += spell.numberOfUses;
                return;
            }
        }

        Spells.Add(new Spell(spell));

        updateUI?.Invoke();
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

        Vector2Int position = ((Vector2)cam.ScreenToWorldPoint(Input.mousePosition)).V2ToV2Int();

        if (!activeSpell.spellSO.CanUseSpell(position))
            return;

        foreach (SpellEffect spellEffect in activeSpell.spellSO.spellEffects)
        {
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
        SaveManager.Save(saveProfile);
    }

    public void Load()
    {
        SaveData saveData = SaveManager.Load<SaveData>(SaveFileName).saveData;
        Spells = saveData.Spells;
        updateUI?.Invoke();
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
