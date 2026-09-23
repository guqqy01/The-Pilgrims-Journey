using TPJ.Gameplay;
using UnityEngine;
namespace TPJ.UI
{

    public class DeckHolderPointsUI : PointHolderUI
    {
        private CardGamePoints _points;
        [SerializeField]
        private DeckHolder deckHolder;

        private void Start()
        {
            Setup(deckHolder.CardGamePoints);
        }

        public void Setup(CardGamePoints points)
        {
            _points = points;
            if (_points == null)
                return;
            _points.OnPointValueChange += UpdateUI;

            UpdateUI();
        }

        private void OnDestroy()
        {
            if(_points != null)
                _points.OnPointValueChange -= UpdateUI;
        }

        public void UpdateUI()
        {
            CreatePointUIs(_points.MaxPoints, _points.CurrentPoints);
        }
    }
}
