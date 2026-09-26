using UnityEngine;
using TMPro;
using TPJ.Gameplay;
namespace TPJ.UI
{
    public class TurnsRemainingUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI text;

        [SerializeField]
        private string remainingTurnsText = "Remaining Turns: ";

        [SerializeField]
        private string endGameText = "Game Ended";

        private void Start()
        {
            CardGameEventsBus.OnBeginTurn += OnBeginTurn;
            CardGameEventsBus.OnGameEnd += EndGameUI;
            CardGameEventsBus.OnGameBegin += UpdateUI;
        }

        private void OnDestroy()
        {
            CardGameEventsBus.OnBeginTurn -= OnBeginTurn;
            CardGameEventsBus.OnGameEnd -= EndGameUI;
            CardGameEventsBus.OnGameBegin -= UpdateUI;
        }

        private void EndGameUI(CardGameEndReason reason)
        {
            text.text = endGameText;
        }
        private void UpdateUI()
        {
            text.text = remainingTurnsText + CardGameManager.Instance.AmountOfTurnsTillEndGame;
        }
        private void OnBeginTurn(DeckHolder holder)
        {
            UpdateUI();
        }
    }
}
