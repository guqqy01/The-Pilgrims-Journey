using UnityEngine;
using System.Collections.Generic;
using Units;
using Tiles;

namespace Spells
{
    public class InvunrabilityEffect : SpellEffect
    {
        public float invunrabilityTime;

        public override bool CanUseEffect(Vector2Int position)
        {
            if (!TilePlayerManager.Instance.IsPlaying)
                return false;

            foreach (Collider2D collider in GetColliders2D(position))
            {
                if (collider.TryGetComponent(out Unit unit))
                {
                    return true;
                }
            }

            return false;
        }

        public override void Effect(Vector2Int origin)
        {
            if (!CanUseEffect(origin))
            { return; }


            List<Collider2D> colliders = GetColliders2D(origin);

            foreach (Collider2D collider in colliders)
            {
                if (collider.TryGetComponent(out Unit unit))
                {
                    Transform tf = unit.transform;
                    if (effectFX != null)
                    {
                        FX fx = GameObject.Instantiate(effectFX, tf.position, Quaternion.identity, tf);
                        fx.Init(fXGreyScaleFactor, fXColor);
                    }
                    unit.MakeInvunrableForTime(invunrabilityTime);
                }
            }

            AudioManager.Instance.PlayClip(effectSound);
        }
    }
}

