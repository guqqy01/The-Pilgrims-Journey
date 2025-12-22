using System;
using Heros;
using UnityEngine;
namespace Relics
{

    [Serializable]
    public abstract class RelicEffect
    {
        public Relic Relic { private set; get; }

        public Action updateUI;

        public void Init(Relic relic)
        {
            Relic = relic;
            OnGainRelic();
        }

        public RelicEffect(Relic relic, Action updateUI = null)
        {
            Relic = relic;
            this.updateUI = updateUI;
        }

        public abstract RelicEffect CreateInstance();

        public virtual void OnGainRelic() { }
        public virtual void OnRemoveRelic() { }
        public virtual void OnLevelChangeRelic() { }

        public virtual string GetBottomText() { return ""; }
        public virtual string GetTopText() { return ""; }
        public virtual void OnPlayEffect() { }
        public virtual void AfterPlayEffect() { }
        public virtual void Reset() { }

        public abstract string Description { get; }
    }

    public class HeroWeaponRelicEffect : RelicEffect
    {
        [SerializeField]
        private HeroAttackSO weapon;

        public HeroWeaponRelicEffect(Relic relic, Action updateUI = null) : base(relic, updateUI) { }

        public override string Description => throw new NotImplementedException();

        public override RelicEffect CreateInstance()
        {
            return new HeroWeaponRelicEffect(Relic, updateUI);
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
            // Implement logic for when the relic level changes, if needed
        }
    }
}
