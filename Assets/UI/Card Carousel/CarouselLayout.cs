using UnityEngine;
using System;

namespace UI.CardCarousel
{
    [Serializable]
    public class CarouselLayout
    {
        public float spacing = 450f;
        public float sideScale = 0.75f;
        public float yPosMultiplier = 60f;
        public float yPosPower = 2f;
        public float rotationMultiplier = 8f;

        public void Evaluate(float relativeIndex, out Vector2 pos, out Quaternion rot, out float scale, out float alpha, out int depth)
        {
            float absIndex = Mathf.Abs(relativeIndex);

            float y = -Mathf.Pow(absIndex, yPosPower) * yPosMultiplier;
            pos = new Vector2(relativeIndex * spacing, y);

            rot = Quaternion.Euler(0, 0, -relativeIndex * rotationMultiplier);

            float t = Mathf.Clamp01(absIndex / 4f);

            scale = Mathf.Lerp(1f, sideScale, t);
            alpha = Mathf.Lerp(1f, 0f, t);

            depth = (int)(1000f * (1 - absIndex));

            if (absIndex < 0.2f)
                scale *= 1.1f;
        }
    }
}

