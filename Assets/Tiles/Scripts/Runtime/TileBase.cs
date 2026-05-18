using System.Collections.Generic;
using UnityEngine;
using System;

namespace Tiles
{
    [Serializable]
    public class TileBase
    {
        public TileSO TileSO { get; }
        public bool locked;

        public Action<TileBase> ActionDestroyTile;
        public Action<float> OnRotated;

        public delegate void OnUpdateUI();
        public OnUpdateUI onUpdateUI;

        public List<Vector2Int> Shape => TileSO.Shape;
        public List<Vector2Int> RotatedShape => TileSO.Shape.Rotate(Rotation);

        public List<Vector2Int> RotatedPlacedShape => RotatedShape.AddToAll(PlacedPosition);

        private float rotation;

        public Vector2Int PlacedPosition;
        public TileGridBase TileGrid;

        public float Rotation
        {
            set
            {
                rotation = value.RoundTo(90f);
                OnRotated?.Invoke(rotation);
            }
            get
            {
                return rotation;
            }
        }

        public virtual void OnDestroy()
        {
            OnRotated = null;
            onUpdateUI = null;
            ActionDestroyTile = null;
        }

        public TileBase(TileSO tileSO)
        {
            TileSO = tileSO;
        }

        protected void DestroyTile()
        {
            ActionDestroyTile?.Invoke(this);
        }

        
    }
 }

