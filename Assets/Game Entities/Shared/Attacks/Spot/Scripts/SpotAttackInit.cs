using UnityEngine;
namespace Attacks.Spot
{
    public class SpotAttackInit : AttackInit
    {
        public readonly float? lifeTimeEnd;
        public readonly float movementSpeed;
        public SpotAttackInit(
            LayerMask hitLayerMask, 
            Vector2 position, 
            Quaternion rotation, 
            float? lifeTimeEnd, 
            float movementSpeed, 
            Transform parent = null) : 
                base(hitLayerMask, position, rotation, parent)
        {
            this.lifeTimeEnd = lifeTimeEnd;
            this.movementSpeed = movementSpeed;
        }
    }
}