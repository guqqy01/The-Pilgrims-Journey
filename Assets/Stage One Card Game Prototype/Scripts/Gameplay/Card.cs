using System;

namespace TPJ.Gameplay
{

    public interface ICard
    {
        public bool TryPlayCard<ICardPlayContext>(ICardPlayContext context);
    }

    [Serializable]
    public abstract class Card<TCardable, TPlayContext> : CardBase
        where TCardable : ICardable, IDescriptable
        where TPlayContext : ICardPlayContext
    {
        public TCardable CardableRef { protected set; get; }

        protected Card(TCardable cardableRef)
        {
            CardableRef = cardableRef;
            CardActionCost = cardableRef.ActionPointCost;
        }

        public override bool TryPlayCard<TMethodPlayContext>(TMethodPlayContext methodContext)
        {
            if (methodContext is not TPlayContext context)
                return false;

            if (HasBeenPlayed)
                return false;

            if (!AbstractCanPlay(context))
                return false;

            Owner.CardGamePoints.ModifyPoints(-CardActionCost);

            ProtectedPlayCard(context);

            CardGameEventsBus.RaiseOnCardPlayed(Owner, this);
            HasBeenPlayed = true;
            return true;
        }

        public abstract bool AbstractCanPlay(TPlayContext context);
        protected abstract void ProtectedPlayCard(TPlayContext context);

        public override string ToString() => $"{GetType().Name}({CardableRef})";
    }
}