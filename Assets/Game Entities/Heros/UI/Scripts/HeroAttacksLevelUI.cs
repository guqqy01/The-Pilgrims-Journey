using Helper.Wait;
using Heros;
using System.Collections.Generic;
using UnityEngine;

namespace Attacks.UI
{
    public class HeroAttacksLevelUI : Singleton<HeroAttacksLevelUI>
    {
        [SerializeField]
        private HeroAttackItemUI heroAttackItemPrefab;

        [SerializeField]
        private Transform heroAttacksHolderTransform;

        private Dictionary<BaseAttack, HeroAttackItemUI> heroAttackItemUIs = new Dictionary<BaseAttack, HeroAttackItemUI>();

        public override void InitiateFromRootAtAwake()
        {
            base.InitiateFromRootAtAwake();
            WaitForHelper.WaitForObjectThenPerformAction(() => HeroAttacksManager.Instance, SubscribeToHeroAttacksManager);
        }

        private void OnDestroy()
        {
            if (HeroAttacksManager.Instance == null)
                return;

            HeroAttacksManager.Instance.OnAddAttackEvent -= CreateHeroAttackItemUI;
            HeroAttacksManager.Instance.OnAttackLevelChangeEvent -= UpdateHeroAttackItemUI;
            HeroAttacksManager.Instance.OnRemoveAttackEvent -= RemoveHeroAttackItemUI;
        }

        private void SubscribeToHeroAttacksManager()
        {
            HeroAttacksManager.Instance.OnAddAttackEvent += CreateHeroAttackItemUI;
            HeroAttacksManager.Instance.OnAttackLevelChangeEvent += UpdateHeroAttackItemUI;
            HeroAttacksManager.Instance.OnRemoveAttackEvent += RemoveHeroAttackItemUI;
        }

        public void CreateHeroAttackItemUI(BaseAttack heroAttack)
        {
            if (heroAttackItemUIs.ContainsKey(heroAttack))
            {
                if (heroAttackItemUIs[heroAttack] != null)
                {
                    UpdateHeroAttackItemUI(heroAttack);
                    return;
                }
                else
                {
                    heroAttackItemUIs.Remove(heroAttack);
                }
            }

            HeroAttackItemUI heroAttackItemUI = Instantiate(heroAttackItemPrefab, heroAttacksHolderTransform);
            heroAttackItemUI.UpdateUI(heroAttack);


            heroAttackItemUIs.Add(heroAttack, heroAttackItemUI);
        }

        public void UpdateHeroAttackItemUI(BaseAttack heroAttack)
        {
            heroAttackItemUIs[heroAttack].UpdateUI(heroAttack);
        }

        public void RemoveHeroAttackItemUI(BaseAttack heroAttack)
        {
            Destroy(heroAttackItemUIs[heroAttack].gameObject);
            heroAttackItemUIs.Remove(heroAttack);
        }
    }

}
