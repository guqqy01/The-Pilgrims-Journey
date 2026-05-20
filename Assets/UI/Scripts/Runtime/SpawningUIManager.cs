using System.Collections.Generic;
using Tiles;
using Units;
using UnityEngine;
using UnityEngine.UI;
public class SpawningUIManager : MonoBehaviour
{
    [SerializeField]
    private Image spawnImagePrefab;

    private List<Image> currentSpawningImages = new List<Image>();
    private List<Image> nextSpawningImages = new List<Image>();

    [SerializeField]
    private Transform currentSpawningHolder;
    [SerializeField]
    private Transform nextSpawningHolder;

    private void Start()
    {
        TilePlayerManager.Instance.onPositionChangeCurrentUnitTiles += OnTilePlayerPositionChangeCurrentSpawning;
        TilePlayerManager.Instance.onPositionChangeNextUnitTiles += OnTilePlayerPositionChangeNextSpawning;
    }
    
    private void OnTilePlayerPositionChangeCurrentSpawning(Dictionary<UnitTile, int> currentSpawningTilesAndSize)
    {
        currentSpawningImages.DestroyAndClearList();

        foreach (var tileAndSize in currentSpawningTilesAndSize)
        {
            UnitTile tile = tileAndSize.Key;

            Image image = Instantiate(spawnImagePrefab, currentSpawningHolder);
            image.sprite = tile.UnitSO.sprite;

            currentSpawningImages.Add(image);
        }
    }

    private void OnTilePlayerPositionChangeNextSpawning(Dictionary<UnitTile, int> nextSpawningTilesAndSize)
    {
        nextSpawningImages.DestroyAndClearList();

        foreach (var tileAndSize in nextSpawningTilesAndSize)
        {
            UnitTile tile = tileAndSize.Key;

            Image image = Instantiate(spawnImagePrefab, nextSpawningHolder);
            image.sprite = tile.UnitSO.sprite;

            nextSpawningImages.Add(image);
        }


    }
}
