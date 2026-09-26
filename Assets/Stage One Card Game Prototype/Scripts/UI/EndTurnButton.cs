using UnityEngine;
using UnityEngine.UI;
using TPJ.Gameplay;
namespace TPJ.UI
{
    public class EndTurnButton : MonoBehaviour
    {
        [SerializeField]
        private Button button;

        [SerializeField]
        private CardGameManager cardGameManager;

        private void Start()
        {
            CardGameEventsBus.OnBeginTurn += OnTurnChange;
            CardGameEventsBus.OnCardPlayed += OnCardPlayed;   // add — hero placement needs a refresh trigger too
        }

        private void OnDestroy()
        {
            CardGameEventsBus.OnBeginTurn -= OnTurnChange;
            CardGameEventsBus.OnCardPlayed -= OnCardPlayed;
        }

        public void OnTurnChange(DeckHolder turnHolder) => Refresh();
        private void OnCardPlayed(DeckHolder holder, CardBase card) => Refresh();
        private void Refresh() => button.interactable = cardGameManager.IsPlayerTurn && cardGameManager.CanEndTurn;
        
        public void EndTurn()
        {
            if (cardGameManager.CanEndTurn && cardGameManager.IsPlayerTurn)
                cardGameManager.EndTurn();
        }
    }
}
