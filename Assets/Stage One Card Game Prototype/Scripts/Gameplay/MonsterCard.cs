using System;
using Tiles;

namespace TPJ.Gameplay
{
    [Serializable]
    public class MonsterCard : TileCard<MonsterTileBase>
    {
        public MonsterCard(MonsterTileBase cardableRef) : base(cardableRef) { }

        public override IDescriptable GetDisplayInfo() => CardableRef;
    }
}