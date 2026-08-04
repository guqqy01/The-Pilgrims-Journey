using Helper.Wait;
using Heros;
using System.Collections.Generic;
using UnityEngine;

namespace Heros.Attacks.UI
{
    public class HeroAttacksLevelUI : Singleton<HeroAttacksLevelUI>
    {
        [SerializeField]
        private HeroAttackItemUI heroAttackItemPrefab;

        [SerializeField]
        private Transform heroAttacksHolderTransform;

        private Dictionary<BaseHeroAttack, HeroAttackItemUI> heroAttackItemUIs = new Dictionary<BaseHeroAttack, HeroAttackItemUI>();

        public override void InitiateFromRootAtAwake()
        {
            base.InitiateFromRootAtAwake();
            WaitForHelper.WaitForObjectThenPerformAction(() => HeroAttacksManager.Instance, SubscribeToHeroAttacksManager);
        }

        private void OnDestroy()
        {
            if (HeroAttacksManager.Instance == null)
                return;

            HeroAttacksManager.Instance.onAddHeroAttack -= CreateHeroAttackItemUI;
            HeroAttacksManager.Instance.onHeroAttackLevelChange -= UpdateHeroAttackItemUI;
            HeroAttacksManager.Instance.onRemoveHeroAttack -= RemoveHeroAttackItemUI;
        }

        private void SubscribeToHeroAttacksManager()
        {
            HeroAttacksManager.Instance.onAddHeroAttack += CreateHeroAttackItemUI;
            HeroAttacksManager.Instance.onHeroAttackLevelChange += UpdateHeroAttackItemUI;
            HeroAttacksManager.Instance.onRemoveHeroAttack += RemoveHeroAttackItemUI;
        }

        public void CreateHeroAttackItemUI(BaseHeroAttack heroAttack)
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

        public void UpdateHeroAttackItemUI(BaseHeroAttack heroAttack)
        {
            heroAttackItemUIs[heroAttack].UpdateUI(heroAttack);
        }

        public void RemoveHeroAttackItemUI(BaseHeroAttack heroAttack)
        {
            Destroy(heroAttackItemUIs[heroAttack].gameObject);
            heroAttackItemUIs.Remove(heroAttack);
        }
    }

}
