using System;
using UnityEngine;
namespace Heros.Attacks.Spot
{
    [Serializable]
    public struct RandomMovementStats
    {
        public Vector2 randomChangeMovementTimeRange;

        public RandomMovementStats(Vector2 randomChangeMovementTimeRange)
        {
            this.randomChangeMovementTimeRange = randomChangeMovementTimeRange;
        }
    }
}