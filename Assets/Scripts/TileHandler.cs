using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileHandler : MonoBehaviour
{
    public Tile selectedTile;
    [SerializeField]
    private Image selectedTileUI;

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

    private void HandlePlaceTileOnPlayer()
    {
        if (selectedTile == null)
            return;

        Vector2 mousePosition = Input.mousePosition / canvas.scaleFactor;
        Vector2 playerPos = PlayerRectTransform.position / canvas.scaleFactor;
        Vector2 relativePos = mousePosition + PlayerRectTransform.sizeDelta / 2f - playerPos;

        Vector2Int mouseTilePosition = new Vector2Int(Mathf.CeilToInt(relativePos.x / playerTileSizeX), Mathf.CeilToInt(relativePos.y / playerTileSizeY));

        selectedTileUI.rectTransform.position = (Vector2)PlayerRectTransform.position + canvas.scaleFactor * new Vector2(playerTileSizeX * (mouseTilePosition.x - 0.5f - player.tilePlayerSize.x / 2f), playerTileSizeY * (mouseTilePosition.y - 0.5f - player.tilePlayerSize.y / 2f));
        Color white = Color.white;
        white.a = 0.5f;
        selectedTileUI.color = player.CanPlaceTile(selectedTile.shape.v2IntList, mouseTilePosition) ? white : invalidTilePlacementColor;
        if (InputManager.Instance.LeftClick)
        {
            if(player.TryPlaceTile(selectedTile, mouseTilePosition))
            {
                white.a = 1;
                selectedTileUI.color = white;
                selectedTile = null;
                selectedTileUI = null;
            }
        }
    }
}