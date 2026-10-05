using Attacks;
using Equipment;
using System;
using Tiles;
namespace TPJ.Gameplay
{
    public static class CardGameEventsBus
    {
        public static event Action OnGameBegin;
        public static event Action<CardGameEndReason> OnGameEnd;
        public static event Action<DeckHolder> OnBeginTurn;
        public static event Action<DeckHolder> OnEndTurn;
        public static event Action<DeckHolder, CardBase> OnCardPlayed;
        public static event Action<DeckHolder, TileBase> OnTileCardPlayed;
        public static event Action<DeckHolder, EquipmentSO> OnEquipmentCardPlayed;
        public static event Action<DeckHolder, BaseAttackSO> OnAttackCardPlayed;
        public static event Action<TileBase> OnBrigadePlaced;
        public static event Action<TileBase> OnBrigadeResolved;

        public static void RaiseOnGameBegin() => OnGameBegin?.Invoke();
        public static void RaiseOnGameEnd(CardGameEndReason reason) => OnGameEnd?.Invoke(reason);
        public static void RaiseOnBeginTurn(DeckHolder deckHolder) => OnBeginTurn?.Invoke(deckHolder);
        public static void RaiseOnEndTurn(DeckHolder deckHolder) => OnEndTurn?.Invoke(deckHolder);
        public static void RaiseOnCardPlayed(DeckHolder deckHolder, CardBase card) => OnCardPlayed?.Invoke(deckHolder, card);
        public static void RaiseOnTileCardPlayed(DeckHolder deckHolder, TileBase tile) => OnTileCardPlayed?.Invoke(deckHolder, tile);
        public static void RaiseOnEquipmentCardPlayed(DeckHolder deckHolder, EquipmentSO equipment) => OnEquipmentCardPlayed?.Invoke(deckHolder, equipment);
        public static void RaiseOnAttackCardPlayed(DeckHolder deckHolder, BaseAttackSO attack) => OnAttackCardPlayed?.Invoke(deckHolder, attack);
        public static void RaiseOnBrigadePlaced(TileBase tile) => OnBrigadePlaced?.Invoke(tile);
        public static void RaiseOnBrigadeResolved(TileBase tile) => OnBrigadeResolved?.Invoke(tile);

        public static void CleanUp()
        {
            OnGameBegin = null;
            OnGameEnd = null;
            OnBeginTurn = null;
            OnEndTurn = null;
            OnCardPlayed = null;
            OnTileCardPlayed = null;
            OnEquipmentCardPlayed = null;
            OnAttackCardPlayed = null;
            OnBrigadePlaced = null;
            OnBrigadeResolved = null;
        }
    }
}