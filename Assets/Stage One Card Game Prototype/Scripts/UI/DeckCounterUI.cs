using TMPro;
using TPJ.Gameplay;
using UnityEngine;

namespace TPJ.UI
{
    public class DeckCounterUI : MonoBehaviour
    {
        [SerializeField] private DeckHolder deckHolder;
        [SerializeField] private TextMeshProUGUI drawPileText;
        [SerializeField] private TextMeshProUGUI discardPileText;

        private void OnEnable()
        {
            if (deckHolder == null) return;
            deckHolder.OnDraw += OnDraw;
            deckHolder.OnDiscard += OnDiscard;
            Refresh();
        }

        private void OnDisable()
        {
            if (deckHolder == null) return;
            deckHolder.OnDraw -= OnDraw;
            deckHolder.OnDiscard -= OnDiscard;
        }

        private void OnDraw(CardPile _) => Refresh();
        private void OnDiscard(BaseCard _) => Refresh();

        private void Refresh()
        {
            if (drawPileText != null)
                drawPileText.text = deckHolder.Deck.ActivePile.Cards.Count.ToString();

            if (discardPileText != null)
                discardPileText.text = deckHolder.Deck.DiscardPile.Cards.Count.ToString();
        }
    }
}