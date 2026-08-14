using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Attacks.Orbitals
{
    public class OrbitalAttack : Attack<OrbitalAttackSO, OrbitalAttackInit, OrbitalAttackStats>
    {
        private BuffableStat numberOfOrbitals;
        private BuffableStat orbitalSpeed;
        private BuffableStat orbitalLifetime;

        private Transform targetTransformPrefab;
        private readonly Dictionary<Transform, OrbitalAttackSpawn> slots = new Dictionary<Transform, OrbitalAttackSpawn>();
        private int activeOrbitals;

        private float distanceAwayFromSender;
        private float rotationSpeed;
        private bool projectilesCountToHits;

        private float currentAngleRad;

        private bool clockwise;
                  
        private void FixedUpdate()
        {
            int amountOfTargets = slots.Count;

            if (amountOfTargets == 0)
                return;

            float seperationAngleRad = (2 * Mathf.PI) / amountOfTargets;

            Vector2 angle = clockwise ? Vector2.up.RotateClockwise(currentAngleRad) : Vector2.up.RotateCounterClockwise(currentAngleRad);

            foreach (var kvp in slots)
            {
                var target = kvp.Key;
                var orbital = kvp.Value;

                Vector2 targetPosition = angle * distanceAwayFromSender + (Vector2)transform.position;

                target.position = targetPosition;

                angle = angle.RotateClockwise(seperationAngleRad);
            }

            if (distanceAwayFromSender == 0)
                return;

            currentAngleRad += orbitalSpeed / distanceAwayFromSender * Time.fixedDeltaTime;
        }

        protected override OrbitalAttackStats GetAttackStats()
        {
            return new OrbitalAttackStats(
                damage, 
                size,
                criticalChance, 
                criticalDamageMultiplier, 
                orbitalSpeed, 
                knockback, 
                attackSpeed, 
                destroyProjectiles,
                amountOfHits, 
                projectilesCountToHits);
        }

        private void OnOrbitalDestroyed(OrbitalAttackSpawn orbitalDestroyed, Transform target)
        {
            if (slots.Values.NotNullCount() >= numberOfOrbitals)
                ResetAttackTimer();


            slots[target] = null;
            activeOrbitals--;

            if(orbitalDestroyed != null )
                orbitalDestroyed.onDestroyed -= OnOrbitalDestroyed;
        }

        private void SetupAmountOfTargetTransforms()
        {
            if (numberOfOrbitals == null)
                return;

            if (slots.Count == numberOfOrbitals)
                return;

            if (slots.Count < numberOfOrbitals)
            {
                AddTargetTransforms();
            }
            else
            {
                RemoveTargetTransforms();
            }
        }

        private void AddTargetTransforms()
        {
            int amountToAdd = numberOfOrbitals - slots.Count;

            for (int i = 0; i < amountToAdd; i++)
            {
                slots.Add(Instantiate(targetTransformPrefab, transform.position, Quaternion.identity, transform), null);
            }
        }

        private void RemoveTargetTransforms()
        {
            int amountToRemove = slots.Count - numberOfOrbitals;
            Transform[] targetsWithoutOrbitals = slots.Where(x => x.Value == null).Select(x => x.Key).ToArray();
            int index = 0;
            while (amountToRemove > 0)
            {
                if (index < targetsWithoutOrbitals.Length)
                {
                    Transform targetWithoutOrbital = targetsWithoutOrbitals[index];
                    slots.Remove(targetWithoutOrbital);

                    Destroy(targetWithoutOrbital.gameObject);

                    index++;
                    amountToRemove--;
                    continue;
                }

                var kvp = slots.First();
                Transform target = kvp.Key;
                OrbitalAttackSpawn orbitalAttackSpawn = kvp.Value;

                orbitalAttackSpawn.DestroyAttack();

                slots.Remove(target);
            }
        }

        private void SpawnOrbital()
        {
            Transform targetWithoutOrbital = slots.Where(x => x.Value == null).First().Key;

            if (targetWithoutOrbital == null)
                return;

            OrbitalAttackInit attackInit = new OrbitalAttackInit(hitLayerMask, transform.position, Quaternion.identity, targetWithoutOrbital, transform);
            OrbitalAttackSpawn orbitalAttackSpawn = GetAttackSpawnFromPoolAndSetup<OrbitalAttackSpawn>(attackInit);

            orbitalAttackSpawn.onDestroyed += OnOrbitalDestroyed;

            slots[targetWithoutOrbital] = orbitalAttackSpawn;
            activeOrbitals++;
        }

        protected override bool ProtectedAttack()
        {
            if (activeOrbitals >= numberOfOrbitals)
                return true;

            SpawnOrbital();

            return true;
        }

        protected override void InheritLevelSetup()
        {
            numberOfOrbitals.AddBuff("Weapon", AttackSO.GetBuff(StatTypes.NumberOfAttacks, Level));
            orbitalSpeed.AddBuff("Weapon", AttackSO.GetBuff(StatTypes.ProjectileTurnSpeed, Level));

            foreach(var kvp in slots)
            {
                var orbital = kvp.Value;

                if(orbital == null) continue;

                int? amountOfHitsConverted = amountOfHits == null ? null : amountOfHits.Value;
                OrbitalAttackStats statsContext = GetAttackStats();

                orbital.SetupStats(statsContext);
            }
        }

        private void OnNumberOfOrbitalsChanged()
        {
            SetupAmountOfTargetTransforms();
        }

        protected override void InheritSetup(OrbitalAttackSO orbitalAttackSO)
        {
            SetupBuffableStat(ref numberOfOrbitals, StatTypes.NumberOfAttacks);
            SetupBuffableStat(ref orbitalSpeed, StatTypes.ProjectileTurnSpeed);

            numberOfOrbitals.onValueChange += OnNumberOfOrbitalsChanged;

            distanceAwayFromSender = orbitalAttackSO.distanceAwayFromSender;
            rotationSpeed = orbitalAttackSO.rotationSpeed;
            targetTransformPrefab = orbitalAttackSO.targetTransformPrefab;
            destroyProjectiles = orbitalAttackSO.destroyProjectiles;
            amountOfHits = orbitalAttackSO.amountOfHits == 0 ? null : orbitalAttackSO.amountOfHits;
            projectilesCountToHits = orbitalAttackSO.projectilesCountToHits;
            clockwise = orbitalAttackSO.clockwise;

            SetupAmountOfTargetTransforms();
        }

        private void OnDestroy()
        {
            if(numberOfOrbitals != null)
                numberOfOrbitals.onValueChange -= OnNumberOfOrbitalsChanged;
        }

        
    }
}
