using UnityEngine;
using Heros;

namespace LocalMap
{
    public class FlowFieldManager : Singleton<FlowFieldManager>
    {
        public FlowField CurrentField { get; private set; }

        private Vector2Int _lastHeroTile;

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
            CurrentField = FlowFieldComputer.Compute(
                heroPos,
                LocalMapGenerator.Instance.Tiles);
        }

        // Call this when the map changes
        public void ForceRecompute(Vector2 heroPos)
        {
            _lastHeroTile = new Vector2Int(-1, -1);
            UpdateForHeroPosition(heroPos);
        }
    }
}


