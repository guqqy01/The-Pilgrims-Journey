using TPJ.Gameplay;
using Heros;
using Equipment;
using System.Collections.Generic;
using UnityEngine;

namespace Tiles
{
    public class HeroTile : Tile<HeroSO>, ICardable, IEquipmentTarget
    {
        private readonly List<EquipmentSO> _equipment = new List<EquipmentSO>();

        public HeroTile(HeroSO obj, TileSO tileSO) : base(obj, tileSO)
        {

        }

        public int ActionPointCost => 0;

        public void ApplyEquipment(EquipmentSO equipment)
        {
            _equipment.Add(equipment);
        }

        public CardBase CreateCard() => new HeroCard(this);
    }
 }

