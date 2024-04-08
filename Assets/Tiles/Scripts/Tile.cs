using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Units;

public class Tile : MonoBehaviour
{
    [SerializeField]
    private RectTransform unitUITransform;

    private Image unitGraphics;

    public RectTransform rectTransform { private set; get; }

    [SerializeField]
    private Image tileGraphics;

    public TileSO tileSO;

    private static float multiplier = 210;
    public Vector2 Offset => multiplier * GetOffset();

    public bool locked;

    public EquipmentSO equipment1;
    public EquipmentSO equipment2;
    public EquipmentSO equipment3;

    public List<EquipmentSO> Equipment => new List<EquipmentSO>() { equipment1, equipment2, equipment3 };

    public int Level = 1;

    public float spawningMultiplier => 1f + 0.1f * (Level - 1);

    public float movementMultiplier => 1f + 0.1f * (Level - 1);

    public float healthMultiplier => 1f + 0.1f * (Level - 1);

    public float shieldMultiplier => 1f + 0.1f * (Level - 1);

    public float shieldRegenerationMultiplier => 1f + 0.1f * (Level - 1);
    public float shieldCooldownMultiplier => Mathf.Pow(0.9f, (Level - 1));

    public int requiredGoldToLevel => Level * Level * Shape.Count * Shape.Count * 15;
    public int requiredDiamondsToEvolve => Shape.Count;


    public delegate void OnStatsChange();
    public OnStatsChange onStatsChange;

    

    private UnitSO unit;
    public UnitSO Unit
    {
        get
        {
            return unit;
        }
        set
        {
            unit = value;

            unitGraphics.sprite = unit.sprite;

            onStatsChange?.Invoke();
        }
    }

    public bool Selected { private set; get; } = false;

    [SerializeField]
    private GameObject highlight;

    [HideInInspector]
    public float TimeForNextSpawn = 0;
    
    public float SpawnRateValue
    {
        get
        {
            float equipmentBaseValue = 0;
            float equipmentMultiplierValue = 1;

            foreach (EquipmentSO equipment in Equipment)
            {
                if (equipment != null)
                {
                    equipmentBaseValue += equipment.FloatBaseValue(UnitFloatStats.SpawnRate);
                    equipmentMultiplierValue *= equipment.FloatMultiplierValue(UnitFloatStats.SpawnRate);
                }
            }

            return spawningMultiplier * equipmentMultiplierValue * (Unit.BaseSpawnRate + equipmentBaseValue);
        }
    }

    public float MovementValue
    {
        get
        {
            float equipmentBaseValue = 0;
            float equipmentMultiplierValue = 1;

            foreach (EquipmentSO equipment in Equipment)
            {
                if (equipment != null)
                {
                    equipmentBaseValue += equipment.FloatBaseValue(UnitFloatStats.Movement);
                    equipmentMultiplierValue *= equipment.FloatMultiplierValue(UnitFloatStats.Movement);
                }
            }

            return (movementMultiplier * equipmentMultiplierValue * (Unit.MovementSpeed + equipmentBaseValue));
        }
    }

    public int HealthValue
    {
        get
        {
            int equipmentBaseValue = 0;
            float equipmentMultiplierValue = 1;

            foreach (EquipmentSO equipment in Equipment)
            {
                if (equipment != null)
                {
                    equipmentBaseValue += equipment.IntBaseValue(UnitIntStats.Health);
                    equipmentMultiplierValue *= equipment.IntMultiplierValue(UnitIntStats.Health);
                }
            }

            return Mathf.CeilToInt(healthMultiplier * equipmentMultiplierValue * (Unit.baseHealth + equipmentBaseValue));
        }
    }

    public int ShieldValue
    {
        get
        {
            int equipmentBaseValue = 0;
            float equipmentMultiplierValue = 1;

            foreach (EquipmentSO equipment in Equipment)
            {
                if (equipment != null)
                {
                    equipmentBaseValue += equipment.IntBaseValue(UnitIntStats.Shield);
                    equipmentMultiplierValue *= equipment.IntMultiplierValue(UnitIntStats.Shield);
                }
            }

            return Mathf.CeilToInt(shieldMultiplier * equipmentMultiplierValue * (Unit.Shield + equipmentBaseValue));
        }
    }

    public float ShieldRegenerationValue
    {
        get
        {
            float equipmentBaseValue = 0;
            float equipmentMultiplierValue = 1;

            foreach (EquipmentSO equipment in Equipment)
            {
                if (equipment != null)
                {
                    equipmentBaseValue += equipment.FloatBaseValue(UnitFloatStats.ShieldRegeneration);
                    equipmentMultiplierValue *= equipment.FloatMultiplierValue(UnitFloatStats.ShieldRegeneration);
                }
            }

            return (shieldRegenerationMultiplier * equipmentMultiplierValue * (Unit.ShieldRegeneration + equipmentBaseValue));
        }
    }

    public float ShieldCooldownValue
    {
        get
        {
            float equipmentBaseValue = 0;
            float equipmentMultiplierValue = 1;

            foreach (EquipmentSO equipment in Equipment)
            {
                if (equipment != null)
                {
                    equipmentBaseValue += equipment.FloatBaseValue(UnitFloatStats.ShieldCooldown);
                    equipmentMultiplierValue *= equipment.FloatMultiplierValue(UnitFloatStats.ShieldCooldown);
                }
            }

            return (shieldCooldownMultiplier * equipmentMultiplierValue * (Unit.ShieldCooldown + equipmentBaseValue));
        }
    }

    public Color Color
    {
        set
        {
            if (tileGraphics != null)
                tileGraphics.color = value;
        }
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        unitGraphics = unitUITransform.GetComponent<Image>();
    }

    public void Upgrade()
    {
        if (ResourceManager.Instance.Gold < requiredGoldToLevel)
            return;

        ResourceManager.Instance.ModifyGold(-requiredGoldToLevel);

        Level++;

        onStatsChange?.Invoke();
    }

    public void Highlight(bool highlighted)
    {
        highlight.SetActive(highlighted);
    }

    public Vector2 GetOffset()
    {
        int xMin = int.MaxValue, xMax = int.MinValue, yMin = int.MaxValue, yMax = int.MinValue;

        foreach (Vector2Int v2int in Shape)
        {
            if (v2int.x > xMax)
                xMax = v2int.x;

            if (v2int.x < xMin)
                xMin = v2int.x;

            if (v2int.y > yMax)
                yMax = v2int.y;

            if (v2int.y < yMin)
                yMin = v2int.y;
        }
        Vector2 v2 = new Vector2((xMax + xMin) / 2f, (yMax + yMin) / 2f);
        return v2;
    }

    public List<Vector2Int> Shape
    {
        get
        {
            float rotation = rectTransform.rotation.eulerAngles.z;

            List<Vector2Int> returnShape = new List<Vector2Int>();

            for (int i = 0; i < tileSO.shape.v2IntList.Count; i++)
            {
                int x = Mathf.RoundToInt(tileSO.shape.v2IntList[i].x * Mathf.Cos(rotation * Mathf.Deg2Rad) - tileSO.shape.v2IntList[i].y * Mathf.Sin(rotation * Mathf.Deg2Rad));
                int y = Mathf.RoundToInt(tileSO.shape.v2IntList[i].x * Mathf.Sin(rotation * Mathf.Deg2Rad) + tileSO.shape.v2IntList[i].y * Mathf.Cos(rotation * Mathf.Deg2Rad));
                returnShape.Add(new Vector2Int(x, y));
            }


            return returnShape;
        }
    }

    public void Rotate(int angle)
    {
        rectTransform.Rotate(new Vector3(0, 0, angle));
        unitUITransform.rotation = Quaternion.Euler(Vector3.zero);
    }

    public void RotateTo(float angle)
    {
        Vector3 eulerAngles = rectTransform.rotation.eulerAngles;
        eulerAngles.z = angle;
        rectTransform.rotation = Quaternion.Euler(eulerAngles);
        unitUITransform.rotation = Quaternion.Euler(Vector3.zero);
    }

    public void Init(TileSO tileSO)
    {
        this.tileSO = tileSO;

        tileGraphics.sprite = tileSO.TileSprite;
        tileGraphics.SetNativeSize();
    }
}