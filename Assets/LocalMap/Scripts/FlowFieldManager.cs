using UnityEngine;
using Heros;
using System.Collections.Generic;
using GameManagerCalls;
namespace LocalMap
{
    public class FlowFieldManager : Singleton<FlowFieldManager>, IOnAfterCreateLocalMap
    {
        public FlowField CurrentHeroField { get; private set; }

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
            if (heroTile == _lastHeroTile && CurrentHeroField != null)
                return;

            _lastHeroTile = heroTile;
            CurrentHeroField = GetField(heroTile);
        }

        public FlowField GetField(Vector2 position)
        {
            return GetField(position.V2ToV2Int());
        }

        public FlowField GetField(Vector2Int tilePosition)
        {
            if(_savedFlowFields.TryGetValue(tilePosition, out FlowField flowField))
            {
                return flowField;
            }

            flowField = FlowFieldComputer.Compute(
                tilePosition,
                LocalMapGenerator.Instance.Tiles);

            _savedFlowFields[tilePosition] = flowField;

            return flowField;
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
            _lastHeroTile = new Vector2Int(-1, -1);
            _savedFlowFields.Clear();
        }
    }
}


