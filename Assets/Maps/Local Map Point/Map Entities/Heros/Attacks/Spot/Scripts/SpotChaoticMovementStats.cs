using System;
using UnityEngine;
namespace Heros.Attacks.Spot
{
    [Serializable]
    public struct SpotChaoticMovementStats
    {
        public float chaoticMovementSpeed;
        public float chaoticMovementRadius;

        public Vector2 chaoticChangeMovementTimeRange;

        public SpotChaoticMovementStats(float chaoricMovementSpeed, float chaoticMovementRadius, Vector2 chaoticChangeMovementTimeRange)
        {
            this.chaoticMovementSpeed = chaoricMovementSpeed;
            this.chaoticMovementRadius = chaoticMovementRadius;
            this.chaoticChangeMovementTimeRange = chaoticChangeMovementTimeRange;
        }
    }
}