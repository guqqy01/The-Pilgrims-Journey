using UnityEngine;
using System;
namespace TPJ.UI
{
    [Serializable]
    public class CardHandUILayout
    {
        public float spacing = 450f;
        public float sideScale = 0.75f;
        public float yPosMultiplier = 60f;
        public float yPosPower = 2f;
        public float rotationMultiplier = 8f;

        public void Evaluate(float index, out Vector2 pos, out Quaternion rot, out float scale)
        {
            float absIndex = Mathf.Abs(index);

            float y = -Mathf.Pow(absIndex, yPosPower) * yPosMultiplier;
            pos = new Vector2(index * spacing, y);

            rot = Quaternion.Euler(0, 0, -index * rotationMultiplier);

            float t = Mathf.Clamp01(absIndex / 4f);

            scale = Mathf.Lerp(1f, sideScale, t);
        }
    }
}
