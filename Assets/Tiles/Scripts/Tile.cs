using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Units;
using Equipment;
using UnityEngine.EventSystems;
using System;
using LevelingSystem;

namespace Tiles
{
    public class Tile : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler, IComparable
    {
        public RectTransform RectTransform { private set; get; }
        public TileSO TileSO { private set; get; }

        private UnitSO unitSO;
        public UnitSO UnitSO
        {
            get
            {
                return unitSO;
            }
            set
            {
                unitSO = value;

                if (unitGraphics == null)
                    unitGraphics = unitUITransform.GetComponent<Image>();

                unitGraphics.sprite = unitSO.sprite;
                onUpdateUI?.Invoke();
            }
        }

        [SerializeField]
        private RectTransform unitUITransform;
        private Image unitGraphics;

        [SerializeField]
        private Image tileGraphics;

        public bool locked;

        public EquipmentSO[] Equipment { private set; get; } = new EquipmentSO[4];

        public delegate void OnUpdateUI();
        public OnUpdateUI onUpdateUI;
        


        public TileGrid tileGrid;

        public delegate void OnTileSelected(Tile tile);
        public static OnTileSelected onTileSelected;

        public delegate void OnDragTile(Tile tile);
        public static OnDragTile onDragTile;

        public float Rotation
        {
            get
            {
                if(RectTransform == null)
                    RectTransform = GetComponent<RectTransform>();

                return RectTransform.rotation.eulerAngles.z;
            }
            set
            {
                RotateTo(value);
            }
        }

        public List<Vector2Int> Shape
        {
            get
            {
                return TileSO.Shape.Rotate(Rotation);
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

        

        public Buff GetTileBuff(UnitStatsEnum stat) => TileManager.Instance.GetBuff(stat, Level);

        public bool Selected { private set; get; } = false;

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
            unitGraphics = unitUITransform.GetComponent<Image>();
        }

        private void Start()
        {
            onTileSelected += Highlight;
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

        private void Highlight(Tile tile)
        {
            Highlight(tile == this);
        }

        private void OnDestroy()
        {
            onTileSelected -= Highlight;
        }

        #region Leveling and health

        [Header("Leveling and Tile Health")]
        public int Level { private set; get; } = 1;
        public bool IsMaxLevel => Level == 5;

        public int currentAmountOfUnitsInTile;
        public int MaxAmountOfUnitsInTile;


        public int AmountOfUnitsSpawned { private set; get; }
        public int AmountOfUnitsEscapedNeededToLevel { private set; get; }
        public int CurrentAmountOfUnitsEscaped { private set; get; }

        public OnUpdateUI onUpdateLevelUnitsAndHealthUI;

        public static float AmountOfAdditionalUnitsPerLevelFunction(int Level)
        {
            return Level * Level * 0.2f + 1f;
        }

        public void OnUnitDie(Health _)
        {
            if (gameObject == null)
                return;

            if (currentAmountOfUnitsInTile <= 0 && AmountOfUnitsSpawned <= 0)
            {
                DestroyTile();
            }

            AmountOfUnitsSpawned = Mathf.Max(0, AmountOfUnitsSpawned--);
        }

        public void OnUnitEscape()
        {
            CurrentAmountOfUnitsEscaped = Mathf.Clamp(CurrentAmountOfUnitsEscaped + 1, 0, AmountOfUnitsEscapedNeededToLevel);

            onUpdateLevelUnitsAndHealthUI?.Invoke();

            if (CurrentAmountOfUnitsEscaped >= AmountOfUnitsEscapedNeededToLevel)
            {
                LevelUp();
            }

            AmountOfUnitsSpawned = Mathf.Max(0, AmountOfUnitsSpawned--);
            currentAmountOfUnitsInTile = Mathf.Clamp(currentAmountOfUnitsInTile + 1, 0, MaxAmountOfUnitsInTile);
        }

        public void OnUnitSpawn()
        {
            if (gameObject == null)
                return;

            currentAmountOfUnitsInTile = Mathf.Clamp(currentAmountOfUnitsInTile - 1, 0, MaxAmountOfUnitsInTile);

            onUpdateLevelUnitsAndHealthUI?.Invoke();

            AmountOfUnitsSpawned = Mathf.Max(0, AmountOfUnitsSpawned + 1);
        }

        public void DestroyTile()
        {
            Destroy(gameObject);
        }

        public void LevelUp()
        {
            Level = Mathf.Clamp(Level + 1, 1, 5);

            CurrentAmountOfUnitsEscaped = 0;
            AmountOfUnitsEscapedNeededToLevel = Mathf.RoundToInt(AmountOfAdditionalUnitsPerLevelFunction(Level) * UnitSO.numberOfUnitsEscapedToLevelUp * TileSO.amountOfUnitsAbleToBeSpawnedAndRequiredToLevelPerTileSectionMultiplier * Shape.Count);
            MaxAmountOfUnitsInTile = Mathf.RoundToInt(AmountOfAdditionalUnitsPerLevelFunction(Level) * UnitSO.numberOfUnitsAbleToSpawn * TileSO.amountOfUnitsAbleToBeSpawnedAndRequiredToLevelPerTileSectionMultiplier * Shape.Count);
            currentAmountOfUnitsInTile = MaxAmountOfUnitsInTile;
            onUpdateLevelUnitsAndHealthUI?.Invoke();

            onUpdateUI?.Invoke();
        }

        #endregion

        #region Dragging

        [SerializeField]
        private CanvasGroup canvasGroup;
        public bool Dragging;
        public TileGrid draggedTileGrid;

        public Vector2Int PlacedPosition;
        public float PlacedRotation;

        private Vector2Int currentTilePosition;
        private TileGrid currentTileGrid;

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
            onTileSelected?.Invoke(this);
            AudioManager.Instance.PlaySelectTile();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!Dragging)
                return;

            if (draggedTileGrid != null && !draggedTileGrid.IsWorldPositionInGrid(eventData.position))
            {
                draggedTileGrid = null;
            }

            if (draggedTileGrid == null)
            {
                foreach (GameObject obj in eventData.hovered)
                {
                    if (eventData.pointerDrag == obj)
                        continue;

                    if (obj.TryGetComponent(out draggedTileGrid))
                    {
                        if (draggedTileGrid.GridTransform.gameObject.activeInHierarchy && draggedTileGrid.IsWorldPositionInGrid(eventData.position))
                        {
                            break;
                        }
                        else
                        {
                            draggedTileGrid = null;
                        }
                    }
                }
            }
        }

        private void HandleCancelMoveTile()
        {
            if (!Input.GetKeyDown(KeyCode.Escape))
                return;

            onTileSelected?.Invoke(null);

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

            foreach (Vector2Int v2int in Shape)
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
            if (draggedTileGrid != null)
            {
                Vector2Int gridPosition = draggedTileGrid.GetGridPositon(Input.mousePosition);
                RectTransform.position = draggedTileGrid.Position(this, gridPosition);
                Color = (draggedTileGrid.CanPlaceTile(this, gridPosition)) ? Color.white : Color.red;

                if (currentTileGrid != draggedTileGrid)
                {
                    AudioManager.Instance.PlayMoveTile();
                    currentTileGrid = draggedTileGrid;
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
                currentTileGrid = null;
            }
        }


        public void OnBeginDrag(PointerEventData eventData)
        {
            if (locked)
                return;
            
            Dragging = true;
            RectTransform.SetParent(TileUIManager.Canvas.transform, true);
            canvasGroup.blocksRaycasts = false;

            onDragTile?.Invoke(this);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!Dragging)
                return;

            onDragTile?.Invoke(null);

            Dragging = false;
            canvasGroup.blocksRaycasts = true;

            if(draggedTileGrid != null && draggedTileGrid.GridTransform.gameObject.activeInHierarchy && draggedTileGrid.CanPlaceTileFromWorldPosition(this, eventData.position))
            { 
                tileGrid.RemoveTile(this);
                draggedTileGrid.TryPlaceTileFromWorldPosition(this, eventData.position);
                tileGrid = draggedTileGrid;
                AudioManager.Instance.PlayPlaceTile();
            }
            else
            {
                Rotation = PlacedRotation;
                RectTransform.position = tileGrid.Position(this, PlacedPosition);
                transform.SetParent(tileGrid.transform);
                Color = Color.white;
                AudioManager.Instance.PlayCancelMoveTile();
            }
        }


        #endregion

        #region Stats

        [SerializeField]
        private GameObject highlight;

        private IntOrFloatObject GetStat(UnitStatsEnum stat)
        {
            // Start with base stat
            var baseValue = unitSO.GetStat(stat);

            // Collect buffs
            List<Buff> allBuffs = new List<Buff>
            {
                GetTileBuff(stat),
                UpgradeManager.Instance.GetBuff(stat),
                UnitBuffManager.Instance.GetBuff(stat)
            };

            foreach (var equipment in Equipment)
            {
                if (equipment != null && equipment.TryGetUnitBuff(stat, out Buff outBuff))
                {
                    allBuffs.Add(outBuff);
                }
            }

            // Combine buffs if any
            if (allBuffs.Count == 0)
                return baseValue;

            Buff combined = allBuffs[0];

            for (int i = 1; i < allBuffs.Count; i++)
            {
                combined += allBuffs[i];
            }

            var returnValue = combined.Value(baseValue);

            // Apply buffs to base value
            return returnValue;
        }



        /// <summary>
        /// The amount of spawns per second
        /// </summary>
        public float SpawnRateValue
        {
            get
            {
                return Mathf.Max(GetStat(UnitStatsEnum.SpawnRate), 0f);
            }
        }

        public float MovementValue
        {
            get
            {
                return Mathf.Max(GetStat(UnitStatsEnum.MovementSpeed), 0f);
            }
        }

        public int HealthValue
        {
            get
            {
                return Mathf.Max(GetStat(UnitStatsEnum.Health), 0);
            }
        }

        public int ShieldValue
        {
            get
            {
                return Mathf.Max(GetStat(UnitStatsEnum.Shield), 0);
            }
        }


        public float ShieldRegenerationValue
        {
            get
            {
                return Mathf.Max(GetStat(UnitStatsEnum.ShieldRegeneration), 0f);
            }
        }

        public float ShieldCooldownValue
        {
            get
            {
                return Mathf.Max(GetStat(UnitStatsEnum.ShieldCooldown), 0f);
            }
        }

        #endregion

        
        public void Highlight(bool highlighted)
        {
            highlight.SetActive(highlighted);
        }

        
        public string Title => $"{UnitSO.name} {TileSO.name}";

        public string Description => UnitSO.Description;

        public string GoldAmount => (Shape.Count * 100).ToString();


        #region Init

        public void BaseInit(TileSO tileSO, UnitSO unit)
        {
            TileSO = tileSO;
            UnitSO = unit;
            tileGraphics.sprite = tileSO.TileSprite;

            AmountOfUnitsEscapedNeededToLevel = Mathf.RoundToInt(AmountOfAdditionalUnitsPerLevelFunction(Level) * unit.numberOfUnitsEscapedToLevelUp * TileSO.amountOfUnitsAbleToBeSpawnedAndRequiredToLevelPerTileSectionMultiplier * Shape.Count);
            MaxAmountOfUnitsInTile = Mathf.RoundToInt(AmountOfAdditionalUnitsPerLevelFunction(Level) * unit.numberOfUnitsAbleToSpawn * TileSO.amountOfUnitsAbleToBeSpawnedAndRequiredToLevelPerTileSectionMultiplier * Shape.Count);
            currentAmountOfUnitsInTile = MaxAmountOfUnitsInTile;
            tileGraphics.SetNativeSize();
        }

        public void TileSaveInit(TileSave tileSave)
        {
            Level = tileSave.level;

            BaseInit(tileSave.tile, tileSave.unit);

            currentAmountOfUnitsInTile = tileSave.tileHealth;

            CurrentAmountOfUnitsEscaped = tileSave.tileExp;


            RotateTo(tileSave.rotation);
            Equipment = tileSave.equipment;
        }

        #endregion

        public int CompareTo(object obj)
        {
            return Shape.Count.CompareTo(obj);
        }
    }
}
