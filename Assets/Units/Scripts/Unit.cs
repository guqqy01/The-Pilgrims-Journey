using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Tiles;
using Equipment;

namespace Units
{
    [RequireComponent(typeof(UnitMovement), typeof(Health), typeof(Shield))]
    [RequireComponent(typeof(CoinDrop), typeof(HitEffects), typeof(PoisonDebuff))]
    public class Unit : MonoBehaviour
    {
        public UnitSO UnitSO { private set; get; }
        public UnitMovement Movement { private set; get; }
        public Health Health { private set; get; }
        public Shield Shield { private set; get; }
        public CoinDrop CoinDrop { private set; get; }


        public List<EquipmentSpecial> EquipmentSpecials { private set; get; } = new List<EquipmentSpecial>();


        

        private void Awake()
        {
            Movement = GetComponent<UnitMovement>();
            Health = GetComponent<Health>();
            Shield = GetComponent<Shield>();
            CoinDrop = GetComponent<CoinDrop>();
        }

        public void Init(UnitSO unitSO, List<Vector2Int> path, int currentPathIndex)
        {
            UnitSO = unitSO;
            Movement.Init(path, currentPathIndex, unitSO);
            Health.Init(unitSO.Health);
            Shield.Init(unitSO.Shield, unitSO.ShieldRegeneration, unitSO.ShieldCooldown);
            CoinDrop.Init(unitSO);
        }

        public void Init(Tile tile, List<Vector2Int> path, int currentPathIndex)
        {
            UnitSO = tile.UnitSO;
            Movement.Init(path, currentPathIndex, tile);
            Health.Init(UnitStatBase.CreateBuffableStat(tile, tile.UnitSO.Health, UnitStatsEnum.Health));

            Shield.Init
                (
                UnitStatBase.CreateBuffableStat(tile, tile.UnitSO.Shield, UnitStatsEnum.Shield),
                UnitStatBase.CreateBuffableStat(tile, tile.UnitSO.ShieldRegeneration, UnitStatsEnum.ShieldRegeneration),
                UnitStatBase.CreateBuffableStat(tile, tile.UnitSO.ShieldCooldown, UnitStatsEnum.ShieldCooldown)
                );

            CoinDrop.Init(tile.UnitSO);

            EquipmentSO[] equipmentSOs = tile.Equipment;

            if(equipmentSOs != null)
                foreach (EquipmentSO equipment in equipmentSOs)
                {
                    if (equipment == null)
                        continue;

                    if (equipment.EquipmentSpecial == null)
                        continue;

                    EquipmentSpecial equipmentSpecial = equipment.EquipmentSpecial;

                    equipmentSpecial.Init(Health);

                    EquipmentSpecials.Add(equipmentSpecial);
                }
        }


        protected virtual void Update()
        {
            foreach(EquipmentSpecial equipmentSpecial in EquipmentSpecials)
            {
                equipmentSpecial.Update();
            }
        }
    }

}

