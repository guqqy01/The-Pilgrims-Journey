using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Units;

[RequireComponent(typeof(TileStats))]
public class Tile : TileImage
{
    public bool locked;

    public EquipmentSO equipment1;
    public EquipmentSO equipment2;
    public EquipmentSO equipment3;

    public List<EquipmentSO> Equipment => new List<EquipmentSO>() { equipment1, equipment2, equipment3 };

    public UnitSO baseUnit;

    public UnitSO Unit
    {
        get
        {
            return baseUnit.GetUnit(Equipment);
        }
    }

    public int UnlockedEquipment { private set; get; } = 1;

    public TileStats tileStats { private set; get; }

    public bool Selected { private set; get; } = false;

    [SerializeField]
    private GameObject highlight;

    [HideInInspector]
    public float TimeForNextSpawn = 0;
    public float SpawnRate(int positionTileSize) => Unit.BaseSpawnRate * tileStats.spawningMultiplier * positionTileSize;

    protected override void Awake()
    {
        base.Awake();
        tileStats = GetComponent<TileStats>();
    }

    public void SelectTile()
    {
        highlight.SetActive(true);

        UIManager.instance.SelectedTileEquipmentContainer1.onEquipmentChange = (EquipmentSO fromEquipment, EquipmentSO toEquipment) => equipment1 = toEquipment;
        UIManager.instance.SelectedTileEquipmentContainer2.onEquipmentChange = (EquipmentSO fromEquipment, EquipmentSO toEquipment) => equipment2 = toEquipment;
        UIManager.instance.SelectedTileEquipmentContainer3.onEquipmentChange = (EquipmentSO fromEquipment, EquipmentSO toEquipment) => equipment3 = toEquipment;
    }

    public void DeselectTile()
    {
        highlight.SetActive(false);
    }
}

public class TileImage : MonoBehaviour
{
    [SerializeField]
    private RectTransform unitUITransform;

    public RectTransform rectTransform { private set; get; }

    [SerializeField]
    private Image tileGraphics;

    public TileSO tileSO;

    private static float multiplier = 210;
    public Vector2 Offset => multiplier * GetOffset();

    public Color Color
    {
        set
        {
            if (tileGraphics != null)
                tileGraphics.color = value;
        }
    }

    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
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

    public void Init(TileSO tileSO)
    {
        this.tileSO = tileSO; 

        tileGraphics.sprite = tileSO.TileSprite;
        tileGraphics.SetNativeSize();
    }
}