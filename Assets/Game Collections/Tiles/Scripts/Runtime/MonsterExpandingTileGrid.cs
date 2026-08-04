namespace Tiles
{
    public class MonsterExpandingTileGrid : ExpandingTileGridBase
    {
        public override bool AcceptTile(TileBase tile)
        {
            return tile is MonsterTileBase;
        }
    }
}

