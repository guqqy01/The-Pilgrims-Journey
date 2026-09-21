using Equipment;
using System.Collections.Generic;
using Units;
using UnityEngine;

namespace Tiles.UI
{
    public class UnitTileUI : TileUI<UnitTile>, IDescriptable
    {
        public UnitTile UnitTile => ObjectTile;

        public UnitSO UnitSO
        {
            get
            {
                if (UnitTile == null)
                    return null;

                return UnitTile.TiledObject as UnitSO;
            }
        }

        #region Stats

        public float SpawnRateValue => UnitTile.SpawnRate;
        public float MovementValue => UnitTile.Movement;
        public int HealthValue => UnitTile.Health;
        public int ShieldValue => UnitTile.Shield;
        public float ShieldRegenerationValue => UnitTile.ShieldRegen;
        public float ShieldCooldownValue => UnitTile.ShieldCooldown;

        public string Title => UnitSO.Title;

        public string MainDescription => UnitSO.MainDescription;

        public Sprite Sprite => UnitSO.Sprite;

        public List<ISubDescriptionData> GetSubDescriptionData => UnitSO.GetSubDescriptionData;

        public string ShortDescription => UnitSO.ShortDescription;

        #endregion
    }
 }

