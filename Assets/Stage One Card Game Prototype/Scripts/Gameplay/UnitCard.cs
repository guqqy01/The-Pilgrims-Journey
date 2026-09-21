using System;
using Tiles;

namespace TPJ.Gameplay
{
    [Serializable]
    public class UnitCard : TileCard<UnitTile>
    {
        public UnitCard(UnitTile cardableRef) : base(cardableRef) { }

        public override IDescriptable GetDisplayInfo() => CardableRef;
    }
}