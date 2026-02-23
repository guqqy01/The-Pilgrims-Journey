using Heros;
using UnityEngine;


namespace Monsters
{
    public class StationaryMonsterMovement : MonsterMovement
    {
        public Vector2Int StationaryPosition { private set; get; }

        private void Start()
        {
            StationaryPosition = new Vector2Int(Mathf.FloorToInt(rb.position.x), Mathf.FloorToInt(rb.position.y));
        }

        protected override Vector2? GetTargetPosition()
        {
            return StationaryPosition;
        }

        protected override bool GetIfAtTargetPosition()
        {
            return ((Vector2)StationaryPosition).Approximately(rb.position);
        }
    }
}