using UnityEngine;
using System;
namespace TPJ.UI
{
    [Serializable]
    public class CardHandUILayout
    {
        public float sideScale = 0.75f;
        public float yPosMultiplier = 60f;
        public float yPosPower = 2f;
        public float rotationMultiplier = 8f;

        [Header("Fit To Area")]
        [Tooltip("Minimum spacing allowed between card centers, as a fraction of one card's width. " +
                 "Prevents cards from fully overlapping when there are too many to fit comfortably.")]
        [Range(0.05f, 1f)]
        public float minSpacingRatio = 0.35f;

        /// <summary>
        /// Spacing between card centers so that `cardCount` cards (each `cardWidth` wide) fit
        /// exactly within `availableWidth`, floored at `minSpacingRatio * cardWidth` so cards
        /// never fully stack on top of each other.
        /// </summary>
        public float GetSpacing(int cardCount, float availableWidth, float cardWidth)
        {
            if (cardCount <= 1)
                return 0f;

            float minSpacing = cardWidth * minSpacingRatio;
            float spacingToFit = (availableWidth - cardWidth) / (cardCount - 1);

            return Mathf.Max(spacingToFit, minSpacing);
        }

        public void Evaluate(float index, float spacing, out Vector2 pos, out Quaternion rot, out float scale)
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