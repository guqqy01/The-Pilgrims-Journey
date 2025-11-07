using DialogueSystem;
using Relics;
using UnityEngine;

namespace RandomEvents.HoardingWizard
{
    public class HoardingWizardEventListernerAndConditions : MonoBehaviour
    {
        [SerializeField]
        private DialogueEventListener acceptTheHoardingWizardsItemListener;

        [SerializeField]
        private RelicSO hoardingWizardRelic;

        private void OnEnable()
        {
            acceptTheHoardingWizardsItemListener.Register();
        }

        private void OnDisable()
        {
            acceptTheHoardingWizardsItemListener.Unregister();
        }

        public void AcceptTheHoardingWizardsItem()
        {
            RelicManager.Instance.AddRelic(hoardingWizardRelic);
        }
    }
}

