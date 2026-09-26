using Attacks;
using Equipment;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using TPJ.Gameplay;
using UnityEngine;

namespace Tiles
{
    [Serializable]
    public abstract class Tile<TObject> : TileBase, IDescriptable 
        where TObject : ITileable, IDescriptable, ITileCardType, IEquipmentHolder, IAttackHolder
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

        public override bool HasRoomForEquipment => _equipment.Count < _object.MaxEquipmentCapacity;
        public override bool HasRoomForAttack => _attacks.Count < _object.MaxAttackCapacity;

        /// <summary>Overridden by Tile&lt;TObject&gt;, where the actual mutable tileCardType field lives.</summary>
        protected override void AddTileCardTypeFlag(TileCardType flag)
        {
            tileCardType |= flag;
        }

    }
}

