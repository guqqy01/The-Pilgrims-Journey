using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEditor;

namespace Tiles
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


    public static class TileGridUISpriteBuilder
    {
        public static Sprite GetSprite(Vector2Int size, TileGridUISpriteStyle gridStyle, float uiCellSize)
        {
            if (size == null || size.x == 0 || size.y == 0)
            {
                Debug.LogWarning($"size is empty or zero.");
                return null;
            }

            if (!ValidateStyle(gridStyle)) return null;

            CachePixels(gridStyle,
            out Color[] backgroundFillPixels,
            out Color[] outerCornerPixels,
            out Color[] innerCornerPixels,
            out Color[] farOuterCornerPixels,
            out Color[] verticalSeperationPixels,
            out Color[] horizontalSeperationPixels);



            return gridStyle.BackgroundFillSprite;
        }

        private static void CachePixels(TileGridUISpriteStyle gridStyle, 
            out Color[] backgroundFillPixels, 
            out Color[] outerCornerPixels, 
            out Color[] innerCornerPixels, 
            out Color[] farOuterCornerPixels, 
            out Color[] verticalSeperationPixels,
            out Color[] horizontalSeperationPixels)
        {
            backgroundFillPixels = GetSpritePixels(gridStyle.BackgroundFillSprite);
            outerCornerPixels = GetSpritePixels(gridStyle.OuterCornerSprites);
            innerCornerPixels = GetSpritePixels(gridStyle.InnerCornerSprites);
            farOuterCornerPixels = GetSpritePixels(gridStyle.FarOuterCornerSprites);
            verticalSeperationPixels = GetSpritePixels(gridStyle.VerticalSeperationSprites);
            horizontalSeperationPixels = GetSpritePixels(gridStyle.HorizontalSeperationSprites);
        }

        private static Color[] GetSpritePixels(Sprite sprite)
        {
            Rect rect = sprite.textureRect;
            return sprite.texture.GetPixels(
                (int)rect.x,
                (int)rect.y,
                (int)rect.width,
                (int)rect.height);
        }

        private static bool ValidateStyle(TileGridUISpriteStyle uiStyle)
        {
            bool ok = true;
            ok &= AssertSprite(uiStyle.BackgroundFillSprite, nameof(uiStyle.BackgroundFillSprite));
            ok &= AssertSprite(uiStyle.OuterCornerSprites, nameof(uiStyle.OuterCornerSprites));
            ok &= AssertSprite(uiStyle.InnerCornerSprites, nameof(uiStyle.InnerCornerSprites));
            ok &= AssertSprite(uiStyle.FarOuterCornerSprites, nameof(uiStyle.FarOuterCornerSprites));
            ok &= AssertSprite(uiStyle.VerticalSeperationSprites, nameof(uiStyle.VerticalSeperationSprites));
            ok &= AssertSprite(uiStyle.HorizontalSeperationSprites, nameof(uiStyle.HorizontalSeperationSprites));
            return ok;
        }

        private static bool AssertSprite(Sprite sprite, string fieldName)
        {
            if (sprite != null) return true;
            Debug.LogError($"{fieldName} in style is not assigned.");
            return false;
        }

    }

    public class TileGridUISpriteStyle : ScriptableObject 
    {
        [SerializeField]
        private int spritePixelSize;
        private int previousSpritePixelSize;
        public int SpritePixelSize => spritePixelSize;

        public Sprite BackgroundFillSprite;
        public Sprite OuterCornerSprites;
        public Sprite InnerCornerSprites;
        public Sprite FarOuterCornerSprites;
        public Sprite VerticalSeperationSprites;
        public Sprite HorizontalSeperationSprites;

        private Sprite previousBackgroundFillSprite;
        private Sprite previousOuterCornerSprites;
        private Sprite previousInnerCornerSprites;
        private Sprite previousFarOuterCornerSprites;
        private Sprite previousVerticalSeperationSprites;
        private Sprite previousHorizontalSeperationSprites;

        private void OnValidate()
        {
            if (previousSpritePixelSize != spritePixelSize)
            {
                previousSpritePixelSize = spritePixelSize;
                MakeSureSpriteHasCorrectSize(ref BackgroundFillSprite);
                MakeSureSpriteHasCorrectSize(ref OuterCornerSprites);
                MakeSureSpriteHasCorrectSize(ref InnerCornerSprites);
                MakeSureSpriteHasCorrectSize(ref FarOuterCornerSprites);
                MakeSureSpriteHasCorrectSize(ref VerticalSeperationSprites);
                MakeSureSpriteHasCorrectSize(ref HorizontalSeperationSprites);
            }

            IfDifferentCheckIfValidIfNotRevert(ref BackgroundFillSprite, ref previousBackgroundFillSprite);
            IfDifferentCheckIfValidIfNotRevert(ref OuterCornerSprites, ref previousOuterCornerSprites);
            IfDifferentCheckIfValidIfNotRevert(ref InnerCornerSprites, ref previousInnerCornerSprites);
            IfDifferentCheckIfValidIfNotRevert(ref FarOuterCornerSprites, ref previousFarOuterCornerSprites);
            IfDifferentCheckIfValidIfNotRevert(ref VerticalSeperationSprites, ref previousVerticalSeperationSprites);
            IfDifferentCheckIfValidIfNotRevert(ref HorizontalSeperationSprites, ref previousHorizontalSeperationSprites);
        }

        private void IfDifferentCheckIfValidIfNotRevert(ref Sprite newSprite, ref Sprite previous)
        {
            if (previous != newSprite && MakeSureSpriteHasCorrectSize(ref newSprite))
            {
                previous = newSprite;
            }
        }

        private bool MakeSureSpriteHasCorrectSize(ref Sprite sprite)
        {
            if (sprite == null)
                return true;

            if (sprite.rect.size.x != spritePixelSize || sprite.rect.size.y != spritePixelSize)
            {
                Debug.LogError($"Sprite {sprite.name} has size {sprite.rect.size.x}×{sprite.rect.size.y} pixels, but must be {spritePixelSize}×{spritePixelSize} pixels.");
                sprite = null;
                return false;
            }

            return true;
        }
    }
}

