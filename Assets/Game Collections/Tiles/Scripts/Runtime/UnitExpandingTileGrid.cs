using Units;
namespace Tiles
{
    public class UnitExpandingTileGrid : ExpandingTileGridBase
    {
        public override bool AcceptTile(TileBase tile)
        {
            return tile is UnitTile;
        }
    }

}

