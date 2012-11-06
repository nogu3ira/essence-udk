using System;

namespace EssenceUDK.TilesInfo.Components.Tiles
{
    [Serializable()]
    public class TileDoor : Tile
    {
        public TileDoor()
            :base()
        {
            Type = Enums.TypeTile.Doors;
        }
    }
}
