using Attacks;
using System;
using Tiles;

namespace TPJ.Gameplay
{
    [Serializable]
    public class AttackCard : Card<BaseAttackSO, AttackCardPlayContext>, ITileCardType
    {
        public AttackCard(BaseAttackSO cardableRef) : base(cardableRef)
        {

        }

        public TileCardType TileCardType => CardableRef.TileCardType;

        public override bool AbstractCanPlay(AttackCardPlayContext context)
        {
            if (context.TargetTile is not IAttackCardTarget)
                return false;

            if (TileOwnership.Get(context.TargetTile) != Owner)
                return false;

            if (!context.TargetTile.HasRoomForAttack)
                return false;

            return true;
        }

        public override IDescriptable GetDisplayInfo()
        {
            return CardableRef;
        }

        public override bool HasAnyLegalPlay(ICardLegalityProvider provider)
        {
            foreach (var tile in provider.GetTargetableTiles(Owner))
                if (tile is IAttackCardTarget && TileOwnership.Get(tile) == Owner && tile.HasRoomForAttack)
                    return true;

            return false;
        }

        protected override void ProtectedPlayCard(AttackCardPlayContext context)
        {
            ((IAttackCardTarget)context.TargetTile).ApplyEquipment(CardableRef);

            CardGameEventsBus.RaiseOnAttackCardPlayed(Owner, CardableRef);
        }
    }
}