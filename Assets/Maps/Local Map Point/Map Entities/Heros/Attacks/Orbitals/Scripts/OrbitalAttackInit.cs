using UnityEngine;

namespace Attacks.Orbitals
{
    public class OrbitalAttackInit : AttackInit
    {
        public readonly Transform targetPositionTransform;

        public OrbitalAttackInit(LayerMask hitLayerMask, Transform targetPositionTransform) : base(hitLayerMask)
        {
            this.targetPositionTransform = targetPositionTransform;
        }
    }
}

