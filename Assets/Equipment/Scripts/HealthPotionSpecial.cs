using UnityEngine;
using Units;
using System;

namespace Equipment
{
    [Serializable]
    public class HealthPotionSpecial : EquipmentSpecial
    {
        private Health health;

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

        public void Init(params object[] initParams)
        {
            if (initParams.Length < 1 || initParams[0] is not Health)
                return;

            health = (Health)initParams[0];
            health.onHealthChange += OnHealthChange;
            amountOfMaxHealthToRecover = Mathf.CeilToInt(health.MaxHealth * percentOfHealthToRecover);
            recoveryRate = periodOfTimeToHeal / amountOfMaxHealthToRecover; ;
        }

        private void OnHealthChange(StatChangeInfo healthChangeInfo)
        {
            if ((float)healthChangeInfo.CurrentAmount / health.MaxHealth < healthPercentageToStartRecovery)
                started = true;
        }

        public void Update()
        {
            if (!started)
                return;

            if (timeToNextHeal > Time.time)
                return;

            if (health != null && health.HealingPS != null && !health.HealingPS.isPlaying)
                health.HealingPS.Play();

            timeToNextHeal = Time.time + recoveryRate;

            health.ModifyHealth(1);

            amountOfMaxHealthToRecover--;

            if (health != null && health.HealingPS != null && amountOfMaxHealthToRecover <= 0)
            {
                health.HealingPS.Stop();
            }
        }

        public int GoldValue()
        {
            return goldValue;
        }
    }
}
