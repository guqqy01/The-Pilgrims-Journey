using Tiles;
namespace TPJ.Gameplay
{
    /// <summary>
    /// Shared placement logic for any card whose payload is a TileBase. Delegates
    /// entirely to TileGridBase so placement rules stay defined in exactly one place.
    /// </summary>
    public abstract class TileCard<T> : Card<T> where T : TileBase, ICardable
    {
        protected TileCard(T cardableRef) : base(cardableRef) { }

        public override bool CanPlay(CardPlayContext context)
        {
            if (context.Grid == null)
                return false;

            return context.Grid.CanPlaceTile(CardableRef, context.GridPosition, context.Rotation);
        }

        protected override void ProtectedPlayCard(CardPlayContext context)
        {
            context.Grid.TryPlaceTile(CardableRef, context.GridPosition, context.Rotation);
            TileOwnership.Set(CardableRef, Owner);
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