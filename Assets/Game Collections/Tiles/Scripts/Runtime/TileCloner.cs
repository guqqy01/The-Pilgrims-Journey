using Attacks;
using Equipment;
using UnityEngine;

namespace Tiles
{
    public static class TileCloner
    {
        public static TileBase Clone(TileBase src)
        {
            TileBase clone;

            switch (src)
            {
                case UnitTile u:
                    var unit = new UnitTile(u.TileSO, u.UnitSO);
                    unit.SetupFromSave(new TileSave(
                        u.Level,
                        u.CurrentAmountOfUnitsInTile,
                        u.CurrentAmountOfUnitsEscaped,
                        u.Rotation, u.PlacedPosition, u.TileSO, u.UnitSO));
                    clone = unit;
                    break;

                case NormalMonsterTile n:
                    clone = new NormalMonsterTile(n.TileSO, n.MonsterSpawn);
                    break;

                case WaveMonsterTile w:
                    clone = new WaveMonsterTile(w.TileSO, w.MonsterSpawn);
                    break;

                default:
                    Debug.LogWarning($"TileCloner: unsupported tile type {src.GetType().Name}, skipped.");
                    return null;
            }

            foreach (EquipmentSO equipment in src.Equipment)
                clone.AddEquipment(equipment);

            foreach (BaseAttackSO attack in src.Attacks)
                clone.AddAttack(attack);

            foreach (var kvp in src.AdjacentTileTypes)
                clone.AdjacentTileTypes[kvp.Key] = kvp.Value;

            return clone;
        }
    }
}