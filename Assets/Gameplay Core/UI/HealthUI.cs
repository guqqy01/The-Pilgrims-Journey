using UnityEngine;
using UnityEngine.UI;

namespace Units.UI
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
            if (health != null)
                health.onHealthChange += OnHealthChange;

            if(poisonDebuff != null)
            {
                poisonDebuff.onCured += OnCured;
                poisonDebuff.onPoisoned += OnPoisoned;
            }
        }

        private void OnDestroy()
        {
            if (health != null)
                health.onHealthChange -= OnHealthChange;

            if (poisonDebuff != null)
            {
                poisonDebuff.onCured -= OnCured;
                poisonDebuff.onPoisoned -= OnPoisoned;
            }
        }

        private void OnHealthChange(StatChangeInfo healthChangeInfo)
        {
            healthFill.fillAmount = Mathf.Clamp01((float)healthChangeInfo.CurrentAmount / health.MaxHealth);
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

