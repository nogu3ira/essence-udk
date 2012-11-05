using System;
using System.Collections.Generic;
using EssenceUDK.Platform;
using EssenceUDK.TilesInfo.Components;
using EssenceUDK.TilesInfo.Interfaces;

namespace EssenceUDK.TilesInfo.Factories
{
    public class Tile256 : Factory , IFactory
    {
        #region Implementation of Translator

        public Tile256(UODataManager location) : base(location)
        {
        }

        public List<TileCategory> Categories
        {
            get { throw new NotImplementedException(); }
        }

        public void Populate()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
