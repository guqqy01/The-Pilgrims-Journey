using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Linq;

namespace Tiles
{
    public abstract class TileUIBase : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler, IComparable
    {
        protected const float _singleTileSize = 43.75f;

        [SerializeField]
        protected TileShapeTextureBuilder tileShapeTextureBuilder;
        public RectTransform RectTransform { private set; get; }

        public TileBase Tile { private set; get; }

        [SerializeField]
        private Image tileGraphics;

        [HideInInspector]
        public bool locked;

        [HideInInspector]
        public TileGridUI tileGridUI;

        public float BaseRotation => Tile.Rotation;

        public float UIRotation
        {
            get
            {
                if(RectTransform == null)
                    RectTransform = GetComponent<RectTransform>();

                return RectTransform.eulerAngles.z;
            }
            set
            {
                RotateTo(value);
            }
        }

        public List<Vector2Int> RotatedShape
        {
            get
            {
                return Tile.Shape.Rotate(UIRotation);
            }
        }

        public Color Color
        {
            set
            {
                if (tileGraphics != null)
                    tileGraphics.color = value;
            }
        }

        public bool Selected { private set; get; } = false;

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            TileUIEventBus.OnTileSelected += Highlight;
        }

        private void Update()
        {
            HandleCancelMoveTile();

            if (isDragging)
            {
                HandleTileRotation();
                HandlePosition();
            }
        }

        private void Highlight(TileUIBase tile)
        {
            Highlight(tile == this);
        }

        private void OnDestroy()
        {
            TileUIEventBus.OnTileSelected -= Highlight;
        }

        #region Leveling and health

        

        public void DestroyTile()
        {
            Destroy(gameObject);
        }

        #endregion

        #region Dragging

        [SerializeField]
        private CanvasGroup canvasGroup;
        private bool isDragging;
        [HideInInspector]
        public TileGridUI draggedTileGridUI;

        public Vector2Int BasePlacedPosition => Tile.PlacedPosition;

        private Vector2Int currentTilePosition;
        private TileGridUI currentTileGridUI;

        public virtual void Rotate(int angle)
        {
            RectTransform.Rotate(new Vector3(0, 0, angle));
        }

        public virtual void RotateTo(float angle)
        {
            if (RectTransform == null)
                RectTransform = GetComponent<RectTransform>();

            Vector3 eulerAngles = RectTransform.rotation.eulerAngles;
            eulerAngles.z = angle;
            RectTransform.rotation = Quaternion.Euler(eulerAngles);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            TileUIEventBus.RaiseTileSelected(this);
            AudioManager.Instance.PlaySelectTile();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging)
                return;

            if (draggedTileGridUI != null && !draggedTileGridUI.IsWorldPositionInGrid(eventData.position))
            {
                draggedTileGridUI = null;
            }

            if (draggedTileGridUI == null)
            {
                foreach (GameObject obj in eventData.hovered)
                {
                    if (eventData.pointerDrag == obj)
                        continue;

                    if (obj.TryGetComponent(out TileGridUI candidate))
                    {
                        if (candidate.GridTransform.gameObject.activeInHierarchy && 
                            candidate.IsWorldPositionInGrid(eventData.position) && 
                            IsCompatibleGrid(candidate))
                        {
                            draggedTileGridUI = candidate;
                            break;
                        }
                        else
                        {
                            draggedTileGridUI = null;
                        }
                    }
                }
            }
        }

        private bool IsCompatibleGrid(TileGridUI candidate)
        {
            return candidate.AcceptsTile(this);
        }

        private void HandleCancelMoveTile()
        {
            if (!Input.GetKeyDown(KeyCode.Escape))
                return;
            TileUIEventBus.RaiseTileSelected(null);

            AudioManager.Instance.PlayCancelMoveTile();
        }

        private void HandleTileRotation()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Rotate(90);
                AudioManager.Instance.PlayRotateTile();
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                Rotate(-90);
                AudioManager.Instance.PlayRotateTile();
            }
        }

        public Vector2 GetOffset()
        {
            int xMin = int.MaxValue, xMax = int.MinValue, yMin = int.MaxValue, yMax = int.MinValue;

            foreach (Vector2Int v2int in RotatedShape)
            {
                if (v2int.x > xMax)
                    xMax = v2int.x;

                if (v2int.x < xMin)
                    xMin = v2int.x;

                if (v2int.y > yMax)
                    yMax = v2int.y;

                if (v2int.y < yMin)
                    yMin = v2int.y;
            }
            Vector2 v2 = new Vector2((xMax + xMin) / 2f, (yMax + yMin) / 2f);
            return v2;
        }

        protected Vector2 GetCenteredAnchoredPositionPositionInShape(List<Vector2Int> shape)
        {
            Vector2Int boundsMin = shape.Aggregate(
                new Vector2Int(int.MaxValue, int.MaxValue),
                (acc, p) => new Vector2Int(Mathf.Min(acc.x, p.x), Mathf.Min(acc.y, p.y)));

            Vector2Int boundsMax = shape.Aggregate(
                new Vector2Int(int.MinValue, int.MinValue),
                (acc, p) => new Vector2Int(Mathf.Max(acc.x, p.x), Mathf.Max(acc.y, p.y)));

            // Bounding box center in tile space (matches what the texture builder centers on)
            Vector2 boundsCenter = (Vector2)(boundsMin + boundsMax) * 0.5f;

            // Find the tile closest to the bounding box center
            Vector2Int closestPoint = shape[0];
            float closestSqrDistance = float.MaxValue;
            foreach (Vector2Int point in shape)
            {
                float sqrDistance = Vector2.SqrMagnitude(boundsCenter - point);
                if (sqrDistance < closestSqrDistance)
                {
                    closestSqrDistance = sqrDistance;
                    closestPoint = point;
                }
            }

            // Offset from bounding box center to the chosen tile, in pixel space
            return (closestPoint - boundsCenter) * 43.75f;
        }

        private void HandlePosition()
        {
            if (draggedTileGridUI != null)
            {
                Vector2Int gridPosition = draggedTileGridUI.GetGridPositon(Input.mousePosition);
                RectTransform.position = draggedTileGridUI.TileRectPositionAtVector2IntPosition(this, gridPosition);
                Color = (draggedTileGridUI.CanPlaceTile(this, gridPosition)) ? Color.white : Color.red;

                if (currentTileGridUI != draggedTileGridUI)
                {
                    AudioManager.Instance.PlayMoveTile();
                    currentTileGridUI = draggedTileGridUI;
                    currentTilePosition = gridPosition;
                }
                else if(currentTilePosition  != gridPosition)
                {
                    AudioManager.Instance.PlayMoveTile();
                    currentTilePosition = gridPosition;
                }
            }
            else
            {
                RectTransform.position = (Vector2)Input.mousePosition + GetCenteredAnchoredPositionPositionInShape(RotatedShape) * TileUIManager.Canvas.scaleFactor;
                Color = Color.white;
                currentTileGridUI = null;
            }
        }


        public void OnBeginDrag(PointerEventData eventData)
        {
            if (locked)
                return;
            
            isDragging = true;
            RectTransform.SetParent(TileUIManager.Canvas.transform, true);
            canvasGroup.blocksRaycasts = false;
            TileUIEventBus.RaiseDragTile(this);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging)
                return;
            TileUIEventBus.RaiseDragTile(null);

            isDragging = false;
            canvasGroup.blocksRaycasts = true;

            if(draggedTileGridUI != null && 
                draggedTileGridUI.GridTransform.gameObject.activeInHierarchy &&
                draggedTileGridUI.CanPlaceTileFromWorldPosition(this, eventData.position))
            {
                bool sameTileGrid = tileGridUI == draggedTileGridUI;

                if (!sameTileGrid)
                {
                    tileGridUI.RemoveTile(this);
                }

                draggedTileGridUI.AdoptTileUI(this);

                draggedTileGridUI.TryPlaceTileFromWorldPosition(this, eventData.position, !sameTileGrid, !sameTileGrid);
                tileGridUI = draggedTileGridUI;
                AudioManager.Instance.PlayPlaceTile();
            }
            else
            {
                UIRotation = Tile.Rotation;
                RectTransform.position = tileGridUI.TileRectPositionAtVector2IntPosition(this, BasePlacedPosition);
                transform.SetParent(tileGridUI.transform);
                Color = Color.white;
                AudioManager.Instance.PlayCancelMoveTile();
            }
        }

        #endregion

        [SerializeField]
        private GameObject highlight;

       public abstract TileUIStyle TileUIStyle { get; }

        public void Highlight(bool highlighted)
        {
            highlight.SetActive(highlighted);
        }

        #region Init

        public virtual void BaseInit(TileBase tile)
        {
            Tile = tile;
            Tile.OnRotated += RotateTo;

            /*tileGraphics.sprite = Tile.TileSO.TileSprite;
            tileGraphics.SetNativeSize();*/

            tileShapeTextureBuilder.Build(tile.Shape, TileUIStyle);

            if (RectTransform == null)
                RectTransform = GetComponent<RectTransform>();
        }

        #endregion

        public int CompareTo(object obj)
        {
            if(obj is TileUIBase other)
                return RotatedShape.Count.CompareTo(other.RotatedShape.Count);
            return 0;
        }
    }
 }

