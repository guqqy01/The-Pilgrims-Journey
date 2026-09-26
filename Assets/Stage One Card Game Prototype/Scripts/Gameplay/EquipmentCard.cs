using Equipment;
using System;
using Tiles;

namespace TPJ.Gameplay
{

    [Serializable]
    public class EquipmentCard : Card<EquipmentSO, EquipmentCardPlayContext>, ITileCardType
    {
        public TileCardType TileCardType => CardableRef.GrantedTileCardType;

        public EquipmentCard(EquipmentSO cardableRef) : base(cardableRef) { }

        public override IDescriptable GetDisplayInfo() => CardableRef;

        public override bool AbstractCanPlay(EquipmentCardPlayContext context)
        {
            if (context.TargetTile is not IEquipmentCardTarget)
                return false;

            if (TileOwnership.Get(context.TargetTile) != Owner)
                return false;

            if (!context.TargetTile.HasRoomForEquipment)
                return false;

            return true;
        }

        public override bool HasAnyLegalPlay(ICardLegalityProvider provider)
        {
            foreach (var tile in provider.GetTargetableTiles(Owner))
                if (tile is IEquipmentCardTarget && TileOwnership.Get(tile) == Owner && tile.HasRoomForEquipment)
                    return true;

            return false;
        }

        protected override void ProtectedPlayCard(EquipmentCardPlayContext context)
        {
            ((IEquipmentCardTarget)context.TargetTile).ApplyEquipment(CardableRef);

            CardGameEventsBus.RaiseOnEquipmentCardPlayed(Owner, CardableRef);
        }
    }
}