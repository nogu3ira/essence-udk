using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EssenceUDK.Platform.DataTypes;
using EssenceUDK.Platform.DataTypes.FileFormat.Containers;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace EssenceUDK.Platform.Factories
{
    /*
     * anim.mul
anim2.mul
anim3.mul
anim4.mul
anim5.mul
animdata.mul
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
texidx.mul
texmaps.mul
tiledata.mul
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
        private IDataContainer container_Art;
        
        private IDataContainer[] container_UniFont;



        private IDataContainer[] container_Map;
        private IDataContainer[] container_Sta;
        private IDataContainer[] container_MapDif;
        private IDataContainer[] container_StaDif;
        private IDataContainer[] container_Facet;

        internal ClassicFactory(Uri uri, UODataType type, bool realtime)
        {
            string folder = uri.LocalPath;

            container_Art = new MulContainer(Path.Combine(folder, "artidx.mul"), Path.Combine(folder, "art.mul"), realtime);


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

        #region Data Convertors

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

        private static unsafe void ConvertItemSurface(byte[] rawdata, out WriteableBitmap bmp)
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
                int start = (height + 4);
                for (int i = 0; i < height; ++i)
                    lookups[i] = (int)(start + (bindata[count++]));

                //bmp = new Bitmap(width, height, PixelFormat.Format16bppArgb1555);
                bmp = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr555, null);
                bmp.Lock();

                ushort* line = (ushort*)bmp.BackBuffer;
                int delta = bmp.BackBufferStride >> 1;

                for (int y = 0; y < height; ++y, line += delta) {
                    count = lookups[y];
                    ushort* cur = line;
                    ushort* end;
                    int xOffset, xRun;
                    
					while (((xOffset = bindata[count++]) + (xRun = bindata[count++])) != 0) {
                        if (cur >= (ushort*)bmp.BackBuffer + delta * height)
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

                bmp.AddDirtyRect(new Int32Rect(0, 0, width, height));
                bmp.Unlock();
            }//*/
        }

        private static unsafe void ConvertItemSurface(ImageSource bmp, out byte[] rawdata)
        {
            rawdata = null;
        }

        #endregion

        ILandTile[] IDataFactory.GetLandTiles()
        {
            return null;
        }

        ISurface IDataFactory.GetLandSurface(uint id)
        {
            return null;
        }

        IItemTile[] IDataFactory.GetItemTiles()
        {
            var tiles = new ItemTile[container_Art.EntryLength - 0x4000];
            for (uint i = 0; i < tiles.Length; ++i) {
                tiles[i] = new ItemTile(i, this);
            }
                
            return tiles;
        }

        ISurface IDataFactory.GetItemSurface(uint id)
        {
            WriteableBitmap bitmap;
            ConvertItemSurface(container_Art[0x4000 + id], out bitmap);
            return bitmap != null ? new BitmapSurface(bitmap) : null;
        }

    }
}
