using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileHandler : MonoBehaviour
{
    private Tile selectedTile;

    [SerializeField]
    private Color invalidTilePlacementColor;

    public TileInventory inventory;
    private RectTransform inventoryRectTransform;

    public TilePlayer player;
    private RectTransform playerRectTransform;

    public Canvas canvas;

    private float playerTileSizeX => PlayerRectTransform.sizeDelta.x / player.TilePlayerSize.x;
    private float playerTileSizeY => PlayerRectTransform.sizeDelta.y / player.TilePlayerSize.y;

    private float inventoryTileSizeX => InventoryRectTransform.sizeDelta.x / inventory.TileInventorySize.x;
    private float inventoryTileSizeY => InventoryRectTransform.sizeDelta.y / inventory.TileInventorySize.y;

    public RectTransform PlayerRectTransform
    {
        get
        {
            if (playerRectTransform == null)
                playerRectTransform = player.GetComponent<RectTransform>();

            return playerRectTransform;
        }
    }

    public RectTransform InventoryRectTransform
    {
        get
        {
            if (inventoryRectTransform == null)
                inventoryRectTransform = inventory.GetComponent<RectTransform>();

            return inventoryRectTransform;
        }
    }

    private void Update()
    {
        if (selectedTile != null)
        {
            HandlePlaceTileOnPlayer();
            HandlePlaceTileOnInventory();
        }
        else
        {
            HandleSelectTileOnPlayer();
            HandleSelectTileOnInventory();
        }
    }

    private void HandleSelectTileOnPlayer()
    {
        if (!InputManager.Instance.LeftClick)
            return;

        Vector2Int playerMousePos = GetMouseTilePositionOnPlayer();
        if (player.TryGetTileAtPosition(playerMousePos, out Tile tile))
        {
            selectedTile = tile;
            tile.SelectTile(true);
        }
    }

    private void HandleSelectTileOnInventory()
    {
        if (!InputManager.Instance.LeftClick)
            return;

        Vector2Int inventoryMousePos = GetMouseTilePositionOnInventory();
        if (inventory.TryGetTileAtPosition(inventoryMousePos, out Tile tile))
        {
            selectedTile = tile;
            tile.SelectTile(true);
        }
    }

    private Vector2Int GetMouseTilePositionOnInventory()
    {
        Vector2 mousePosition = Input.mousePosition / canvas.scaleFactor;
        Vector2 inventoryPos = InventoryRectTransform.position / canvas.scaleFactor;
        Vector2 relativePos = mousePosition + InventoryRectTransform.sizeDelta / 2f - inventoryPos;

        return new Vector2Int(Mathf.CeilToInt(relativePos.x / inventoryTileSizeX), Mathf.CeilToInt(relativePos.y / inventoryTileSizeY));
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
        Vector2Int mouseTilePosition = GetMouseTilePositionOnPlayer();

        selectedTile.rectTransform.position = (Vector2)PlayerRectTransform.position + canvas.scaleFactor * new Vector2(playerTileSizeX * (mouseTilePosition.x - 0.5f - player.TilePlayerSize.x / 2f), playerTileSizeY * (mouseTilePosition.y - 0.5f - player.TilePlayerSize.y / 2f));

        Color color = player.CanPlaceTile(selectedTile, mouseTilePosition) ? Color.white.SetAlpha(0.5f) : invalidTilePlacementColor;
        selectedTile.Color = color;
        if (InputManager.Instance.LeftClick)
        {
            if(player.TryPlaceTile(selectedTile, mouseTilePosition))
            {
                inventory.RemoveTile(selectedTile);

                selectedTile.Color = Color.white;
                selectedTile = null;
            }
        }
    }

    private void HandlePlaceTileOnInventory()
    {
        Vector2Int mouseTilePosition = GetMouseTilePositionOnInventory();

        selectedTile.rectTransform.position = (Vector2)InventoryRectTransform.position + canvas.scaleFactor * new Vector2(inventoryTileSizeX * (mouseTilePosition.x - 0.5f - inventoryTileSizeX / 2f), inventoryTileSizeY * (mouseTilePosition.y - 0.5f - inventoryTileSizeY / 2f));

        Color color = inventory.CanPlaceTile(selectedTile, mouseTilePosition) ? Color.white.SetAlpha(0.5f) : invalidTilePlacementColor;
        selectedTile.Color = color;
        if (InputManager.Instance.LeftClick)
        {
            if (inventory.TryPlaceTile(selectedTile, mouseTilePosition))
            {
                player.RemoveTile(selectedTile);

                selectedTile.Color = Color.white;
                selectedTile = null;
            }
        }
    }
}
