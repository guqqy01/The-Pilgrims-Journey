using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using System.Collections.Generic;
namespace Units
{
    internal class Shield : MonoBehaviour
    {
        private BuffableIntStat shield;
        public int MaxShield => shield.Value;
        public int CurrentShield { private set; get; } = 5;


        private BuffableFloatStat regeneration;
        public float Regeneration => regeneration.Value;


        private BuffableFloatStat cooldown;
        public float Cooldown => cooldown.Value;


        private float timeToNextRegenPoint;


        public delegate void OnDestroy();
        public OnDestroy onDestroy;

        public delegate void OnShieldChange(int currentShield);
        public OnShieldChange onShieldChange;

        [SerializeField]
        private ParticleSystem shieldBrokenPS;

        public void Init(UnitSO unit)
        {
            shield = new BuffableIntStat(unit.Shield);

            CurrentShield = MaxShield;

            regeneration = new BuffableFloatStat(unit.ShieldRegeneration);

            cooldown = new BuffableFloatStat(unit.ShieldCooldown);

            onShieldChange?.Invoke(CurrentShield);
        }

        public void Init(Tile tile)
        {
            UnitSO unitSO = tile.Unit;
            List<EquipmentSO> equipmentSOs = tile.Equipment;

            shield = new BuffableIntStat(unitSO.Shield);

            if (tile != null)
                shield.AddMultiplierBuff("Tile", tile.shieldMultiplier);
            
            if (equipmentSOs != null)
                for (int i = 0; i < equipmentSOs.Count; i++)
                {
                    if (equipmentSOs[i] == null)
                        continue;

                    shield.AddBaseBuff("Equipment" + (i + 1).ToString(), equipmentSOs[i].IntBaseValue(UnitIntStats.Shield));
                    shield.AddMultiplierBuff("Equipment" + (i + 1).ToString(), equipmentSOs[i].IntMultiplierValue(UnitIntStats.Shield));
                }

            CurrentShield = MaxShield;

            regeneration = new BuffableFloatStat(unitSO.ShieldRegeneration);
            if (tile != null)
                regeneration.AddMultiplierBuff("Tile", tile.shieldRegenerationMultiplier);

            if (equipmentSOs != null)
                for (int i = 0; i < equipmentSOs.Count; i++)
                {
                    if (equipmentSOs[i] == null)
                        continue;

                    regeneration.AddBaseBuff("Equipment" + (i + 1).ToString(), equipmentSOs[i].FloatBaseValue(UnitFloatStats.ShieldRegeneration));
                    regeneration.AddMultiplierBuff("Equipment" + (i + 1).ToString(), equipmentSOs[i].FloatMultiplierValue(UnitFloatStats.ShieldRegeneration));
                }

            cooldown = new BuffableFloatStat(unitSO.ShieldCooldown);
            if (tile != null)
                cooldown.AddMultiplierBuff("Tile", tile.shieldCooldownMultiplier);

            if (equipmentSOs != null)
                for (int i = 0; i < equipmentSOs.Count; i++)
                {
                    if (equipmentSOs[i] == null)
                        continue;

                    cooldown.AddBaseBuff("Equipment" + (i + 1).ToString(), equipmentSOs[i].FloatBaseValue(UnitFloatStats.ShieldCooldown));
                    cooldown.AddMultiplierBuff("Equipment" + (i + 1).ToString(), equipmentSOs[i].FloatMultiplierValue(UnitFloatStats.ShieldCooldown));
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
                    shield.RemoveAdditionalBuff(kvp.Key);
                    
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

        public void ModifyShieldForTime(string reference, int amount, float time)
        {
            shield.AddAdditionalBuff(reference, amount);
            RegenerateShield(amount);

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


