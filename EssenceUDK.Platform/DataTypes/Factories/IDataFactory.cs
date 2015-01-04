﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EssenceUDK.Platform.DataTypes;

namespace EssenceUDK.Platform.Factories
{
    [Flags]
    public enum ContainerDataType : uint
    {
        GumpArt = 0x00000001,
        Texture = 0x00000002,
        LandArt = 0x00000004,
        ItemArt = 0x00000008,
        TileArt = 0x0000000C,

        Facet00 = 0x00100000,
        Facet01 = 0x00200000,
        Facet02 = 0x00400000,
        Facet03 = 0x00800000,
        Facet04 = 0x01000000,
        Facet05 = 0x02000000,
        //FacetX0 = 0x04000000,
        //FacetX1 = 0x08000000,
        //FacetX2 = 0x10000000,
        //FacetX3 = 0x20000000,
        //FacetX4 = 0x40000000,
        //FacetX5 = 0x80000000,
        Unknown = 0x00000000
    }

    /// <summary>
    /// UDK API DataFactory Interface for managing UO data
    /// </summary>
    public interface IDataFactory
    {
        IMapFacet[]  GetMapFacets();
        ILandTile[]  GetLandTiles();
        IItemTile[]  GetItemTiles();
        IGumpEntry[] GetGumpSurfs();
        IAnimation[] GetAnimations();
    }

    /// <summary>
    /// UDK API DataFactory Interface for reading UO data
    /// </summary>
    public interface IDataFactoryReader
    {
        ISurface GetLandSurface(uint id);
        ISurface GetTexmSurface(uint id);
        ISurface GetItemSurface(uint id);
        ISurface GetGumpSurface(uint id);
        IMapBlockData GetMapBlock(byte mapindex, uint id);
    }

    /// <summary>
    /// UDK API - DataFactory Interface for writing UO data
    /// </summary>
    public interface IDataFactoryWriter
    {
        void Defrag(ContainerDataType datatype);
        void SetLandSurface(uint id, ISurface surface);
        void SetTexmSurface(uint id, ISurface surface);
        void SetItemSurface(uint id, ISurface surface);
        void SetGumpSurface(uint id, ISurface surface);
        void SetMapBlock(byte mapindex, uint id, IMapBlockData tiles);
    }
}
