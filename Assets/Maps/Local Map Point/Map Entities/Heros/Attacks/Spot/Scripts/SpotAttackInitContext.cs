using UnityEngine;
namespace Heros.Attacks.Spot
{
    public class SpotAttackInitContext : HeroAttackInitContext
    {
        public readonly Vector2 currentPosition;
        public readonly float? lifeTimeEnd;
        public readonly float movementSpeed;
        public SpotAttackInitContext(Vector2 currentPosition, float? lifeTimeEnd, float movementSpeed)
        {
            this.currentPosition = currentPosition;
            this.lifeTimeEnd = lifeTimeEnd;
            this.movementSpeed = movementSpeed;
        }
    }
}