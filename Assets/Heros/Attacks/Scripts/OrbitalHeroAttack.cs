using System.Collections.Generic;
using System.Linq;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
namespace Heros
{
    public class OrbitalHeroAttack : HeroAttack
    {
        private BuffableStat numberOfOrbitals;
        private BuffableStat orbitalSpeed;
        private BuffableStat orbitalLifetime;

        private OrbitalHeroAttackSpawn orbitalHeroAttackSpawnPrefab;

        private Transform targetTransformPrefab;
        private readonly Dictionary<Transform, OrbitalHeroAttackSpawn> targetsAndTheirOrbitals = new Dictionary<Transform, OrbitalHeroAttackSpawn>();

        private float distanceAwayFromHero;
        private float rotationSpeed;
        private bool destroyProjectiles;
        private int? amountOfHits;
        private bool projectilesCountToHits;

        private float currentAngle;

        private void FixedUpdate()
        {
            int amountOfTargets = targetsAndTheirOrbitals.Count;

            float seperationAngle = 360f / amountOfTargets;

            Vector2 angle = Vector2.up.RotateClockwise(currentAngle);

            foreach (var kvp in targetsAndTheirOrbitals)
            {
                var target = kvp.Key;
                var orbital = kvp.Value;

                Vector2 targetPosition = angle * distanceAwayFromHero + (Vector2)transform.position;

                target.position = targetPosition;

                angle = angle.RotateClockwise(seperationAngle);
            }

            currentAngle += 
        }

        private void OnOrbitalDestroyed(OrbitalHeroAttackSpawn orbitalDestroyed, Transform target)
        {
            if (targetsAndTheirOrbitals.Values.NotNullCount() >= numberOfOrbitals)
                ResetAttackTimer();

            targetsAndTheirOrbitals[target] = null;

            if(orbitalDestroyed != null )
                orbitalDestroyed.onDestroyed -= OnOrbitalDestroyed;
        }

        private void SetupAmountOfTargetTransforms()
        {
            if (numberOfOrbitals == null)
                return;

            if (targetsAndTheirOrbitals.Count == numberOfOrbitals)
                return;

            if (targetsAndTheirOrbitals.Count < numberOfOrbitals)
            {
                int amountToAdd = numberOfOrbitals - targetsAndTheirOrbitals.Count;

                for (int i = 0; i < amountToAdd; i++)
                {
                    targetsAndTheirOrbitals.Add(Instantiate(targetTransformPrefab, transform), null);
                }
            }
            else
            {
                int amountToRemove = targetsAndTheirOrbitals.Count - numberOfOrbitals;
                Transform[] targetsWithoutOrbitals = targetsAndTheirOrbitals.Where(x => x.Value == null).Select(x => x.Key).ToArray();
                int index = 0;
                while (amountToRemove > 0)
                {
                    if(index < targetsWithoutOrbitals.Length)
                    {
                        Transform targetWithoutOrbital = targetsWithoutOrbitals[index];
                        targetsAndTheirOrbitals.Remove(targetWithoutOrbital);

                        Destroy(targetWithoutOrbital.gameObject);

                        index++;
                        amountToRemove--;
                        continue;
                    }

                    var kvp = targetsAndTheirOrbitals.First();
                    Transform target = kvp.Key;
                    OrbitalHeroAttackSpawn orbitalHeroAttackSpawn = kvp.Value;

                    orbitalHeroAttackSpawn.DestroyAttack();

                    targetsAndTheirOrbitals.Remove(target);
                }
            }
        }

        private void SpawnOrbital()
        {
            Transform targetWithoutOrbital = targetsAndTheirOrbitals.Where(x => x.Value == null).First().Key;

            if (targetWithoutOrbital == null)
                return;

            OrbitalHeroAttackSpawn orbitalHeroAttackSpawn = Instantiate(orbitalHeroAttackSpawnPrefab, transform);

            orbitalHeroAttackSpawn.Init(targetWithoutOrbital, damage, orbitalSpeed, destroyProjectiles, knockback, amountOfHits, projectilesCountToHits);

            targetsAndTheirOrbitals.Add(targetWithoutOrbital, orbitalHeroAttackSpawn);
        }

        protected override bool Attack()
        {
            if (targetsAndTheirOrbitals.Values.NotNullCount() >= numberOfOrbitals)
                return true;

            SpawnOrbital();

            return true;
        }

        protected override void InheritLevelSetup()
        {
            
        }

        private void OnNumberOfOrbitalsChanged()
        {
            SetupAmountOfTargetTransforms();
        }

        protected override void InheritSetup(HeroSO heroSO, HeroAttackSO heroAttackSO)
        {
            if (heroAttackSO is not OrbitalHeroAttackSO orbitalHeroAttackSO)
                return;

            SetupBuffableStat(ref numberOfOrbitals, HeroStats.NumberOfAttacks);
            SetupBuffableStat(ref orbitalSpeed, HeroStats.ProjectileTurnSpeed);

            numberOfOrbitals.onValueChange += OnNumberOfOrbitalsChanged;

            SetupAmountOfTargetTransforms();

            orbitalHeroAttackSpawnPrefab = orbitalHeroAttackSO.OrbitalHeroAttackSpawnPrefab;
            distanceAwayFromHero = orbitalHeroAttackSO.distanceAwayFromHero;
            rotationSpeed = orbitalHeroAttackSO.rotationSpeed;
            targetTransformPrefab = orbitalHeroAttackSO.targetTransformPrefab;
            destroyProjectiles = orbitalHeroAttackSO.destroyProjectiles;
            amountOfHits = orbitalHeroAttackSO.amountOfHits == 0 ? null : orbitalHeroAttackSO.amountOfHits;
            projectilesCountToHits = orbitalHeroAttackSO.projectilesCountToHits;
        }

        private void OnDestroy()
        {
            if(numberOfOrbitals != null)
                numberOfOrbitals.onValueChange -= OnNumberOfOrbitalsChanged;
        }
    }
}
