using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heros.Attacks.Orbital
{
    public class OrbitalHeroAttack : HeroAttack<OrbitalHeroAttackSO>
    {
        private BuffableStat numberOfOrbitals;
        private BuffableStat orbitalSpeed;
        private BuffableStat orbitalLifetime;

        private OrbitalHeroAttackSpawn orbitalHeroAttackSpawnPrefab;

        private Transform targetTransformPrefab;
        private readonly Dictionary<Transform, OrbitalHeroAttackSpawn> slots = new Dictionary<Transform, OrbitalHeroAttackSpawn>();
        private int activeOrbitals;

        private float distanceAwayFromHero;
        private float rotationSpeed;
        private bool destroyProjectiles;
        private int? amountOfHits;
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

                Vector2 targetPosition = angle * distanceAwayFromHero + (Vector2)transform.position;

                target.position = targetPosition;

                angle = angle.RotateClockwise(seperationAngleRad);
            }

            if (distanceAwayFromHero == 0)
                return;

            currentAngleRad += orbitalSpeed / distanceAwayFromHero * Time.fixedDeltaTime;
        }

        private void OnOrbitalDestroyed(OrbitalHeroAttackSpawn orbitalDestroyed, Transform target)
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
                OrbitalHeroAttackSpawn orbitalHeroAttackSpawn = kvp.Value;

                orbitalHeroAttackSpawn.DestroyAttack();

                slots.Remove(target);
            }
        }

        private void SpawnOrbital()
        {
            Transform targetWithoutOrbital = slots.Where(x => x.Value == null).First().Key;

            if (targetWithoutOrbital == null)
                return;

            OrbitalHeroAttackSpawn orbitalHeroAttackSpawn = Instantiate(orbitalHeroAttackSpawnPrefab, transform.position, Quaternion.identity, transform);

            OrbitalHeroAttackInitContext initContext = new OrbitalHeroAttackInitContext(targetWithoutOrbital);
            OrbitalHeroAttackStatsContext statsContext = new OrbitalHeroAttackStatsContext(damage, orbitalSpeed, knockback, attackSpeed, destroyProjectiles, amountOfHits, projectilesCountToHits);

            orbitalHeroAttackSpawn.Init(initContext, statsContext);
            orbitalHeroAttackSpawn.onDestroyed += OnOrbitalDestroyed;

            slots[targetWithoutOrbital] = orbitalHeroAttackSpawn;
            activeOrbitals++;
        }

        protected override bool Attack()
        {
            if (activeOrbitals >= numberOfOrbitals)
                return true;

            SpawnOrbital();

            return true;
        }

        protected override void InheritLevelSetup()
        {
            numberOfOrbitals.AddBuff("Weapon", HeroAttackSO.GetBuff(HeroStats.NumberOfAttacks, Level));
            orbitalSpeed.AddBuff("Weapon", HeroAttackSO.GetBuff(HeroStats.ProjectileTurnSpeed, Level));

            foreach(var kvp in slots)
            {
                var orbital = kvp.Value;

                if(orbital == null) continue;

                OrbitalHeroAttackStatsContext statsContext = new OrbitalHeroAttackStatsContext(damage, orbitalSpeed, knockback, attackSpeed, destroyProjectiles, amountOfHits, projectilesCountToHits);

                orbital.SetupStats(statsContext);
            }
        }

        private void OnNumberOfOrbitalsChanged()
        {
            SetupAmountOfTargetTransforms();
        }

        protected override void InheritSetup(HeroSO heroSO, OrbitalHeroAttackSO orbitalHeroAttackSO)
        {
            SetupBuffableStat(ref numberOfOrbitals, HeroStats.NumberOfAttacks);
            SetupBuffableStat(ref orbitalSpeed, HeroStats.ProjectileTurnSpeed);

            numberOfOrbitals.onValueChange += OnNumberOfOrbitalsChanged;

            orbitalHeroAttackSpawnPrefab = orbitalHeroAttackSO.OrbitalHeroAttackSpawnPrefab;
            distanceAwayFromHero = orbitalHeroAttackSO.distanceAwayFromHero;
            rotationSpeed = orbitalHeroAttackSO.rotationSpeed;
            targetTransformPrefab = orbitalHeroAttackSO.targetTransformPrefab;
            destroyProjectiles = orbitalHeroAttackSO.destroyProjectiles;
            amountOfHits = orbitalHeroAttackSO.amountOfHits == 0 ? null : orbitalHeroAttackSO.amountOfHits;
            projectilesCountToHits = orbitalHeroAttackSO.projectilesCountToHits;
            clockwise = orbitalHeroAttackSO.clockwise;

            SetupAmountOfTargetTransforms();
        }

        private void OnDestroy()
        {
            if(numberOfOrbitals != null)
                numberOfOrbitals.onValueChange -= OnNumberOfOrbitalsChanged;
        }
    }
}
