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
        }

        private void OnDestroy()
        {
            CardGameEventsBus.OnBeginTurn -= OnTurnChange;
        }

        public void OnTurnChange(DeckHolder turnHolder)
        {
            button.interactable = cardGameManager.IsPlayerTurn;
        }

        public void EndTurn()
        {
            if (cardGameManager.IsPlayerTurn)
                cardGameManager.EndTurn();
        }
    }
}
