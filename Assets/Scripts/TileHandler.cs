using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileHandler : MonoBehaviour
{
    private Tile selectedTile;
    [SerializeField]
    private TileUI selectedTileUIPrefab;

    private TileUI instantiatedTileUI;

    [SerializeField]
    private Color invalidTilePlacementColor;

    public TileInventory inventory;

    public TilePlayer player;

    private RectTransform playerRectTransform;

    public Canvas canvas;

    public Vector2ListWrapper test;

    private float playerTileSizeX => PlayerRectTransform.sizeDelta.x / player.tilePlayerSize.x;
    private float playerTileSizeY => PlayerRectTransform.sizeDelta.y / player.tilePlayerSize.y;

    public RectTransform PlayerRectTransform
    {
        get
        {
            if (playerRectTransform == null)
                playerRectTransform = player.GetComponent<RectTransform>();

            return playerRectTransform;
        }
    }

    private void Update()
    {
        HandlePlaceTileOnPlayer();


    }

    private void HandleSelectTileOnInventory()
    {

    }

    public void SelectTile(Tile tile)
    {
        selectedTile = tile;

        instantiatedTileUI = Instantiate(selectedTileUIPrefab, player.transform);

        instantiatedTileUI.Init(tile.shape.shapeSprite, tile.unit.unitSprite);
    }

    private Vector2Int GetMouseTilePositionOnPlayer()
    {
        Vector2 mousePosition = Input.mousePosition / canvas.scaleFactor;
        Vector2 playerPos = PlayerRectTransform.position / canvas.scaleFactor;
        Vector2 relativePos = mousePosition + PlayerRectTransform.sizeDelta / 2f - playerPos;

        return new Vector2Int(Mathf.CeilToInt(relativePos.x / playerTileSizeX), Mathf.CeilToInt(relativePos.y / playerTileSizeY));
    }

    private void HandlePlaceTileOnPlayer()
    {
        if (selectedTile == null)
            return;

        Vector2Int mouseTilePosition = GetMouseTilePositionOnPlayer();

        instantiatedTileUI.rectTransform.position = (Vector2)PlayerRectTransform.position + canvas.scaleFactor * new Vector2(playerTileSizeX * (mouseTilePosition.x - 0.5f - player.tilePlayerSize.x / 2f), playerTileSizeY * (mouseTilePosition.y - 0.5f - player.tilePlayerSize.y / 2f));

        Color color = player.CanPlaceTile(selectedTile.Shape, mouseTilePosition) ? Color.white.SetAlpha(0.5f) : invalidTilePlacementColor;
        instantiatedTileUI.SetColor(color);
        if (InputManager.Instance.LeftClick)
        {
            if(player.TryPlaceTile(selectedTile, mouseTilePosition))
            {
                instantiatedTileUI.SetColor(Color.white);
                selectedTile = null;
                instantiatedTileUI = null;
            }
        }
    }
}
