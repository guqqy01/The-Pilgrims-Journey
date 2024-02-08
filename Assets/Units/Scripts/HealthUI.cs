using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Units
{
    public class HealthUI : MonoBehaviour
    {
        [SerializeField]
        private Image healthFill;

        [SerializeField]
        private Health health;

        private void Awake()
        {
            health.onHealthChange += OnHealthChange;
        }

        private void OnHealthChange(int currentHealth)
        {
            healthFill.fillAmount = (float)currentHealth / health.MaxHealth;
        }
    }
}

