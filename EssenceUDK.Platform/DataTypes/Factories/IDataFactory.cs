﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EssenceUDK.Platform.DataTypes;

namespace EssenceUDK.Platform.Factories
{
    public interface IDataFactory
    {
        ILandTile[] GetLandTiles();
        ISurface GetLandSurface(uint id);
        ISurface GetTexmSurface(uint id);

        IItemTile[] GetItemTiles();
        ISurface GetItemSurface(uint id);

        IAnimation[] GetAnimations();
    }
}
