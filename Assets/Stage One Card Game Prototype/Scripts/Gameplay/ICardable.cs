namespace TPJ.Gameplay
{
    /// <summary>
    /// Implemented by anything a card can be generated from (tile templates, equipment).
    /// </summary>
    public interface ICardable
    {
        CardBase CreateCard();
        int ActionPointCost { get; }
    }

    public interface IActionPointCoster
    {
        public Buff ActionCost { get; }
    }

    public interface ITileCardType
    {
        public TileCardType TileCardType { get; }
    }
}
