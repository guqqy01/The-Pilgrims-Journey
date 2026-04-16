using System.Collections.Generic;
using UnityEngine;
namespace LocalMap
{
    public class FlowFieldComputer
    {
        const float edgeTolerance = 0.05f;

        public static FlowField Compute(Vector2Int heroWorldPos, MapTile[,] tiles)
        {
            return Compute((Vector2)heroWorldPos, tiles);
        }

        public static FlowField Compute(Vector2 heroWorldPos, MapTile[,] tiles)
        {
            int width = tiles.GetLength(0);
            int height = tiles.GetLength(1);

            FlowField field = new FlowField(width, height);
            float[,] costs = new float[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    costs[x, y] = float.MaxValue;

            Queue<Vector2Int> frontier = new Queue<Vector2Int>();

            Vector2Int heroTile = new Vector2Int(
                Mathf.RoundToInt(heroWorldPos.x),
                Mathf.RoundToInt(heroWorldPos.y));

            costs[heroTile.x, heroTile.y] = 0;
            frontier.Enqueue(heroTile);

            // BFS outward from hero, only through walkable connected tiles
            while (frontier.Count > 0)
            {
                Vector2Int current = frontier.Dequeue();

                foreach (Vector2Int neighbour in LocalMapGenerator.Instance.GetAdjacentNeighbours(current))
                {
                    if (!LocalMapGenerator.Instance.IsConnectedToPath(neighbour))
                        continue;

                    MapTile tile = tiles[neighbour.x, neighbour.y];

                    if (!tile.TileTypeAndVisual.TileType.IsWalkable())
                        continue;

                    float newCost = costs[current.x, current.y] + 1;
                    if (newCost < costs[neighbour.x, neighbour.y])
                    {
                        costs[neighbour.x, neighbour.y] = newCost;
                        frontier.Enqueue(neighbour);
                    }
                }
            }

            // Build direction vectors from costs
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (costs[x, y] == float.MaxValue)
                    {
                        field.Directions[x, y] = Vector2.zero;
                        continue;
                    }

                    Vector2Int bestNeighbour = new Vector2Int(x, y);
                    float bestCost = costs[x, y];

                    foreach (Vector2Int neighbour in LocalMapGenerator.Instance.GetAdjacentNeighbours(new Vector2Int(x, y)))
                    {
                        if (costs[neighbour.x, neighbour.y] < bestCost)
                        {
                            bestCost = costs[neighbour.x, neighbour.y];
                            bestNeighbour = neighbour;
                        }
                    }

                    field.Directions[x, y] = ((Vector2)(bestNeighbour - new Vector2Int(x, y))).normalized;
                }
            }

            field.Costs = costs;
            return field;
        }
    }
}


