﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Drawing;
using System.Windows.Media;

namespace EssenceUDK.Platform
{
    public interface IPalette
    {
    }

    public interface IClipper
    {
        short  X1        { get; }
        short  X2        { get; }
        short  Y1        { get; }
        short  Y2        { get; }
        ushort Width     { get; }
        ushort Height    { get; }
    }

    public interface ISurface
    {
        ImageSource Image { get; }

        ushort Width     { get; }
        ushort Height    { get; }
        IHuedSurface  GetSurface(IPalette palette);
        IClipSurface  GetSurface(IClipper clipper);
        IImageSurface GetSurface(); // ???
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

    public interface IImageSurface : ISurface//, ISerializable, ICloneable, IDisposable
    {
        ImageSource Image { get; }
    }

    internal interface IDataContainer
    {
        bool IsValid(uint id);
        uint EntryLength { get; }
        byte[] this[uint id] { get; set; }
        T   Read<T>(uint id, uint offset) where T : struct;
        T[] Read<T>(uint fromId, uint offset, uint count) where T : struct;
    }
}
