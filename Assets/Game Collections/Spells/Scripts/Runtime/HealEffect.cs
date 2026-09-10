using UnityEngine;
using System;


namespace Spells
{
    [Serializable]
    public class HealEffect : SpellEffect
    {
        public int HealAmount = 10;

        public float HealPercentage = 0.2f;

        public override bool CanUseEffect(Vector2Int position)
        {
            foreach (Collider2D collider in GetColliders2D(position))
            {
                if (collider.TryGetComponent(out Health health))
                {
                    return true;
                }
            }

            return false;
        }

        public override void Effect(Vector2Int position)
        {
            if (!CanUseEffect(position))
            { return; }

            foreach (Collider2D collider in GetColliders2D(position))
            {
                if (collider.TryGetComponent(out Health health))
                {
                    health.ModifyHealth(HealAmount);
                    health.ModifyHealth(Mathf.RoundToInt(HealPercentage * health.MaxHealth));

                    if (effectFX == null)
                        continue;

                    Transform tf = health.transform;
                    FX fx = GameObject.Instantiate(effectFX, tf.position, Quaternion.identity, tf);
                    fx.Init(fXGreyScaleFactor, fXColor);
                }
            }

            AudioManager.Instance.PlayClip(effectSound);
        }
    }
}

