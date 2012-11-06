﻿﻿using System;
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
using Color = System.Drawing.Color;

namespace EssenceUDK.Platform.Factories
{
    /*
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
        public readonly UODataManager Data;
        
        private IDataContainer   container_LandData, container_ItemData, container_LandTile, container_ItemTile, container_LandTexm, container_ItemAnim;

        private IDataContainer[] container_Animation;


        private IDataContainer[] container_UniFont;
        private IDataContainer[] container_Map;
        private IDataContainer[] container_Sta;
        private IDataContainer[] container_MapDif;
        private IDataContainer[] container_StaDif;
        private IDataContainer[] container_Facet;

        private string GetPath(string file)
        {
            var folder = Data.Location.LocalPath;
            var flocat = Path.Combine(folder, file);
            return File.Exists(flocat) ? flocat : null;
        }

        internal ClassicFactory(UODataManager data)
        {
            Data = data;
            if (data.DataType.HasFlag(UODataType.UseUopFiles))
                throw new NotImplementedException();


            //IDataContainer uop = new UopContainer(GetPath("AnimationSequence.uop"), data.RealTime);
            //var sequence = new byte[uop.EntryLength][];
            //for (var i = 0U; i < uop.EntryLength; ++i)
            //    sequence[i] = uop[i];

            MulContainer virtualcontainer = null;
            virtualcontainer   = MulContainer.GetVirtual(null, GetPath("tiledata.mul"), data.RealTime);
            container_LandData = new MulContainer(virtualcontainer, 0, (_LandLength>>5), (uint)(data.DataType.HasFlag(UODataType.UseNewDatas) ?  964 :  836));
            container_ItemData = new MulContainer(virtualcontainer,    (_LandLength>>5)* (uint)(data.DataType.HasFlag(UODataType.UseNewDatas) ?  964 :  836), 
                                                                                      0, (uint)(data.DataType.HasFlag(UODataType.UseNewDatas) ? 1316 : 1188));
            
            virtualcontainer   = MulContainer.GetVirtual(GetPath("artidx.mul"), GetPath("art.mul"), data.RealTime);
            container_LandTile = new MulContainer(virtualcontainer, 0, _LandLength);
            container_ItemTile = new MulContainer(virtualcontainer, _LandLength, 0);

            container_LandTexm = new MulContainer(GetPath("texidx.mul"), GetPath("texmaps.mul"), data.RealTime);

            //container_ItemAnim = new MulContainer(0, GetPath("animdata.mul"), realtime);

            var animationcontainer = new List<IDataContainer>(16);
            if (!String.IsNullOrEmpty(GetPath("anim.idx")) && !String.IsNullOrEmpty(GetPath("anim.mul")))
                animationcontainer.Add(new MulContainer(GetPath("anim.idx"), GetPath("anim.mul"), data.RealTime));
            if (!String.IsNullOrEmpty(GetPath("anim2.idx")) && !String.IsNullOrEmpty(GetPath("anim2.mul")))
                animationcontainer.Add(new MulContainer(GetPath("anim2.idx"), GetPath("anim2.mul"), data.RealTime));
            if (!String.IsNullOrEmpty(GetPath("anim3.idx")) && !String.IsNullOrEmpty(GetPath("anim3.mul")))
                animationcontainer.Add(new MulContainer(GetPath("anim3.idx"), GetPath("anim3.mul"), data.RealTime));
            if (!String.IsNullOrEmpty(GetPath("anim4.idx")) && !String.IsNullOrEmpty(GetPath("anim4.mul")))
                animationcontainer.Add(new MulContainer(GetPath("anim4.idx"), GetPath("anim4.mul"), data.RealTime));
            if (!String.IsNullOrEmpty(GetPath("anim5.idx")) && !String.IsNullOrEmpty(GetPath("anim5.mul")))
                animationcontainer.Add(new MulContainer(GetPath("anim5.idx"), GetPath("anim5.mul"), data.RealTime));
            if (!String.IsNullOrEmpty(GetPath("animationframe1.uop")))
                animationcontainer.Add(new UopContainer(GetPath("animationframe1.uop"), data.RealTime));
            if (!String.IsNullOrEmpty(GetPath("animationframe2.uop")))
                animationcontainer.Add(new UopContainer(GetPath("animationframe2.uop"), data.RealTime));
            if (!String.IsNullOrEmpty(GetPath("animationframe3.uop")))
                animationcontainer.Add(new UopContainer(GetPath("animationframe3.uop"), data.RealTime));
            if (!String.IsNullOrEmpty(GetPath("animationframe4.uop")))
                animationcontainer.Add(new UopContainer(GetPath("animationframe4.uop"), data.RealTime));
            container_Animation = animationcontainer.ToArray();


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

        internal sealed unsafe class LandData : ILandData
        {
            internal interface IRawData
            {
                TileFlag   Flags        { get; set; }
                ushort     TexID        { get; set; }
                byte*       Name        { get; set; }
            }

            private    IRawData     _Data;
            private    Language     _Lang;
            private    string       _Name;

            string     ILandData.Name         { get { return _Name ?? (_Name = _Lang.ReadAnsiString(_Data.Name, 20)); }     set { throw new NotImplementedException(); } }
            TileFlag   ILandData.Flags        { get { return _Data.Flags; }        set { _Data.Flags = value; } }
            ushort     ILandData.TexID        { get { return _Data.TexID; }        set { _Data.TexID = value; } }

            internal LandData(Language lang, IRawData data)
            {
                _Data = data;
                _Lang = lang;
                _Name = null;
            }
        }

        internal sealed unsafe class ItemData : IItemData
        {
            internal interface IRawData
            {
                TileFlag    Flags       { get; set; }
                byte        Weight      { get; set; }
                byte        Quality     { get; set; }
                ushort      Miscdata    { get; set; }
                byte        Unk1        { get; set; }
                byte        Quantity    { get; set; }
                ushort      Animation   { get; set; }
                byte        Unk2        { get; set; }
                byte        Hue         { get; set; }
                byte        StackingOff { get; set; }
                byte        Value       { get; set; }
                byte        Height      { get; set; }
                byte*       Name        { get; set; }
            }

            private    IRawData     _Data;
            private    Language     _Lang;
            private    string       _Name;

            string     IItemData.Name         { get { return _Name ?? (_Name = _Lang.ReadAnsiString(_Data.Name, 20)); }     set { throw new NotImplementedException(); } }
            TileFlag   IItemData.Flags        { get { return _Data.Flags; }        set { _Data.Flags = value; } }
            byte       IItemData.Height       { get { return _Data.Height; }       set { _Data.Height = value; } }
            byte       IItemData.Quality      { get { return _Data.Quality; }      set { _Data.Quality = value; } }
            byte       IItemData.Quantity     { get { return _Data.Quantity; }     set { _Data.Quantity = value; } }
            ushort     IItemData.Animation    { get { return _Data.Animation; }    set { _Data.Animation = value; } }
            byte       IItemData.StackingOff  { get { return _Data.StackingOff; }  set { _Data.StackingOff = value; } }

            internal ItemData(Language lang, IRawData data)
            {
                _Data = data;
                _Lang = lang;
                _Name = null;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Size = 26, Pack = 1)]
        private unsafe struct OldLandData : LandData.IRawData
        {
            private uint       _Flags;
            private ushort     _TexID;
            private fixed byte _Name[20];
            
            TileFlag   LandData.IRawData.Flags        { get { return (TileFlag)_Flags; }    set { _Flags = (uint)value; } }
            ushort     LandData.IRawData.TexID        { get { return _TexID; }              set { _TexID = value; } }
            byte*      LandData.IRawData.Name         { get { byte* ptr; fixed (byte* p = _Name) ptr = p; return ptr; }   
                                                        set { ; } }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Size = 26, Pack = 1)]
        private unsafe struct NewLandData : LandData.IRawData
        {
            [MarshalAs(UnmanagedType.U8)]
            private TileFlag   _Flags;
            private ushort     _TexID;
            private fixed byte _Name[20];

            TileFlag   LandData.IRawData.Flags        { get { return _Flags; }        set { _Flags = value; } }
            ushort     LandData.IRawData.TexID        { get { return _TexID; }        set { _TexID = value; } }
            byte*      LandData.IRawData.Name         { get { byte* ptr; fixed (byte* p = _Name) ptr = p; return ptr; }   
                                                        set { ; } }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Size = 37, Pack = 1)]
        private unsafe struct OldItemData : ItemData.IRawData
        {
            private uint       _Flags;
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

            TileFlag   ItemData.IRawData.Flags        { get { return (TileFlag)_Flags; }set { _Flags = (uint)value; } }
            byte       ItemData.IRawData.Weight       { get { return _Weight; }         set { _Weight = value; } }
            byte       ItemData.IRawData.Quality      { get { return _Quality; }        set { _Quality = value; } }
            ushort     ItemData.IRawData.Miscdata     { get { return _Miscdata; }       set { _Miscdata = value; } }
            byte       ItemData.IRawData.Unk1         { get { return _Unk1; }           set { _Unk1 = value; } }
            byte       ItemData.IRawData.Quantity     { get { return _Quantity; }       set { _Quantity = value; } }
            ushort     ItemData.IRawData.Animation    { get { return _Animation; }      set { _Animation = value; } }
            byte       ItemData.IRawData.Unk2         { get { return _Unk2; }           set { _Unk2 = value; } }
            byte       ItemData.IRawData.Hue          { get { return _Hue; }            set { _Hue = value; } }
            byte       ItemData.IRawData.StackingOff  { get { return _StackingOff; }    set { _StackingOff = value; } }
            byte       ItemData.IRawData.Value        { get { return _Value; }          set { _Value = value; } }
            byte       ItemData.IRawData.Height       { get { return _Height; }         set { _Height = value; } }
            byte*      ItemData.IRawData.Name         { get { byte* ptr; fixed (byte* p = _Name) ptr = p; return ptr; }   
                                                        set { ; } }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Size = 41, Pack = 1)]
        private unsafe struct NewItemData : ItemData.IRawData
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
            
            TileFlag   ItemData.IRawData.Flags        { get { return _Flags; }        set { _Flags = value; } }
            byte       ItemData.IRawData.Weight       { get { return _Weight; }       set { _Weight = value; } }
            byte       ItemData.IRawData.Quality      { get { return _Quality; }      set { _Quality = value; } }
            ushort     ItemData.IRawData.Miscdata     { get { return _Miscdata; }     set { _Miscdata = value; } }
            byte       ItemData.IRawData.Unk1         { get { return _Unk1; }         set { _Unk1 = value; } }
            byte       ItemData.IRawData.Quantity     { get { return _Quantity; }     set { _Quantity = value; } }
            ushort     ItemData.IRawData.Animation    { get { return _Animation; }    set { _Animation = value; } }
            byte       ItemData.IRawData.Unk2         { get { return _Unk2; }         set { _Unk2 = value; } }
            byte       ItemData.IRawData.Hue          { get { return _Hue; }          set { _Hue = value; } }
            byte       ItemData.IRawData.StackingOff  { get { return _StackingOff; }  set { _StackingOff = value; } }
            byte       ItemData.IRawData.Value        { get { return _Value; }        set { _Value = value; } }
            byte       ItemData.IRawData.Height       { get { return _Height; }       set { _Height = value; } }
            byte*      ItemData.IRawData.Name         { get { byte* ptr; fixed (byte* p = _Name) ptr = p; return ptr; }   
                                                        set { ; } }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Size = 40, Pack = 1)]
        private unsafe struct NewAnimHeader
        {
            internal uint       _Header; // 0x41 0x4D 0x4F (0x55) AMO(U)
            internal uint       _Version;
            internal uint       _FileSize;
            internal uint       _AnimationID;
            internal short      _MainInitX;
            internal short      _MainInitY;
            internal short      _MainEndX;
            internal short      _MainEndY;
            internal uint       _ColourCount;
            internal uint       _ColourOffset;
            internal uint       _FramesCount;
            internal uint       _FramesOffset;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Size = 16, Pack = 1)]
        private unsafe struct NewAnimFrame
        {
            internal ushort     _Unknown1;
            internal ushort     _Unknown2;
            internal short      _MainInitX;
            internal short      _MainInitY;
            internal short      _MainEndX;
            internal short      _MainEndY;
            internal uint       _FrameLook; // offset to frame data from begining of it's header
            internal ushort     Width       { get { return (ushort)Math.Abs(_MainEndX - _MainInitX); } }
            internal ushort     Height      { get { return (ushort)Math.Abs(_MainEndY - _MainInitY); } }
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

        // gumps convertors       --------------------------------------------------------------

        private static unsafe void ConvertGumpSurface(byte[] rawdata, out Bitmap bmp)
        {
            bmp = null;
            if (rawdata == null || rawdata.Length < 8) {
                bmp = null;
                return;
            }


        }

        // animation convertors   --------------------------------------------------------------

        public static unsafe void ConvertAnimSurface(byte[] rawdata, ushort[] palette, out Bitmap bmp, int offset = 0)
        {
            bmp = null;
            var centX  = BitConverter.ToInt16(rawdata,   offset + 0);
            var centY  = BitConverter.ToInt16(rawdata,   offset + 2);
            var width  = BitConverter.ToUInt16(rawdata,  offset + 4);
            var height = BitConverter.ToUInt16(rawdata,  offset + 6);
            if (height == 0 || width == 0)
                return;

            bmp = new Bitmap(width, height, PixelFormat.Format16bppArgb1555);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
            ushort* line = (ushort*)bd.Scan0;
            int delta = bd.Stride >> 1;

            int xBase = centX - 0x200;
            int yBase = centY + height - 0x200;

            line += xBase;
            line += yBase * delta;

            uint header; int pos = offset + 4;
            while ((header = BitConverter.ToUInt32(rawdata,  pos+=4)) != 0x7FFF7FFF) {
                header ^= 0x80200000; // DoubleXor = (0x200 << 22) | (0x200 << 12);
                var drun = (header & 0xFFF);
                var offy = ((header >> 12) & 0x3FF);
                var offx = ((header >> 22) & 0x3FF);

                ushort* cur = line + (((offy) * delta) + ((offx) & 0x3FF));
                ushort* end = cur + (drun);
                while (cur < end)
                    *cur++ = palette[rawdata[pos++]];

            }
            bmp.UnlockBits(bd);
        }

        private static uint GetInternalAnimIndex(IDataContainer container, uint id)
        {
            var internid = 0U;
            var filename = Path.GetFileName(container is MulContainer ? (container as MulContainer).FNameMul :
                                            container is UopContainer ? (container as UopContainer).FNameUop : String.Empty).ToLower();
            switch (filename) {
                default         : internid = 0xDEADBEEFU; break;
                case "anim.mul" : internid = (id < 200) ? id * 110 : (id < 400) ? 22000 + (id - 200) * 65 : 35000 + (id - 400) * 175;    break;
                case "anim2.mul": internid = (id < 200) ? id * 110 : 22000 + (id - 200) * 65;    break;
                case "anim3.mul": internid = (id < 200) ? 9000 + id * 65 : (id < 400) ? 22000 + (id - 200) * 110 : 35000 + (id - 400) * 175; break;
                case "anim4.mul": internid = (id < 200) ? id * 110 : (id < 400) ? 22000 + (id - 200) * 65 : 35000 + (id - 400) * 175; break;
                case "anim5.mul": internid = (id < 200) ? id * 110 : (id < 400) ? 22000 + (id - 200) * 65 : 35000 + (id - 400) * 175; break;
                case "animationframe1.uop" : internid = 0xDEADBEEFU; break;
                case "animationframe2.uop" : internid = 0xDEADBEEFU; break;
                case "animationframe3.uop" : internid = 0xDEADBEEFU; break;
                case "animationframe4.uop" : internid = 0xDEADBEEFU; break;
            }
            return internid < container.EntryLength ? internid : 0xDEADBEEFU;
        }

        //                        --------------------------------------------------------------

        #endregion

        ILandTile[] IDataFactory.GetLandTiles()
        {
            uint i;
            var tiles = new LandTile[container_LandTile.EntryLength];
            for (uint b = i = 0; i < tiles.Length && b < container_LandData.EntryLength; ++b, ++i) {
                var tdata = Data.DataType.HasFlag(UODataType.UseNewDatas) // we just skip block header
                          ? container_LandData.Read<NewLandData>(b, 4, 32).Select(t=>(ILandData)new LandData(Data.Language, t)).ToArray()
                          : container_LandData.Read<OldLandData>(b, 4, 32).Select(t=>(ILandData)new LandData(Data.Language, t)).ToArray();
                for (uint c = 0; i < tiles.Length && c < 32; ++c, ++i)
                    tiles[i] = new LandTile(i, this, tdata[c], container_LandTile.IsValid(i) || container_LandTexm.IsValid(tdata[c].TexID));
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
                var tdata = Data.DataType.HasFlag(UODataType.UseNewDatas) // we just skip block header
                          ? container_ItemData.Read<NewItemData>(b, 4, 32).Select(t=>(IItemData)new ItemData(Data.Language, t)).ToArray()
                          : container_ItemData.Read<OldItemData>(b, 4, 32).Select(t=>(IItemData)new ItemData(Data.Language, t)).ToArray();
                for (uint c = 0; i < tiles.Length && c < 32; ++c, ++i)
                    tiles[i] = new ItemTile(i, this, tdata[c], container_ItemTile.IsValid(i));
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


        IAnimation[] IDataFactory.GetAnimations()
        {
return new IAnimation[0];
int ooo = 0;
            var list = new List<IAnimation>(1000);
            foreach (var container in container_Animation) {
                if (container is MulContainer) continue;
                var anim = new Animation();
                
                for (var i = 0U; i < container.EntryLength; ++i) {
                    var buffer = container[i]; // as anim use compresion is faster to work with its buffer
                    var header = Utils.BuffToStruct<NewAnimHeader>(buffer, 0)[0];
                    var frames = Utils.BuffToStruct<NewAnimFrame>(buffer, (int)header._FramesOffset, (int)header._FramesCount);

                    
    Color[]  colors  = new Color[0x100];
    ushort[] palette = new ushort[0x100];
    for (int k = 0; k < header._ColourCount; k++) {
        //colors[k] = Color.FromArgb(BitConverter.ToInt32(buffer, (int)header._ColourOffset + 4*k));
        colors[k] = Color.FromArgb(buffer[header._ColourOffset+4*k + 0], buffer[header._ColourOffset+4*k + 1], buffer[header._ColourOffset+4*k + 2]);
        //palette[k] = (ushort)((colors[k].R >> 3) << 10 | (colors[k].G >> 3) << 5 | (colors[k].B >> 3));// | (bin.ReadByte()>>7)<<15);
        //palette[k] ^= 0x8000;
        palette[k] = (ushort)(colors[k].R >> 3 << 10 | colors[k].G >> 3 << 5 | colors[k].B >> 3 << 0); if (palette[k] > 0) palette[k] |= 0x8000;

        //palette[p] = (ushort)(bin.ReadUInt16() ^ 0x8000);
    }
    //palette = Utils.BuffToStruct<ushort>(buffer, (int)header._ColourOffset, 0x100);
    //for (int p = 0; p < 0x100; p++)
    //    palette[p] ^= 0x8000;
if (header._AnimationID == 0197) {
    

                    //ushort[] palete = palette;
                    //for (var f = 0U; f < frames.Length; f++) {
                    //    int offset = (int)(header._FramesOffset + f * 16 + frames[f]._FrameLook);
                    //    palete = Utils.BuffToStruct<ushort>(buffer, offset, 0x100);
                    //    for (int p = 0; p < 0x100; p++)
                    //        if (palette[p] > 0)
                    //            palete[p] |= 0x8000;
                    //    Bitmap   bitmap;
                    //    ConvertAnimSurface(buffer, palete, out bitmap, offset + 0x200);
                    //    short centrx = BitConverter.ToInt16(buffer, offset + 0x200);
                    //    short centry = BitConverter.ToInt16(buffer, offset + 0x202);

                    //    var path = Path.Combine(@"C:\UltimaOnline", Path.GetFileNameWithoutExtension((container as UopContainer).FNameUop), String.Format("{0:D4}_{1:D2}_{2:D2}.bmp", header._AnimationID, ooo, f));
                    //    if (Directory.Exists((container as UopContainer).FNameUop))
                    //        Directory.CreateDirectory((container as UopContainer).FNameUop);
                    //    if (bitmap != null)
                    //        bitmap.Save(path);
                    //}
++ooo;
}

                }


            }
            return list.ToArray();
        }

    }
}
