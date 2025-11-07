using UnityEngine;
using System;
using Random = UnityEngine.Random;
using static UnityEngine.ParticleSystem;
using System.Collections.Generic;
using UnityEditor.UIElements;

namespace GlobalMap
{
    [Serializable]
    public class GlobalPathSettings: ISerializationCallbackReceiver

    {
        public int NumberOfPaths = 3;
        public int NumberOfRelics;
        public int NumberOfRandomEncounters;
        public int NumberOfShops;

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

        [Range(0f, 0.5f)]
        public float reverseDirectionChance = 0f;
        public bool Reverse => Random.value <= reverseDirectionChance;


        public GlobalPathSettings()
        {
            TraverseChanceAtPointOnMap = new AnimationCurve();
            ConvergeToCenterOfMapChance = new AnimationCurve();
            chanceToTend = new AnimationCurve();
            directionTendency = new MinMaxCurve
            (
                1f,
                new AnimationCurve(),
                new AnimationCurve()
            );
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            if (TraverseChanceAtPointOnMap == null)
                TraverseChanceAtPointOnMap = new AnimationCurve();

            if (ConvergeToCenterOfMapChance == null)
                ConvergeToCenterOfMapChance = new AnimationCurve();

            if (chanceToTend == null)
                chanceToTend = new AnimationCurve();

            // MinMaxCurve defaults may have null curves
            if (directionTendency.curveMin == null)
                directionTendency.curveMin = new AnimationCurve();

            if (directionTendency.curveMax == null)
                directionTendency.curveMax = new AnimationCurve();
        }

    }
}
