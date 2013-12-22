﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EssenceUDK.Platform.DataTypes;

namespace EssenceUDK.Platform.Factories
{
    public interface IDataFactory
    {
        ILandTile[]  GetLandTiles();
        IItemTile[]  GetItemTiles();
        IGumpEntry[] GetGumpSurfs();
        IAnimation[] GetAnimations();
    }

    internal interface IDataFactoryReader
    {
        ISurface GetLandSurface(uint id);
        ISurface GetTexmSurface(uint id);
        ISurface GetItemSurface(uint id);
        ISurface GetGumpSurface(uint id);
    }

    internal interface IDataFactoryWriter
    {
        void SetLandSurface(uint id, ISurface surface);
        void SetTexmSurface(uint id, ISurface surface);
        void SetItemSurface(uint id, ISurface surface);
        void SetGumpSurface(uint id, ISurface surface);
    }
}
