namespace Tiles
{
    public abstract class Tile<TObject> : TileBase where TObject : ITileable
    {
        protected TObject _object;
        public override ITileable TiledObject => _object;

        protected Tile(TObject obj, TileSO tileSO) : base(tileSO)
        {
            _object = obj;
        }
    }
}

