using UnityEngine;
using System;
using Equipment;
using Units;

namespace Tiles
{
    [Serializable]
    public class TileSave
    {
        public int level;
        public int tileHealth;
        public int tileExp;

        public float rotation;
        public Vector2Int position;
        public TileSO tile;
        public UnitSO unit;
        public EquipmentSO[] equipment;

        public TileSave(int level, int tileHealth, int tileExp, float rotation, Vector2Int position, TileSO tile, UnitSO unit, EquipmentSO[] equipment)
        {
            this.level = level;
            this.rotation = rotation;
            this.tileHealth = tileHealth;
            this.tileExp = tileExp;
            this.position = position;
            this.tile = tile;
            this.unit = unit;
            this.equipment = equipment;
        }
    }

}

