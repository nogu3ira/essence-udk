﻿using System;
using System.Collections.Generic;

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace EssenceUDK.Platform.DataTypes
{
    /// <summary>
    /// Wrapper struct for pixel color representing, to make more easy for wirk with them
    /// </summary>
    public struct Color
    {
        private uint        colval;
        private PixelFormat format;

        public  PixelFormat Format { get { return format; } }

        public enum PixelFormat : byte
        {
            IUNKNOWN    = 0x00,
            A1R5G5B5    = 0x01,
            A8R8G8B8    = 0x10
        }

        private Color(ushort color)
        {
            colval = color;
            format = PixelFormat.A1R5G5B5;
        }

        private Color(uint color)
        {
            colval = color;
            format = PixelFormat.A8R8G8B8;
        }

        public byte R5 { get { 
                uint value = 0xCD;
                switch (format) {
                    case PixelFormat.A1R5G5B5:  value = colval>>10;     break;
                    case PixelFormat.A8R8G8B8:  value = colval>>19;     break;
                }
                return (byte)(value & 0x0000001F);
            }
        }

        public byte G5 { get { 
                uint value = 0xCD;
                switch (format) {
                    case PixelFormat.A1R5G5B5:  value = colval>>5;      break;
                    case PixelFormat.A8R8G8B8:  value = colval>>11;     break;
                }
                return (byte)(value & 0x0000001F);
            }
        }

        public byte B5 { get { 
                uint value = 0xCD;
                switch (format) {
                    case PixelFormat.A1R5G5B5:  value = colval>>0;      break;
                    case PixelFormat.A8R8G8B8:  value = colval>>3;      break;
                }
                return (byte)(value & 0x0000001F);
            }
        }

        public byte R8 { get { 
                uint value = 0xCD;
                switch (format) {
                    case PixelFormat.A1R5G5B5:  value = colval>>10<<3;  break;
                    case PixelFormat.A8R8G8B8:  value = colval>>16;     break;
                }
                return (byte)(value & 0x000000FF);
            }
        }

        public byte G8 { get { 
                uint value = 0xCD;
                switch (format) {
                    case PixelFormat.A1R5G5B5:  value = colval>>5<<3;  break;
                    case PixelFormat.A8R8G8B8:  value = colval>>8;     break;
                }
                return (byte)(value & 0x000000FF);
            }
        }

        public byte B8 { get { 
                uint value = 0xCD;
                switch (format) {
                    case PixelFormat.A1R5G5B5:  value = colval>>0<<3;   break;
                    case PixelFormat.A8R8G8B8:  value = colval>>0;      break;
                }
                return (byte)(value & 0x000000FF);
            }
        }

        public static Color FromARGB(byte r, byte g, byte b, PixelFormat format = PixelFormat.A8R8G8B8)
        {
            return FromARGB(0xFF, r, g, b, format);
        }

        public static Color FromARGB(byte a, byte r, byte g, byte b, PixelFormat format = PixelFormat.A8R8G8B8)
        {
            uint  value;
            Color color;
            switch (format) {
                case PixelFormat.A1R5G5B5:  value = (uint)((r & 0x1F) << 10 | (g & 0x1F) << 5 | (b & 0x1F));
                                            if (a > 0) value |= 0xFF000000;
                                            color = (ushort)(value & 0x0000FFFF);
                                            break;
                case PixelFormat.A8R8G8B8:  value = (uint)(a << 24 | r << 16 | g << 8 | b);
                                            color = (uint)value;
                                            break;
                default: throw new NotImplementedException();
            }
            return color;
        }

        public static implicit operator ushort(Color color)
        {
            uint value = 0xDEAD;
            switch (color.format) {
                case PixelFormat.A1R5G5B5:  value = color.colval;
                                            break;
                case PixelFormat.A8R8G8B8:  value = (color.colval>>19<<10 | color.colval>>11<<5 | color.colval>>3<<0);
                                            if ((color.colval & 0xFF000000) != 0) value |= 0x8000;
                                            break;
            }
            return (ushort)(value & 0x0000FFFF);
        }

        public static implicit operator Color(ushort color)
        {
            return new Color(color);
        }

        public static implicit operator uint(Color color)
        {
            uint value = 0xDEADBEEF;
            switch (color.format) {
                case PixelFormat.A1R5G5B5:  value = (color.colval>>10<<19 | color.colval>>5<<11 | color.colval>>0<<3);
                                            if ((color.colval & 0x00008000) != 0) value |= 0xFF000000;
                                            break;
                case PixelFormat.A8R8G8B8:  value = color.colval;
                                            break;
            }
            return value;
        }

        public static implicit operator Color(uint color)
        {
            return new Color(color);
        }
    }

    internal abstract class PaletteBase
    {
        

        public void GetPalPalette(string file)
        {
            var content = File.ReadAllLines(file, Encoding.ASCII);
            if (content[0].ToUpper() != "JASC-PAL" || content[1].ToUpper() != "0100" || content[2].ToUpper() != "256")
                throw new FileFormatException();

            for (int i = 0; i < 0x100; ++i) {
                var rgb = content[3 + i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                (this as IPalette)[(byte)i] = Color.FromARGB(byte.Parse(rgb[0]), byte.Parse(rgb[1]), byte.Parse(rgb[2]));               
            }
        }

        public void SavePalPalette(string file)
        {
            var content = String.Format("JASC-PAL{0}0100{0}256{0}", Environment.NewLine);
            for (int i = 0; i < 0x100; ++i)
                content += String.Format("{1} {2} {3} {0}", Environment.NewLine, 
                    (this as IPalette)[(byte)i].R8, (this as IPalette)[(byte)i].G8, (this as IPalette)[(byte)i].B8);
            File.WriteAllText(file, content, Encoding.ASCII);
        }

        //public unsafe Bitmap GetAsBimap(int width = 0x100, int height = 4)
        //{
        //    Bitmap bmp = new Bitmap(width, height * (int)(this as IPalette).Length, PixelFormat.Format16bppArgb1555);
        //    BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
        //    ushort* line = (ushort*)bd.Scan0;
        //    int delta = bd.Stride >> 1;

        //    for (int p = 0; p < (int)(this as IPalette).Length; ++p)
        //        for (int y = 0; y < bd.Height / (int)(this as IPalette).Length; ++y, line += delta)
        //        {
        //            ushort* cur = line;
        //            for (int i = 0; i < width; ++i)
        //            {
        //                *cur++ = (int)(this as IPalette)[(byte)p];
        //            }
        //        }

        //    bmp.UnlockBits(bd);
        //    //bmp.Dispose();
        //    return bmp;
        //}
    }

    internal class PaletteHues : Palette16bit, IPalette
    {
        public override uint Length {
            get { return 0x20; }
        }
        public override Color this[byte id] {
            get { return id < 0x20 ? base[id] : ((Color)((ushort)0x0000)); }
            set { if (id < 0x20) base[id] = value; }
        }
    }

    internal class Palette16bit : PaletteBase, IPalette
    {
        internal ushort[] colors;

        public virtual uint Length {
            get { return 0x100; }
        }
        public virtual Color this[byte id]{
            get { return colors[id]; }
            set { colors[id] = value; }
        }
    }

    internal class Palette32bit : PaletteBase, IPalette
    {
        internal uint[] colors;

        public virtual uint Length {
            get { return 0x100; }
        }
        public virtual Color this[byte id] {
            get { return colors[id]; }
            set { colors[id] = value; }
        }
    }

    //TODO: Very dirty right now...
    internal class BitmapSurface : /*Image, !!!DAMN!!! */ IImageSurface
    {
        #region Interface Implementation
        ImageSource IImageSurface.Image     { get { return Bitmap; } }
        ImageSource ISurface.Image          { get { return Bitmap; } }
        public ImageSource  Image { get { return Bitmap; } }
        ushort      ISurface.Width          { get { return (ushort)Bitmap.Width;  } }
        ushort      ISurface.Height         { get { return (ushort)Bitmap.Height; } }

        IHuedSurface  ISurface.GetSurface(IPalette palette)
        {
            throw new NotImplementedException();
        }

        IClipSurface  ISurface.GetSurface(IClipper clipper)
        {
            throw new NotImplementedException();
        }

        IImageSurface ISurface.GetSurface()
        {
            
            return this;
        }
        #endregion


        protected BitmapSource Bitmap;

        internal BitmapSurface(BitmapSource bitmap)
        {
            Bitmap = bitmap;
        }

    }

    //internal class DirectDrawSurface : ISurface
    //{
    // Yea, we need it later   
    //}
}
