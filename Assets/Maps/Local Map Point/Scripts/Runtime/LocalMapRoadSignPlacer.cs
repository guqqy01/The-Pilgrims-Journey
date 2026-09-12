using System.Collections.Generic;
using UnityEngine;

namespace LocalMap
{
    internal class LocalMapRoadSignPlacer
    {
        private readonly LocalMapGenerator _owner;

        public Dictionary<GameObject, Direction> RoadSigns { get; } = new Dictionary<GameObject, Direction>();

        public LocalMapRoadSignPlacer(LocalMapGenerator owner)
        {
            _owner = owner;
        }

        public void PlaceRoadSigns()
        {
            DestroyRoadSigns();

            Dictionary<Direction, Vector2Int> endPoints = _owner.Pathway.ToEndPoints;

            foreach (KeyValuePair<Direction, Vector2Int> kvp in endPoints)
            {
                Vector2 position = kvp.Value;

                switch (kvp.Key)
                {
                    case Direction.South:
                        position += new Vector2(CompatibilityExtensions.RandomSign(), 1);
                        break;
                    case Direction.North:
                        position += new Vector2(CompatibilityExtensions.RandomSign(), -1);
                        break;
                    case Direction.West:
                        position += new Vector2(1, CompatibilityExtensions.RandomSign());
                        break;
                    case Direction.East:
                        position += new Vector2(-1, CompatibilityExtensions.RandomSign());
                        break;
                }

                RoadSigns.Add(Object.Instantiate(_owner.RoadSignPrefab, position, Quaternion.identity, _owner.transform), kvp.Key);
                _owner.ObjectSpawner.MarkOccupied(position.V2ToV2Int());
            }
        }

        public void DestroyRoadSigns()
        {
            foreach (GameObject obj in RoadSigns.Keys)
            {
                if (Application.isPlaying)
                    Object.Destroy(obj);
                else
                    Object.DestroyImmediate(obj);
            }

            RoadSigns.Clear();
        }
    }
}
