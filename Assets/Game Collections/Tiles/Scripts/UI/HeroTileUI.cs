using Heros;
using System.Collections.Generic;
using UnityEngine;

namespace Tiles.UI
{
    public class HeroTileUI : TileUI<HeroTile>, IDescriptable
    {
        public HeroTile HeroTile => ObjectTile;
        public HeroSO HeroSO
        {
            get
            {
                if (HeroTile == null)
                    return null;

                return HeroTile.TiledObject as HeroSO;
            }
        }
        public string Title => HeroTile.Title;

        public string MainDescription => HeroTile.MainDescription;

        public string ShortDescription => HeroTile.ShortDescription;

        public Sprite Sprite => HeroTile.Sprite;

        public List<ISubDescriptionData> GetSubDescriptionData => HeroTile.GetSubDescriptionData;
    }
 }

