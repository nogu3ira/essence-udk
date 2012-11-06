using System;
using EssenceUDK.TilesInfo.Components.Enums;

namespace EssenceUDK.TilesInfo.Components.Tiles
{
    [Serializable()]
    public class TileMisc : Tile
    {
        public TileMisc()
            :base()
        {
            Type = TypeTile.Misc;
        }
    }
}
