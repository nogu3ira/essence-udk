﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EssenceUDK.Platform.DataTypes;
using EssenceUDK.Platform.DataTypes.FileFormat.Containers;
using EssenceUDK.Platform.UtilHelpers;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace EssenceUDK.Platform.Factories
{
    /*
     * anim.mul
anim2.mul
anim3.mul
anim4.mul
anim5.mul
animinfo.mul

 
facet00.mul
facet01.mul
facet02.mul
facet03.mul
facet04.mul
facet05.mul
fonts.mul
Gumpart.mul
Gumpidx.mul
hues.mul
light.mul
lightidx.mul

mapdif0.mul
mapdif1.mul
mapdif2.mul
mapdifl0.mul
mapdifl1.mul
mapdifl2.mul
multi.mul
palette.mul
radarcol.mul
sjis2uni.mul
skillgrp.mul
skills.mul
sound.mul
soundidx.mul
speech.mul
stadif0.mul
stadif1.mul
stadif2.mul
stadifi0.mul
stadifi1.mul
stadifi2.mul
stadifl0.mul
stadifl1.mul
stadifl2.mul
staidx0.mul
staidx0x.mul
staidx1.mul
staidx2.mul
staidx3.mul
staidx4.mul
staidx5.mul
statics0.mul
statics0x.mul
statics1.mul
statics2.mul
statics3.mul
statics4.mul
statics5.mul
    map0.mul
map0x.mul
map1.mul
map2.mul
map3.mul
map4.mul
map5.mul
    */
    internal class ClassicFactory : IDataFactory
    {
        public readonly Uri Uri = null;
        public readonly UODataType DataType = UODataType.Inavalide;
        
        private IDataContainer   container_LandData, container_ItemData, container_LandTile, container_ItemTile, container_LandTexm, container_ItemAnim;



        private IDataContainer[] container_UniFont;
        private IDataContainer[] container_Map;
        private IDataContainer[] container_Sta;
        private IDataContainer[] container_MapDif;
        private IDataContainer[] container_StaDif;
        private IDataContainer[] container_Facet;

        private string GetPath(string file)
        {
            string folder = Uri.LocalPath;
            return Path.Combine(folder, file);
        }

        internal ClassicFactory(Uri uri, UODataType type, bool realtime)
        {
            Uri = uri;
            DataType = type;
            MulContainer virtualcontainer;

            if (type.HasFlag(UODataType.UseUopFiles))
                throw new NotImplementedException();

            virtualcontainer   = MulContainer.GetVirtual(null, GetPath("tiledata.mul"), realtime);
            container_LandData = new MulContainer(virtualcontainer, 0, (_LandLength>>5), (uint)(type.HasFlag(UODataType.UseNewDatas) ?  964 :  836));
            container_ItemData = new MulContainer(virtualcontainer,    (_LandLength>>5)* (uint)(type.HasFlag(UODataType.UseNewDatas) ?  964 :  836), 
                                                                                      0, (uint)(type.HasFlag(UODataType.UseNewDatas) ? 1316 : 1188));
            
            virtualcontainer   = MulContainer.GetVirtual(GetPath("artidx.mul"), GetPath("art.mul"), realtime);
            container_LandTile = new MulContainer(virtualcontainer, 0, _LandLength);
            container_ItemTile = new MulContainer(virtualcontainer, _LandLength, 0);

            container_LandTexm = new MulContainer(GetPath("texidx.mul"), GetPath("texmaps.mul"), realtime);

            //container_ItemAnim = new MulContainer(0, GetPath("animdata.mul"), realtime);


            /*
            container_Map    = new IDataContainer[6];
            container_Sta    = new IDataContainer[6];
            container_MapDif = new IDataContainer[6];
            container_StaDif = new IDataContainer[6];
            container_Facet  = new IDataContainer[6];

            for (var i = 0; i < container_UniFont.Length; ++i)
                container_Map[i] = new MulContainer(0, Path.Combine(folder, String.Format("unifont{0}.mul", i), true));
            */
            //container_UniFont = new IDataContainer[13];
            //for (var i = 0; i < container_UniFont.Length; ++i)
            //    container_UniFont[i] = new MulContainer(0, Path.Combine(folder, String.Format("unifont{0}.mul", i), true));

        }

        #region Data Types

        private const uint _LandLength = 0x4000;

        //private uint[]            _Header;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Size = 26, Pack = 1)]
        internal unsafe struct OldLandData : ILandData
        {
            [MarshalAs(UnmanagedType.U4)]
            private TileFlag   _Flags;
            private ushort     _TexID;
            private fixed byte _Name[20];
            //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            //private string     _Name;

            //string     ILandData.Name         { get { return _Name; }         set { _Name = value; } }
            string     ILandData.Name         { get { return String.Empty; }  set { ; } }
            TileFlag   ILandData.Flags        { get { return _Flags; }        set { _Flags = value; } }
            ushort     ILandData.TexID        { get { return _TexID; }        set { _TexID = value; } }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Size = 30, Pack = 1)]
        internal unsafe struct NewLandData : ILandData
        {
            [MarshalAs(UnmanagedType.U8)]
            internal TileFlag   _Flags;
            internal ushort     _TexID;
            internal fixed byte _Name[20];
            //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            //private string     _Name;

            //string     ILandData.Name         { get { return _Name; }         set { _Name = value; } }
            string     ILandData.Name         { get { return String.Empty; }         set { ; } }
            TileFlag   ILandData.Flags        { get { return _Flags; }        set { _Flags = value; } }
            ushort     ILandData.TexID        { get { return _TexID; }        set { _TexID = value; } }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Size = 37, Pack = 1)]
        internal unsafe struct OldItemData : IItemData
        {
            [MarshalAs(UnmanagedType.U4)]
            private TileFlag   _Flags;
            private byte       _Weight;
            private byte       _Quality;
            private ushort     _Miscdata;
            private byte       _Unk1;
            private byte       _Quantity;
            private ushort     _Animation;
            private byte       _Unk2;
            private byte       _Hue;
            private byte       _StackingOff;
            private byte       _Value;
            private byte       _Height;
            private fixed byte _Name[20];
            //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            //private string     _Name;

            //string     ILandData.Name         { get { return _Name; }         set { _Name = value; } }
            string     IItemData.Name         { get { return String.Empty; }         set { ; } }
            TileFlag   IItemData.Flags        { get { return _Flags; }        set { _Flags = value; } }
            byte       IItemData.Height       { get { return _Height; }       set { _Height = value; } }
            byte       IItemData.Quality      { get { return _Quality; }      set { _Quality = value; } }
            byte       IItemData.Quantity     { get { return _Quantity; }     set { _Quantity = value; } }
            ushort     IItemData.Animation    { get { return _Animation; }    set { _Animation = value; } }
            byte       IItemData.StackingOff  { get { return _StackingOff; }  set { _StackingOff = value; } }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Size = 41, Pack = 1)]
        internal unsafe struct NewItemData : IItemData
        {
            [MarshalAs(UnmanagedType.U8)]
            private TileFlag   _Flags;
            private byte       _Weight;
            private byte       _Quality;
            private ushort     _Miscdata;
            private byte       _Unk1;
            private byte       _Quantity;
            private ushort     _Animation;
            private byte       _Unk2;
            private byte       _Hue;
            private byte       _StackingOff;
            private byte       _Value;
            private byte       _Height;
            private fixed byte _Name[20];
            //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            //private string     _Name;

            //string     ILandData.Name         { get { return _Name; }         set { _Name = value; } }
            string     IItemData.Name         { get { return String.Empty; }         set { ; } }
            TileFlag   IItemData.Flags        { get { return _Flags; }        set { _Flags = value; } }
            byte       IItemData.Height       { get { return _Height; }       set { _Height = value; } }
            byte       IItemData.Quality      { get { return _Quality; }      set { _Quality = value; } }
            byte       IItemData.Quantity     { get { return _Quantity; }     set { _Quantity = value; } }
            ushort     IItemData.Animation    { get { return _Animation; }    set { _Animation = value; } }
            byte       IItemData.StackingOff  { get { return _StackingOff; }  set { _StackingOff = value; } }
        }

        #endregion

        #region Data Convertors

        // art.mul convertors    ---------------------------------------------------------------

        private static unsafe void ConvertLandSurface(byte[] rawdata, out BitmapSource bmp)
        {
            /*
            var bmpWriter = new WriteableBitmap(44, 44, 96, 96, PixelFormats.Bgr555, null);
            bmpWriter.Lock();

            int xOffset = 21;
            int xRun = 2;

            byte* data = rawdata;
            ushort* line = (ushort*)bmpWriter.BackBuffer;
            int delta = bmpWriter.BackBufferStride >> 1;

            for (int y = 0; y < 22; ++y, --xOffset, xRun += 2, line += delta)
            {
                ushort* cur = line + xOffset;
                ushort* end = cur + xRun;

                while (cur < end)
                    *cur++ = (ushort)(bin.ReadUInt16() | 0x8000);
            }

            xOffset = 0;
            xRun = 44;

            for (int y = 0; y < 22; ++y, ++xOffset, xRun -= 2, line += delta)
            {
                ushort* cur = line + xOffset;
                ushort* end = cur + xRun;

                while (cur < end)
                    *cur++ = (ushort)(bin.ReadUInt16() | 0x8000);
            }

            bmpWriter.AddDirtyRect(new Int32Rect(0, 0, 44, 44));
            bmpWriter.Unlock();
            bmpWriter.Freeze();
            */

            if (rawdata == null || rawdata.Length == 0) {
                bmp = null;
                return;
            }

            var bmpWriter = new WriteableBitmap(44, 44, 96, 96, PixelFormats.Bgr555, null);
            bmpWriter.Lock();

            fixed (byte* bindata = rawdata)
            {
                ushort* bdata = (ushort*)bindata;
                int xOffset = 21;
                int xRun = 2;

                ushort* line = (ushort*)bmpWriter.BackBuffer;
                int delta = bmpWriter.BackBufferStride >> 1;

                for (int y = 0; y < 22; ++y, --xOffset, xRun += 2, line += delta)
                {
                    ushort* cur = line + xOffset;
                    ushort* end = cur + xRun;

                    while (cur < end)
                        *cur++ = (ushort)(*bdata++ | 0x8000);
                }

                xOffset = 0;
                xRun = 44;

                for (int y = 0; y < 22; ++y, ++xOffset, xRun -= 2, line += delta)
                {
                    ushort* cur = line + xOffset;
                    ushort* end = cur + xRun;

                    while (cur < end)
                        *cur++ = (ushort)(*bdata++ | 0x8000);
                }
            }
            bmpWriter.AddDirtyRect(new Int32Rect(0, 0, 44, 44));
            bmpWriter.Unlock();
            bmpWriter.Freeze();
            bmp = bmpWriter;
        }

        private static unsafe void ConvertLandSurface(ImageSource bmp, out byte[] rawdata)
        {
            rawdata = null;
        }

        private static unsafe void ConvertItemSurface(byte[] rawdata, out Bitmap bmp)
        {
            fixed (byte* data = rawdata)
            {
                ushort* bindata = (ushort*)data;

                int count = 2;
                int width  = bindata[count++];
                int height = bindata[count++];

				if (width >= 0x0400 || height >= 0x0400) {
                    bmp = null;
                    return;
				}
                    
                int[] lookups = new int[height];
                int start = (height + 4);
                for (int i = 0; i < height; ++i)
                    lookups[i] = (int)(start + (bindata[count++]));

                bmp = new Bitmap(width, height, PixelFormat.Format16bppArgb1555);
                var bd = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);

                ushort* line = (ushort*)bd.Scan0;
                int delta = bd.Stride >> 1;

                for (int y = 0; y < height; ++y, line += delta) {
                    count = lookups[y];
                    ushort* cur = line;
                    ushort* end;
                    int xOffset, xRun;

					while (((xOffset = bindata[count++]) + (xRun = bindata[count++])) != 0) {
                        if (cur >= (ushort*)bd.Scan0 + delta * height)
                            break;
                        
                        if (2 * count >= rawdata.Length)
                            break;

                        if (xOffset + xRun > delta)
                            break;

                        cur += xOffset;
                        end = cur + xRun;

                        while (cur < end)
                            *cur++ = (ushort)(bindata[count++] ^ 0x8000);
                    }
                } 
                bmp.UnlockBits(bd);
            }
        }

        private static unsafe void ConvertItemSurface(byte[] rawdata, out BitmapSource bmp)
        {/*
            if (rawdata == null || rawdata.Length < 8) {
                bmp = new WriteableBitmap(40, 40, 96, 96, PixelFormats.Bgr555, null);
                return;
            }
            fixed (byte* data = rawdata)
            {
                ushort* bindata = (ushort*)data;

                int count = 2;
                int width  = bindata[count++];
                int height = bindata[count++];

				if (width >= 0x0400 || height >= 0x0400) {
                    bmp = null;
                    return;
				}

                int[] lookups = new int[height];

                int start = (int)count + (height * 2);

                for (int i = 0; i < height; ++i)
                    lookups[i] = (int)(start + (bindata[count++] * 2));

                bmp = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr555, null);
                bmp.Lock();

                ushort* line = (ushort*)bmp.BackBuffer;
                int delta = bmp.BackBufferStride >> 1;

                for (int y = 0; y < height; ++y, line += delta)
                {
                    count = lookups[y];

                    ushort* cur = line;
                    ushort* end;

                    int xOffset, xRun;

                    while (((xOffset = bindata[count++]) + (xRun = bindata[count++])) != 0) {
                        cur += xOffset;
                        end = cur + xRun;

                        while (cur < end)
                            *cur++ = (ushort)(bindata[count++] ^ 0x8000);
                    }
                }

                bmp.AddDirtyRect(new Int32Rect(0, 0, width, height));
                bmp.Unlock();
            }
            //index += 0x4000;
            //index &= 0xFFFF;

            /**/
            if (rawdata == null || rawdata.Length < 8) {
                bmp = null;
                return;
            }

            fixed (byte* data = rawdata)
            {
                ushort* bindata = (ushort*)data;

                int count = 2;
                int width  = bindata[count++];
                int height = bindata[count++];

				if (width >= 0x0400 || height >= 0x0400) {
                    bmp = null;
                    return;
				}
                    
                int[] lookups = new int[height];
                int start = (height + 4);
                for (int i = 0; i < height; ++i)
                    lookups[i] = (int)(start + (bindata[count++]));

                //bmp = new Bitmap(width, height, PixelFormat.Format16bppArgb1555);
                var bmpWriter = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr555, null);
                bmpWriter.Lock();

                ushort* line = (ushort*)bmpWriter.BackBuffer;
                int delta = bmpWriter.BackBufferStride >> 1;

                for (int y = 0; y < height; ++y, line += delta) {
                    count = lookups[y];
                    ushort* cur = line;
                    ushort* end;
                    int xOffset, xRun;
                    
					while (((xOffset = bindata[count++]) + (xRun = bindata[count++])) != 0) {
                        if (cur >= (ushort*)bmpWriter.BackBuffer + delta * height)
                            break;
                        
                        if (2 * count >= rawdata.Length)
                            break;

                        if (xOffset + xRun > delta)
                            break;

                        cur += xOffset;
                        end = cur + xRun;

                        while (cur < end)
                            *cur++ = (ushort)(bindata[count++] ^ 0x8000);
                    }
                }

                bmpWriter.AddDirtyRect(new Int32Rect(0, 0, width, height));
                bmpWriter.Unlock();
                bmpWriter.Freeze();

                bmp = bmpWriter;
            }//*/
        }

        private static unsafe void ConvertItemSurface(ImageSource bmp, out byte[] rawdata)
        {
            rawdata = null;
        }

        // texmaps.mul convertors --------------------------------------------------------------

        private static unsafe void ConvertTexmSurface(byte[] rawdata, out BitmapSource bmp)
        {
            /*
            int length, extra;
            Stream stream = _fileIndex.Seek(index, out length, out extra);

            if (stream == null)
                return null;

            int size = extra == 0 ? 64 : 128;

            BinaryReader bin = new BinaryReader(stream);
            WriteableBitmap bmp = new WriteableBitmap(size, size, 96, 96, PixelFormats.Bgr555, null);
            bmp.Lock();

            ushort* line = (ushort*)bmp.BackBuffer;
            int delta = bmp.BackBufferStride >> 1;

            for (int y = 0; y < size; ++y, line += delta)
            {
                ushort* cur = line;
                ushort* end = cur + size;

                while (cur < end)
                    *cur++ = (ushort)(bin.ReadUInt16() ^ 0x8000);
            }

            bmp.AddDirtyRect(new Int32Rect(0, 0, size, size));
            bmp.Unlock();
            */
            
            if (rawdata == null || rawdata.Length == 0) {
                bmp = null;
                return;
            }
            
            // TODO: its greate loooose, we need to view at osi data model
            int size = rawdata.Length == 8192 ? 64 : 128;
            //int size = extra == 0 ? 64 : 128;

            var bmpWriter = new WriteableBitmap(size, size, 96, 96, PixelFormats.Bgr555, null);
            bmpWriter.Lock();

            ushort* line = (ushort*)bmpWriter.BackBuffer;
            int delta = bmpWriter.BackBufferStride >> 1;

            //int max = size * size * 2;
            //if (m_StreamBuffer == null || m_StreamBuffer.Length < max)
            //    m_StreamBuffer = new byte[max];
            //stream.Read(m_StreamBuffer, 0, max);

            fixed (byte* data = rawdata)
            {
                ushort* bindat = (ushort*)data;
                for (int y = 0; y < size; ++y, line += delta)
                {
                    ushort* cur = line;
                    ushort* end = cur + size;

                    while (cur < end)
                        *cur++ = (ushort)(*bindat++ ^ 0x8000);
                }
            }

            bmpWriter.AddDirtyRect(new Int32Rect(0, 0, size, size));
            bmpWriter.Unlock();
            bmpWriter.Freeze();

            bmp = bmpWriter;
        }

        private static unsafe void ConvertTexmSurface(ImageSource bmp, out byte[] rawdata)
        {
            rawdata = null;
        }

        //                        --------------------------------------------------------------

        #endregion

        ILandTile[] IDataFactory.GetLandTiles()
        {
            uint i;
            var tiles = new LandTile[container_LandTile.EntryLength];
            for (uint b = i = 0; i < tiles.Length && b < container_LandData.EntryLength; ++b, ++i) {
                var tdata = DataType.HasFlag(UODataType.UseNewDatas)
                          ? container_LandData.Read<NewLandData>(b, 4, 32).Select(t=>(ILandData)t).ToArray()
                          : container_LandData.Read<OldLandData>(b, 4, 32).Select(t=>(ILandData)t).ToArray();
                for (uint c = 0; i < tiles.Length && c < 32; ++c, ++i) 
                    tiles[i] = new LandTile(i, this, tdata[c]);
                --i;
            }
            return tiles;
        }

        ISurface IDataFactory.GetLandSurface(uint id)
        {
            BitmapSource bitmap;
            ConvertLandSurface(container_LandTile[id], out bitmap);
            return bitmap != null ? new BitmapSurface(bitmap) : null;
        }

        ISurface IDataFactory.GetTexmSurface(uint id)
        {
            BitmapSource bitmap;
            ConvertTexmSurface(container_LandTexm[id], out bitmap);
            return bitmap != null ? new BitmapSurface(bitmap) : null;
        }

        IItemTile[] IDataFactory.GetItemTiles()
        {
            uint i;
            var tiles = new ItemTile[container_ItemTile.EntryLength];
            for (uint b = i = 0; i < tiles.Length && b < container_ItemData.EntryLength; ++b, ++i) {
                var tdata = DataType.HasFlag(UODataType.UseNewDatas)
                          ? container_ItemData.Read<NewItemData>(b, 4, 32).Select(t=>(IItemData)t).ToArray()
                          : container_ItemData.Read<OldItemData>(b, 4, 32).Select(t=>(IItemData)t).ToArray();
                for (uint c = 0; i < tiles.Length && c < 32; ++c, ++i) 
                    tiles[i] = new ItemTile(i, this, tdata[c]);
                --i;
            }
            return tiles;
        }

        ISurface IDataFactory.GetItemSurface(uint id)
        {
            BitmapSource bitmap;
            ConvertItemSurface(container_ItemTile[id], out bitmap);            
            return bitmap != null ? new BitmapSurface(bitmap) : null;
        }

    }
}
