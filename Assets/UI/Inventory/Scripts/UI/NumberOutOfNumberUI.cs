using UnityEngine;
using TMPro;

namespace UI
{

    public class NumberOutOfNumberUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI numberOutOfNumberText;

        [SerializeField]
        private Color lessColor;
        [SerializeField]
        private Color sameColor;
        [SerializeField]
        private Color moreColor;

        protected void UpdateUI(float number, float outOfNumber)
        {
            number = number.RoundTo(0.01f);
            outOfNumber = outOfNumber.RoundTo(0.01f);

            numberOutOfNumberText.text = $"{number}/{outOfNumber}";

            if(number < outOfNumber)
            {
                numberOutOfNumberText.color = lessColor;
            }
            else if(number == outOfNumber)
            {
                numberOutOfNumberText.color = sameColor;
            }
            else
            {
                numberOutOfNumberText.color = moreColor;
            }
        }
    }
}

