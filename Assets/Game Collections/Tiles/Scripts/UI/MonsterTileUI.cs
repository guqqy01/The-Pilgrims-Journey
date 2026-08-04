using Monsters;
using System.Collections.Generic;
using Tiles.UI;
using UnityEngine;
using UnityEngine.UI;
using Tiles;
public class MonsterTileUI : TileUIBase, IDescriptable
{
    [SerializeField]
    private RectTransform monsterUITransform;
    private Image monsterGraphics;

    private MonsterTileBase _monsterTile;
    public MonsterTileBase MonsterTile
    {
        get
        {
            if (_monsterTile == null)
            {
                if (Tile is MonsterTileBase monsterTile)
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

            return MonsterTile.MonsterSpawnBase;
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

        if (tile is not MonsterTileBase monsterTile)
            return;

        monsterGraphics = monsterUITransform.GetComponent<Image>();
        monsterGraphics.sprite = monsterTile.MonsterSpawnBase.monster.sprite;
        monsterGraphics.rectTransform.anchoredPosition = GetCenteredAnchoredPositionPositionInShape(tile.Shape);
    }

    public string Title => MonsterSpawn.monster.Title;

    public string MainDescription => MonsterSpawn.monster.MainDescription;

    public Sprite Sprite => MonsterSpawn.monster.Sprite;

    public List<ISubDescriptionData> GetSubDescriptionData => MonsterSpawn.monster.GetSubDescriptionData;
}
