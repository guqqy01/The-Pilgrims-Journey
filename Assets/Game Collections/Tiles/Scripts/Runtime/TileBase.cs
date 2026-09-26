using Attacks;
using Equipment;
using System;
using System.Collections.Generic;
using System.Linq;
using TPJ.Gameplay;
using UnityEngine;
namespace Tiles
{

    [Serializable]
    public abstract class TileBase : IActionPointCoster
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

        public event Action<TileCardType> OnTileCardTypeChanged;
        public event Action<TileBase> ActionDestroyTile;
        public event Action<float> OnRotated;
        public event Action<bool> OnLockedChanged;
        public event Action<Vector2Int> OnCellDestroyed;

        protected readonly List<EquipmentSO> _equipment = new List<EquipmentSO>();
        public IReadOnlyList<EquipmentSO> Equipment => _equipment;

        protected readonly List<BaseAttackSO> _attacks = new List<BaseAttackSO>();
        public IReadOnlyList<BaseAttackSO> Attacks => _attacks;

        public void AddEquipment(EquipmentSO equipment)
        {
            _equipment.Add(equipment);

            if (!TileCardType.HasFlag(equipment.GrantedTileCardType))
            {
                AddTileCardTypeFlag(equipment.GrantedTileCardType);
                TileGrid?.RecalculateAdjacentTileCardTypes(this);
                OnTileCardTypeChanged?.Invoke(TileCardType);   // add — notify after the flag is actually set
            }
        }

        public void AddAttack(BaseAttackSO attack)
        {
            _attacks.Add(attack);
        }

        protected abstract void AddTileCardTypeFlag(TileCardType flag);

        public delegate void OnUpdateUI();
        public OnUpdateUI onUpdateUI;

        public abstract ITileable TiledObject { get; }

        public abstract bool HasRoomForEquipment { get; }
        public abstract bool HasRoomForAttack { get; }

        public List<Vector2Int> Shape => TileSO != null ? TileSO.Shape : new List<Vector2Int>();
        public List<Vector2Int> RotatedShape => TileSO.Shape.Rotate(Rotation);

        public List<Vector2Int> RotatedPlacedShape => RotatedShape.AddToAll(PlacedPosition);

        // Cells (in placed/world space) that have been individually destroyed — e.g. by a
        // Brigade attack — without the whole tile going away. Tracked separately from Shape,
        // which stays the tile's full template footprint regardless of battle damage.
        private readonly HashSet<Vector2Int> _destroyedCells = new HashSet<Vector2Int>();
        public IReadOnlyCollection<Vector2Int> DestroyedCells => _destroyedCells;

        /// <summary>
        /// This tile's placed cells that are still intact (RotatedPlacedShape minus anything
        /// destroyed). Gameplay that cares about the tile's *current* footprint — generation,
        /// column scoring, adjacency — should use this instead of RotatedPlacedShape.
        /// </summary>
        public List<Vector2Int> ActivePlacedShape =>
            RotatedPlacedShape.Where(c => !_destroyedCells.Contains(c)).ToList();

        /// <summary>
        /// This tile's LOCAL (unrotated, Shape-space) cells that are still intact — same
        /// coordinate space as Shape itself, for feeding into TileShapeTextureBuilder after
        /// a partial destruction. Shape and RotatedPlacedShape are parallel lists (Rotate/
        /// AddToAll map each element in place, they don't reorder), so index i in one always
        /// corresponds to index i in the other.
        /// </summary>
        public List<Vector2Int> ActiveLocalShape
        {
            get
            {
                List<Vector2Int> local = Shape;
                List<Vector2Int> placed = RotatedPlacedShape;
                var result = new List<Vector2Int>(local.Count);

                for (int i = 0; i < local.Count && i < placed.Count; i++)
                {
                    if (!_destroyedCells.Contains(placed[i]))
                        result.Add(local[i]);
                }

                return result;
            }
        }

        /// <summary>Converts one of this tile's placed/world cells back to the corresponding
        /// local (Shape-space) cell, via the same index correspondence as ActiveLocalShape.
        /// Returns null if worldCell isn't one of this tile's cells.</summary>
        public Vector2Int? WorldCellToLocalCell(Vector2Int worldCell)
        {
            List<Vector2Int> local = Shape;
            List<Vector2Int> placed = RotatedPlacedShape;

            for (int i = 0; i < local.Count && i < placed.Count; i++)
            {
                if (placed[i] == worldCell)
                    return local[i];
            }

            return null;
        }

        public bool IsFullyDestroyed => ActivePlacedShape.Count == 0;

        /// <summary>
        /// Marks one placed world cell of this tile as destroyed. Call via
        /// TileGridBase.DestroyTileSection so the grid array and this tile's state stay in
        /// sync — don't call this directly from gameplay code.
        /// </summary>
        internal void MarkCellDestroyed(Vector2Int worldCell)
        {
            if (!_destroyedCells.Add(worldCell))
                return;

            OnCellDestroyed?.Invoke(worldCell);
        }

        private float rotation;

        public Vector2Int PlacedPosition;
        public TileGridBase TileGrid;
        public readonly Dictionary<TileCardType, int> AdjacentTileTypes = new Dictionary<TileCardType, int>();

        public abstract TileCardType TileCardType { get; }

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

        // Width/Height source from ActiveLocalShape rather than Shape — identical to the old
        // behaviour for any undamaged tile (ActiveLocalShape == Shape when nothing's been
        // destroyed), but shrinks correctly for a tile that's had a Brigade section removed.
        public int Width
        {
            get
            {
                List<Vector2Int> activeShape = ActiveLocalShape;
                int maxX = int.MinValue;
                int minX = int.MaxValue;
                foreach (Vector2Int v in activeShape)
                {
                    if (v.x > maxX)
                        maxX = v.x;

                    if (v.x < minX)
                        minX = v.x;
                }

                return maxX - minX + 1;
            }
        }

        public int Height
        {
            get
            {
                List<Vector2Int> activeShape = ActiveLocalShape;
                int maxY = int.MinValue;
                int minY = int.MaxValue;
                foreach (Vector2Int v in activeShape)
                {
                    if (v.y > maxY)
                        maxY = v.y;

                    if (v.y < minY)
                        minY = v.y;
                }

                return maxY - minY + 1;
            }
        }

        public Buff ActionCost => Shape.Count * 2;

        public virtual void OnDestroy()
        {
            OnRotated = null;
            onUpdateUI = null;
            ActionDestroyTile = null;
            OnLockedChanged = null;
            OnTileCardTypeChanged = null;
            OnCellDestroyed = null;
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