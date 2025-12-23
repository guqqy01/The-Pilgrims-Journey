using System;
using Heros;
using UnityEngine;
namespace Relics
{
    public class HeroWeaponRelicEffect : RelicEffect
    {
        [SerializeField]
        private HeroAttackSO weapon;

        public HeroWeaponRelicEffect() : base(null) { }

        public HeroWeaponRelicEffect(Relic relic, HeroAttackSO heroAttackSO, Action updateUI = null) : base(relic, updateUI) 
        { 
            weapon = heroAttackSO;
        }

        public override string Description => throw new NotImplementedException();

        public override RelicEffect CreateInstance()
        {
            return new HeroWeaponRelicEffect(Relic, weapon, updateUI);
        }

        public override void OnGainRelic()
        {
            HeroAttacksManager.Instance.AddHeroAttack(weapon);
        }

        public override void OnRemoveRelic()
        {
            HeroAttacksManager.Instance.RemoveHeroAttack(weapon);
        }

        public override void OnLevelChangeRelic()
        {
            HeroAttacksManager.Instance.SetHeroAttackLevel(weapon, Relic.Level);
        }
    }
}
