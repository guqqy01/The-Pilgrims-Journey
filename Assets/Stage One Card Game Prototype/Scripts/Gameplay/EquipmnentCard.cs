using Equipment;
using System;
using Tiles;

namespace TPJ.Gameplay
{
    [Serializable]
    public class EquipmentCard : Card<EquipmentSO>
    {
        public EquipmentCard(EquipmentSO cardableRef) : base(cardableRef) { }

        public override IDescriptable GetDisplayInfo() => CardableRef;

        public override bool CanPlay(CardPlayContext context)
        {
            if (context.TargetTile is not IEquipmentTarget)
                return false;

            // Self-only for now, per current design - drop this line if/when
            // you allow debuffing the opponent's tiles.
            return TileOwnership.Get(context.TargetTile) == Owner;
        }

        protected override void ProtectedPlayCard(CardPlayContext context)
        {
            ((IEquipmentTarget)context.TargetTile).ApplyEquipment(CardableRef);
        }

        public override bool HasAnyLegalPlay(ICardLegalityProvider provider)
        {
            foreach (var tile in provider.GetTargetableTiles(Owner))
                if (tile is IEquipmentTarget && TileOwnership.Get(tile) == Owner)
                    return true;

            return false;
        }
    }
}