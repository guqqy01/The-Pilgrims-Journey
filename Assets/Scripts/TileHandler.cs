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

    private RectTransform playerRect;

    public RectTransform PlayerRect
    {
        get
        {
            if(playerRect == null)
                playerRect = player.GetComponent<RectTransform>();

            return playerRect;
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

        Vector2 v2 = PlayerRect.rect.position - (Vector2)Input.mousePosition;
        Debug.Log(PlayerRect.rect.x);
    }
}
