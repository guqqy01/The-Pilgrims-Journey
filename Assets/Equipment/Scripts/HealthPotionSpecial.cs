using UnityEngine;
using Units;
using System;

namespace Equipment
{
    [Serializable]
    public class HealthPotionSpecial : EquipmentSpecial
    {
        private Health health;
        private Unit unit;

        [SerializeField]
        private float healthPercentageToStartRecovery;

        private bool started;

        private float timeToNextHeal = 0;

        private float recoveryRate;

        [SerializeField]
        private float percentOfHealthToRecover;

        private int amountOfMaxHealthToRecover;

        [SerializeField]
        private float periodOfTimeToHeal;

        [SerializeField]
        private int goldValue = 100;

        public void Init(Unit unit)
        {
            this.unit = unit;
            health = unit.Health;
            health.onHealthChange += OnHealthChange;
            amountOfMaxHealthToRecover = Mathf.CeilToInt(health.MaxHealth * percentOfHealthToRecover);
            recoveryRate = periodOfTimeToHeal / amountOfMaxHealthToRecover; ;
        }

        private void OnHealthChange(int currentHealth)
        {
            if ((float)currentHealth / health.MaxHealth < healthPercentageToStartRecovery)
                started = true;
        }

        public void Update()
        {
            if (!started)
                return;

            if (timeToNextHeal > Time.time)
                return;

            if (unit != null && unit.HealingPS != null && !unit.HealingPS.isPlaying)
                unit.HealingPS.Play();

            timeToNextHeal = Time.time + recoveryRate;

            health.ModifyHealth(1);

            amountOfMaxHealthToRecover--;

            if (unit != null && unit.HealingPS != null && amountOfMaxHealthToRecover <= 0)
            {
                unit.HealingPS.Stop();
            }
        }

        public int GoldValue()
        {
            return goldValue;
        }
    }

    public interface EquipmentSpecial
    {
        public void Init(Unit unit);

        public void Update();


        public int GoldValue();
    }
}
