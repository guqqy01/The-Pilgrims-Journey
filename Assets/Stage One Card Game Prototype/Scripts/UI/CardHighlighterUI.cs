using System.Collections.Generic;
using TPJ.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace TPJ.UI
{
    public class CardHighlighterUI : MonoBehaviour
    {
        [SerializeField] private List<Image> images;
        [SerializeField] private Color legalColor = Color.white;
        [SerializeField] private Color illegalColor = new Color(1f, 1f, 1f, 0.35f);

        private CardBase _card;

        private void OnEnable()
        {
            if (!CardGameManager.HasInstance) return;
            CardGameEventsBus.OnBeginTurn += OnBeginTurn;
            CardGameManager.Instance.OnBoardChanged += OnBoardChanged;
            CardGameEventsBus.OnCardPlayed += UpdateUI;
        }

        private void OnDisable()
        {
            if (_card?.Owner != null)
                _card.Owner.CardGamePoints.OnPointValueChange -= OnPointsChanged;

            if (!CardGameManager.HasInstance) return;
            CardGameEventsBus.OnBeginTurn -= OnBeginTurn;
            CardGameManager.Instance.OnBoardChanged -= OnBoardChanged;
            CardGameEventsBus.OnCardPlayed -= UpdateUI;
        }

        public void Setup(CardBase card)
        {
            _card = card;
            if (_card.Owner != null)
                _card.Owner.CardGamePoints.OnPointValueChange += OnPointsChanged;

            UpdateUI(null, null);
        }

        private void OnPointsChanged() => UpdateUI(null, null);
        private void OnBeginTurn(DeckHolder _) => UpdateUI(_, null);
        private void OnBoardChanged() => UpdateUI(null, null);

        private void UpdateUI(DeckHolder _, CardBase card)
        {
            if (_card == null || !CardGameManager.HasInstance)
                return;

            bool ownersTurn = _card.Owner != null && _card.Owner.CurrentlyOurTurn;
            bool hasEnoughPoints = _card.Owner.CardGamePoints.HasEnoughPoints(_card.CardActionCost);
            bool legal = ownersTurn && hasEnoughPoints && _card.HasAnyLegalPlay(CardGameManager.Instance);

            Color color = legal ? legalColor : illegalColor;
            foreach (var image in images)
                image.color = color;

            GetComponent<CardWidgetUI>()?.SetInteractable(legal);
        }
    }
}