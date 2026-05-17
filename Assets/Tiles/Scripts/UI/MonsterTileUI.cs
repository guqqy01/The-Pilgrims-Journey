using Equipment;
using Monsters;
using System.Collections.Generic;
using Tiles;
using Units;
using UnityEngine;
using UnityEngine.UI;

public class MonsterTileUI : TileUIBase, IDescriptable
{
    [SerializeField]
    private RectTransform monsterUITransform;
    private Image monsterGraphics;

    private MonsterTile _monsterTile;
    public MonsterTile MonsterTile
    {
        get
        {
            if (_monsterTile == null)
            {
                if (Tile is MonsterTile unitTile)
                {
                    _monsterTile = unitTile;
                }
                else
                {
                    return null;
                }
            }
            return _monsterTile;
        }
    }

    public MonsterSpawn MonsterSpawn
    {
        get
        {
            if (MonsterTile == null)
                return null;

            return MonsterTile.MonsterSpawn;
        }
    }

    private void Awake()
    {
        monsterGraphics = monsterUITransform.GetComponent<Image>();
    }

    public override void Rotate(int angle)
    {
        base.Rotate(angle);
        monsterUITransform.rotation = Quaternion.Euler(Vector3.zero);
    }

    public override void RotateTo(float angle)
    {
        base.RotateTo(angle);
        monsterUITransform.rotation = Quaternion.Euler(Vector3.zero);
    }

    public override void BaseInit(TileBase tile)
    {
        if (tile is not UnitTile unitTile)
            return;

        base.BaseInit(unitTile);

        monsterGraphics = monsterUITransform.GetComponent<Image>();
        monsterGraphics.sprite = unitTile.UnitSO.sprite;
        tileShapeTextureBuilder.Build(tile.Shape);
        monsterGraphics.rectTransform.anchoredPosition = GetCenteredAnchoredPositionPositionInShape(tile.Shape);
    }

    public string Title => MonsterSpawn.monster.Title;

    public string MainDescription => MonsterSpawn.monster.MainDescription;

    public Sprite Sprite => MonsterSpawn.monster.Sprite;

    public List<ISubDescriptionData> GetSubDescriptionData => MonsterSpawn.monster.GetSubDescriptionData;
}
