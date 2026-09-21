using Tiles;
using Tiles.UI;
using TPJ.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TPJ.UI
{
    public class CardPlacementController : MonoBehaviour
    {
        [SerializeField]
        private HologramTileUI holigramTileUIPrefab;

        private HologramTileUI spawnedHologramTile;

        private BaseCard _currentlySelectedCard;
        private TileBase _currentlySelectedCardTile;
        private CardWidgetUI _currentlyDraggingCard;
        private TileGridBase _targetGrid;
        private TileGridUI _targetGridUI;

        private EquipmentTargetHighlighter _equipmentHighlighter;

        private bool _isHologramActive;
        private float _currentRotation;

        // Bumped on every new session/cancel so a delayed async callback from a
        // now-stale request can detect it's no longer relevant and no-op.
        private int _placementRequestId;


        private void Awake()
        {
            _equipmentHighlighter = GetComponent<EquipmentTargetHighlighter>();
        }

        private void OnEnable()
        {
            CardUIEventBus.OnCardSelected += SelectCard;
            CardUIEventBus.OnCardBeginDrag += OnCardBeginDrag;
            CardUIEventBus.OnCardDragging += OnCardDragging;
            CardUIEventBus.OnCardEndDrag += OnCardEndDrag;
        }

        private void OnDisable()
        {
            CardUIEventBus.OnCardSelected -= SelectCard;
            CardUIEventBus.OnCardBeginDrag -= OnCardBeginDrag;
            CardUIEventBus.OnCardDragging -= OnCardDragging;
            CardUIEventBus.OnCardEndDrag -= OnCardEndDrag;
        }

        private void Update()
        {
            // Click-to-place tracking only; drag positions are driven by OnCardDragging.
            if (spawnedHologramTile != null)
                HandleHoligramTracking();
        }

        // ---------------------------------------------------------------
        // Click flow
        // ---------------------------------------------------------------

        public void SelectCard(CardWidgetUI cardWidgetUI)
        {
            BeginSession(cardWidgetUI.Card, null);

            if (_currentlySelectedCard == null)
                return;

            if (_currentlySelectedCard is EquipmentCard)
                _equipmentHighlighter?.BeginTargeting(_currentlySelectedCard, OnEquipmentTargetChosen);
            else
                RequestHologram(_currentlySelectedCardTile);
        }

        private void HandleHoligramTracking()
        {
            if (InputManager.Instance.Actions.UI.Cancel.WasPressedThisFrame())
            {
                EndSession(played: false);
                return;
            }
            HandleRotationInput();

            Vector2Int gridPosition = _targetGridUI.GetMouseGridPosition();
            bool legal = _targetGridUI.CanPlaceTile(spawnedHologramTile, gridPosition);

            spawnedHologramTile.RectTransform.position = _targetGridUI.TileRectPositionAtVector2IntPosition(spawnedHologramTile, gridPosition);
            spawnedHologramTile.Color = legal ? Color.white : Color.red;

            if (legal && Mouse.current.leftButton.wasPressedThisFrame)
                TryConfirmPlacement(gridPosition);
        }

        // ---------------------------------------------------------------
        // Drag flow
        // ---------------------------------------------------------------

        private void OnCardBeginDrag(CardWidgetUI cardWidgetUI)
        {
            BaseCard card = cardWidgetUI.Card;

            if (card.Owner == null || !card.Owner.CurrentlyOurTurn)
                return;

            BeginSession(card, cardWidgetUI);
        }

        private void OnCardDragging(CardWidgetUI cardWidgetUI, Vector2 screenPos)
        {
            if (_currentlyDraggingCard != cardWidgetUI || _targetGridUI == null)
                return;

            if (_currentlySelectedCard is EquipmentCard)
                return; // equipment resolves on drop, not tracked live

            HandleRotationInput();

            bool isOverGrid = _targetGridUI.IsWorldPositionInGrid(screenPos);

            if (isOverGrid && !_isHologramActive)
            {
                _isHologramActive = true;
                SetCardAlpha(cardWidgetUI, 0f);
                RequestHologram(_currentlySelectedCardTile);
            }
            else if (!isOverGrid && _isHologramActive)
            {
                _isHologramActive = false;
                SetCardAlpha(cardWidgetUI, 1f);
                _placementRequestId++; // invalidate any in-flight hologram request before destroying
                DestroyHologramTile();
            }

            if (_isHologramActive && spawnedHologramTile != null)
            {
                Vector2Int gridPosition = _targetGridUI.GetGridPosition(screenPos);
                bool legal = _targetGridUI.CanPlaceTile(spawnedHologramTile, gridPosition);

                spawnedHologramTile.RectTransform.position = _targetGridUI.TileRectPositionAtVector2IntPosition(spawnedHologramTile, gridPosition);
                spawnedHologramTile.Color = legal ? Color.white : Color.red;
            }
        }

        private void OnCardEndDrag(CardWidgetUI cardWidgetUI, Vector2 screenPos)
        {
            if (_currentlyDraggingCard != cardWidgetUI)
                return;

            SetCardAlpha(cardWidgetUI, 1f);

            if (_currentlySelectedCard is EquipmentCard)
            {
                TryConfirmEquipmentDrop(screenPos);
                return;
            }

            if (_targetGridUI != null && _targetGridUI.IsWorldPositionInGrid(screenPos))
                TryConfirmPlacement(_targetGridUI.GetGridPosition(screenPos));
            else
                EndSession(played: false);
        }

        private void TryConfirmEquipmentDrop(Vector2 screenPos)
        {
            if (_targetGridUI != null &&
                _targetGridUI.IsWorldPositionInGrid(screenPos) &&
                _targetGridUI.TryGetTileUIAtPosition(_targetGridUI.GetGridPosition(screenPos), out TileUIBase targetTileUI))
            {
                var context = new CardPlayContext(targetTileUI.Tile);
                bool played = CardGameManager.Instance.TryPlayCard(_currentlySelectedCard, context);
                EndSession(played);
            }
            else
            {
                EndSession(played: false);
            }
        }

        // ---------------------------------------------------------------
        // Shared session management
        // ---------------------------------------------------------------

        /// <summary>
        /// Starts tracking a card for placement, always ending any previous session
        /// (click or drag) first so the two flows can never fight over shared state.
        /// </summary>
        private void BeginSession(BaseCard card, CardWidgetUI cardWidgetUI)
        {
            EndSession(played: false);

            if (card.Owner == null || !card.Owner.CurrentlyOurTurn)
                return;

            _targetGrid = CardGameManager.Instance.GetGrid;

            if (!TileGridUI.TryGetUIForGrid(_targetGrid, out _targetGridUI))
            {
                Debug.LogWarning($"No TileGridUI registered for grid {_targetGrid}");
                return;
            }

            _currentlySelectedCard = card;
            _currentlySelectedCardTile = GetCardableTile(card);
            _currentlyDraggingCard = cardWidgetUI;
            _currentRotation = _currentlySelectedCardTile != null ? _currentlySelectedCardTile.Rotation : 0f;
        }

        private void TryConfirmPlacement(Vector2Int gridPosition)
        {
            var context = new CardPlayContext(_targetGrid, gridPosition, _currentRotation);
            bool played = CardGameManager.Instance.TryPlayCard(_currentlySelectedCard, context);

            if (played)
                SFXManager.IfReady(sfx => sfx.PlayPlaceTile());

            EndSession(played);
        }

        private void OnEquipmentTargetChosen(TileUIBase targetTileUI)
        {
            var context = new CardPlayContext(targetTileUI.Tile);
            bool played = CardGameManager.Instance.TryPlayCard(_currentlySelectedCard, context);
            EndSession(played);
        }

        public void CancelCurrentSelection() => EndSession(played: false);

        private void EndSession(bool played)
        {
            _placementRequestId++; // invalidate any in-flight hologram request

            DestroyHologramTile();
            _equipmentHighlighter?.CancelTargeting();

            if (_currentlyDraggingCard != null)
                SetCardAlpha(_currentlyDraggingCard, 1f);

            _currentlySelectedCard = null;
            _currentlySelectedCardTile = null;
            _currentlyDraggingCard = null;
            _isHologramActive = false;
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private void HandleRotationInput()
        {
            if (InputManager.Instance.Actions.UI_TilePlayer.RotateTileLeft.WasPressedThisFrame())
                Rotate(90);
            else if (InputManager.Instance.Actions.UI_TilePlayer.RotateTileRight.WasPressedThisFrame())
                Rotate(-90);
        }

        private void Rotate(int degrees)
        {
            _currentRotation += degrees;
            spawnedHologramTile?.Rotate(degrees);
        }

        private void SetCardAlpha(CardWidgetUI cardWidgetUI, float alpha)
        {
            if (cardWidgetUI == null)
                return;

            var group = cardWidgetUI.GetComponent<CanvasGroup>();
            if (group == null)
                group = cardWidgetUI.gameObject.AddComponent<CanvasGroup>();

            group.alpha = alpha;
        }

        private TileBase GetCardableTile(BaseCard card)
        {
            return card switch
            {
                UnitCard unitCard => unitCard.CardableRef,
                MonsterCard monsterCard => monsterCard.CardableRef,
                HeroCard heroCard => heroCard.CardableRef,
                _ => null
            };
        }

        private void RequestHologram(TileBase tile)
        {
            if (tile == null)
                return;

            int requestId = _placementRequestId;

            TileUIPrefabManager.WhenReady(_ =>
            {
                if (requestId != _placementRequestId)
                    return; // selection changed/cancelled while this was loading

                if (!TileUIPrefabManager.Instance.TryGetTileUIBase(tile, out TileUIBase tileUI))
                    return;

                spawnedHologramTile = Instantiate(holigramTileUIPrefab, TileUIManager.Canvas.transform);
                spawnedHologramTile.BaseInit(tile);
                spawnedHologramTile.Setup(tileUI);
                spawnedHologramTile.UIRotation = _currentRotation;
                spawnedHologramTile.SetGridContext(_targetGridUI);
            });
        }

        private void DestroyHologramTile()
        {
            if (spawnedHologramTile == null)
                return;

            Destroy(spawnedHologramTile.gameObject);
            spawnedHologramTile = null;
        }
    }
}