using TPJ.Gameplay;
using Heros;
using Equipment;
using Attacks;

namespace Tiles
{
    public class HeroTile : Tile<HeroSO>, ICardable, IEquipmentCardTarget, IAttackCardTarget
    {
        public HeroTile(HeroSO obj, TileSO tileSO) : base(obj, tileSO)
        {

        }

        public int ActionPointCost => 0;

        public void ApplyEquipment(EquipmentSO equipment) => AddEquipment(equipment);

        public void ApplyEquipment(BaseAttackSO equipment)
        {

        }

        public CardBase CreateCard() => new HeroCard(this);
    }
 }

