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
            if(shield.MaxShield == 0)
            {
                shieldFill.fillAmount = 0;
            }
            else
            {
                shieldFill.fillAmount = Mathf.Clamp01((float)currentShield / shield.MaxShield);
            }
        }
    }
}

