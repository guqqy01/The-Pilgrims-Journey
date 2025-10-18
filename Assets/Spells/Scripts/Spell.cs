using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

[Serializable]
public class Spell
{
    public SpellSO spellSO;

    public int numberOfUses;

    public int MaxUses;

    public Spell(SpellSO spellSO, int amountOfUses)
    {
        this.spellSO = spellSO;
        MaxUses = amountOfUses;
        numberOfUses = MaxUses;
    }

    public void Reset()
    {
        numberOfUses = MaxUses;

    }
}
