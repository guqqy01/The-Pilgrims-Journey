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

    public TilePlayer player;

    public Canvas canvas;

    private static Vector2 inventorySizeDeltaOffset = new Vector2(-3, -3);
    private static Vector2 playerSizeDeltaOffset = new Vector2(-3, -3);

    private float playerTileSizeX => (player.GridTransform.sizeDelta.x + playerSizeDeltaOffset.x) / player.TileGridSize.x;
    private float playerTileSizeY => (player.GridTransform.sizeDelta.y + playerSizeDeltaOffset.y) / player.TileGridSize.y;

    private float inventoryTileSizeX => (inventory.GridTransform.sizeDelta.x + inventorySizeDeltaOffset.x) / inventory.TileGridSize.x;
    private float inventoryTileSizeY => (inventory.GridTransform.sizeDelta.y + inventorySizeDeltaOffset.y) / inventory.TileGridSize.y;



    private void Update()
    {
        if (selectedTile != null)
        {

            HandleTileRotation();
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

    private void HandleTileRotation()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            selectedTile.rectTransform.Rotate(new Vector3(0, 0, 90));
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            selectedTile.rectTransform.Rotate(new Vector3(0, 0, -90));
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
            selectedTile.transform.SetParent(canvas.transform, true);
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
            selectedTile.transform.SetParent(canvas.transform, true);
            tile.SelectTile(true);
        }
    }

    private Vector2Int GetMouseTilePositionOnInventory()
    {
        Vector2 mousePosition = Input.mousePosition / canvas.scaleFactor;
        Vector2 inventoryPos = inventory.GridTransform.position / canvas.scaleFactor;
        Vector2 relativePos = mousePosition + (inventory.GridTransform.sizeDelta + inventorySizeDeltaOffset) / 2f - inventoryPos;

        return new Vector2Int(Mathf.CeilToInt(relativePos.x / inventoryTileSizeX), Mathf.CeilToInt(relativePos.y / inventoryTileSizeY));
    }

    private Vector2Int GetMouseTilePositionOnPlayer()
    {
        Vector2 mousePosition = Input.mousePosition / canvas.scaleFactor;
        Vector2 playerPos = player.GridTransform.position / canvas.scaleFactor;
        Vector2 relativePos = mousePosition + (player.GridTransform.sizeDelta + playerSizeDeltaOffset) / 2f - playerPos;

        return new Vector2Int(Mathf.CeilToInt(relativePos.x / playerTileSizeX), Mathf.CeilToInt(relativePos.y / playerTileSizeY));
    }

    private void HandleSelectedTileHover()
    {
        Vector2Int mouseTilePositionPlayer = GetMouseTilePositionOnPlayer();
        Vector2Int mouseTilePositionInventory = GetMouseTilePositionOnInventory();
        if (!player.PointOutOfGrid(mouseTilePositionPlayer))
        {
            selectedTile.rectTransform.position = (Vector2)player.GridTransform.position + canvas.scaleFactor * new Vector2(playerTileSizeX * (mouseTilePositionPlayer.x - 0.5f - player.TileGridSize.x / 2f), playerTileSizeY * (mouseTilePositionPlayer.y - 0.5f - player.TileGridSize.y / 2f)) + selectedTile.Offset;

            selectedTile.Color = player.CanPlaceTile(selectedTile, mouseTilePositionPlayer) ? Color.white.SetAlpha(0.5f) : invalidTilePlacementColor;
        }
        else if (!inventory.PointOutOfGrid(mouseTilePositionInventory))
        {
            selectedTile.rectTransform.position = (Vector2)inventory.GridTransform.position + canvas.scaleFactor * new Vector2(inventoryTileSizeX * (mouseTilePositionInventory.x - 0.5f - inventory.TileGridSize.x / 2f), inventoryTileSizeY * (mouseTilePositionInventory.y - 0.5f - inventory.TileGridSize.y / 2f)) + selectedTile.Offset;

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
            selectedTile.transform.SetParent(player.transform, true);
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
            selectedTile.transform.SetParent(inventory.transform, true);
            player.RemoveTile(selectedTile);

            selectedTile.Color = Color.white;
            selectedTile = null;
        }
    }
}
