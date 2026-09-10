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

        private readonly Dictionary<BaseAttack, HeroAttackItemUI> _heroAttackItemUIs = new ();

        public override void InitiateFromRootAtAwake()
        {
            base.InitiateFromRootAtAwake();
            HeroAttacksManager.WhenReady(SubscribeToHeroAttacksManager);
        }

        protected override void CleanUp()
        {
            HeroAttacksManager.IfReady(UnSubscribeToHeroAttacksManager);
        }

        private void UnSubscribeToHeroAttacksManager(HeroAttacksManager heroAttacksManager)
        {
            heroAttacksManager.OnAddAttackEvent -= CreateHeroAttackItemUI;
            heroAttacksManager.OnAttackLevelChangeEvent -= UpdateHeroAttackItemUI;
            heroAttacksManager.OnRemoveAttackEvent -= RemoveHeroAttackItemUI;
        }

        private void SubscribeToHeroAttacksManager(HeroAttacksManager heroAttacksManager)
        {
            heroAttacksManager.OnAddAttackEvent += CreateHeroAttackItemUI;
            heroAttacksManager.OnAttackLevelChangeEvent += UpdateHeroAttackItemUI;
            heroAttacksManager.OnRemoveAttackEvent += RemoveHeroAttackItemUI;
        }

        public void CreateHeroAttackItemUI(BaseAttack heroAttack)
        {
            if (_heroAttackItemUIs.ContainsKey(heroAttack))
            {
                if (_heroAttackItemUIs[heroAttack] != null)
                {
                    UpdateHeroAttackItemUI(heroAttack);
                    return;
                }
                else
                {
                    _heroAttackItemUIs.Remove(heroAttack);
                }
            }

            HeroAttackItemUI heroAttackItemUI = Instantiate(heroAttackItemPrefab, heroAttacksHolderTransform);
            heroAttackItemUI.UpdateUI(heroAttack);


            _heroAttackItemUIs.Add(heroAttack, heroAttackItemUI);
        }

        public void UpdateHeroAttackItemUI(BaseAttack heroAttack)
        {
            _heroAttackItemUIs[heroAttack].UpdateUI(heroAttack);
        }

        public void RemoveHeroAttackItemUI(BaseAttack heroAttack)
        {
            Destroy(_heroAttackItemUIs[heroAttack].gameObject);
            _heroAttackItemUIs.Remove(heroAttack);
        }
    }

}
