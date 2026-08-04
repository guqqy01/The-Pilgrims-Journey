using Heros;

public interface IHeroDescriptionSection
{
    public void Bind(HeroSO heroSO);
    public bool HasContent(HeroSO heroSO);
}