using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileHandler : MonoBehaviour
{
    public Camera cam;

    public Tile selectedTile;

    public TileInventory inventory;

    public TilePlayer player;

    private RectTransform playerRectTransform;

    public Canvas canvas;

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

        Vector2 Pos = Input.mousePosition / canvas.scaleFactor;
        Vector2 PlayerPos = PlayerRectTransform.position / canvas.scaleFactor;
        Vector2 relativePos = Pos + PlayerRectTransform.sizeDelta / 2f - PlayerPos;

        Vector2Int playerSize = player.tilePlayerSize;

        Vector2 tileSize = new Vector2(PlayerRectTransform.sizeDelta.x/ playerSize.x, PlayerRectTransform.sizeDelta.y / playerSize.y);

        Vector2Int MouseTilePosition = new Vector2Int(Mathf.CeilToInt(relativePos.x / tileSize.x), Mathf.CeilToInt(relativePos.y / tileSize.y));


    }
}