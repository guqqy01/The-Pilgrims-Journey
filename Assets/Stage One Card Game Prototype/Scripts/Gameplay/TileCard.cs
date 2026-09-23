using Tiles;
namespace TPJ.Gameplay
{
    /// <summary>
    /// Shared placement logic for any card whose payload is a TileBase. Delegates
    /// entirely to TileGridBase so placement rules stay defined in exactly one place.
    /// </summary>
    public abstract class TileCard<T> : Card<T, TileCardPlayContext>, ITileCardType 
        where T : TileBase, ICardable
    {
        public TileCardType TileCardType { protected set; get; }

        protected TileCard(T cardableRef) : base(cardableRef)
        {
            TileCardType = cardableRef.TileCardType;
        }

        public override bool AbstractCanPlay(TileCardPlayContext context)
        {
            if (context.Grid == null)
                return false;

            return context.Grid.CanPlaceTile(CardableRef, context.GridPosition, context.Rotation);
        }

        protected override void ProtectedPlayCard(TileCardPlayContext context)
        {
            if (!context.Grid.TryPlaceTile(CardableRef, context.GridPosition, context.Rotation, false))
                return;

            CardGameEventsBus.RaiseOnTileCardPlayed(Owner, CardableRef);
            TileOwnership.Set(CardableRef, Owner);
            context.Grid.onTileAdded?.Invoke(CardableRef);
        }

        public override bool HasAnyLegalPlay(ICardLegalityProvider provider)
        {
            var grid = provider.GetGridFor(Owner);
            if (grid == null) return false;

            foreach (var pos in grid.GetFreeTilePositions())
                for (float rot = 0f; rot < 360f; rot += 90f)
                    if (grid.CanPlaceTile(CardableRef, pos, rot))
                        return true;

            return false;
        }
    }
}