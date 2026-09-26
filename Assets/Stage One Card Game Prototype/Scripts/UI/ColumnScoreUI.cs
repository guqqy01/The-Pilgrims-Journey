using TMPro;
using UnityEngine;
namespace TPJ.UI
{

    public class ColumnScoreUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI text;
        [SerializeField]
        private Color positiveColor;
        [SerializeField]
        private Color negativeColor;
        [SerializeField]
        private Color neutralColor;
        public void UpdateScore(int score)
        {
            text.text = score.ToString();
            if(score == 0)
            {
                text.color = neutralColor;
            }
            else if (score.Positive())
            {
                text.color = positiveColor;
            }
            else
            {
                text.color = negativeColor;
            }
        }
    }
}
