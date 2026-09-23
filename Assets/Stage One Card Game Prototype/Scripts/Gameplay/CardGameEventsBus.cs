using Equipment;
using System;
using Tiles;
namespace TPJ.Gameplay
{
    public static class CardGameEventsBus
    {
        public static event Action<DeckHolder> OnBeginTurn;
        public static event Action<DeckHolder> OnEndTurn;
        public static event Action<DeckHolder, CardBase> OnCardPlayed;
        public static event Action<DeckHolder, TileBase> OnTileCardPlayed;
        public static event Action<DeckHolder, EquipmentSO> OnEquipmentCardPlayed;

        public static void RaiseOnBeginTurn(DeckHolder deckHolder) => OnBeginTurn?.Invoke(deckHolder);
        public static void RaiseOnEndTurn(DeckHolder deckHolder) => OnEndTurn?.Invoke(deckHolder);
        public static void RaiseOnCardPlayed(DeckHolder deckHolder, CardBase card) => OnCardPlayed?.Invoke(deckHolder, card);
        public static void RaiseOnTileCardPlayed(DeckHolder deckHolder, TileBase tile) => OnTileCardPlayed?.Invoke(deckHolder, tile);
        public static void RaiseOnEquipmentCardPlayed(DeckHolder deckHolder, EquipmentSO equipment) => OnEquipmentCardPlayed?.Invoke(deckHolder, equipment);
        
        public static void CleanUp()
        {
            OnBeginTurn = null;
            OnEndTurn = null;
            OnCardPlayed = null;
            OnTileCardPlayed = null;
            OnEquipmentCardPlayed = null;
        }
    }
}