using System;
namespace Relics
{

    [Serializable]
    public abstract class RelicEffect
    {
        public Relic Relic { private set; get; }

        public delegate void UpdateUI();
        public UpdateUI updateUI;

        public void Init(Relic relic)
        {
            Relic = relic;
            OnGainRelic();
        }

        public RelicEffect Clone
        {
            get
            {
                return (RelicEffect)MemberwiseClone();
            }
        }

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
}
