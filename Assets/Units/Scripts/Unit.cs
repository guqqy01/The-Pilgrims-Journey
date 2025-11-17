using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Tiles;
using Equipment;

namespace Units
{
    [RequireComponent(typeof(UnitMovement), typeof(Health), typeof(Shield))]
    [RequireComponent(typeof(CoinDrop), typeof(HitEffects))]
    public class Unit : MonoBehaviour
    {
        public UnitSO UnitSO { private set; get; }
        public UnitMovement Movement { private set; get; }
        public Health Health { private set; get; }
        public Shield Shield { private set; get; }
        public CoinDrop CoinDrop { private set; get; }
        public Material UnitMat { get; private set; }

        public List<EquipmentSpecial> EquipmentSpecials { private set; get; } = new List<EquipmentSpecial>();

        public bool Invunrable {  get; private set; }
        protected float removeInvunrabilityTime;
        private const float percentageOfRemainingTimeToStartFlickering = 0.3f;
        private const float percentageOfRemainingTimeToChangeFlickerState = 0.2f;
        private float timeToStartFlickering;
        private float timeToChangeFlickerColor;
        private bool flickerOnOrOff;

        [SerializeField]
        private ParticleSystem healingPS;
        public ParticleSystem HealingPS => healingPS;
        [SerializeField]
        private ParticleSystem poisonPS;
        public ParticleSystem PoisonPS => poisonPS;

        private Color invunrabilityColor = Color.blue;

        private void Awake()
        {
            Movement = GetComponent<UnitMovement>();
            Health = GetComponent<Health>();
            Shield = GetComponent<Shield>();
            CoinDrop = GetComponent<CoinDrop>();
            UnitMat = GetComponentInChildren<Renderer>().material;
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

                    equipmentSpecial.Init(this);

                    EquipmentSpecials.Add(equipmentSpecial);
                }
        }

        public OnHitInfo Damage(int damage, Vector3 damageDirection, float knockBackMultiplier = 1f)
        {
            if (Invunrable)
                return new OnHitInfo();

            OnHitInfo hitInfo = new OnHitInfo();

            int leftDmg = Shield.DamageShield(damage, damageDirection);

            hitInfo.dmgToShield = damage - leftDmg;

            hitInfo.dmgToHealth = Mathf.Min(Health.CurrentHealth, leftDmg);
            hitInfo.killed = leftDmg >= Health.CurrentHealth;
            hitInfo.unit = this;

            if (leftDmg > 0)
            {
                Health.ModifyHealth(-leftDmg, damageDirection, knockBackMultiplier);
            }

            return hitInfo;
        }

        private void Flicker()
        {
            if (Time.time >= timeToChangeFlickerColor)
            {
                UnitMat.SetColor("_Tint", flickerOnOrOff ? invunrabilityColor : Color.white);
                timeToChangeFlickerColor = Time.time + (removeInvunrabilityTime - Time.time) * percentageOfRemainingTimeToChangeFlickerState;
                flickerOnOrOff = !flickerOnOrOff;
            }
        }

        protected virtual void Update()
        {
            if (Invunrable && Time.time >= removeInvunrabilityTime)
            {
                RemoveShock();
            }
            else if (Invunrable && Time.time >= removeInvunrabilityTime - timeToStartFlickering)
            {
                Flicker();
            }

            foreach(EquipmentSpecial equipmentSpecial in EquipmentSpecials)
            {
                equipmentSpecial.Update();
            }
        }

        public void MakeInvunrableForTime(float shockTime)
        {
            Invunrable = true;

            timeToStartFlickering = shockTime * percentageOfRemainingTimeToStartFlickering;

            removeInvunrabilityTime = Time.time + shockTime;

            UnitMat.SetFloat("_EffectAmount", 1);

            UnitMat.SetColor("_Tint", invunrabilityColor);
        }

        private void RemoveShock()
        {
            Invunrable = false;

            UnitMat.SetFloat("_EffectAmount", 0);
        }
    }

    public struct OnHitInfo
    {
        public Unit unit;
        public bool killed;
        public int dmgToShield;
        public int dmgToHealth;

        public int TotalDamage => dmgToShield + dmgToHealth;
    }

}

