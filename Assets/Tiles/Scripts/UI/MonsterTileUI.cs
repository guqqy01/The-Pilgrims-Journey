using Monsters;
using System.Collections.Generic;
using Tiles;
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
                if (Tile is MonsterTile monsterTile)
                {
                    _monsterTile = monsterTile;
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

    [SerializeField]
    private TileUIStyle tileUIStyle;
    public override TileUIStyle TileUIStyle => tileUIStyle;

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
        base.BaseInit(tile);

        if (tile is not MonsterTile monsterTile)
            return;

        monsterGraphics = monsterUITransform.GetComponent<Image>();
        monsterGraphics.sprite = monsterTile.MonsterSpawn.monster.sprite;
        monsterGraphics.rectTransform.anchoredPosition = GetCenteredAnchoredPositionPositionInShape(tile.Shape);
    }

    public string Title => MonsterSpawn.monster.Title;

    public string MainDescription => MonsterSpawn.monster.MainDescription;

    public Sprite Sprite => MonsterSpawn.monster.Sprite;

    public List<ISubDescriptionData> GetSubDescriptionData => MonsterSpawn.monster.GetSubDescriptionData;
}
