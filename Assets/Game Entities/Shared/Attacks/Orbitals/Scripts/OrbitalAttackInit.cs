using UnityEngine;

namespace Attacks.Orbitals
{
    public class OrbitalAttackInit : AttackInit
    {
        public readonly Transform targetPositionTransform;

        public OrbitalAttackInit(
            LayerMask hitLayerMask,
            Vector2 position,
            Quaternion rotation,
            Transform targetPositionTransform,
            Transform parent = null) : base(hitLayerMask, position, rotation, parent)
        {
            this.targetPositionTransform = targetPositionTransform;
        }
    }
}

