﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using EssenceUDK.Platform.Factories;

namespace EssenceUDK.Platform.DataTypes
{
    /// <summary>
    /// An enumeration of 64 different tile flags.
    /// </summary>
    [Flags]
    public enum TileFlag : ulong
    {
        /// <summary>
        /// Nothing is flagged.
        /// </summary>
        None            = 0x0000000000000000,
        /// <summary>
        /// Not yet documented.
        /// </summary>
        Background      = 0x0000000000000001,
        /// <summary>
        /// Not yet documented.
        /// </summary>
        Weapon          = 0x0000000000000002,
        /// <summary>
        /// Not yet documented.
        /// </summary>
        Transparent     = 0x0000000000000004,
        /// <summary>
        /// The tile is rendered with partial alpha-transparency.
        /// </summary>
        Translucent     = 0x0000000000000008,
        /// <summary>
        /// The tile is a wall.
        /// </summary>
        Wall            = 0x0000000000000010,
        /// <summary>
        /// The tile can cause damage when moved over.
        /// </summary>
        Damaging        = 0x0000000000000020,
        /// <summary>
        /// The tile may not be moved over or through.
        /// </summary>
        Impassable      = 0x0000000000000040,
        /// <summary>
        /// Not yet documented.
        /// </summary>
        Wet             = 0x0000000000000080,
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown1        = 0x0000000000000100,
        /// <summary>
        /// The tile is a surface. It may be moved over, but not through.
        /// </summary>
        Surface         = 0x0000000000000200,
        /// <summary>
        /// The tile is a stair, ramp, or ladder.
        /// </summary>
        Bridge          = 0x0000000000000400,
        /// <summary>
        /// The tile is stackable
        /// </summary>
        Generic         = 0x0000000000000800,
        /// <summary>
        /// The tile is a window. Like <see cref="TileFlag.NoShoot" />, tiles with this flag block line of sight.
        /// </summary>
        Window          = 0x0000000000001000,
        /// <summary>
        /// The tile blocks line of sight.
        /// </summary>
        NoShoot         = 0x0000000000002000,
        /// <summary>
        /// For single-amount tiles, the string "a " should be prepended to the tile name.
        /// </summary>
        ArticleA        = 0x0000000000004000,
        /// <summary>
        /// For single-amount tiles, the string "an " should be prepended to the tile name.
        /// </summary>
        ArticleAn       = 0x0000000000008000,
        /// <summary>
        /// Not yet documented.
        /// </summary>
        Internal        = 0x0000000000010000,
        /// <summary>
        /// The tile becomes translucent when walked behind. Boat masts also have this flag.
        /// </summary>
        Foliage         = 0x0000000000020000,
        /// <summary>
        /// Only gray pixels will be hued
        /// </summary>
        PartialHue      = 0x0000000000040000,
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown2        = 0x0000000000080000,
        /// <summary>
        /// The tile is a map--in the cartography sense. Unknown usage.
        /// </summary>
        Map             = 0x0000000000100000,
        /// <summary>
        /// The tile is a container.
        /// </summary>
        Container       = 0x0000000000200000,
        /// <summary>
        /// The tile may be equiped.
        /// </summary>
        Wearable        = 0x0000000000400000,
        /// <summary>
        /// The tile gives off light.
        /// </summary>
        LightSource     = 0x0000000000800000,
        /// <summary>
        /// The tile is animated.
        /// </summary>
        Animation       = 0x0000000001000000,
        /// <summary>
        /// Gargoyles can fly over
        /// </summary>
        HoverOver       = 0x0000000002000000,
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown3        = 0x0000000004000000,
        /// <summary>
        /// Not yet documented.
        /// </summary>
        Armor           = 0x0000000008000000,
        /// <summary>
        /// The tile is a slanted roof.
        /// </summary>
        Roof            = 0x0000000010000000,
        /// <summary>
        /// The tile is a door. Tiles with this flag can be moved through by ghosts and GMs.
        /// </summary>
        Door            = 0x0000000020000000,
        /// <summary>
        /// Not yet documented.
        /// </summary>
        StairBack       = 0x0000000040000000,
        /// <summary>
        /// Not yet documented.
        /// </summary>
        StairRight      = 0x0000000080000000,
        UnUsed01        = 0x0000000100000000,
        /// <summary>
        /// Чтото для ноудрау тайла
        /// </summary>
        Unknown4        = 0x0000000200000000,
        UnUsed03        = 0x0000000400000000,
        /// <summary>
        /// 
        /// </summary>
        Unknown5        = 0x0000000800000000,
        /// <summary>
        /// 
        /// </summary>
        Unknown6        = 0x0000001000000000,
        /// <summary>
        /// 
        /// </summary>
        Unknown7        = 0x0000002000000000,
        UnUsed07        = 0x0000004000000000,

        UnUsed08        = 0x0000008000000000,
        /// <summary>
        /// Чтото связаное с мачтами
        /// </summary>
        Unknown8        = 0x0000010000000000,
        UnUsed10        = 0x0000020000000000,
        UnUsed11        = 0x0000040000000000,
        UnUsed12        = 0x0000080000000000,
        UnUsed13        = 0x0000100000000000,
        UnUsed14        = 0x0000200000000000,
        UnUsed15        = 0x0000400000000000,
        UnUsed16        = 0x0000800000000000,
        UnUsed17        = 0x0001000000000000,
        UnUsed18        = 0x0002000000000000,
        UnUsed19        = 0x0004000000000000,
        UnUsed20        = 0x0008000000000000,
        UnUsed21        = 0x0010000000000000,
        UnUsed22        = 0x0020000000000000,
        UnUsed23        = 0x0040000000000000,
        UnUsed24        = 0x0080000000000000,
        UnUsed25        = 0x0100000000000000,
        UnUsed26        = 0x0200000000000000,
        UnUsed27        = 0x0400000000000000,
        UnUsed28        = 0x0800000000000000,
        UnUsed29        = 0x1000000000000000,
        UnUsed30        = 0x2000000000000000,
        UnUsed31        = 0x4000000000000000,
        UnUsed32        = 0x8000000000000000
    }

    public interface ITileData
    {
    }

    public interface ILandData : ITileData
    {
        string     Name         { get; set; }
        TileFlag   Flags        { get; set; }
        ushort     TexID        { get; set; }
    }

    public interface IItemData : ITileData
    {
        string     Name         { get; set; }
        TileFlag   Flags        { get; set; }
        byte       Height       { get; set; }
        byte       Quality      { get; set; }
        byte       Quantity     { get; set; }
        ushort     Animation    { get; set; }
        byte       StackingOff  { get; set; }
    }

    public interface IEntryTile : ITileData
    {
        uint       TileId       { get; set; }
        ISurface   Surface      { get; set; }
        bool       IsValid      { get; }
    }

    public interface ILandTile : IEntryTile, ILandData
    {
        ISurface   Texture      { get; set; }
    }

    public interface IItemTile : IEntryTile, IItemData
    { 
    }

    public abstract class EntryTile : IEntryTile
    {
        protected uint      _TileId;
        protected ISurface  _Surface;
        protected ITileData _TileData;
        protected bool      _IsValid;

        //protected Color   RadarColor;

        protected IDataFactory dataFactory;
        internal  EntryTile(uint id, IDataFactory factory, ITileData tiledata, bool valid)
        {
            _TileId     = id;
            dataFactory = factory;
            _TileData   = tiledata;
            _IsValid    = valid;
        }

        public uint TileId  { get { return _TileId; } set { _TileId = value; } }
        public bool IsValid { get { return _IsValid; } }
        public abstract ISurface Surface { get; set; }
    }

    public sealed class ItemTile : EntryTile, IItemTile, IItemData
    {
        public override ISurface     Surface {
            get { return _Surface ?? (_Surface = dataFactory.GetItemSurface(_TileId)); }
            set { ; }
        }

        string   IItemData.Name         { get { return ((IItemData)_TileData).Name; }           set { ((IItemData)_TileData).Name = value; } }
        TileFlag IItemData.Flags        { get { return ((IItemData)_TileData).Flags; }          set { ((IItemData)_TileData).Flags = value; } }
        byte     IItemData.Height       { get { return ((IItemData)_TileData).Height; }         set { ((IItemData)_TileData).Height = value; } }
        byte     IItemData.Quality      { get { return ((IItemData)_TileData).Quality; }        set { ((IItemData)_TileData).Quality = value; } }
        byte     IItemData.Quantity     { get { return ((IItemData)_TileData).Quantity; }       set { ((IItemData)_TileData).Quantity = value; } }
        ushort   IItemData.Animation    { get { return ((IItemData)_TileData).Animation; }      set { ((IItemData)_TileData).Animation = value; } }
        byte     IItemData.StackingOff  { get { return ((IItemData)_TileData).StackingOff; }    set { ((IItemData)_TileData).StackingOff = value; } }

        internal  ItemTile(uint id, IDataFactory factory, IItemData tiledata, bool valid) : base(id, factory, tiledata, valid)
        {          
        }
    }

    public sealed class LandTile : EntryTile, ILandTile, ILandData
    {
        public override ISurface     Surface {
            get { return _Surface ?? (_Surface = dataFactory.GetLandSurface(_TileId)); }
            set { ; }
        }

        public          ISurface     Texture {
            get { return _Texture ?? (_Texture = dataFactory.GetTexmSurface(((ILandData)_TileData).TexID)); }
            set { ; }
        }
        protected ISurface _Texture;

        string   ILandData.Name         { get { return ((ILandData)_TileData).Name; }           set { ((ILandData)_TileData).Name = value; } }
        TileFlag ILandData.Flags        { get { return ((ILandData)_TileData).Flags; }          set { ((ILandData)_TileData).Flags = value; } }
        ushort   ILandData.TexID        { get { return ((ILandData)_TileData).TexID; }          set { ((ILandData)_TileData).TexID = value; } }

        internal LandTile(uint id, IDataFactory factory, ILandData tiledata, bool valid) : base(id, factory, tiledata, valid)
        {
        }
    }



    public interface IAnimation
    {     
    }

    public sealed class Animation : IAnimation
    {
        private uint        _AnimId;
        private AnimEntry[] _Entries;
    }

    public sealed class AnimEntry
    {
        private uint        _AnimId;
        private Animation   _Parent;
        private byte        _Action;
        private byte        _Direct;
        private IPalette    _Palette;
        private AnimFrame[] _Frames;
    }

    public sealed class AnimFrame
    {
        private AnimEntry   _Parent;
        private IPalette    _Palette;
        private ISurface    _Surface;
        private short       _SCentrX;
        private short       _SCentrY;
    }

}
