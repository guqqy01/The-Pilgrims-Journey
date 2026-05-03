using UnityEngine;
namespace LocalMap
{
    public class FlowField
    {
        public Vector2[,] Directions;  // per-tile direction toward hero
        public float[,] Costs;         // useful for debugging/gizmos

        private int _width, _height;

        public FlowField(int width, int height)
        {
            _width = width;
            _height = height;
            Directions = new Vector2[width, height];
            Costs = new float[width, height];
        }

        public Vector2 SampleDirection(Vector2 worldPos)
        {
            int x = Mathf.RoundToInt(worldPos.x);
            int y = Mathf.RoundToInt(worldPos.y);
            x = Mathf.Clamp(x, 0, _width - 1);
            y = Mathf.Clamp(y, 0, _height - 1);
            return Directions[x, y];
        }
    }
}


