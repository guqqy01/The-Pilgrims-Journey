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
        [SerializeField]
        private PoisonDebuff poisonDebuff;
        private void Awake()
        {
            health.onHealthChange += OnHealthChange;

            if(poisonDebuff != null)
            {
                poisonDebuff.onCured += OnCured;
                poisonDebuff.onPoisoned += OnPoisoned;
            }
        }

        private void OnDestroy()
        {
            health.onHealthChange -= OnHealthChange;
            poisonDebuff.onCured -= OnCured;
            poisonDebuff.onPoisoned -= OnPoisoned;
        }

        private void OnHealthChange(int currentHealth)
        {
            healthFill.fillAmount = Mathf.Clamp01((float)currentHealth / health.MaxHealth);
        }

        private void OnPoisoned()
        {
            healthFill.color = Color.green;
        }

        private void OnCured()
        {
            healthFill.color = Color.white;
        }
    }
}

