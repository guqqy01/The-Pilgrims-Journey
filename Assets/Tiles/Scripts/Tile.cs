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

        public TileModel TileModel { private set; get; }

        public UnitSO UnitSO
        {
            get
            {
                return TileModel.UnitSO;
            }
            set
            {
                TileModel.SetupNewUnitSO(value);

                if (unitGraphics == null)
                    unitGraphics = unitUITransform.GetComponent<Image>();

                unitGraphics.sprite = TileModel.UnitSO.sprite;
                onUpdateUI?.Invoke();
            }
        }

        public EquipmentSO[] Equipment => TileModel.Equipment;


        [SerializeField]
        private RectTransform unitUITransform;
        private Image unitGraphics;

        [SerializeField]
        private Image tileGraphics;

        public bool locked;

        public delegate void OnUpdateUI();
        public OnUpdateUI onUpdateUI;
        public OnUpdateUI onUpdateLevelUnitsAndHealthUI;

        public TileGrid tileGrid;

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
                return TileModel.TileSO.Shape.Rotate(Rotation);
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
            TileEventBus.OnTileSelected += Highlight;
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
            TileEventBus.OnTileSelected -= Highlight;
            onUpdateUI = null;
        }

        #region Leveling and health

        
        public static float AmountOfAdditionalUnitsPerLevelFunction(int Level)
        {
            return Level * Level * 0.2f + 1f;
        }

        public void OnUnitDie(Health _)
        {
            TileModel.OnUnitDie();

            if (TileModel.CurrentAmountOfUnitsInTile <= 0 && TileModel.AmountOfUnitsSpawned <= 0)
            {
                DestroyTile();
            }
        }

        public void OnUnitEscape()
        {
            TileModel.OnUnitEscape();
        }

        public void OnUnitSpawn()
        {
            TileModel.OnUnitSpawn();
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
            TileEventBus.RaiseTileSelected(this);
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
            TileEventBus.RaiseTileSelected(null);

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
            TileEventBus.RaiseDragTile(this);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!Dragging)
                return;
            TileEventBus.RaiseDragTile(null);

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

        public float SpawnRateValue => TileModel.SpawnRate;
        public float MovementValue => TileModel.Movement;
        public int HealthValue => TileModel.Health;
        public int ShieldValue => TileModel.Shield;
        public float ShieldRegenerationValue => TileModel.ShieldRegen;
        public float ShieldCooldownValue => TileModel.ShieldCooldown;

        #endregion

        [SerializeField]
        private GameObject highlight;

        public void Highlight(bool highlighted)
        {
            highlight.SetActive(highlighted);
        }

        public string Title => $"{TileModel.UnitSO.name} {TileModel.TileSO.name}";

        public string Description => TileModel.UnitSO.Description;

        public string GoldAmount => (Shape.Count * 100).ToString();

        #region Init

        public void BaseInit(TileSO tileSO, UnitSO unit)
        {
            UnitSO = unit;
            tileGraphics.sprite = tileSO.TileSprite;

            TileModel = new TileModel(tileSO, unit);

            TileModel.OnModelChanged += () =>
            {
                onUpdateUI?.Invoke();
            };

            TileModel.OnLevelHealthChanged += () =>
            {
                onUpdateLevelUnitsAndHealthUI?.Invoke();
            };

            tileGraphics.SetNativeSize();
        }

        public void TileSaveInit(TileSave tileSave)
        {
            BaseInit(tileSave.tile, tileSave.unit);

            TileModel.SetupFromSave(tileSave);

            RotateTo(tileSave.rotation);
        }

        #endregion

        public int CompareTo(object obj)
        {
            return Shape.Count.CompareTo(obj);
        }
    }

    public class TileModel
    {
        public TileSO TileSO { get; }
        public UnitSO UnitSO { get; private set; }

        public EquipmentSO[] Equipment { get; private set; } = new EquipmentSO[4];

        public int Level { get; private set; } = 1;
        public bool IsMaxLevel => Level == 5;

        public int CurrentAmountOfUnitsInTile { get; private set; }
        public int MaxAmountOfUnitsInTile { get; private set; }

        public int AmountOfUnitsSpawned { get; private set; }
        public int AmountOfUnitsEscapedNeededToLevel { get; private set; }
        public int CurrentAmountOfUnitsEscaped { get; private set; }

        public event Action OnModelChanged;
        public event Action OnLevelHealthChanged;

        public TileModel(TileSO tileSO, UnitSO unitSO)
        {
            TileSO = tileSO;
            UnitSO = unitSO;

            RecalculateLevelValues();
        }

        public void SetupNewUnitSO(UnitSO unitSO)
        {
            UnitSO = unitSO;

            RecalculateLevelValues();
        }

        #region Leveling

        private static float AdditionalUnitsPerLevel(int level)
        {
            return level * level * 0.2f + 1f;
        }

        private void RecalculateLevelValues()
        {
            AmountOfUnitsEscapedNeededToLevel =
                Mathf.RoundToInt(
                    AdditionalUnitsPerLevel(Level) *
                    UnitSO.numberOfUnitsEscapedToLevelUp *
                    TileSO.amountOfUnitsAbleToBeSpawnedAndRequiredToLevelPerTileSectionMultiplier *
                    TileSO.Shape.Count);

            MaxAmountOfUnitsInTile =
                Mathf.RoundToInt(
                    AdditionalUnitsPerLevel(Level) *
                    UnitSO.numberOfUnitsAbleToSpawn *
                    TileSO.amountOfUnitsAbleToBeSpawnedAndRequiredToLevelPerTileSectionMultiplier *
                    TileSO.Shape.Count);

            CurrentAmountOfUnitsInTile = MaxAmountOfUnitsInTile;
        }

        public void OnUnitSpawn()
        {
            CurrentAmountOfUnitsInTile = Mathf.Clamp(CurrentAmountOfUnitsInTile - 1, 0, MaxAmountOfUnitsInTile);
            AmountOfUnitsSpawned = Mathf.Max(0, AmountOfUnitsSpawned + 1);
            OnLevelHealthChanged?.Invoke();
        }

        public void OnUnitDie()
        {
            AmountOfUnitsSpawned = Mathf.Max(0, AmountOfUnitsSpawned - 1);
        }

        public void OnUnitEscape()
        {
            CurrentAmountOfUnitsEscaped =
                Mathf.Clamp(CurrentAmountOfUnitsEscaped + 1, 0, AmountOfUnitsEscapedNeededToLevel);

            if (CurrentAmountOfUnitsEscaped >= AmountOfUnitsEscapedNeededToLevel)
            {
                LevelUp();
            }

            AmountOfUnitsSpawned = Mathf.Max(0, AmountOfUnitsSpawned - 1);
            CurrentAmountOfUnitsInTile =
                Mathf.Clamp(CurrentAmountOfUnitsInTile + 1, 0, MaxAmountOfUnitsInTile);

            OnLevelHealthChanged?.Invoke();
        }

        private void LevelUp()
        {
            Level = Mathf.Clamp(Level + 1, 1, 5);
            CurrentAmountOfUnitsEscaped = 0;
            RecalculateLevelValues();
            OnModelChanged?.Invoke();
        }

        public void SetupFromSave(TileSave tileSave)
        {
            Level = tileSave.level;
            RecalculateLevelValues();
            CurrentAmountOfUnitsEscaped = tileSave.tileExp;
            CurrentAmountOfUnitsInTile = tileSave.tileHealth;
            OnModelChanged?.Invoke();
            Equipment = tileSave.equipment;
        }

        #endregion

        #region Stats

        public Buff GetTileBuff(UnitStatsEnum stat)
        {
            return TileManager.Instance.GetBuff(stat, Level);
        }

        private IntOrFloatObject GetStat(UnitStatsEnum stat)
        {
            var baseValue = UnitSO.GetStat(stat);

            List<Buff> buffs = new()
        {
            GetTileBuff(stat),
            UpgradeManager.Instance.GetBuff(stat),
            UnitBuffManager.Instance.GetBuff(stat)
        };

            foreach (var equipment in Equipment)
            {
                if (equipment != null && equipment.TryGetUnitBuff(stat, out Buff outBuff))
                {
                    buffs.Add(outBuff);
                }
            }

            if (buffs.Count == 0)
                return baseValue;

            Buff combined = buffs[0];
            for (int i = 1; i < buffs.Count; i++)
                combined += buffs[i];

            return combined.Value(baseValue);
        }

        public float SpawnRate => Mathf.Max(GetStat(UnitStatsEnum.SpawnRate), 0f);
        public float Movement => Mathf.Max(GetStat(UnitStatsEnum.MovementSpeed), 0f);
        public int Health => Mathf.Max(GetStat(UnitStatsEnum.Health), 0);
        public int Shield => Mathf.Max(GetStat(UnitStatsEnum.Shield), 0);
        public float ShieldRegen => Mathf.Max(GetStat(UnitStatsEnum.ShieldRegeneration), 0f);
        public float ShieldCooldown => Mathf.Max(GetStat(UnitStatsEnum.ShieldCooldown), 0f);

        #endregion
    }
 }

