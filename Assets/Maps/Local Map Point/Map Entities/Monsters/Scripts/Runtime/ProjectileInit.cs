using UnityEngine;

namespace Attacks.Projectiles
{
    public class ProjectileInit : AttackInit
    {
        public bool Homing;
        public bool Bounce;
        public Transform Target;
        public Vector2 Direction;
        public ProjectileOwner Owner = ProjectileOwner.Monster;
        public HealthAndShieldManager[] IgnoreExceptions;

        public ProjectileInit(
            LayerMask hitMask,
            bool homing,
            bool bounce,
            Transform target,
            Vector2 direction,
            ProjectileOwner owner,
            params HealthAndShieldManager[] ignoreExceptions) :
            base(hitMask)
        {
            Homing = homing;
            Bounce = bounce;
            Target = target;
            Direction = direction;
            Owner = owner;
            IgnoreExceptions = ignoreExceptions;
        }
    }

}
