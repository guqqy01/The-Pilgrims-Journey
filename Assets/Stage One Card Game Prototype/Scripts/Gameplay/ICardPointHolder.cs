namespace TPJ.Gameplay
{
    public interface ICardPointHolder
    {
        public CardGamePoints CardGamePoints { get; }
        public void CleanUp();
    }
}