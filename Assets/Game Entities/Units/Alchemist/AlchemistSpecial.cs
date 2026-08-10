using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Units
{
    public class AlchemistSpecial : MonoBehaviour
    {
        [SerializeField]
        private float range;

        [SerializeField]
        private float throwCooldown;

        private float timeToNextThrow;

        [SerializeField]
        private LayerMask unitLayerMask;

        [SerializeField]
        private List<Potion> potions;

        private void Update()
        {
            if (timeToNextThrow > Time.time)
                return;

            Collider2D random = (Collider2D)Physics2D.OverlapCircleAll(transform.position, range, unitLayerMask).GetRandom();

            Instantiate(potions.GetRandom(), transform.position, Quaternion.identity).Init(random.transform.position);

            timeToNextThrow = Time.time + throwCooldown;
        }
    }
}

