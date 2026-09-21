namespace TPJ.Gameplay
{
    /// <summary>
    /// Implemented by anything a card can be generated from (tile templates, equipment).
    /// </summary>
    public interface ICardable
    {
        BaseCard CreateCard();
    }
}
