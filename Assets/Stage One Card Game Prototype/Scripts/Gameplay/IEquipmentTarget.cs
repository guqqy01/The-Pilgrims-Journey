using Equipment;

namespace Tiles
{
    /// <summary>
    /// Implement on whichever TileBase subclasses can legally receive equipment
    /// (e.g. UnitTile, HeroTile). Should apply the equipment via your existing
    /// Buff/BuffableStat system on TiledObject.
    /// </summary>
    public interface IEquipmentTarget
    {
        void ApplyEquipment(EquipmentSO equipment);
    }
}