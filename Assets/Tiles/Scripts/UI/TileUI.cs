using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace Tiles
{

    public class TileUI<TItem> : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler, IComparable
    {
        [SerializeField]
        protected TileShapeTextureBuilder tileShapeTextureBuilder;
        public RectTransform RectTransform { private set; get; }

        public Tile<TItem> Tile { private set; get; }

        [SerializeField]
        private RawImage tileGraphics;

        [HideInInspector]
        public bool locked;

        [HideInInspector]
        public TileGridUI<TItem> tileGridUI;

        public float BaseRotation
        {
            get
            {
                return Tile.Rotation;
            }
            set
            {
                Tile.Rotation = value;
            }
        }

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
            TileUIEventBus<TItem>.OnTileSelected += Highlight;
            //tileGraphics.alphaHitTestMinimumThreshold = 0.2f;
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

        private void Highlight(TileUI<TItem> tile)
        {
            Highlight(tile == this);
        }

        private void OnDestroy()
        {
            TileUIEventBus<TItem>.OnTileSelected -= Highlight;
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
        public TileGridUI<TItem> draggedTileGridUI;

        [HideInInspector]
        public Vector2Int PlacedPosition;
        [HideInInspector]
        public float PlacedRotation;

        private Vector2Int currentTilePosition;
        private TileGridUI<TItem> currentTileGridUI;

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
            TileUIEventBus<TItem>.RaiseTileSelected(this);
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

                    if (obj.TryGetComponent(out draggedTileGridUI))
                    {
                        if (draggedTileGridUI.GridTransform.gameObject.activeInHierarchy && draggedTileGridUI.IsWorldPositionInGrid(eventData.position))
                        {
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

        private void HandleCancelMoveTile()
        {
            if (!Input.GetKeyDown(KeyCode.Escape))
                return;
            TileUIEventBus<TItem>.RaiseTileSelected(null);

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
                RectTransform.position = (Vector2)Input.mousePosition + (GetOffset() * 43.75f) * TileUIManager.Canvas.scaleFactor;
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
            TileUIEventBus<TItem>.RaiseDragTile(this);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging)
                return;
            TileUIEventBus<TItem>.RaiseDragTile(null);

            isDragging = false;
            canvasGroup.blocksRaycasts = true;

            if(draggedTileGridUI != null && draggedTileGridUI.GridTransform.gameObject.activeInHierarchy && draggedTileGridUI.CanPlaceTileFromWorldPosition(this, eventData.position))
            {
                if (tileGridUI != draggedTileGridUI)
                {
                    tileGridUI.RemoveTile(this);
                }

                draggedTileGridUI.AdoptTileUI(this);

                draggedTileGridUI.TryPlaceTileFromWorldPosition(this, eventData.position);
                tileGridUI = draggedTileGridUI;
                AudioManager.Instance.PlayPlaceTile();
            }
            else
            {
                UIRotation = PlacedRotation;
                RectTransform.position = tileGridUI.TileRectPositionAtVector2IntPosition(this, PlacedPosition);
                transform.SetParent(tileGridUI.transform);
                Color = Color.white;
                AudioManager.Instance.PlayCancelMoveTile();
            }
        }

        #endregion

        [SerializeField]
        private GameObject highlight;

        public void Highlight(bool highlighted)
        {
            highlight.SetActive(highlighted);
        }

        #region Init

        public virtual void BaseInit(Tile<TItem> tile)
        {
            Tile = tile;
            Tile.OnRotated += RotateTo;

            /*tileGraphics.sprite = Tile.TileSO.TileSprite;
            tileGraphics.SetNativeSize();*/

            tileShapeTextureBuilder.Build(tile.Shape);

            if (RectTransform == null)
                RectTransform = GetComponent<RectTransform>();
        }

        #endregion

        public int CompareTo(object obj)
        {
            if(obj is TileUI<TItem> other)
                return RotatedShape.Count.CompareTo(other.RotatedShape.Count);
            return 0;
        }
    }
 }

