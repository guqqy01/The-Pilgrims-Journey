using System.Collections.Generic;
using Tiles;
using TPJ.Gameplay;
using UnityEngine;
namespace TPJ.UI
{
    public class ColumnScoreHolderUI : MonoBehaviour
    {
        [SerializeField]
        private TileGridBase tileGrid;

        [SerializeField]
        private ColumnScoreUI columnScoreUIPrefab;
        private readonly List<ColumnScoreUI> _columnScoreUIs = new List<ColumnScoreUI>();

        private void Start()
        {
            Setup();
        }

        public void Setup()
        {
            _columnScoreUIs.DestroyAndClearList();

            for (int i = 0; i < tileGrid.GridSize.x; i++)
            {
                ColumnScoreUI spawned = Instantiate(columnScoreUIPrefab, transform);
                _columnScoreUIs.Add(spawned);
            }

            CardGameManager.WhenReady(manager => manager.OnBoardChanged += UpdateUIs);
            CardGameManager.WhenReady(_ => UpdateUIs());
        }

        private void OnDestroy()
        {
            CardGameManager.IfReady(manager => manager.OnBoardChanged -= UpdateUIs);
        }

        private bool IsPlayersTile(TileBase tile)
        {
            return TileOwnership.Get(tile) == CardGameManager.Instance.PlayerDeckHolder;
        }

        private bool IsEnemiesTile(TileBase tile)
        {
            return TileOwnership.Get(tile) == CardGameManager.Instance.EnemyDeckHolder;
        }

        public void UpdateUIs()
        {
            int[] values = new int[tileGrid.GridSize.x];
            foreach(TileBase tile in tileGrid.ListOfTiles)
            {
                List<Vector2Int> placedPositions = tile.RotatedPlacedShape;
                if (IsPlayersTile(tile))
                {
                    foreach(Vector2Int position in placedPositions)
                    {
                        if (!values.IsInBounds<int>(position.x))
                            continue;

                        values[position.x]++;
                    }
                }
                else if (IsEnemiesTile(tile))
                {
                    foreach (Vector2Int position in placedPositions)
                    {
                        if (!values.IsInBounds<int>(position.x))
                            continue;

                        values[position.x]--;
                    }
                }
            }

            for (int i = 0; i < values.Length; i++)
            {
                _columnScoreUIs[i].UpdateScore(values[i]);
            }
        }
    }
}
