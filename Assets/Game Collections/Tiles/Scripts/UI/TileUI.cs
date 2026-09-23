using Tiles.UI;
using UnityEngine;
using UnityEngine.UI;
using Tiles;

public abstract class TileUI<TObjectTile> : TileUIBase where TObjectTile : TileBase
{
    [SerializeField]
    private RectTransform objectImageTransform;
    private Image objectImage;

    private TObjectTile _objectTile;

    public TObjectTile ObjectTile
    {
        get
        {
            if (_objectTile == null)
            {
                if (Tile is TObjectTile facadeTile)
                {
                    _objectTile = facadeTile;
                }
                else
                {
                    return null;
                }
            }
            return _objectTile;
        }
    }

    [SerializeField]
    private TileUIStyle tileUIStyle;
    public override TileUIStyle TileUIStyle => tileUIStyle;
    
    private const float objectImageRelativeSizePercent = 60f;

    private void Awake()
    {
        objectImage = objectImageTransform.GetComponent<Image>();
    }

    public override void Rotate(int angle)
    {
        base.Rotate(angle);
        objectImageTransform.rotation = Quaternion.Euler(Vector3.zero);
    }

    public override void RotateTo(float angle)
    {
        base.RotateTo(angle);
        objectImageTransform.rotation = Quaternion.Euler(Vector3.zero);
    }

    private void SizeChange(TileGridUI tileGridUI)
    {
        if (objectImage == null)
            objectImage = objectImageTransform.GetComponent<Image>();

        objectImage.rectTransform.anchoredPosition = GetCenteredAnchoredPositionPositionInShape(Tile.Shape, tileGridUI);

        if (tileGridUI != null)
        {
            objectImage.NativeRatioScaleToSize(tileGridUI.SingleTileSquareSize.Min().Percent(objectImageRelativeSizePercent/*%*/));
            return;
        }

        objectImage.NativeRatioScaleToSize((BaseSize / new Vector2(Tile.Width, Tile.Height)).Min().Percent(objectImageRelativeSizePercent/*%*/));
    }

    public override void BaseInit(TileBase tile)
    {
        base.BaseInit(tile);

        if (tile is not TObjectTile objectTile)
            return;

        objectImage = objectImageTransform.GetComponent<Image>();
        objectImage.sprite = objectTile.TiledObject.Sprite;
        SizeChange(tileGridUI);
    }

    protected override void OnTileSizeChanged(TileGridUI grid)
    {
        SizeChange(grid);
    }
}
