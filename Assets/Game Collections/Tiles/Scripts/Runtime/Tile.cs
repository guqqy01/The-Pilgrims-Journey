using System;
using System.Collections.Generic;
using TPJ.Gameplay;
using UnityEngine;

namespace Tiles
{
    [Serializable]
    public abstract class Tile<TObject> : TileBase, IDescriptable 
        where TObject : ITileable, IDescriptable, ITileCardType
    {
        protected TObject _object;
        public override ITileable TiledObject => _object;

        public TileCardType tileCardType;

        public override TileCardType TileCardType => tileCardType;

        public string Title => $"{TileSO.name} {_object.Title}";

        public string MainDescription => _object.MainDescription;

        public Sprite Sprite => ((IDescriptable)_object).Sprite;

        public List<ISubDescriptionData> GetSubDescriptionData => _object.GetSubDescriptionData;

        public string ShortDescription => _object.ShortDescription;

        protected Tile(TObject obj, TileSO tileSO) : base(tileSO)
        {
            _object = obj;
            tileCardType = obj.TileCardType;
        }
    }
}

