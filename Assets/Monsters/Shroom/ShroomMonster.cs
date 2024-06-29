using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Units;

namespace Monsters
{
    public class ShroomMonster : Monster
    {
        [SerializeField]
        private float attackSpeed = 1f;

        private float nextAttackTime;
        [SerializeField]
        private int damage = 1;

        [SerializeField]
        private ParticleSystem ps;

        [SerializeField]
        private AudioSource popSource;


        protected override void Update()
        {
            base.Update();

            /*HandleBloomAudio();*/

            if (Time.time >= nextAttackTime)
            {
                Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, range, targetMask);

                if (colliders.Length > 0)
                {
                    ani.SetTrigger("Attack");

                    nextAttackTime = Time.time + 1f / attackSpeed;
                }
            }
        }

        public void Attack()
        {
            ps.Play();

            popSource.Play();
            /*bloomSource.Play();
            timeToFallOff = Time.time + playLength * fallOffTimePercent;
            endTime = Time.time + playLength;
            playing = true;*/

            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, range, targetMask);

            for (int i = 0; i < colliders.Length; i++)
            {
                if (1 << colliders[i].gameObject.layer == targetMask)
                {
                    if (colliders[i].TryGetComponent(out Units.Unit unit))
                    {
                        OnHitInfo hitInfo = unit.Damage(damage);

                        OnDamageUnit(hitInfo);
                    }
                }
            }
        }

        /*private void HandleBloomAudio()
        {
            if (!playing)
                return;

            if (bloomSource.pitch != pitch)
                bloomSource.pitch = pitch;

            bloomSource.volume = (Time.time <= timeToFallOff) ?
                volume :
                ((timeToFallOff == endTime) ?
                    0 :
                    volume * (1 - (timeToFallOff / (timeToFallOff - endTime)) + (Time.time / (timeToFallOff - endTime))));

            if (Time.time > endTime)
            {
                bloomSource.Stop();
                playing = false;
            }

        }*/

        protected override void ShockEffect(float time)
        {
            nextAttackTime = Mathf.Max(Time.time, nextAttackTime) + time;
        }
    }
}
