using System;
using Spells;

namespace Relics
{
    [Serializable]
    public class SpellAndAmountOfUses
    {
        public SpellSO spell;
        public int amountOfUses = 1;
    }
}