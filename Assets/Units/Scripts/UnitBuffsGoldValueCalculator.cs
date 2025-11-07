using UnityEngine;
namespace Units
{
    public static class UnitBuffsGoldValueCalculator
    {
        public static int roundToNearestValue = 10;

        public static float additionalReducedValueToBase = 0.8f;

        public static int GetValue(Buff buff, UnitStatsEnum Stat, out int baseBuffValue, out int multiplierBuffValue, out int additionalBuffValue)
        {
            switch (Stat)
            {
                case UnitStatsEnum.Health:
                    baseBuffValue = CalculateHealthBaseBuffValue(buff.BaseBuff.IntValue);
                    multiplierBuffValue = CalculateHealthMultiplierBuffValue(buff.Multiplier);
                    additionalBuffValue = CalculateHealthAdditionalBuffValue(buff.AdditionalBuff.IntValue);
                    return (baseBuffValue + multiplierBuffValue + additionalBuffValue).RoundToNearedInt(roundToNearestValue);
                case UnitStatsEnum.Shield:
                    baseBuffValue = CalculateShieldBaseBuffValue(buff.BaseBuff.IntValue);
                    multiplierBuffValue = CalculateShieldMultiplierBuffValue(buff.Multiplier);
                    additionalBuffValue = CalculateShieldAdditionalBuffValue(buff.AdditionalBuff.IntValue);
                    return (baseBuffValue + multiplierBuffValue + additionalBuffValue).RoundToNearedInt(roundToNearestValue);
                case UnitStatsEnum.ShieldCooldown:
                    baseBuffValue = CalculateShieldCooldownBaseBuffValue((float)buff.BaseBuff.Value);
                    multiplierBuffValue = CalculateShieldCooldownMultiplierBuffValue(buff.Multiplier);
                    additionalBuffValue = CalculateShieldCooldownAdditionalBuffValue((float)buff.AdditionalBuff.Value);
                    return (baseBuffValue + multiplierBuffValue + additionalBuffValue).RoundToNearedInt(roundToNearestValue);
                case UnitStatsEnum.ShieldRegeneration:
                    baseBuffValue = CalculateShieldRegenerationBaseBuffValue((float)buff.BaseBuff.Value);
                    multiplierBuffValue = CalculateShieldRegenerationMultiplierBuffValue(buff.Multiplier);
                    additionalBuffValue = CalculateShieldRegenerationAdditionalBuffValue((float)buff.AdditionalBuff.Value);
                    return (baseBuffValue + multiplierBuffValue + additionalBuffValue).RoundToNearedInt(roundToNearestValue);
                case UnitStatsEnum.MovementSpeed:
                    baseBuffValue = CalculateMovementSpeedBaseBuffValue((float)buff.BaseBuff.Value);
                    multiplierBuffValue = CalculateMovementSpeedMultiplierBuffValue(buff.Multiplier);
                    additionalBuffValue = CalculateMovementSpeedAdditionalBuffValue((float)buff.AdditionalBuff.Value);
                    return (baseBuffValue + multiplierBuffValue + additionalBuffValue).RoundToNearedInt(roundToNearestValue);
                case UnitStatsEnum.SpawnRate:
                    baseBuffValue = CalculateSpawnRateBaseBuffValue((float)buff.BaseBuff.Value);
                    multiplierBuffValue = CalculateSpawnRateMultiplierBuffValue(buff.Multiplier);
                    additionalBuffValue = CalculateSpawnRateAdditionalBuffValue((float)buff.AdditionalBuff.Value);
                    return (baseBuffValue + multiplierBuffValue + additionalBuffValue).RoundToNearedInt(roundToNearestValue);
            }

            baseBuffValue = 0;
            multiplierBuffValue = 0;
            additionalBuffValue = 0;

            return 0;
        }


        #region Funcitons

        public static int Linear(float value, float multiplierPerValue)
        {
            return Mathf.RoundToInt(value * multiplierPerValue);
        }

        public static int LinearCenterAt1(float value, float multiplierPerValue)
        {
            return Mathf.RoundToInt((value - 1) * multiplierPerValue);
        }

        public static int CubedCenteredAt1(float value, float multiplier)
        {
            return Mathf.RoundToInt(multiplier * (Mathf.Pow(value - 1, 3)));
        }

        public static int InvertedCubedCenteredAt1(float value, float multiplier)
        {
            return Mathf.RoundToInt(-multiplier * (Mathf.Pow(value - 1, 3)));
        }

        public static int Quadratic(float value, float multiplierA, float xAxisIntersection)
        {
            return Mathf.RoundToInt(multiplierA * (xAxisIntersection - Mathf.Pow(value, 2)));
        }

        #endregion

        #region Health

        private const int valuePerBaseHealthPoint = 10;

        private const float healthMultiplierA = 10f;

        public static int CalculateHealthBaseBuffValue(int baseValue)
        {
            return baseValue * valuePerBaseHealthPoint;
        }

        public static int CalculateHealthMultiplierBuffValue(float multiplier)
        {
            return Linear(multiplier, healthMultiplierA);
        }

        public static int CalculateHealthAdditionalBuffValue(int additionalValue)
        {
            return Mathf.RoundToInt(additionalValue * valuePerBaseHealthPoint * additionalReducedValueToBase);
        }

        #endregion

        #region Shield

        private const int valuePerBaseShieldPoint = 10;

        private const float shieldMultiplierA = 10f;

        public static int CalculateShieldBaseBuffValue(int baseValue)
        {
            return baseValue * valuePerBaseShieldPoint;
        }

        public static int CalculateShieldMultiplierBuffValue(float multiplier)
        {
            return Linear(multiplier, shieldMultiplierA);
        }

        public static int CalculateShieldAdditionalBuffValue(int additionalValue)
        {
            return Mathf.RoundToInt(additionalValue * valuePerBaseShieldPoint * additionalReducedValueToBase);
        }

        #endregion

        #region ShieldCooldown

        private const int valuePerBaseShieldCooldownPoint = -150;

        private const float shieldCooldownMultiplierA = 10f;

        public static int CalculateShieldCooldownBaseBuffValue(float baseValue)
        {
            return Mathf.RoundToInt(baseValue * valuePerBaseShieldCooldownPoint);
        }

        public static int CalculateShieldCooldownMultiplierBuffValue(float multiplier)
        {
            return Linear(-multiplier, shieldCooldownMultiplierA);
        }

        public static int CalculateShieldCooldownAdditionalBuffValue(float additionalValue)
        {
            return Mathf.RoundToInt(additionalValue * valuePerBaseShieldCooldownPoint * additionalReducedValueToBase);
        }

        #endregion

        #region ShieldRegeneration

        private const int valuePerBaseShieldRegenerationPoint = 50;

        private const float shieldRegenerationMultiplierA = 2f;

        public static int CalculateShieldRegenerationBaseBuffValue(float baseValue)
        {
            return Mathf.RoundToInt(baseValue * valuePerBaseShieldRegenerationPoint);
        }

        public static int CalculateShieldRegenerationMultiplierBuffValue(float multiplier)
        {
            return Linear(multiplier, shieldRegenerationMultiplierA);
        }

        public static int CalculateShieldRegenerationAdditionalBuffValue(float additionalValue)
        {
            return Mathf.RoundToInt(additionalValue * valuePerBaseShieldRegenerationPoint * additionalReducedValueToBase);
        }

        #endregion

        #region MovementSpeed

        private const int valuePerBaseMovementSpeedPoint = 200;

        private const float movementSpeedMultiplierA = 5f;

        public static int CalculateMovementSpeedBaseBuffValue(float baseValue)
        {
            return Mathf.RoundToInt(baseValue * valuePerBaseMovementSpeedPoint);
        }

        public static int CalculateMovementSpeedMultiplierBuffValue(float multiplier)
        {
            return Linear(multiplier, movementSpeedMultiplierA);
        }

        public static int CalculateMovementSpeedAdditionalBuffValue(float additionalValue)
        {
            return Mathf.RoundToInt(additionalValue * valuePerBaseMovementSpeedPoint * additionalReducedValueToBase);
        }

        #endregion

        #region SpawnRate

        private const int valuePerBaseSpawnRatePoint = 100;

        private const float spawnRateMultiplierA = 5f;

        public static int CalculateSpawnRateBaseBuffValue(float baseValue)
        {
            return Mathf.RoundToInt(baseValue * valuePerBaseSpawnRatePoint);
        }

        public static int CalculateSpawnRateMultiplierBuffValue(float multiplier)
        {
            return Linear(multiplier, spawnRateMultiplierA);
        }

        public static int CalculateSpawnRateAdditionalBuffValue(float additionalValue)
        {
            return Mathf.RoundToInt(additionalValue * valuePerBaseSpawnRatePoint * additionalReducedValueToBase);
        }

        #endregion
    }
}
