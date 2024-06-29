using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using System.Collections.Generic;
using Tiles;
using Equipment;

namespace Units
{
    public class Shield : MonoBehaviour
    {
        private BuffableStat shield;
        public int MaxShield => (int)shield.Value.Value;
        public int CurrentShield { private set; get; } = 5;


        private BuffableStat regeneration;
        public float Regeneration => (float)regeneration.Value.Value;


        private BuffableStat cooldown;
        public float Cooldown => (float)cooldown.Value.Value;


        private float timeToNextRegenPoint;


        public delegate void OnDestroy();
        public OnDestroy onDestroy;

        public delegate void OnShieldChange(int currentShield);
        public OnShieldChange onShieldChange;

        [SerializeField]
        private ParticleSystem shieldBrokenPS;

        public void Init(UnitSO unit)
        {
            shield = new BuffableStat(unit.Shield);

            CurrentShield = MaxShield;

            regeneration = new BuffableStat(unit.ShieldRegeneration);

            cooldown = new BuffableStat(unit.ShieldCooldown);

            onShieldChange?.Invoke(CurrentShield);
        }

        public void Init(Tile tile)
        {
            UnitSO unitSO = tile.Unit;
            EquipmentSO[] equipmentSOs = tile.Equipment;

            shield = new BuffableStat(unitSO.Shield);

            shield.AddBuff("UpgradeManager", UpgradeManager.Instance.GetBuff(UnitStatsEnum.Shield));

            if (tile != null)
                shield.AddBuff("Tile", tile.GetTileBuff(UnitStatsEnum.Shield));
            
            if (equipmentSOs != null)
            {
                for (int i = 0; i < equipmentSOs.Length; i++)
                {
                    if (equipmentSOs[i] == null)
                        continue;

                    if (equipmentSOs[i].TryGetBuff(UnitStatsEnum.Shield, out Buff outBuff))
                    {
                        shield.AddBuff("Equipment" + (i + 1).ToString(), outBuff);
                    }
                }
            }
                

            CurrentShield = MaxShield;

            regeneration = new BuffableStat(unitSO.ShieldRegeneration);

            regeneration.AddBuff("UpgradeManager", UpgradeManager.Instance.GetBuff(UnitStatsEnum.ShieldRegeneration));

            if (tile != null)
                regeneration.AddBuff("Tile", tile.GetTileBuff(UnitStatsEnum.ShieldRegeneration));

            if (equipmentSOs != null)
                for (int i = 0; i < equipmentSOs.Length; i++)
                {
                    if (equipmentSOs[i] == null)
                        continue;

                    if (equipmentSOs[i].TryGetBuff(UnitStatsEnum.ShieldRegeneration, out Buff outBuff))
                    {
                        regeneration.AddBuff("Equipment" + (i + 1).ToString(), outBuff);
                    }
                }

            cooldown = new BuffableStat(unitSO.ShieldCooldown);

            cooldown.AddBuff("UpgradeManager", UpgradeManager.Instance.GetBuff(UnitStatsEnum.ShieldCooldown));

            if (tile != null)
                cooldown.AddBuff("Tile", tile.GetTileBuff(UnitStatsEnum.ShieldCooldown));

            if (equipmentSOs != null)
                for (int i = 0; i < equipmentSOs.Length; i++)
                {
                    if (equipmentSOs[i] == null)
                        continue;

                    if (equipmentSOs[i].TryGetBuff(UnitStatsEnum.ShieldCooldown, out Buff outBuff))
                    {
                        cooldown.AddBuff("Equipment" + (i + 1).ToString(), outBuff);
                    }
                }

            onShieldChange?.Invoke(CurrentShield);
        }


        private void Update()
        {
            HandleRegeneration();
            HandleAdditionalTimedShield();
        }

        private void HandleAdditionalTimedShield()
        {
            if (referenceAndTimeToRemoveBuff.Count <= 0)
                return;

            List<string> toRemove = new List<string>();

            foreach(KeyValuePair<string, float> kvp in referenceAndTimeToRemoveBuff)
            {
                if(Time.time >= kvp.Value)
                {
                    shield.RemoveBuff(kvp.Key);
                    
                    toRemove.Add(kvp.Key);
                }
            }

            CurrentShield = Mathf.Min(CurrentShield, MaxShield);
            onShieldChange?.Invoke(CurrentShield);

            if (toRemove.Count <= 0)
                return;

            for (int i = 0; i < toRemove.Count; i++)
            {
                referenceAndTimeToRemoveBuff.Remove(toRemove[i]);
            }

            
        }

        private void HandleRegeneration()
        {
            if (shield.Value == null)
                return;

            if (CurrentShield == MaxShield) return;

            if (Time.time >= timeToNextRegenPoint)
            {
                RegenerateShield(1);
                timeToNextRegenPoint = Time.time + 1f / Regeneration;
            }
        }

        public void RegenerateShield(int amount)
        {
            CurrentShield = Mathf.Clamp(CurrentShield + amount, 0, MaxShield);

            onShieldChange?.Invoke(CurrentShield);
        }

        /// <summary>
        /// Deal damage to the shield
        /// </summary>
        /// <param name="amount">The amount of dmg dealt to the shield</param>
        /// <returns>Returns the amount of damage left over</returns>
        public int DamageShield(int amount)
        {
            int leftOverDmg = Mathf.Max(amount - CurrentShield, 0);

            CurrentShield = Mathf.Clamp(CurrentShield - amount, 0, MaxShield);

            onShieldChange?.Invoke(CurrentShield);

            if (amount > 0) timeToNextRegenPoint = Regeneration != 0 ? Time.time + Cooldown : float.PositiveInfinity;

            if (CurrentShield == 0 && amount != leftOverDmg)
            {
                onDestroy?.Invoke();
                if(shieldBrokenPS != null)
                    shieldBrokenPS?.Play();
            }

            return leftOverDmg;
        }

        private Dictionary<string, float> referenceAndTimeToRemoveBuff = new Dictionary<string, float>();

        public void ModifyShieldForTime(string reference, Buff shieldBuff, float time)
        {
            RegenerateShield((int)shield.AddBuff(reference, shieldBuff));

            if (referenceAndTimeToRemoveBuff.ContainsKey(reference))
            {
                referenceAndTimeToRemoveBuff[reference] = Time.time + time;
            }
            else
            {
                referenceAndTimeToRemoveBuff.Add(reference, Time.time + time);
            }
        }

    }
}


