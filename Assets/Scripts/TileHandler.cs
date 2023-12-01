using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileHandler : MonoBehaviour
{
    [SerializeField]
    private Tile selectedTile;

    [SerializeField]
    private Color invalidTilePlacementColor;

    public TileGrid inventory;
    private RectTransform inventoryRectTransform;

    public TilePlayer player;
    private RectTransform playerRectTransform;

    public Canvas canvas;

    private float playerTileSizeX => PlayerRectTransform.sizeDelta.x / player.TileGridSize.x;
    private float playerTileSizeY => PlayerRectTransform.sizeDelta.y / player.TileGridSize.y;

    private float inventoryTileSizeX => InventoryRectTransform.sizeDelta.x / inventory.TileGridSize.x;
    private float inventoryTileSizeY => InventoryRectTransform.sizeDelta.y / inventory.TileGridSize.y;

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
            HandleSelectedTileHover();
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
            selectedTile.transform.parent = canvas.transform;
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
            selectedTile.transform.parent = canvas.transform;
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

    private void HandleSelectedTileHover()
    {
        Vector2Int mouseTilePositionPlayer = GetMouseTilePositionOnPlayer();
        Vector2Int mouseTilePositionInventory = GetMouseTilePositionOnInventory();
        if (!player.PointOutOfGrid(mouseTilePositionPlayer))
        {
            selectedTile.rectTransform.position = (Vector2)PlayerRectTransform.position + canvas.scaleFactor * new Vector2(playerTileSizeX * (mouseTilePositionPlayer.x - 0.5f - player.TileGridSize.x / 2f), playerTileSizeY * (mouseTilePositionPlayer.y - 0.5f - player.TileGridSize.y / 2f));

            selectedTile.Color = player.CanPlaceTile(selectedTile, mouseTilePositionPlayer) ? Color.white.SetAlpha(0.5f) : invalidTilePlacementColor;
        }
        else if (!inventory.PointOutOfGrid(mouseTilePositionInventory))
        {
            selectedTile.rectTransform.position = (Vector2)InventoryRectTransform.position + canvas.scaleFactor * new Vector2(inventoryTileSizeX * (mouseTilePositionInventory.x - 0.5f - inventory.TileGridSize.x / 2f), inventoryTileSizeY * (mouseTilePositionInventory.y - 0.5f - inventory.TileGridSize.y / 2f));

            selectedTile.Color = inventory.CanPlaceTile(selectedTile, mouseTilePositionInventory) ? Color.white.SetAlpha(0.5f) : invalidTilePlacementColor;
        }
        else
        {
            selectedTile.rectTransform.position = Input.mousePosition;

            selectedTile.Color = Color.white.SetAlpha(0.5f);
        }
    }

    private void HandlePlaceTileOnPlayer()
    {
        if (!InputManager.Instance.LeftClick || selectedTile == null)
            return;

        Vector2Int mouseTilePosition = GetMouseTilePositionOnPlayer();

        if(player.TryPlaceTile(selectedTile, mouseTilePosition))
        {
            selectedTile.transform.parent = player.transform;
            inventory.RemoveTile(selectedTile);

            selectedTile.Color = Color.white;
            selectedTile = null;
        }
    }

    private void HandlePlaceTileOnInventory()
    {
        if (!InputManager.Instance.LeftClick || selectedTile == null)
            return;

        Vector2Int mouseTilePosition = GetMouseTilePositionOnInventory();

        
        if (inventory.TryPlaceTile(selectedTile, mouseTilePosition))
        {
            selectedTile.transform.parent = inventory.transform;
            player.RemoveTile(selectedTile);

            selectedTile.Color = Color.white;
            selectedTile = null;
        }
    }
}
