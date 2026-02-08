using System;
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
        public virtual void OnLevelChangeRelic(int currentLevel) { }

        public virtual string GetBottomText() { return ""; }
        public virtual string GetTopText() { return ""; }
        public virtual void OnPlayEffect() { }
        public virtual void AfterPlayEffect() { }
        public virtual void Reset() { }

        public abstract string Description { get; }
    }
}
