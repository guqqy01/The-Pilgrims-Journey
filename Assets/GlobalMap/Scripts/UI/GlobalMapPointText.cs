using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace GlobalMap.UI
{
    public class GlobalMapPointText : MonoBehaviour
    {
        [SerializeField]
        private Image image;

        public Color Color
        {
            set
            {
                image.color = value;
            }
        }
    }

}
