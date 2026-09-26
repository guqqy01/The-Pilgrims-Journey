using System.Collections.Generic;
using Tiles;
using Tiles.UI;
using TPJ.Gameplay;
using UnityEngine;

namespace TPJ.UI
{
    /// <summary>
    /// Handles the "pick a section of an adjacent tile to destroy" flow triggered the
    /// instant a player-owned Brigade tile is placed. Highlights every cell of every
    /// adjacent tile that's actually touching the Brigade (a tile can contact it at more
    /// than one cell) as a clickable yellow candidate; clicking one commits it, turns it
    /// red, and it stays that way until TileCardTypeEffectManager actually resolves the
    /// destruction at end of turn (OnBrigadeResolved).
    ///
    /// Keyed per-brigade-tile so more than one Brigade can be awaiting a choice at once —
    /// otherwise placing a second Brigade before confirming the first would silently strand
    /// it, locked forever with no way to choose.
    ///
    /// Add this component alongside your other placement/targeting UI scripts (e.g. next
    /// to EquipmentTargetHighlighter) so it's alive to catch OnBrigadePlaced, and assign a
    /// CellHighlightUI prefab in the Inspector.
    /// </summary>
    public class BrigadeTargetHighlighter : MonoBehaviour
    {
        [SerializeField] private CellHighlightUI cellHighlightPrefab;

        private class Session
        {
            public readonly List<CellHighlightUI> Selectable = new();
            public CellHighlightUI Selected;
        }

        private readonly Dictionary<TileBase, Session> _sessions = new();

        private void OnEnable()
        {
            CardGameEventsBus.OnBrigadePlaced += BeginTargeting;
            CardGameEventsBus.OnBrigadeResolved += EndSession;
        }

        private void OnDisable()
        {
            CardGameEventsBus.OnBrigadePlaced -= BeginTargeting;
            CardGameEventsBus.OnBrigadeResolved -= EndSession;
            ClearAllSessions();
        }

        private void BeginTargeting(TileBase brigadeTile)
        {
            TileGridBase grid = brigadeTile.TileGrid;
            if (grid == null)
                return;

            if (!TileGridUI.TryGetUIForGrid(grid, out TileGridUI gridUI))
                return;

            List<TileBase> adjacent = grid.GetAdjacentTiles(brigadeTile);

            if (adjacent.Count == 0)
            {
                // Nothing to hit — release the tile immediately instead of leaving it
                // locked forever waiting for a choice that can't be made.
                TileCardTypeEffectManager.CancelBrigadeChoice(brigadeTile);
                brigadeTile.Locked = false;
                return;
            }

            var session = new Session();
            _sessions[brigadeTile] = session;

            foreach (TileUIBase tileUI in gridUI.TileUIs)
            {
                if (!adjacent.Contains(tileUI.Tile))
                    continue;

                foreach (Vector2Int worldCell in TileCardTypeAdjacency.FindContactCells(brigadeTile, tileUI.Tile))
                {
                    Vector2Int? localCell = tileUI.Tile.WorldCellToLocalCell(worldCell);
                    if (localCell == null)
                        continue;

                    CellHighlightUI highlight = Instantiate(cellHighlightPrefab);
                    highlight.ShowSelectable(tileUI, localCell.Value, worldCell,
                        clicked => OnCellClicked(brigadeTile, clicked));

                    session.Selectable.Add(highlight);
                }
            }
        }

        private void OnCellClicked(TileBase brigadeTile, CellHighlightUI clicked)
        {
            if (!_sessions.TryGetValue(brigadeTile, out Session session))
                return;

            TileCardTypeEffectManager.SetBrigadeTarget(brigadeTile, clicked.TileUI.Tile, clicked.WorldCell);

            foreach (CellHighlightUI highlight in session.Selectable)
            {
                if (highlight == clicked) continue;
                if (highlight != null) Destroy(highlight.gameObject);
            }
            session.Selectable.Clear();

            clicked.MarkSelected();
            session.Selected = clicked;
        }

        private void EndSession(TileBase brigadeTile)
        {
            if (!_sessions.TryGetValue(brigadeTile, out Session session))
                return;

            foreach (CellHighlightUI highlight in session.Selectable)
                if (highlight != null) Destroy(highlight.gameObject);

            if (session.Selected != null)
                Destroy(session.Selected.gameObject);

            _sessions.Remove(brigadeTile);
        }

        private void ClearAllSessions()
        {
            foreach (TileBase brigadeTile in new List<TileBase>(_sessions.Keys))
                EndSession(brigadeTile);
        }

        private void OnDestroy() => ClearAllSessions();
    }
}