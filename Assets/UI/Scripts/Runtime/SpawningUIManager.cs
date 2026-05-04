using System.Collections.Generic;
using Tiles;
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
        TilePlayerManager.Instance.onPositionChangeCurrentSpawning += OnTilePlayerPositionChangeCurrentSpawning;
        TilePlayerManager.Instance.onPositionChangeNextSpawning += OnTilePlayerPositionChangeNextSpawning;
    }
    
    private void OnTilePlayerPositionChangeCurrentSpawning(Dictionary<TileUI, int> currentSpawningTilesAndSize)
    {
        currentSpawningImages.DestroyAndClearList();

        foreach (var tileAndSize in currentSpawningTilesAndSize)
        {
            TileUI tile = tileAndSize.Key;

            Image image = Instantiate(spawnImagePrefab, currentSpawningHolder);
            image.sprite = tile.UnitSO.sprite;

            currentSpawningImages.Add(image);
        }
    }

    private void OnTilePlayerPositionChangeNextSpawning(Dictionary<TileUI, int> nextSpawningTilesAndSize)
    {
        nextSpawningImages.DestroyAndClearList();

        foreach (var tileAndSize in nextSpawningTilesAndSize)
        {
            TileUI tile = tileAndSize.Key;

            Image image = Instantiate(spawnImagePrefab, nextSpawningHolder);
            image.sprite = tile.UnitSO.sprite;

            nextSpawningImages.Add(image);
        }


    }
}
