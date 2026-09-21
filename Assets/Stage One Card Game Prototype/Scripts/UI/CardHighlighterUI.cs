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

        private BaseCard _card;

        private void OnEnable()
        {
            if (!CardGameManager.HasInstance) return;
            CardGameManager.Instance.OnTurnChanged += OnTurnChanged;
            CardGameManager.Instance.OnBoardChanged += UpdateUI;
        }

        private void OnDisable()
        {
            if (!CardGameManager.HasInstance) return;
            CardGameManager.Instance.OnTurnChanged -= OnTurnChanged;
            CardGameManager.Instance.OnBoardChanged -= UpdateUI;
        }

        public void Setup(BaseCard card)
        {
            _card = card;
            UpdateUI();
        }

        private void OnTurnChanged(DeckHolder _) => UpdateUI();

        private void UpdateUI()
        {
            if (_card == null || !CardGameManager.HasInstance)
                return;

            bool ownersTurn = _card.Owner != null && _card.Owner.CurrentlyOurTurn;
            bool legal = ownersTurn && _card.HasAnyLegalPlay(CardGameManager.Instance);

            Color color = legal ? legalColor : illegalColor;
            foreach (var image in images)
                image.color = color;

            GetComponent<CardWidgetUI>()?.SetInteractable(legal);
        }
    }
}