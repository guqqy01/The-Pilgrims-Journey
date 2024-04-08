using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    private InputManager inputManager;

    private TileStatsUI tileStatsUI;

    [SerializeField]
    private EvolutionWindow evolutionWindow;

    [SerializeField]
    private GameObject inventoryGO;

    [SerializeField]
    private Tile movingTile;
    [SerializeField]
    private Tile selectedTile;

    private bool prizeTileSelected;

    [SerializeField]
    private EquipmentContainer selectedContainer;

    [SerializeField]
    private Tile hoverTile;

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
        tileStatsUI = GetComponentInChildren<TileStatsUI>();
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

    public void OpenEvolutionWindow()
    {
        evolutionWindow.gameObject.SetActive(true);
        evolutionWindow.Init(selectedTile);
    }

    private void MoveTile(Tile tile)
    {
        movingTile = tile;
        
        if (!prizeTileSelected)
            SelectTile(tile);
    }

    private void SelectTile(Tile tile)
    {
        if (selectedTile != null)
            selectedTile.Highlight(false);

        selectedTile = tile;

        tileStatsUI.TileSelected(tile);

        tile.Highlight(true);
    }

    private void HandleTiles()
    {
        if (movingTile != null)
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

        HandleCancelMoveTile();
    }

    public void EquipmentContainerSelectEvent(EquipmentContainer container)
    {
        if (!container.Active)
            return;

        if (selectedContainer != null)
        {
            if (container == selectedContainer)
            {
                DeselectContainer();
                return;
            }

            if (container is EquipmentContainer prizeContainer)
            {
                if (prizeContainer.AllowedEquipment != null && prizeContainer.AllowedEquipment != selectedContainer.EquipmentSO)
                {
                    //Fail to change equipment
                    return;
                }
            }

            if (container.CanChangeEquipment(selectedContainer.EquipmentSO) && selectedContainer.CanChangeEquipment(container.EquipmentSO))
            {
                EquipmentSO holder = container.EquipmentSO;

                container.TryChangeEquipment(selectedContainer.EquipmentSO);

                selectedContainer.TryChangeEquipment(holder);

                DeselectContainer();
            }
        }
        else
        {
            if (container.EquipmentSO == null)
                return;

            selectedContainer = container;
            selectedContainer.Select();
        }
    }

    public void DeselectContainer()
    {
        if (selectedContainer == null)
            return;

        selectedContainer.Deselect();
        selectedContainer = null;
    }


    private void HandleCancelMoveTile()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
            return;

        if (prizeTileSelected)
            prizeTileSelected = false;

        if (movingTile != null)
        {
            movingTile = null;
            if (hoverTile != null)
            {
                Destroy(hoverTile.gameObject);
                hoverTile = null;
            }
        }
        else if (selectedTile != null)
        {
            selectedTile.Highlight(false);
            selectedTile = null;

            tileStatsUI.DeselectTile();
        }
    }

    private void HandleTileRotation()
    {
        if (hoverTile == null)
            return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            hoverTile.Rotate(90);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            hoverTile.Rotate(-90);
        }
    }

    private void HandleSelectTileOnPlayer()
    {
        if (!InputManager.Instance.LeftClick)
            return;

        if (!tilePlayer.GridTransform.gameObject.activeInHierarchy)
            return;

        Vector2Int playerMousePos = GetMouseTilePositionOnPlayer();
        if (tilePlayer.TryGetTileAtPosition(playerMousePos, out Tile tile))
        {
            if (!tile.locked && Input.GetKey(KeyCode.LeftShift))
            {
                MoveTile(tile);
            }
            else
            {
                SelectTile(tile);
            }
        }
    }

    private void HandleSelectTileOnInventory()
    {
        if (!InputManager.Instance.LeftClick)
            return;

        if (!tileInventory.GridTransform.gameObject.activeInHierarchy)
            return;

        Vector2Int inventoryMousePos = GetMouseTilePositionOnInventory();
        if (tileInventory.TryGetTileAtPosition(inventoryMousePos, out Tile tile))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                MoveTile(tile);
            }
            else
            {
                SelectTile(tile);
            }
        }
    }

    public void SelectTileOnPrizeWindow()
    {
        PrizeWindow pw = PrizeWindow.instance;
        if (pw.TileTaken == true)
            return;

        prizeTileSelected = true;
        MoveTile(pw.prizeTile);

        pw.prizeTile.Highlight(true);
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
        if (!tilePlayer.PointOutOfGrid(mouseTilePositionPlayer) && tilePlayer.GridTransform.gameObject.activeInHierarchy)
        {
            if (hoverTile == null)
            {
                Vector3 position = (Vector2)tilePlayer.GridTransform.position + canvas.scaleFactor * new Vector2(playerTileSizeX * (mouseTilePositionPlayer.x - 0.5f - tilePlayer.TileGridSize.x / 2f), playerTileSizeY * (mouseTilePositionPlayer.y - 0.5f - tilePlayer.TileGridSize.y / 2f)) + movingTile.Offset;
                hoverTile = Instantiate(movingTile, position, movingTile.rectTransform.rotation, canvas.transform);
                hoverTile.Highlight(false);
            }
            
            hoverTile.rectTransform.position = (Vector2)tilePlayer.GridTransform.position + canvas.scaleFactor * new Vector2(playerTileSizeX * (mouseTilePositionPlayer.x - 0.5f - tilePlayer.TileGridSize.x / 2f), playerTileSizeY * (mouseTilePositionPlayer.y - 0.5f - tilePlayer.TileGridSize.y / 2f)) + hoverTile.Offset;
            
            hoverTile.Color = tilePlayer.HoverTileCanPlace(hoverTile, movingTile, mouseTilePositionPlayer) ? Color.white.SetAlpha(0.5f) : invalidTilePlacementColor;
        }
        else if (!tileInventory.PointOutOfGrid(mouseTilePositionInventory) && tileInventory.GridTransform.gameObject.activeInHierarchy)
        {
            if (hoverTile == null)
            {
                Vector3 position = (Vector2)tileInventory.GridTransform.position + canvas.scaleFactor * new Vector2(inventoryTileSizeX * (mouseTilePositionInventory.x - 0.5f - tileInventory.TileGridSize.x / 2f), inventoryTileSizeY * (mouseTilePositionInventory.y - 0.5f - tileInventory.TileGridSize.y / 2f)) + movingTile.Offset;
                hoverTile = Instantiate(movingTile, position, movingTile.rectTransform.rotation, canvas.transform);
                hoverTile.Highlight(false);
            }
           
            hoverTile.rectTransform.position = (Vector2)tileInventory.GridTransform.position + canvas.scaleFactor * new Vector2(inventoryTileSizeX * (mouseTilePositionInventory.x - 0.5f - tileInventory.TileGridSize.x / 2f), inventoryTileSizeY * (mouseTilePositionInventory.y - 0.5f - tileInventory.TileGridSize.y / 2f)) + hoverTile.Offset;

            hoverTile.Color = tileInventory.HoverTileCanPlace(hoverTile, movingTile, mouseTilePositionInventory) ? Color.white.SetAlpha(0.5f) : invalidTilePlacementColor;
        }
        else if (hoverTile != null)
        {
            Destroy(hoverTile.gameObject);
            hoverTile = null;
        }
    }

    private void HandlePlaceTileOnPlayer()
    {
        if (!InputManager.Instance.LeftClick || movingTile == null)
            return;

        if (!tilePlayer.GridTransform.gameObject.activeInHierarchy)
            return;

        Vector2Int mouseTilePosition = GetMouseTilePositionOnPlayer();

        if (tilePlayer.PointOutOfGrid(mouseTilePosition))
            return;

        float rotationHolder = movingTile.rectTransform.rotation.eulerAngles.z;
        movingTile.RotateTo(hoverTile.rectTransform.rotation.eulerAngles.z);

        if (tilePlayer.TryPlaceTile(movingTile, mouseTilePosition))
        {
            movingTile.transform.SetParent(tilePlayer.GridTransform, true);
            tileInventory.RemoveTile(movingTile);

            movingTile.rectTransform.position = hoverTile.rectTransform.position;
            
            if (prizeTileSelected)
            {
                PrizeWindow.instance.TileTaken = true;
                prizeTileSelected = false;
                MoveTile(movingTile);
            }

            movingTile = null;

            Destroy(hoverTile.gameObject);
        }
        else
        {
            movingTile.RotateTo(rotationHolder);
        }
    }

    private void HandlePlaceTileOnInventory()
    {
        if (!InputManager.Instance.LeftClick || movingTile == null)
            return;

        if (!tileInventory.GridTransform.gameObject.activeInHierarchy)
            return;

        Vector2Int mouseTilePosition = GetMouseTilePositionOnInventory();

        if (tileInventory.PointOutOfGrid(mouseTilePosition))
            return;

        float rotationHolder = movingTile.rectTransform.rotation.eulerAngles.z;
        movingTile.RotateTo(hoverTile.rectTransform.rotation.eulerAngles.z);

        if (tileInventory.TryPlaceTile(movingTile, mouseTilePosition))
        {
            movingTile.transform.SetParent(tileInventory.GridTransform, true);
            tilePlayer.RemoveTile(movingTile);

            movingTile.rectTransform.position = hoverTile.rectTransform.position;

            if (prizeTileSelected)
            {
                PrizeWindow.instance.TileTaken = true;
                prizeTileSelected = false;
                MoveTile(movingTile);
            }

            movingTile = null;

            Destroy(hoverTile.gameObject);
        }
        else
        {
            movingTile.RotateTo(rotationHolder);
        }
    }

    
}
