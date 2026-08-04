using UnityEngine;
using System.Collections.Generic;
using Monsters;
using Tiles;
namespace Spells
{
    public class StaticShockEffect : SpellEffect
    {
        public float shockTime;

        public override bool CanUseEffect(Vector2Int position)
        {
            if (!LocalMapLevelCompletedManager.Instance.CurrentlyPlayingLocalLevel)
                return false;

            foreach (Collider2D collider in GetColliders2D(position))
            {
                if (collider.TryGetComponent(out Monster monster))
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
                if (collider.TryGetComponent(out Monster monster))
                {
                    Transform tf = monster.transform;
                    if (effectFX != null)
                    {
                        FX fx = GameObject.Instantiate(effectFX, tf.position, Quaternion.identity, tf);
                        fx.Init(fXGreyScaleFactor, fXColor);
                    }
                    monster.ShockMonster(shockTime);
                }
            }

            AudioManager.Instance.PlayClip(effectSound);
        }
    }
}
