using UnityEngine;
using System;
using UnityEditor;
using System.Collections.Generic;

namespace UI.WindowStyles
{
    public class UIWindowStyleManager : MonoBehaviour
    {
        public UIWindowStyle currentStyle;

        [SerializeField]
        private UIWindowStyleObject[] cachedObjects;

        private void Reset()
        {
            CacheObjects();
        }

        private void OnTransformChildrenChanged()
        {
            CacheObjects();
            ApplyStyle();
        }

        private void OnValidate()
        {
            ApplyStyle();
        }

        private void CacheObjects()
        {
            cachedObjects = GetComponentsInChildren<UIWindowStyleObject>(true);
        }

        public void SetStyle(UIWindowStyle newStyle)
        {
            currentStyle = newStyle;
            ApplyStyle();
        }

        public void ApplyStyle()
        {
            if (currentStyle == null)
                return;

            if (cachedObjects == null || cachedObjects.Length == 0)
                CacheObjects();

            foreach (var obj in cachedObjects)
            {
#if UNITY_EDITOR
                if (obj == null)
                    continue;
                if (!currentStyle.TryGetSprite(obj.label, out _))
                {
                    Debug.LogWarning(
                        $"Missing sprite for label {obj.label.name} in style {currentStyle.name}",
                        obj);
                }
#endif
                obj.Apply(currentStyle);
            }
        }
    }

}

