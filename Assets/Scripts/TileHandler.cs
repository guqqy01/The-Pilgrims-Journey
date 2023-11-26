using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileHandler : MonoBehaviour
{
    public Tile selectedTile;

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

    private void HandlePlaceTileOnPlayer()
    {
        /*if (selectedTile == null)
            return;*/

        Vector2 mousePosition = Input.mousePosition / canvas.scaleFactor;
        Vector2 PlayerPos = PlayerRectTransform.position / canvas.scaleFactor;
        Vector2 relativePos = mousePosition + PlayerRectTransform.sizeDelta / 2f - PlayerPos;

        Vector2Int mouseTilePosition = new Vector2Int(Mathf.CeilToInt(relativePos.x / playerTileSizeX), Mathf.CeilToInt(relativePos.y / playerTileSizeY));

        if (InputManager.Instance.LeftClick)
        {
            if(player.TryPlaceTile(selectedTile, mouseTilePosition))
            {
                selectedTile = null;
            }
        }
    }
}