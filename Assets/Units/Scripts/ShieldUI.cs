using UnityEngine;
using UnityEngine.UI;

namespace Units
{
    public class ShieldUI : MonoBehaviour
    {
        [SerializeField]
        private Image shieldFill;

        [SerializeField]
        private Shield shield;

        private void Awake()
        {
            shield.onShieldChange += OnShieldChange;
        }

        private void OnShieldChange(int currentShield)
        {
            shieldFill.fillAmount = (float)currentShield / shield.MaxShield;
        }
    }
}

