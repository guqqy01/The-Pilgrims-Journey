using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    private InputManager inputManager;

    [SerializeField]
    private GameObject inventoryGO;

    [SerializeField]
    private Tile selectedTile;
    [SerializeField]
    private Tile lastSelectedTile;
  
    [SerializeField]
    private EquipmentContainer selectedContainer;

    [SerializeField]
    public EquipmentContainer SelectedTileEquipmentContainer1;
    [SerializeField]
    public EquipmentContainer SelectedTileEquipmentContainer2;
    [SerializeField]
    public EquipmentContainer SelectedTileEquipmentContainer3;

    [SerializeField]
    private Color invalidTilePlacementColor;

    public TileGrid tileInventory;

    public TilePlayer tilePlayer;

    public Canvas canvas;

    private static Vector2 inventorySizeDeltaOffset = new Vector2(-3, -3);
    private static Vector2 playerSizeDeltaOffset = new Vector2(-3, -3);

    private float playerTileSizeX => (tilePlayer.GridTransform.sizeDelta.x + playerSizeDeltaOffset.x) / tilePlayer.TileGridSize.x;
    private float playerTileSizeY => (tilePlayer.GridTransform.sizeDelta.y + playerSizeDeltaOffset.y) / tilePlayer.TileGridSize.y;

    private float inventoryTileSizeX => (tileInventory.GridTransform.sizeDelta.x + inventorySizeDeltaOffset.x) / tileInventory.TileGridSize.x;
    private float inventoryTileSizeY => (tileInventory.GridTransform.sizeDelta.y + inventorySizeDeltaOffset.y) / tileInventory.TileGridSize.y;

    private void Start()
    {
        inputManager = InputManager.Instance;
    }

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (inputManager.OpenCloseInventory)
        {
            inventoryGO.SetActive(!inventoryGO.activeInHierarchy);
        }

        HandleTiles();
    }

    private void SelectTile(Tile tile)
    {
        selectedTile = tile;
        selectedTile.transform.SetParent(canvas.transform, true);
        tile.SelectTile(true);

        lastSelectedTile = tile;

        SelectedTileEquipmentContainer1.EquipmentSO = tile.equipment1;
        SelectedTileEquipmentContainer2.EquipmentSO = tile.equipment2;
        SelectedTileEquipmentContainer3.EquipmentSO = tile.equipment3;
    }

    private void HandleTiles()
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

    public void EquipmentContainerSelectEvent(EquipmentContainer container)
    {
        if (!container.Active)
            return;

        if (selectedContainer != null)
        {
            if (container == selectedContainer)
            {
                selectedContainer.Deselect();
                selectedContainer = null;
                return;
            }

            if (container is PrizeContainer prizeContainer)
            {
                if (prizeContainer.AllowedEquipment != null && prizeContainer.AllowedEquipment != selectedContainer.EquipmentSO)
                {
                    //Fail to change equipment
                    return;
                }
            }

            EquipmentSO holder = container.EquipmentSO;

            container.EquipmentSO = selectedContainer.EquipmentSO;
            
            selectedContainer.EquipmentSO = holder;

            selectedContainer.Deselect();
            selectedContainer = null;
        }
        else
        {
            if (container.EquipmentSO == null)
                return;

            selectedContainer = container;
            selectedContainer.Select();
        }
    }


    private void HandleTileRotation()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            selectedTile.Rotate(90);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            selectedTile.Rotate(-90);
        }
    }

    private void HandleSelectTileOnPlayer()
    {
        if (!InputManager.Instance.LeftClick)
            return;

        Vector2Int playerMousePos = GetMouseTilePositionOnPlayer();
        if (tilePlayer.TryGetTileAtPosition(playerMousePos, out Tile tile) && !tile.locked)
        {
            SelectTile(tile);
        }
    }

    private void HandleSelectTileOnInventory()
    {
        if (!InputManager.Instance.LeftClick)
            return;

        Vector2Int inventoryMousePos = GetMouseTilePositionOnInventory();
        if (tileInventory.TryGetTileAtPosition(inventoryMousePos, out Tile tile))
        {
            SelectTile(tile);
        }
    }

    private Vector2Int GetMouseTilePositionOnInventory()
    {
        Vector2 mousePosition = Input.mousePosition / canvas.scaleFactor;
        Vector2 inventoryPos = tileInventory.GridTransform.position / canvas.scaleFactor;
        Vector2 relativePos = mousePosition + (tileInventory.GridTransform.sizeDelta + inventorySizeDeltaOffset) / 2f - inventoryPos;

        return new Vector2Int(Mathf.CeilToInt(relativePos.x / inventoryTileSizeX), Mathf.CeilToInt(relativePos.y / inventoryTileSizeY));
    }

    private Vector2Int GetMouseTilePositionOnPlayer()
    {
        Vector2 mousePosition = Input.mousePosition / canvas.scaleFactor;
        Vector2 playerPos = tilePlayer.GridTransform.position / canvas.scaleFactor;
        Vector2 relativePos = mousePosition + (tilePlayer.GridTransform.sizeDelta + playerSizeDeltaOffset) / 2f - playerPos;

        return new Vector2Int(Mathf.CeilToInt(relativePos.x / playerTileSizeX), Mathf.CeilToInt(relativePos.y / playerTileSizeY));
    }

    private void HandleSelectedTileHover()
    {
        Vector2Int mouseTilePositionPlayer = GetMouseTilePositionOnPlayer();
        Vector2Int mouseTilePositionInventory = GetMouseTilePositionOnInventory();
        if (!tilePlayer.PointOutOfGrid(mouseTilePositionPlayer))
        {
            selectedTile.rectTransform.position = (Vector2)tilePlayer.GridTransform.position + canvas.scaleFactor * new Vector2(playerTileSizeX * (mouseTilePositionPlayer.x - 0.5f - tilePlayer.TileGridSize.x / 2f), playerTileSizeY * (mouseTilePositionPlayer.y - 0.5f - tilePlayer.TileGridSize.y / 2f)) + selectedTile.Offset;

            selectedTile.Color = tilePlayer.CanPlaceTile(selectedTile, mouseTilePositionPlayer) ? Color.white.SetAlpha(0.5f) : invalidTilePlacementColor;
        }
        else if (!tileInventory.PointOutOfGrid(mouseTilePositionInventory))
        {
            selectedTile.rectTransform.position = (Vector2)tileInventory.GridTransform.position + canvas.scaleFactor * new Vector2(inventoryTileSizeX * (mouseTilePositionInventory.x - 0.5f - tileInventory.TileGridSize.x / 2f), inventoryTileSizeY * (mouseTilePositionInventory.y - 0.5f - tileInventory.TileGridSize.y / 2f)) + selectedTile.Offset;

            selectedTile.Color = tileInventory.CanPlaceTile(selectedTile, mouseTilePositionInventory) ? Color.white.SetAlpha(0.5f) : invalidTilePlacementColor;
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

        if (tilePlayer.TryPlaceTile(selectedTile, mouseTilePosition))
        {
            selectedTile.transform.SetParent(tilePlayer.transform, true);
            tileInventory.RemoveTile(selectedTile);

            selectedTile.Color = Color.white;
            selectedTile = null;
        }
    }

    private void HandlePlaceTileOnInventory()
    {
        if (!InputManager.Instance.LeftClick || selectedTile == null)
            return;

        Vector2Int mouseTilePosition = GetMouseTilePositionOnInventory();


        if (tileInventory.TryPlaceTile(selectedTile, mouseTilePosition))
        {
            selectedTile.transform.SetParent(tileInventory.transform, true);
            tilePlayer.RemoveTile(selectedTile);

            selectedTile.Color = Color.white;
            selectedTile = null;
        }
    }

    
}
