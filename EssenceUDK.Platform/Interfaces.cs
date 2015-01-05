﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Drawing;
using System.Windows.Media;
using Color = EssenceUDK.Platform.DataTypes.Color;
using PixelFormat = EssenceUDK.Platform.DataTypes.PixelFormat;

namespace EssenceUDK.Platform
{
    public interface IPoint2D
    {
        short X { get; set; }
        short Y { get; set; }
    }

    public interface IClipper
    {
        short X1 { get; }
        short X2 { get; }
        short Y1 { get; }
        short Y2 { get; }
        ushort Width { get; }
        ushort Height { get; }
    }

    public interface IPalette
    {
        uint Length { get; }
        Color this[byte id] { get; set; }
    }

    public interface ISurface
    {
        ushort      Width       { get; }
        ushort      Height      { get; }
        PixelFormat PixelFormat { get; }

        unsafe uint*    ImageUIntPtr    { get; }
        unsafe ushort*  ImageWordPtr    { get; }
        unsafe byte*    ImageBytePtr    { get; }
        uint            Stride          { get; }
        
        IHuedSurface  GetSurface(IPalette palette);
        IClipSurface  GetSurface(IClipper clipper);
        IImageSurface GetSurface(); // ???

        //void BitBlt(ISurface dstSurface, short trgX, short trgY);
        //void BitBlt(IClipper srsClipper, ISurface dstSurface, short trgX, short trgY);
        //void BitBlt(IPalette srsPalette, IClipper srsClipper, ISurface dstSurface, IClipper dstClipper);

        bool Equals(IImageSurface surface);
    }

    public interface IImageSurface : ISurface//, ISerializable, ICloneable, IDisposable
    {
        ImageSource Image { get; }
        void Invalidate();

        IClipper GetImageRect();
        IPoint2D GetImageCenter();
        
        ushort GetHammingDistanceForAvrHash008(IImageSurface surface);
        ushort GetHammingDistanceForAvrHash032(IImageSurface surface);
        ushort GetHammingDistanceForAvrHash128(IImageSurface surface);
    }

    public interface IHuedSurface  : ISurface
    {
        IPalette Pallete { get; }
        ISurface Parent  { get; }
    }

    public interface IClipSurface  : ISurface
    {
        IClipper Clipper { get; }
        ISurface Parent  { get; }
    }

    public interface IAnimSurface  : ISurface
    {
    }



    public interface IDataContainer
    {
        bool IsValid(uint id);
        uint EntryLength { get; }
        bool IsIndexBased { get; }
        uint GetExtra(uint id);
        void SetExtra(uint id, uint value);
        byte[] this[uint id, bool item = false] { get; set; }
        T   Read<T>(uint id, uint offset) where T : struct;
        T[] ReadAll<T>(uint id, uint offset) where T : struct;
        T[] Read<T>(uint fromId, uint offset, uint count) where T : struct;
        void Write<T>(uint id, uint offset, T data) where T : struct;
        void Write<T>(uint id, uint offset, T[] data, uint sfrom, uint count) where T : struct;

        void Replace(uint id1, uint id2);
        void Delete(uint id);
        void Resize(uint entries);
        void Defrag();

        uint EntryHeaderSize { get; }
        uint EntryItemsCount { get; } 
    }
}
