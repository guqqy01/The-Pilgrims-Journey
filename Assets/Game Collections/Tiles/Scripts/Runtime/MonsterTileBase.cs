using Equipment;
using Monsters;
using System;
using System.Collections.Generic;
using TPJ.Gameplay;
using Units;
using UnityEngine;
namespace Tiles
{
    [Serializable]
    public abstract class MonsterTileBase : Tile<MonsterSpawn>, ICardable, IEquipmentTarget
    {
        private readonly List<EquipmentSO> _equipment = new List<EquipmentSO>();

        public MonsterSpawn MonsterSpawnBase => TiledObject as MonsterSpawn;

        protected MonsterTileBase(MonsterSpawn obj, TileSO tileSO) : base(obj, tileSO) { }

        public float Difficulty
        {
            get
            {
                return Shape.Count * MonsterSpawnBase.Difficulty; 
            }
        }

        public int ActionPointCost => ActionCost + MonsterSpawnBase.ActionCost;

        public CardBase CreateCard() => new MonsterCard(this);

        public void ApplyEquipment(EquipmentSO equipment)
        {
            _equipment.Add(equipment);
        }
    }
}

