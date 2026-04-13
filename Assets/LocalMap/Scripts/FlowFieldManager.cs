using UnityEngine;
using Heros;
using System.Collections.Generic;
using GameManagerCalls;
namespace LocalMap
{
    public class FlowFieldManager : Singleton<FlowFieldManager>, IOnAfterCreateLocalMap
    {
        public FlowField CurrentField { get; private set; }

        private Vector2Int _lastHeroTile;

        private readonly Dictionary<Vector2Int, FlowField> _savedFlowFields = new Dictionary<Vector2Int, FlowField>();

        private void Update()
        {
            if (HeroManager.Instance == null)
                return;

            if (HeroManager.Instance.ActiveHero == null)
                return;

            UpdateForHeroPosition(HeroManager.Instance.ActiveHero.transform.position);
        }

        public void UpdateForHeroPosition(Vector2 heroPos)
        {
            Vector2Int heroTile = new Vector2Int(
                Mathf.Clamp(Mathf.RoundToInt(heroPos.x), 0, LocalMapGenerator.Instance.MapSizeX - 1),
                Mathf.Clamp(Mathf.RoundToInt(heroPos.y), 0, LocalMapGenerator.Instance.MapSizeY - 1));

            // Only recompute when hero crosses a tile boundary
            if (heroTile == _lastHeroTile && CurrentField != null)
                return;

            _lastHeroTile = heroTile;
            GetField(heroTile);
        }

        private void GetField(Vector2Int heroTile)
        {
            if(_savedFlowFields.TryGetValue(heroTile, out FlowField flowField))
            {
                CurrentField = flowField;
            }
            else
            {
                flowField = FlowFieldComputer.Compute(
                    heroTile,
                    LocalMapGenerator.Instance.Tiles);

                _savedFlowFields[heroTile] = flowField;
            }
            CurrentField = flowField;
        }

        // Call this when the map changes
        public void ForceRecompute(Vector2 heroPos)
        {
            _lastHeroTile = new Vector2Int(-1, -1);
            _savedFlowFields.Clear();
            UpdateForHeroPosition(heroPos);
        }

        public void AfterCreateLocalMap()
        {
            _savedFlowFields.Clear();
        }
    }
}


