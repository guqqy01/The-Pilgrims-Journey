using UnityEngine;
using AYellowpaper.SerializedCollections;

namespace UI.WindowStyles
{
    [CreateAssetMenu(menuName = "UI/Window Style")]
    public class UIWindowStyle : ScriptableObject
    {
        public UIWindowStyle parentStyle;

        public SerializedDictionary<UIWindowStyleLabel, Sprite> styleItems;

        public bool TryGetSprite(UIWindowStyleLabel label, out Sprite sprite)
        {
            if (label == null)
            {
                sprite = null;
                return false;
            }

            if (styleItems != null &&
                styleItems.TryGetValue(label, out sprite) &&
                sprite != null)
                return true;

            if (parentStyle != null)
                return parentStyle.TryGetSprite(label, out sprite);

            sprite = null;
            return false;
        }
    }

}

