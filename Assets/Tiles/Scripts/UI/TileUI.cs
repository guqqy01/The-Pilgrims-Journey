using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Units;
using Equipment;
using UnityEngine.EventSystems;
using System;

namespace Tiles
{
    public class TileUI : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler, IComparable
    {
        public RectTransform RectTransform { private set; get; }

        public Tile Tile { private set; get; }

        public UnitSO UnitSO
        {
            get
            {
                return Tile.UnitSO;
            }
            set
            {
                Tile.SetupNewUnitSO(value);

                if (unitGraphics == null)
                    unitGraphics = unitUITransform.GetComponent<Image>();

                unitGraphics.sprite = Tile.UnitSO.sprite;
            }
        }

        public EquipmentSO[] Equipment => Tile.Equipment;


        [SerializeField]
        private RectTransform unitUITransform;
        private Image unitGraphics;

        [SerializeField]
        private Image tileGraphics;

        public bool locked;

        

        public TileGridUI tileGridUI;

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
            unitGraphics = unitUITransform.GetComponent<Image>();
        }

        private void Start()
        {
            TileUIEventBus.OnTileSelected += Highlight;
            tileGraphics.alphaHitTestMinimumThreshold = 0.2f;
        }

        private void Update()
        {
            HandleCancelMoveTile();

            if (Dragging)
            {
                HandleTileRotation();
                HandlePosition();
            }
        }

        private void Highlight(TileUI tile)
        {
            Highlight(tile == this);
        }

        private void OnDestroy()
        {
            TileUIEventBus.OnTileSelected -= Highlight;
            
        }

        #region Leveling and health

        public void OnUnitSpawn()
        {
            Tile.OnUnitSpawn();
        }

        public void DestroyTile()
        {
            Destroy(gameObject);
        }

        #endregion

        #region Dragging

        [SerializeField]
        private CanvasGroup canvasGroup;
        public bool Dragging;
        public TileGridUI draggedTileGridUI;

        public Vector2Int PlacedPosition;
        public float PlacedRotation;

        private Vector2Int currentTilePosition;
        private TileGridUI currentTileGridUI;

        public void Rotate(int angle)
        {
            RectTransform.Rotate(new Vector3(0, 0, angle));
            unitUITransform.rotation = Quaternion.Euler(Vector3.zero);
        }

        public void RotateTo(float angle)
        {
            if (RectTransform == null)
                RectTransform = GetComponent<RectTransform>();

            Vector3 eulerAngles = RectTransform.rotation.eulerAngles;
            eulerAngles.z = angle;
            RectTransform.rotation = Quaternion.Euler(eulerAngles);
            unitUITransform.rotation = Quaternion.Euler(Vector3.zero);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            TileUIEventBus.RaiseTileSelected(this);
            AudioManager.Instance.PlaySelectTile();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!Dragging)
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
            
            Dragging = true;
            RectTransform.SetParent(TileUIManager.Canvas.transform, true);
            canvasGroup.blocksRaycasts = false;
            TileUIEventBus.RaiseDragTile(this);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!Dragging)
                return;
            TileUIEventBus.RaiseDragTile(null);

            Dragging = false;
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

        #region Stats

        public float SpawnRateValue => Tile.SpawnRate;
        public float MovementValue => Tile.Movement;
        public int HealthValue => Tile.Health;
        public int ShieldValue => Tile.Shield;
        public float ShieldRegenerationValue => Tile.ShieldRegen;
        public float ShieldCooldownValue => Tile.ShieldCooldown;

        #endregion

        [SerializeField]
        private GameObject highlight;

        public void Highlight(bool highlighted)
        {
            highlight.SetActive(highlighted);
        }

        public string Title => $"{Tile.UnitSO.name} {Tile.TileSO.name}";

        public string Description => Tile.UnitSO.Description;

        public string GoldAmount => (RotatedShape.Count * 100).ToString();

        #region Init

        public void BaseInit(Tile tile)
        {
            Tile = tile;
            Tile.OnRotated += RotateTo;

            tileGraphics.sprite = Tile.TileSO.TileSprite;
            tileGraphics.SetNativeSize();

            unitGraphics = unitUITransform.GetComponent<Image>();
            unitGraphics.sprite = Tile.UnitSO.sprite;

            if (RectTransform == null)
                RectTransform = GetComponent<RectTransform>();
        }

        #endregion

        public int CompareTo(object obj)
        {
            if(obj is TileUI other)
                return RotatedShape.Count.CompareTo(other.RotatedShape.Count);
            return 0;
        }
    }
 }

