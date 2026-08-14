using UnityEngine;

namespace Attacks
{
    public class AttackInit
    {
        public LayerMask HitMask;
        public Vector2 Position;
        public Quaternion Rotation;
        public Transform Parent;
        public AttackInit(LayerMask hitMask, Vector2 position, Quaternion rotation, Transform parent = null)
        {
            HitMask = hitMask;
            Position = position;
            Rotation = rotation;
            Parent = parent;
        }
    }
}
