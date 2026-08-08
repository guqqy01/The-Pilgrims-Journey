using UnityEngine;
namespace Attacks.Spot
{
    public class SpotAttackInit : AttackInit
    {
        public readonly Vector2 currentPosition;
        public readonly float? lifeTimeEnd;
        public readonly float movementSpeed;
        public SpotAttackInit(Vector2 currentPosition, float? lifeTimeEnd, float movementSpeed)
        {
            this.currentPosition = currentPosition;
            this.lifeTimeEnd = lifeTimeEnd;
            this.movementSpeed = movementSpeed;
        }
    }
}