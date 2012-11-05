using System;
using System.Collections.Generic;
using EssenceUDK.Platform;
using EssenceUDK.TilesInfo.Components;
using EssenceUDK.TilesInfo.Interfaces;

namespace EssenceUDK.TilesInfo.Factories
{
    public class Tile1024 : Factory , IFactory
    {
        #region Implementation of Translator

        public Tile1024(UODataManager location) : base(location)
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
