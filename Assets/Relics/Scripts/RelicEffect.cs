using System;
namespace Relics
{
    [Serializable]
    public abstract class RelicEffect
    {
        public Action updateUI;

        public void Init(Relic relic)
        {
            OnGainRelic(relic);
        }

        public RelicEffect(Action updateUI = null)
        {
            this.updateUI = updateUI;
        }

        public abstract RelicEffect CreateInstance(Relic relic);

        public virtual void OnGainRelic(Relic relic) { }
        public virtual void OnRemoveRelic(Relic relic) { }
        public virtual void OnLevelChangeRelic(Relic relic, int currentLevel) { }

        public virtual string GetBottomText() { return ""; }
        public virtual string GetTopText() { return ""; }
        public virtual void OnPlayEffect(Relic relic) { }
        public virtual void AfterPlayEffect(Relic relic) { }
        public virtual void Reset(Relic relic) { }

        public abstract string Description(Relic relic);
        public abstract string Description(RelicSO relicSO);
    }
}
