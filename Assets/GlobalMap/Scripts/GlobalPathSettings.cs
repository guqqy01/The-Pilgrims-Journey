using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Collections.Generic;

namespace GlobalMap
{
    [Serializable]
    public class GlobalPathSettings: ISerializationCallbackReceiver

    {
        public int NumberOfPaths = 3;

        public List<MapPointSettings> mapPointSettings;

        /// <summary>
        /// Chance to traverse North and South
        /// </summary>
        public AnimationCurve TraverseChanceAtPointOnMap;

        /// <summary>
        /// Time 0 is the chance at startOfInnerMap
        /// Time 1 is the chance at endOfInnerMap
        /// </summary>
        

        public bool Traverse(float time) => Random.value <= TraverseChanceAtPointOnMap.Evaluate(time);

        /// <summary>
        /// Chance to converge to center of map
        /// </summary>
        public AnimationCurve ConvergeToCenterOfMapChance;
        public bool Converge(float time) => Random.value <= ConvergeToCenterOfMapChance.Evaluate(time);

        /// <summary>
        /// Additional chance to go in tended direction if already traversing and not converging
        /// </summary>
        public ParticleSystem.MinMaxCurve directionTendency;

        public AnimationCurve chanceToTend;
        public bool TendTowardsDirection(float time, out float value)
        {
            value = 0;

            if (Random.value > chanceToTend.Evaluate(time))
                return false;
            
            value = directionTendency.Evaluate(time, Random.value);

            return true;
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            TraverseChanceAtPointOnMap ??= AnimationCurve.Linear(0, 0.5f, 1, 0.5f);
            ConvergeToCenterOfMapChance ??= AnimationCurve.Linear(0, 0.5f, 1, 0.5f);
            chanceToTend ??= AnimationCurve.Linear(0, 0.5f, 1, 0.5f);

            directionTendency.curveMin ??= AnimationCurve.Linear(0, 0, 1, 0);
            directionTendency.curveMax ??= AnimationCurve.Linear(0, 1, 1, 1);
        }

    }

    [Serializable]
    public class MapPointSettings
    {
        public GlobalMapPointType type;

        public Vector2Int amountOfPointsInRange;

        public FloatRangeSlider range;
    }
}
