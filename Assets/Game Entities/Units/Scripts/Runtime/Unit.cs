using Equipment;
using System.Collections.Generic;
using Tiles;
using UnityEngine;

namespace Units
{
    [RequireComponent(typeof(UnitMovement), typeof(Health), typeof(Shield))]
    [RequireComponent(typeof(CoinDrop), typeof(KnockbackEffectReciever), typeof(PoisonDebuff))]
    public class Unit : MonoBehaviour
    {
        [SerializeField]
        private StatContainer stats;
        public StatContainer Stats => stats;
        public UnitSO UnitSO { private set; get; }
        public UnitMovement Movement { private set; get; }
        public Health Health { private set; get; }
        public Shield Shield { private set; get; }
        public CoinDrop CoinDrop { private set; get; }

        public List<EquipmentSpecial> EquipmentSpecials { private set; get; } = new List<EquipmentSpecial>();

        private void Awake()
        {
            if (stats == null)
                stats = GetComponent<StatContainer>();
            Movement = GetComponent<UnitMovement>();
            Health = GetComponent<Health>();
            Shield = GetComponent<Shield>();
            CoinDrop = GetComponent<CoinDrop>();
        }

        public void Init(UnitSO unitSO, List<Vector2Int> path, int currentPathIndex)
        {
            UnitSO = unitSO;
            Stats.Setup(UnitSO);
            Movement.Init(path, currentPathIndex, Stats.Get(StatTypes.MovementSpeed));
            Health.Init(Stats.Get(StatTypes.Health));
            Shield.Init(Stats.Get(StatTypes.Shield), Stats.Get(StatTypes.ShieldRegeneration), Stats.Get(StatTypes.ShieldCooldown));
            CoinDrop.Init(unitSO);
        }

        public void Init(UnitTile tile, List<Vector2Int> path, int currentPathIndex)
        {
            Init(tile.UnitSO, path, currentPathIndex);
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

