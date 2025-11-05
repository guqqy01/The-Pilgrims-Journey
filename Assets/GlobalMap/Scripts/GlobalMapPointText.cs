using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
namespace Map
{
    public class GlobalMapPointText : MonoBehaviour
    {
        [SerializeField]
        public TextMeshProUGUI text;

        public Color Color
        {
            set
            {
                text.color = value;
            }
        }
    }

}
