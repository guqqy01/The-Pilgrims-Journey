using System;
using Tiles;

namespace TPJ.Gameplay
{
    [Serializable]
    public class HeroCard : TileCard<HeroTile>
    {
        public HeroCard(HeroTile cardableRef) : base(cardableRef) { }

        public override IDescriptable GetDisplayInfo() => CardableRef;
    }
}