using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
namespace Tiles
{

    public class TileGridUI<TItem> : MonoBehaviour, IToBeInitiatedFromRoot
    {
        [SerializeField]
        private RectTransform gridTransform;
        public RectTransform GridTransform => gridTransform;

        [SerializeField]
        private TileGrid<TItem> tileGrid;

        protected List<TileUI<TItem>> tileUIs = new List<TileUI<TItem>>();
        public List<TileUI<TItem>> TileUIs => tileUIs;

        public int GridSizeX => tileGrid.GridSize.x;
        public int GridSizeY => tileGrid.GridSize.y;
        

        //One pixel width, 32 pixels per unit
        private static Vector2 SizeDeltaOffset = new Vector2(-3.125f, -3.125f);

        public float SingleTileSquareUISizeX => (GridTransform.sizeDelta.x + SizeDeltaOffset.x) / tileGrid.GridSize.x;
        public float SingleTileSquareUISizeY => (GridTransform.sizeDelta.y + SizeDeltaOffset.y) / tileGrid.GridSize.y;

        public Vector2 SingleTileSquareSize => new Vector2(SingleTileSquareUISizeX, SingleTileSquareUISizeY);

        public void InitiateFromRootAtAwake()
        {
            if (tileGrid == null)
                return;

            tileGrid.onTileAdded += OnTileAdded;
            tileGrid.onTileRemoved += OnTileRemoved;
            tileGrid.onTileDestroyed += OnTileDestroyed;
        }

        private void OnDestroy()
        {
            if (tileGrid == null)
                return;

            tileGrid.onTileAdded -= OnTileAdded;
            tileGrid.onTileRemoved -= OnTileRemoved;
            tileGrid.onTileDestroyed -= OnTileDestroyed;
        }

        private Image gridImageForValidation;

        private void OnValidate()
        {
            if (gridImageForValidation == null)
                gridImageForValidation = gridTransform.GetComponent<Image>();

            if (gridImageForValidation == null || gridImageForValidation.sprite == null)
                return;


            gridTransform.sizeDelta = (100f / gridImageForValidation.sprite.pixelsPerUnit) * (14f * ((Vector2)tileGrid.GridSize) + Vector2.one);
        }

        private void OnTileAdded(Tile<TItem> tileAdded)
        {
            if (TryGetTilesUI(tileAdded, out TileUI<TItem> existing))
            {
                existing.PlacedPosition = tileAdded.PlacedPosition;
                existing.PlacedRotation = tileAdded.Rotation;
                existing.UIRotation = tileAdded.Rotation;
                existing.RectTransform.position = TileRectPositionAtVector2IntPosition(existing, tileAdded.PlacedPosition);
                return;
            }

            if (TileUIPrefabManager<TItem>.Instance.TileSOAndPrefab.TryGetValue(tileAdded.TileSO, out TileUI<TItem> prefab) && prefab != null)
            {
                TileUI<TItem> tileUI = Instantiate(prefab, GridTransform);
                tileUI.BaseInit(tileAdded);
                tileUI.tileGridUI = this;
                tileUI.PlacedPosition = tileAdded.PlacedPosition;
                tileUI.PlacedRotation = tileAdded.Rotation;
                tileUI.UIRotation = tileAdded.Rotation;
                tileUI.RectTransform.position = TileRectPositionAtVector2IntPosition(tileUI, tileAdded.PlacedPosition);
                tileUIs.Add(tileUI);
            }
        }

        public bool RemoveTile(TileUI<TItem> tileUI)
        {
            return tileGrid.RemoveTile(tileUI.Tile);
        }

        private void OnTileRemoved(Tile<TItem> tileRemoved)
        {
            if (TryGetTilesUI(tileRemoved, out TileUI<TItem> tileUI))
            {
                tileUIs.Remove(tileUI);
            }
        }

        private void OnTileDestroyed(Tile<TItem> tileDestroyed)
        {
            if(TryGetTilesUI(tileDestroyed, out TileUI<TItem> tileUI))
            {
                TileUIs.Remove(tileUI);
                Destroy(tileUI.gameObject);
            }
        }

        public bool TryGetTilesUI(Tile<TItem> tile, out TileUI<TItem> tileUI)
        {
            tileUI = tileUIs.SingleOrDefault(x => x.Tile == tile);
            return tileUI != null;
        }

        public void Clear()
        {
            foreach (var tile in TileUIs)
            {
                if (tile != null)
                    Destroy(tile.gameObject);
            }

            tileUIs.Clear();
        }

        public bool PointOutOfGrid(Vector2Int point) => point.x >= tileGrid.GridSize.x || point.x < 0 || point.y >= tileGrid.GridSize.y || point.y < 0;

        public bool CanPlaceTile(TileUI<TItem> tileUI, Vector2Int position)
        {
            return tileGrid.CanPlaceTile(tileUI.Tile, position, tileUI.UIRotation);
        }

        public bool CanPlaceTileFromWorldPosition(TileUI<TItem> tileUI, Vector2 worldPosition)
        {
            return tileGrid.CanPlaceTile(tileUI.Tile, GetGridPositon(worldPosition), tileUI.UIRotation);
        }

        public bool TryPlaceTile(TileUI<TItem> tileUI, Vector2Int position)
        {
            return tileGrid.TryPlaceTile(tileUI.Tile, position, tileUI.UIRotation);
        }

        public bool TryPlaceTileFromWorldPosition(TileUI<TItem> tile, Vector2 worldPosition)
        {
            return TryPlaceTile(tile, GetGridPositon(worldPosition));
        }

        public void AdoptTileUI(TileUI<TItem> tileUI)
        {
            if (!tileUIs.Contains(tileUI))
                tileUIs.Add(tileUI);
            tileUI.tileGridUI = this;
            tileUI.transform.SetParent(GridTransform, true);
        }

        public Vector2 TileRectPositionAtVector2IntPosition(TileUI<TItem> tile, Vector2Int position)
        {
            return (Vector2)GridTransform.position + GridTransform.lossyScale * (SingleTileSquareSize * (position - ((Vector2)tileGrid.GridSize - Vector2.one) / 2f) + tile.GetOffset() * SingleTileSquareSize);
        }

        public bool TryGetTileUIAtMousePosition(out TileUI<TItem> tile)
        {
            return TryGetTileUIAtPosition(GetMouseGridPosition(), out tile);
        }

        public bool TryGetTileUIAtPosition(Vector2Int v2Int, out TileUI<TItem> tileUI)
        {
            tileUI = null;

            if (!tileGrid.TryGetTileAtPosition(v2Int, out Tile<TItem> tile))
                return false;

            return TryGetTilesUI(tile, out tileUI);
        }

        public Vector2Int GetGridPositon(Vector2 worldPosition)
        {
            Vector2 relativePos = (worldPosition - (Vector2)GridTransform.position) / GridTransform.lossyScale.x + (GridTransform.sizeDelta + SizeDeltaOffset) / 2f;
            return new Vector2Int(Mathf.CeilToInt(relativePos.x / SingleTileSquareUISizeX), Mathf.CeilToInt(relativePos.y / SingleTileSquareUISizeY)) - Vector2Int.one;
        }

        public Vector2Int GetMouseGridPosition()
        {
            return GetGridPositon(Input.mousePosition);
        }

        public bool IsWorldPositionInGrid(Vector2 position)
        {
            return !PointOutOfGrid(GetGridPositon(position));
        }

        
    }

}

