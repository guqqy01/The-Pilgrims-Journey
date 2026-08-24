using System.Collections.Generic;
using UnityEngine;
using System;

namespace Tiles
{

    [Serializable]
    public abstract class TileBase
    {
        public TileSO TileSO { get; }
        private bool locked;
        public bool Locked
        {
            get
            {
                return locked;
            }
            set
            {
                locked = value;
                OnLockedChanged?.Invoke(value);
            }
        }

        public event Action<TileBase> ActionDestroyTile;
        public event Action<float> OnRotated;
        public event Action<bool> OnLockedChanged;
        
        public delegate void OnUpdateUI();
        public OnUpdateUI onUpdateUI;

        public abstract ITileable TiledObject { get; }

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

        public int Size
        {
            get
            {
                return Shape.Count;
            }
        }

        public int Width
        {
            get
            {
                int maxX = int.MinValue;
                int minX = int.MaxValue;
                foreach (Vector2Int v in Shape)
                {
                    if (v.x > maxX)
                        maxX = v.x;

                    if(v.x < minX)
                        minX = v.x;
                }

                return maxX - minX + 1;
            }
        }

        public int Height
        {
            get
            {
                int maxY = int.MinValue;
                int minY = int.MaxValue;
                foreach (Vector2Int v in Shape)
                {
                    if (v.y > maxY)
                        maxY = v.y;

                    if (v.y < minY)
                        minY = v.y;
                }

                return maxY - minY + 1;
            }
        }

        public virtual void OnDestroy()
        {
            OnRotated = null;
            onUpdateUI = null;
            ActionDestroyTile = null;
            OnLockedChanged = null;
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

