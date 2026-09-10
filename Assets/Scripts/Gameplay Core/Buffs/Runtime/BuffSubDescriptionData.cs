using UnityEngine;

public class BuffSubDescriptionData : ISubDescriptionData
{
    public Buff Buff;
    public string StatString;

    public BuffSubDescriptionData(Buff buff, string statString)
    {
        Buff = buff;
        StatString = statString;
    }
}

public class SpellSubDescriptionData : ISubDescriptionData
{
    public string Name;
    public Sprite Sprite;
    public string Description;
    public int AmountOfUses;

    public SpellSubDescriptionData(string name, Sprite sprite, string description, int amountOfUses)
    {
        Name = name;
        Sprite = sprite;
        Description = description;
        AmountOfUses = amountOfUses;
    }
}