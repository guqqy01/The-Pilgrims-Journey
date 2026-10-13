using Monsters;
using System;
using Units;

namespace Quests
{

    /// <summary>
    /// A runtime quest for one level. Override the hooks you need, then call Complete() or Fail().
    /// </summary>
    public abstract class Quest
    {
        public QuestState State { get; private set; } = QuestState.Active;
        public QuestReward Reward { get; set; }

        public abstract string Title { get; }
        public abstract string Description { get; }
        public virtual string ProgressText => string.Empty;
        public virtual string FailureReason => "You failed a required quest.";
        public event Action<Quest> onChanged;

        protected QuestContext Context { get; private set; }

        public void Init(QuestContext context) => Context = context;
        public void Begin() => OnBegin();

        protected virtual void OnBegin() { }

        /// <summary>Called every frame while the quest is active and the level is playing.</summary>
        public virtual void Tick(float deltaTime) { }

        public virtual void OnUnitSpawned(Unit unit) { }
        public virtual void OnUnitDied(Unit unit) { }
        public virtual void OnUnitEscaped(Unit unit) { }
        public virtual void OnMonsterKilled(Monster monster) { }

        public virtual void OnBossKilled(Monster monster) { }
        public virtual void OnUnitPassed(int totalPassed) { }

        /// <summary>Called once when the level finishes. Anything still active should resolve here. Default: fail.</summary>
        public virtual void OnLevelEnd() => Fail();

        protected void NotifyChanged() => onChanged?.Invoke(this);

        protected void Complete() => SetState(QuestState.Completed);
        protected void Fail() => SetState(QuestState.Failed);

        private void SetState(QuestState state)
        {
            if (State != QuestState.Active)
                return;

            State = state;
            onChanged?.Invoke(this);
        }
    }
}
