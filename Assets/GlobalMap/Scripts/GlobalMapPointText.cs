using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
namespace Map
{
    public class GlobalMapPointText : MonoBehaviour
    {
        [SerializeField]
        public Text text;

        public Color Color
        {
            set
            {
                text.color = value;
            }
        }
    }

}
