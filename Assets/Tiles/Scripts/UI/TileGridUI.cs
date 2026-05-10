using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using Unity.VisualScripting;

namespace Tiles
{

    public class TileGridUI : MonoBehaviour, IToBeInitiatedFromRoot
    {
        [SerializeField]
        private RectTransform gridTransform;
        public RectTransform GridTransform => gridTransform;

        [SerializeField]
        private TileGrid tileGrid;

        protected List<TileUI> tileUIs = new List<TileUI>();
        public List<TileUI> TileUIs => tileUIs;

        

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

        private void OnTileAdded(Tile tileAdded)
        {
            if (TryGetTilesUI(tileAdded, out TileUI existing))
            {
                existing.PlacedPosition = tileAdded.PlacedPosition;
                existing.PlacedRotation = tileAdded.Rotation;
                existing.UIRotation = tileAdded.Rotation;
                existing.RectTransform.position = TileRectPositionAtVector2IntPosition(existing, tileAdded.PlacedPosition);
                return;
            }

            TileUI tileUI = Instantiate(tileAdded.TileSO.tilePrefab, GridTransform);
            tileUI.BaseInit(tileAdded);
            tileUI.tileGridUI = this;
            tileUI.PlacedPosition = tileAdded.PlacedPosition;
            tileUI.PlacedRotation = tileAdded.Rotation;
            tileUI.UIRotation = tileAdded.Rotation;
            tileUI.RectTransform.position = TileRectPositionAtVector2IntPosition(tileUI, tileAdded.PlacedPosition);
            tileUIs.Add(tileUI);
        }

        public bool RemoveTile(TileUI tileUI)
        {
            return tileGrid.RemoveTile(tileUI.Tile);
        }

        private void OnTileRemoved(Tile tileRemoved)
        {
            if (TryGetTilesUI(tileRemoved, out TileUI tileUI))
            {
                tileUIs.Remove(tileUI);
            }
        }

        private void OnTileDestroyed(Tile tileDestroyed)
        {
            if(TryGetTilesUI(tileDestroyed, out TileUI tileUI))
            {
                TileUIs.Remove(tileUI);
                Destroy(tileUI.gameObject);
            }
        }

        public bool TryGetTilesUI(Tile tile, out TileUI tileUI)
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

        public bool CanPlaceTile(TileUI tileUI, Vector2Int position)
        {
            return tileGrid.CanPlaceTile(tileUI.Tile, position, tileUI.UIRotation);
        }

        public bool CanPlaceTileFromWorldPosition(TileUI tileUI, Vector2 worldPosition)
        {
            return tileGrid.CanPlaceTile(tileUI.Tile, GetGridPositon(worldPosition), tileUI.UIRotation);
        }

        public bool TryPlaceTile(TileUI tileUI, Vector2Int position)
        {
            return tileGrid.TryPlaceTile(tileUI.Tile, position, tileUI.UIRotation);
        }

        public bool TryPlaceTileFromWorldPosition(TileUI tile, Vector2 worldPosition)
        {
            return TryPlaceTile(tile, GetGridPositon(worldPosition));
        }

        public void AdoptTileUI(TileUI tileUI)
        {
            if (!tileUIs.Contains(tileUI))
                tileUIs.Add(tileUI);
            tileUI.tileGridUI = this;
            tileUI.transform.SetParent(GridTransform, true);
        }

        public Vector2 TileRectPositionAtVector2IntPosition(TileUI tile, Vector2Int position)
        {
            return (Vector2)GridTransform.position + GridTransform.lossyScale * (SingleTileSquareSize * (position - ((Vector2)tileGrid.GridSize - Vector2.one) / 2f) + tile.GetOffset() * SingleTileSquareSize);
        }

        public bool TryGetTileUIAtMousePosition(out TileUI tile)
        {
            return TryGetTileUIAtPosition(GetMouseGridPosition(), out tile);
        }

        public bool TryGetTileUIAtPosition(Vector2Int v2Int, out TileUI tileUI)
        {
            tileUI = null;

            if (!tileGrid.TryGetTileAtPosition(v2Int, out Tile tile))
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

