using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Units;
using UnityEngine.Experimental.Playables;
using static UnityEngine.GraphicsBuffer;
using System;
using System.Linq;

public class HealingBeacon : MonoBehaviour
{
    [SerializeField]
    private int numberOfTargets = 3;

    [SerializeField]
    private Beam beamPrefab;

    [SerializeField]
    private float range = 5f;

    [SerializeField]
    private float timeAlive = 10f;
    private float timeToDestory;

    [SerializeField]
    private LayerMask targetMask;

    /// <summary>
    /// Amount of healing every second
    /// </summary>
    [SerializeField]
    private float healingRate = 2f;

    private SingleItemUseList<Beam> beams = new SingleItemUseList<Beam>();

    private Targets targets = new Targets();

    [SerializeField]
    private SetGreyScale setGreyScale;



    private void Awake()
    {
        timeToDestory = Time.time + timeAlive;
        for (int i = 0; i < numberOfTargets; i++)
        {
            Beam beam = Instantiate(beamPrefab, transform.position, Quaternion.identity, transform);
            beams.Add(beam);
        }

        setGreyScale.SetEndFlickerTime(timeToDestory);
    }

    private void Update()
    {
        RemoveTargets();

        AddTargets();

        HealTargets();

        DestroyBeacon();
    }


    private void DestroyBeacon()
    {
        if (Time.time >= timeToDestory)
            Destroy(gameObject);
    }

    private void HealTargets()
    {
        foreach (Target target in targets)
        {
            target.buildUpToHeal += Time.deltaTime * healingRate;

            int amountToHeal = Mathf.FloorToInt(target.buildUpToHeal);

            if(amountToHeal >= 1f)
            {
                target.Health.ModifyHealth(amountToHeal);
                target.buildUpToHeal -= amountToHeal;
            }
        }
    }

    private void RemoveTargets()
    {
        if (targets == null || targets.Count <= 0)
            return;

        foreach (Target target in targets)
        {
            if(target.Transform == null)
            {
                targets.Remove(target);
                beams.MakeAvaliable(target.Beam);
                continue;
            }

            if ((target.Transform.position - transform.position).sqrMagnitude > range * range ||
                target.Health.currentHealth >= target.Health.MaxHealth)
            {
                targets.Remove(target);
                beams.MakeAvaliable(target.Beam);
            }
        }
    }

    private void AddTargets()
    {
        while (targets.Count < numberOfTargets)
        {
            KeyValuePair<Transform, Health> kvp = GetClosestTargetInRange();

            if (kvp.Key == null)
                break;

            Target target = new Target(kvp.Key, beams.GetAvaliableItemOrNull(), kvp.Value);

            targets.Add(target);
        }
    }

    private KeyValuePair<Transform, Health> GetClosestTargetInRange()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, range, targetMask);

        float closestDistance = float.MaxValue;
        KeyValuePair<Transform, Health> closestTarget = new KeyValuePair<Transform, Health>();

        foreach (Collider2D collider in colliders)
        {
            if (targets.Contains(collider.transform))
                continue;

            if (!collider.TryGetComponent(out Health health))
                continue;

            if (health.currentHealth >= health.MaxHealth)
                continue;

            float distance = Vector2.Distance(collider.transform.position, transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = new KeyValuePair<Transform, Health>(collider.transform, health);
            }
        }

        return closestTarget;
    }

    [Serializable]
    private class Targets : IEnumerable
    {
        private Dictionary<Transform, Target> transformTargets = new Dictionary<Transform, Target>();
        private Dictionary<Health, Target> healthTargets = new Dictionary<Health, Target>();
        private Dictionary<Beam, Target> beamTargets = new Dictionary<Beam, Target>();

        public int Count => transformTargets.Count;

        public Target this[Transform transform]
        {
            get
            {
                return transformTargets[transform];
            }
        }

        public Target this[Health health]
        {
            get
            {
                return healthTargets[health];
            }
        }

        public Target this[Beam beam]
        {
            get
            {
                return beamTargets[beam];
            }
        }

        public void Add(Target target)
        {
            if (transformTargets.ContainsKey(target.Transform) ||healthTargets.ContainsKey(target.Health) ||beamTargets.ContainsKey(target.Beam))
                throw new Exception($"Can't add duplicate: Transform: {target.Transform}, Health: {target.Health}, Beam: {target.Beam}.");

            transformTargets.Add(target.Transform, target);
            healthTargets.Add(target.Health, target);
            beamTargets.Add(target.Beam, target);
            target.Beam.Target = target.Transform;
        }

        public void Remove(Target target)
        {
            target.Beam.Target = null;
            transformTargets.Remove(target.Transform);
            healthTargets.Remove(target.Health);
            beamTargets.Remove(target.Beam);
        }

        public void Remove(Transform transform)
        {
            Target target = transformTargets[transform];
            Remove(target);
        }

        public void Remove(Health health)
        {
            Target target = healthTargets[health];
            Remove(target);
        }

        public void Remove(Beam beam)
        {
            Target target = beamTargets[beam];
            Remove(target);
        }

        public bool Contains(Transform transform)
        {
            return transformTargets.ContainsKey(transform);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public TargetsEnum GetEnumerator()
        {
            return new TargetsEnum(transformTargets.Values.ToArray());
        }
    }

    private class TargetsEnum : IEnumerator
    {
        public Target[] targets;

        // Enumerators are positioned before the first element
        // until the first MoveNext() call.
        int position = -1;

        public TargetsEnum(Target[] list)
        {
            targets = list;
        }

        public bool MoveNext()
        {
            position++;
            return (position < targets.Length);
        }

        public void Reset()
        {
            position = -1;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public Target Current
        {
            get
            {
                try
                {
                    return targets[position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }

    private class Target
    {
        public readonly Transform Transform;
        public readonly Beam Beam;
        public readonly Health Health;
        public float buildUpToHeal;

        public Target(Transform transform, Beam beam, Health health)
        {
            Health = health;
            Transform = transform;
            Beam = beam;
        }
    }
}
