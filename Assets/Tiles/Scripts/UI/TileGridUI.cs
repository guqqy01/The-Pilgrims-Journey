using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Tiles.UI
{
    public class TileGridUI : MonoBehaviour, IToBeInitiatedFromRoot
    {
        [SerializeField]
        private RectTransform gridTransform;
        public RectTransform GridTransform => gridTransform;

        [SerializeField]
        protected TileGridBase tileGrid;

        protected List<TileUIBase> tileUIs = new List<TileUIBase>();
        public List<TileUIBase> TileUIs => tileUIs;

        [SerializeField]
        private Vector2 scale;

        public int GridSizeX => tileGrid.GridSize.x;
        public int GridSizeY => tileGrid.GridSize.y;
        

        //One pixel width, 32 pixels per unit
        private static Vector2 SizeDeltaOffset = new Vector2(-3.125f, -3.125f);

        public float SingleTileSquareUISizeX => (GridTransform.sizeDelta.x + 2f * SizeDeltaOffset.x) / tileGrid.GridSize.x;
        public float SingleTileSquareUISizeY => (GridTransform.sizeDelta.y + 2f * SizeDeltaOffset.y) / tileGrid.GridSize.y;

        public Vector2 SingleTileSquareSize => new Vector2(SingleTileSquareUISizeX, SingleTileSquareUISizeY);

        private Sprite _generatedSprite;

        [SerializeField]
        private TileGridUISpriteStyle tileGridUIStyle;

        [SerializeField]
        protected Image gridImage;

        public void InitiateFromRootAtAwake()
        {
            if (tileGrid == null)
                return;

            tileGrid.onTileAdded += OnTileAdded;
            tileGrid.onTileRemoved += OnTileRemoved;
            tileGrid.onTileDestroyed += OnTileDestroyed;

            if (tileGridUIStyle == null)
                return;

            _generatedSprite = TileGridUISpriteBuilder.GetSprite(tileGrid.GridSize, tileGridUIStyle, tileGridUIStyle.SpritePixelSize);
            gridImage.sprite = _generatedSprite;
        }

        private void OnDestroy()
        {
            if(_generatedSprite != null)
            {
                Destroy(_generatedSprite.texture);
                Destroy(_generatedSprite);
            }

            if (tileGrid == null)
                return;

            tileGrid.onTileAdded -= OnTileAdded;
            tileGrid.onTileRemoved -= OnTileRemoved;
            tileGrid.onTileDestroyed -= OnTileDestroyed;
        }

        private Image gridImageForValidation;

        private void OnValidate()
        {
            if(gridTransform == null)
                return;

            if (gridImageForValidation == null)
                gridImageForValidation = gridTransform.GetComponent<Image>();

            if (gridImageForValidation == null || gridImageForValidation.sprite == null)
                return;


            gridTransform.sizeDelta = (100f / gridImageForValidation.sprite.pixelsPerUnit) * (14f * ((Vector2)tileGrid.GridSize) + Vector2.one);
        }

        public bool AcceptsTile(TileUIBase tileUIBase)
        {
            return tileGrid.AcceptTile(tileUIBase.Tile);
        }

        private void OnTileAdded(TileBase tileAdded)
        {
            if (tileAdded == null)
                return;

            if (TryGetTilesUI(tileAdded, out TileUIBase existing))
            {
                existing.UIRotation = tileAdded.Rotation;
                existing.RectTransform.position = TileRectPositionAtVector2IntPosition(existing, tileAdded.PlacedPosition);
                return;
            }

            if (TileUIPrefabManager.Instance.TryGetTileUIBase(tileAdded, out TileUIBase prefab) && prefab != null)
            {
                TileUIBase tileUI = Instantiate(prefab, GridTransform);
                tileUI.BaseInit(tileAdded);
                tileUI.tileGridUI = this;
                tileUI.UIRotation = tileAdded.Rotation;
                tileUI.RectTransform.position = TileRectPositionAtVector2IntPosition(tileUI, tileAdded.PlacedPosition);
                tileUIs.Add(tileUI);
            }
        }

        public bool RemoveTile(TileUIBase tileUI)
        {
            return tileGrid.RemoveTile(tileUI.Tile);
        }

        private void OnTileRemoved(TileBase tileRemoved)
        {
            if (TryGetTilesUI(tileRemoved, out TileUIBase tileUI))
            {
                tileUIs.Remove(tileUI);
            }
        }

        private void OnTileDestroyed(TileBase tileDestroyed)
        {
            if(TryGetTilesUI(tileDestroyed, out TileUIBase tileUI))
            {
                Debug.Log($"Destroying tileUI: {tileUI}");
                TileUIs.Remove(tileUI);
                Destroy(tileUI.gameObject);
            }
            else
            {
                Debug.LogWarning($"Could not Find Tile UI for Tile {tileDestroyed}");
            }
        }

        public bool TryGetTilesUI(TileBase tile, out TileUIBase tileUI)
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

        public bool CanPlaceTile(TileUIBase tileUI, Vector2Int position)
        {
            return tileGrid.CanPlaceTile(tileUI.Tile, position, tileUI.UIRotation);
        }

        public bool CanPlaceTileFromWorldPosition(TileUIBase tileUI, Vector2 worldPosition)
        {
            return tileGrid.CanPlaceTile(tileUI.Tile, GetGridPositon(worldPosition), tileUI.UIRotation);
        }

        public bool TryPlaceTile(TileUIBase tileUI, Vector2Int position, bool invokeAddedTile = true, bool invokeRemovedTile = true)
        {
            return tileGrid.TryPlaceTile(tileUI.Tile, position, tileUI.UIRotation, invokeAddedTile, invokeRemovedTile);
        }

        public bool TryPlaceTileFromWorldPosition(TileUIBase tile, Vector2 worldPosition, bool invokeAddedTile = true, bool invokeRemovedTile = true)
        {
            return TryPlaceTile(tile, GetGridPositon(worldPosition), invokeAddedTile, invokeRemovedTile);
        }

        public void AdoptTileUI(TileUIBase tileUI)
        {
            if (!tileUIs.Contains(tileUI))
                tileUIs.Add(tileUI);
            tileUI.tileGridUI = this;
            tileUI.transform.SetParent(GridTransform, true);
        }

        public Vector2 TileRectPositionAtVector2IntPosition(TileUIBase tileUI, Vector2Int position)
        {
            return (Vector2)GridTransform.position + GridTransform.lossyScale * (SingleTileSquareSize * (position - ((Vector2)tileGrid.GridSize - Vector2.one) / 2f) + tileUI.GetOffset() * SingleTileSquareSize);
        }

        public bool TryGetTileUIAtMousePosition(out TileUIBase tile)
        {
            return TryGetTileUIAtPosition(GetMouseGridPosition(), out tile);
        }

        public bool TryGetTileUIAtPosition(Vector2Int v2Int, out TileUIBase tileUI)
        {
            tileUI = null;

            if (!tileGrid.TryGetTileAtPosition(v2Int, out TileBase tile))
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

