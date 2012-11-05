using System;
using System.Runtime.Serialization;
using EssenceUDK.TilesInfo.Components.Enums;

namespace EssenceUDK.TilesInfo.Components.Tiles
{
    [Serializable()]
    [DataContract]
    public class TileWall : Tile
    {
        #region Fields
        #endregion

        #region ctor
        public TileWall()
            :base()
        {
            Type = TypeTile.Wall;
        }

        
        #endregion

        #region props

        #endregion



    }
}
