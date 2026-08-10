using UnityEngine;

namespace Attacks
{
    public class AttackInit
    {
        public LayerMask HitMask;
        public AttackInit(LayerMask hitMask)
        {
            HitMask = hitMask;
        }
    }
}
