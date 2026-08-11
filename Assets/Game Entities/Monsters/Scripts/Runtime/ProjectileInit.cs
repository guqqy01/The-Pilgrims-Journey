using UnityEngine;

namespace Attacks.Projectiles
{
    public class ProjectileInit : AttackInit
    {
        public bool Homing;
        public bool Bounce;
        public Transform Target;
        public Vector2 Direction;
        public HealthAndShieldManager[] IgnoreExceptions;

        public ProjectileInit(
            LayerMask hitMask,
            Vector2 position,
            Quaternion rotation,
            bool homing,
            bool bounce,
            Transform target,
            Vector2 direction,
            Transform parent = null,
            params HealthAndShieldManager[] ignoreExceptions) :
            base(hitMask, position, rotation, parent)
        {
            Homing = homing;
            Bounce = bounce;
            Target = target;
            Direction = direction;
            IgnoreExceptions = ignoreExceptions;
        }
    }

}
