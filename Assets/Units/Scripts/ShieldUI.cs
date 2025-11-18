using UnityEngine;
using UnityEngine.UI;

namespace Units.UI
{
    public class ShieldUI : MonoBehaviour
    {
        [SerializeField]
        private Image shieldFill;

        [SerializeField]
        private Shield shield;

        [SerializeField]
        private GameObject shieldUIGO;

        private void Awake()
        {
            if(shield == null)
                shield = GetComponentInParent<Shield>();

            if(shield != null)
                shield.onShieldChange += OnShieldChange;
            else
                shieldUIGO.SetActive(false);
        }

        private void OnDestroy()
        {
            if(shield != null)
                shield.onShieldChange -= OnShieldChange;
        }

        private void OnShieldChange(StatChangeInfo shieldChangeInfo)
        {
            if(shield.MaxShield == 0)
            {
                shieldUIGO.SetActive(false);
                shieldFill.fillAmount = 0;
            }
            else
            {
                shieldUIGO.SetActive(true);
                shieldFill.fillAmount = Mathf.Clamp01((float)shieldChangeInfo.CurrentAmount / shield.MaxShield);
            }
        }
    }
}

