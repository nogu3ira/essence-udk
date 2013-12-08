﻿using System;
using System.Collections.Generic;

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
﻿using System.Security.Cryptography;
﻿using System.Text;
using System.Runtime.InteropServices;
﻿using System.Windows;
﻿using System.Windows.Interop;
﻿using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelFormat = EssenceUDK.Platform.DataTypes.PixelFormat;
using MediaPixelFormat = System.Windows.Media.PixelFormat;


namespace EssenceUDK.Platform.DataTypes
{
    public enum PixelFormat : byte
    {
        Bpp16X1R5G5B5 = 0x21,
        Bpp16A1R5G5B5 = 0x22,
        Bpp32X8R8G8B8 = 0x41,
        Bpp32A8R8G8B8 = 0x42,
        
        UnknownFormat = 0x00,
        BppFormatMask = 0xF0,
        BppFormatOffs = 0x04
    }

    /// <summary>
    /// Wrapper struct for pixel color representing, to make more easy for work with them
    /// </summary>
    public struct Color
    {
        private uint        colval;
        private byte        bytepp;
        private PixelFormat format;

        public  PixelFormat Format { get { return format; } }

        private Color(ushort color)
        {
            colval = color;
            bytepp = 2;
            format = PixelFormat.Bpp16A1R5G5B5;
        }

        private Color(uint color)
        {
            colval = color;
            bytepp = 4;
            format = PixelFormat.Bpp32A8R8G8B8;
        }

        public byte R5 { get { 
                uint value = 0xCD;
                switch (format) {
                    case PixelFormat.Bpp16X1R5G5B5:  value = colval>>10;     break;
                    case PixelFormat.Bpp16A1R5G5B5:  value = colval>>10;     break;
                    case PixelFormat.Bpp32X8R8G8B8:  value = colval>>19;     break;
                    case PixelFormat.Bpp32A8R8G8B8:  value = colval>>19;     break;
                    default: throw new NotImplementedException();
                }
                return (byte)(value & 0x0000001F);
            }
        }

        public byte G5 { get { 
                uint value = 0xCD;
                switch (format) {
                    case PixelFormat.Bpp16X1R5G5B5:  value = colval>>5;      break;
                    case PixelFormat.Bpp16A1R5G5B5:  value = colval>>5;      break;
                    case PixelFormat.Bpp32X8R8G8B8:  value = colval>>11;     break;
                    case PixelFormat.Bpp32A8R8G8B8:  value = colval>>11;     break;
                    default: throw new NotImplementedException();
                }
                return (byte)(value & 0x0000001F);
            }
        }

        public byte B5 { get { 
                uint value = 0xCD;
                switch (format) {
                    case PixelFormat.Bpp16X1R5G5B5:  value = colval>>0;      break;
                    case PixelFormat.Bpp16A1R5G5B5:  value = colval>>0;      break;
                    case PixelFormat.Bpp32X8R8G8B8:  value = colval>>3;      break;
                    case PixelFormat.Bpp32A8R8G8B8:  value = colval>>3;      break;
                    default: throw new NotImplementedException();
                }
                return (byte)(value & 0x0000001F);
            }
        }

        public byte R8 { get { 
                uint value = 0xCD;
                switch (format) {
                    case PixelFormat.Bpp16X1R5G5B5:  value = colval>>10<<3;  break;
                    case PixelFormat.Bpp16A1R5G5B5:  value = colval>>10<<3;  break;
                    case PixelFormat.Bpp32X8R8G8B8:  value = colval>>16;     break;
                    case PixelFormat.Bpp32A8R8G8B8:  value = colval>>16;     break;
                    default: throw new NotImplementedException();
                }
                return (byte)(value & 0x000000FF);
            }
        }

        public byte G8 { get { 
                uint value = 0xCD;
                switch (format) {
                    case PixelFormat.Bpp16X1R5G5B5:  value = colval>>5<<3;  break;
                    case PixelFormat.Bpp16A1R5G5B5:  value = colval>>5<<3;  break;
                    case PixelFormat.Bpp32X8R8G8B8:  value = colval>>8;     break;
                    case PixelFormat.Bpp32A8R8G8B8:  value = colval>>8;     break;
                    default: throw new NotImplementedException();
                }
                return (byte)(value & 0x000000FF);
            }
        }

        public byte B8 { get { 
                uint value = 0xCD;
                switch (format) {
                    case PixelFormat.Bpp16X1R5G5B5:  value = colval>>0<<3;   break;
                    case PixelFormat.Bpp16A1R5G5B5:  value = colval>>0<<3;   break;
                    case PixelFormat.Bpp32X8R8G8B8:  value = colval>>0;      break;
                    case PixelFormat.Bpp32A8R8G8B8:  value = colval>>0;      break;
                    default: throw new NotImplementedException();
                }
                return (byte)(value & 0x000000FF);
            }
        }

        public static Color FromARGB(byte r, byte g, byte b, PixelFormat format = PixelFormat.Bpp32A8R8G8B8)
        {
            return FromARGB(0xFF, r, g, b, format);
        }

        public static Color FromARGB(byte a, byte r, byte g, byte b, PixelFormat format = PixelFormat.Bpp32A8R8G8B8)
        {
            uint  value;
            Color color;
            switch (format) {
                case PixelFormat.Bpp16A1R5G5B5: value = (uint)((r & 0x1F) << 10 | (g & 0x1F) << 5 | (b & 0x1F));
                                                if (a > 0) value |= 0xFF000000;
                                                color = (ushort)(value & 0x0000FFFF);
                                                break;
                case PixelFormat.Bpp32A8R8G8B8: value = (uint)(a << 24 | r << 16 | g << 8 | b);
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
                case PixelFormat.Bpp16A1R5G5B5: value = color.colval;
                                                break;
                case PixelFormat.Bpp32A8R8G8B8: value = (color.colval>>19<<10 | color.colval>>11<<5 | color.colval>>3<<0);
                                                if ((color.colval & 0xFF000000) != 0) value |= 0x8000;
                                                break;
                default: throw new NotImplementedException();
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
                case PixelFormat.Bpp16A1R5G5B5: value = (color.colval>>10<<19 | color.colval>>5<<11 | color.colval>>0<<3);
                                                if ((color.colval & 0x00008000) != 0) value |= 0xFF000000;
                                                break;
                case PixelFormat.Bpp32A8R8G8B8: value = color.colval;
                                                break;
                default: throw new NotImplementedException();
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
    [Obsolete("Its too shit...")]
    internal class _BitmapSurface : /*Image, !!!DAMN!!! */ IImageSurface
    {
        #region Interface Implementation
        ImageSource IImageSurface.Image     { get { return Bitmap; } }
        ImageSource ISurface.Image          { get { return Bitmap; } }
        public ImageSource  Image { get { return Bitmap; } }
        ushort      ISurface.Width          { get { return (ushort)Bitmap.Width;  } }
        ushort      ISurface.Height         { get { return (ushort)Bitmap.Height; } }

        unsafe uint* ISurface.ImageUIntPtr { get { return null; } }
        unsafe ushort* ISurface.ImageWordPtr { get { return null; } }
        unsafe byte* ISurface.ImageBytePtr { get { return null; } }
        uint ISurface.Stride { get { return 0; } }

        PixelFormat ISurface.PixelFormat { get { return PixelFormat.UnknownFormat; } }

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

        byte IImageSurface.GetHammingDistanceForAvrHash(IImageSurface surface)
        {
            return 0;
        }

        void IImageSurface.Invalidate()
        {
        }


        protected BitmapSource Bitmap;

        internal _BitmapSurface(IImageSurface surface)
        {
            if (surface is _BitmapSurface)
                Bitmap = (surface as _BitmapSurface).Bitmap;
            else
                throw new NotImplementedException();
        }

        internal _BitmapSurface(BitmapSource bitmap)
        {
            Bitmap = bitmap;
        }

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        internal _BitmapSurface(Bitmap bitmap)
        {
            IntPtr ptr = bitmap.GetHbitmap();

            Bitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            
            DeleteObject(ptr);
        }

        internal _BitmapSurface(Image image) : this(new Bitmap(image))
        {
        }

        internal _BitmapSurface(Stream stream)
        {
            Bitmap = BitmapFrame.Create(stream);
        }

        internal _BitmapSurface(byte[] bytes)
        {
            var stream = new MemoryStream(bytes);
            Bitmap = BitmapFrame.Create(stream);
            stream.Close();
        }

        internal _BitmapSurface(string filepath)
        {
            Bitmap = new BitmapImage(new Uri(filepath));
        }

        public Bitmap ToBitmap()
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(Bitmap));
                enc.Save(outStream);
                var bitmap = new System.Drawing.Bitmap(outStream);
                return bitmap;
            }
        }

        public byte[] ToArray(BitmapEncoder encoder)
        {
            var frame = BitmapFrame.Create(Bitmap);
            encoder.Frames.Add(frame);
            var stream = new MemoryStream();

            encoder.Save(stream);
            return stream.ToArray();
        }

        public byte[] ToArray()
        {
            return ToArray(new PngBitmapEncoder());
        }

        private static SHA256Managed shaM = new SHA256Managed();

        protected byte[] Hash { get; private set; }

        public byte[] GetHash()
        {
            if (Hash == null)
                Hash = ToArray();
            //return BitConverter.ToString(shaM.ComputeHash(btBitmap));
            return Hash;
        }

        public bool Equals(IImageSurface surface)
        {
            if (Equals((object)surface))
                return true;
            if (surface is BitmapSurface) {
                return (surface as BitmapSurface).GetHash().SequenceEqual(GetHash());
            } else
                throw new NotImplementedException();
        }

        //public Rect GetImgRect()
       // {
           // buf = new WPFUtil.BitmapBuffer(bmpsrc);
           // var bmpReader = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr555, null);

       // }

        //public float Relevance(IImageSurface surface)
        //{
        //    if (Compare(surface))
        //        return 1.0f;
        //}
    }



    internal unsafe class BitmapSurface : IDisposable, IImageSurface
    {
        #region ISurface Implementation
        ImageSource IImageSurface.Image     { get { return BitmapSource; } }
        ImageSource ISurface.Image          { get { return BitmapSource; } }
        public ImageSource  Image { get { return BitmapSource; } }
        ushort      ISurface.Width          { get { return (ushort)Width;  } }
        ushort      ISurface.Height         { get { return (ushort)Height; } }
        PixelFormat ISurface.PixelFormat    { get { return PixelFormat; } }

        unsafe uint* ISurface.ImageUIntPtr  { get { return ImageUIntPtr; } }
        unsafe ushort* ISurface.ImageWordPtr { get { return ImageWordPtr; } }
        unsafe byte* ISurface.ImageBytePtr  { get { return ImageBytePtr; } }
        uint         ISurface.Stride        { get { return Stride; } }

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
        
        bool ISurface.Equals(IImageSurface surface)
        {
            if (Equals((object)surface))
                return true;
            if (surface is BitmapSurface) {
                return (surface as BitmapSurface).GetHash().SequenceEqual(GetHash());
            } else
                throw new NotImplementedException();
        }
        
        #endregion

        #region IImageSurface Implementation

        IClipper GetImageRect()
        {
            short dx = -1, dy = -1;
            ushort width = 0, height = 0;

            lock (LockObject) 
            {
                var line = ImageWordPtr;
                var delta = Stride >> 1;
                for (short Y = 0; Y < Height; ++Y, line += delta) {
                    var cur = line;
                    for (short X = 0; X < Width; ++X) {
                        byte a = (byte)((cur[X] & 0xFF000000) >> 24);
                        byte r = (byte)((cur[X] & 0x00FF0000) >> 16);
                        byte g = (byte)((cur[X] & 0x0000FF00) >> 8);
                        byte b = (byte)((cur[X] & 0x000000FF) >> 0);

                        if (r >= 1 || g >= 1 || b >= 1) {
                            dx = Math.Min(dx, X);
                            dy = Math.Min(dy, Y);
                            width  = (ushort)Math.Max(width, X);
                            height = (ushort)Math.Max(height, Y);
                        }
                    }
                }
            }

            if (dx != -1 && dy != -1) {
                width -= (ushort)dx;
                height -= (ushort)dy;
                //dx += width / 2;
                //dx *= -1;
                //dy += height / 2;
                //dy *= -1;
            } else {
                dx = dy = 0;
                width = height = 0;
            }

            return new Clipper2D(dx, dy, width, height);
        }

        IPoint2D GetImageCenter()
        {
            var rect = GetImageRect();
            var dx = (rect.X1 + Width) / 2;
            var dy = (rect.Y1 + Height) / 2;
            return new Point2D(dx, dy);
        }

 

        internal ulong GetAvrHash()
        {
            // NOTE: In theory hash can be 0, so it's not very good
            if (_AvrHash > 0) 
                return _AvrHash;

            var minimg = new byte[64];
            var pixels = Width*Height;
            fixed (byte* colors = new byte[pixels])
            {
                // Creating grayscale...
                int a, r, g, b;
                uint stride = 0;
                var trg = colors;
                if (BytesPerPixel == 2) 
                    lock (LockObject) {
                        var srs = ImageWordPtr;
                        for (var i = 0; i < pixels; ++i, ++srs, ++trg) {
                            a = (*srs>>15);
                            if (a > 0) {
                                r = ((*srs>>10) & 0x1F) << 3;
                                g = ((*srs>>5) & 0x1F) << 3;
                                b = ((*srs) & 0x1F) << 3;
                            } else {
                                r = g = b = 0;
                            }
                            *trg = (byte)((r + g + b) / 3);
                            //++trg;
                            //++srs;
                        }
                        stride = Stride >> 1;
                    }
                else if (BytesPerPixel == 4) 
                    lock (LockObject) {
                        var srs = ImageWordPtr;
                        for (var i = 0; i < pixels; ++i, ++srs, ++trg) {
                            a = (*srs>>24);
                            if (a > 0) {
                                r = (*srs>>16) & 0xFF;
                                g = (*srs>>8) & 0xFF;
                                b = (*srs) & 0xFF;
                            } else {
                                r = g = b = 0;
                            }
                            *trg = (byte)((r + g + b) / 3);
                            //++trg;
                            //++srs;
                        }
                        stride = Stride >> 2;
                    }
                else
                    throw new NotImplementedException();
  

                // Resizing...
                double colavg;
                double x_ratio = Width  / 8D;
                double y_ratio = Height / 8D;
                double xy_ratio = x_ratio * y_ratio;
                double[] px1 = new[] {0D,        x_ratio, 2*x_ratio, 3*x_ratio, 4*x_ratio, 5*x_ratio, 6*x_ratio, 7*x_ratio};
                double[] px2 = new[] {x_ratio, 2*x_ratio, 3*x_ratio, 4*x_ratio, 5*x_ratio, 6*x_ratio, 7*x_ratio, 8*x_ratio};
                double[] py1 = new[] {0D,        y_ratio, 2*y_ratio, 3*y_ratio, 4*y_ratio, 5*y_ratio, 6*y_ratio, 7*y_ratio};
                double[] py2 = new[] {y_ratio, 2*y_ratio, 3*y_ratio, 4*y_ratio, 5*y_ratio, 6*y_ratio, 7*y_ratio, 8*y_ratio};
                int[] mx1 = new[] {0, (int)(px1[1]), (int)(px1[2]), (int)(px1[3]), (int)(px1[4]), (int)(px1[5]), (int)(px1[6]), (int)(px1[7])};
                int[] mx2 = new[] { (int)Math.Ceiling(px2[0]), (int)Math.Ceiling(px2[1]), (int)Math.Ceiling(px2[2]), (int)Math.Ceiling(px2[3]), 
                                    (int)Math.Ceiling(px2[4]), (int)Math.Ceiling(px2[5]), (int)Math.Ceiling(px2[6]), (int)Math.Ceiling(px2[7])};
                int[] my1 = new[] {0, (int)(py1[1]), (int)(py1[2]), (int)(py1[3]), (int)(py1[4]), (int)(py1[5]), (int)(py1[6]), (int)(py1[7])};
                int[] my2 = new[] { (int)Math.Ceiling(py2[0]), (int)Math.Ceiling(py2[1]), (int)Math.Ceiling(py2[2]), (int)Math.Ceiling(py2[3]), 
                                    (int)Math.Ceiling(py2[4]), (int)Math.Ceiling(py2[5]), (int)Math.Ceiling(py2[6]), (int)Math.Ceiling(py2[7])};

                for (int y = 0; y < 8; y++) {             
                    for (int x = 0; x < 8; x++, trg++) {
                        colavg = 0;
                        for (int srs_y = my1[y]; srs_y < my2[y]; srs_y++) {
                            trg = colors + srs_y * Width + mx1[x];
                            for (int srs_x = mx1[x]; srs_x < mx2[x]; srs_x++) {
                                colavg += (*trg++) * (Math.Min(px2[x], srs_x + 1) - Math.Max(px1[x], srs_x))
                                                   * (Math.Min(py2[y], srs_y + 1) - Math.Max(py1[y], srs_y));
                            }
                        }
                        minimg[(y<<3) + x] = (byte)(colavg / xy_ratio);
                    }
                }

                // TEST CODE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                /*
                lock (LockObject) {
                    var srs = ImageWordPtr;
                    for (int y = 0; y < Height; y++) {
                        srs = ImageWordPtr + y * Width;
                        for (int x = 0; x < Width; x++, srs++) {
                            ushort gcol = (ushort)(colors[y * Width + x] >> 3);
                            *srs = (ushort)(0x0000 | gcol | gcol << 5 | gcol << 10);
                        }
                    }
                }
                return 0;
                */

            }
 

            // Get Avarage color
            uint avgcolor = 0;
            for (var c = 0; c < 64; ++c)
                avgcolor += minimg[c];
            avgcolor /= 64;

            // Get HashSum
            ulong hashsum = 0;
            for (var c = 0; c < 64; ++c) 
                if (minimg[c] > avgcolor)
                    hashsum |= ((ulong)0x01 << c);



            // TEST CODE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            /*
            lock (LockObject) {
                var srs = ImageWordPtr;
                for (int y = 0; y < 8; y++) {
                    srs = ImageWordPtr + y * Width;
                    for (int x = 0; x < 8; x++, srs++) {
                        ushort gcol = (ushort)(minimg[(y << 3) + x] >> 3);
                        *srs = (ushort)(0x8000 | gcol | gcol << 5 | gcol << 10);
                    }
                }
            }
            */

            return _AvrHash = hashsum;
        }
        private ulong _AvrHash = 0;

        private static readonly byte[] _BitCountTable = new byte[] {
                0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,
                1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 
                1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 
                2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 
                1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 
                2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 
                2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 
                3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 4, 5, 5, 6, 5, 6, 6, 7, 5, 6, 6, 7, 6, 7, 7, 8 };

        byte IImageSurface.GetHammingDistanceForAvrHash(IImageSurface surface)
        {
            if (surface == null) {
            //    GetAvrHash();
            //    (this as IImageSurface).Invalidate();

                //GetAvrHash();
                return 0xFF;
            }

            byte result;
            ulong avrhash = 0;
            if (surface is BitmapSurface)
                avrhash = (surface as BitmapSurface).GetAvrHash();
            else
                throw new NotImplementedException();

            avrhash ^= GetAvrHash();

            result  = _BitCountTable[avrhash & 0xFF];
            result += _BitCountTable[(avrhash >>  8) & 0xFF];
            result += _BitCountTable[(avrhash >> 16) & 0xFF];
            result += _BitCountTable[(avrhash >> 24) & 0xFF];
            result += _BitCountTable[(avrhash >> 32) & 0xFF];
            result += _BitCountTable[(avrhash >> 40) & 0xFF];
            result += _BitCountTable[(avrhash >> 48) & 0xFF];
            result += _BitCountTable[(avrhash >> 56) & 0xFF];
            return result;

            // avrhash -=  (avrhash>>1) & 0x55555555UL;
            // avrhash = ((avrhash >> 2) & 0x33333333UL) + (avrhash & 0x33333333UL);
            // avrhash = ((avrhash >> 4) + avrhash) & 0x0f0f0f0fUL;
            // avrhash *= 0x01010101UL;
            // return avrhash >> 24;
        }

        /// <summary>
        /// Invalidates the bitmap causing a redraw GUI
        /// </summary>
        void IImageSurface.Invalidate()
        {
            _AvrHash = 0;

            //lock (LockObject) {
            BitmapSource.Invalidate();
            //}
        }
        #endregion

        #region ISurface BitBlt methods Implementation

        public unsafe void BitBlt(BitmapSurface destinationBitmap, int top, int left, int height, int width)
        {
            lock (LockObject)
            {
                // Find smallest array
                //int size = (int)Math.Min(ByteLength, destinationBitmap.ByteLength) / destinationBitmap.BytesPerPixel;
                // Copy memory
                //CopyMemory(ImageData, destinationBitmap.ImageData, size);
                //Marshal.Copy(src, destinationBitmap.ImageData, 0, size);

                // First copy int64 as far as we can (faster than copying single bytes)
                Int64* src64 = (Int64*)ImageData;
                Int64* dst64 = (Int64*)destinationBitmap.ImageData;
                byte* src8 = (byte*)ImageData;
                byte* dst8 = (byte*)destinationBitmap.ImageData;

                int maxWidth = Math.Min(Math.Min(Width - left, destinationBitmap.Width - left), width + top);
                int maxHeight = Math.Min(Math.Min(Height - top, destinationBitmap.Height - top), height + top);

                for (int x = top; x < maxHeight; x++)
                {
                    for (int y = left; y < maxWidth; y++)
                    {
                        int srcp = (x * Width) + y;
                        int dstp = (x * destinationBitmap.Width) + y;
                        dst8[dstp] = src8[srcp];
                    }
                }

            }

        }
        public unsafe void BitBlt(BitmapSurface destinationBitmap)
        {
            lock (LockObject)
            {
                // Find smallest array
                //int size = (int)Math.Min(ByteLength, destinationBitmap.ByteLength) / destinationBitmap.BytesPerPixel;
                // Copy memory
                //CopyMemory(ImageData, destinationBitmap.ImageData, size);
                //Marshal.Copy(src, destinationBitmap.ImageData, 0, size);

                // First copy int64 as far as we can (faster than copying single bytes)
                int copied = 0;
                Int64* src64 = (Int64*)ImageData;
                Int64* dst64 = (Int64*)destinationBitmap.ImageData;

                int srcHeight = Height * Width;
                int dstHeight = destinationBitmap.Height * destinationBitmap.Width;
                int maxLen = Math.Min(srcHeight, dstHeight);

                int copyLength = maxLen - 8;
                int i = 0;
                while (copied < copyLength)
                {
                    dst64[i] = src64[i];
                    i++;
                    copied += 8;
                }

                // Then copy single bytes until end of data
                byte* src8 = (byte*)ImageData;
                byte* dst8 = (byte*)destinationBitmap.ImageData;

                i *= 8;
                while (copied < ByteLength)
                {
                    dst8[i] = src8[i];
                    i++;
                    copied++;
                }

            }
        }

        #endregion


        // some ideas/code borowed from CL NUI sample CLNUIImage.cs
        [DllImport("kernel32.dll", SetLastError = true)] //, EntryPoint = "RtlCopyMemory")]
        static extern void CopyMemory(IntPtr destination, IntPtr source, uint length);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateFileMapping(IntPtr hFile, IntPtr lpFileMappingAttributes, uint flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, uint dwNumberOfBytesToMap);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool UnmapViewOfFile(IntPtr hMap);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hHandle);

        //[DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        //private static extern void CopyMemory(IntPtr Destination, IntPtr Source, uint Length);

        private IntPtr          _section = IntPtr.Zero;
        public IntPtr           ImageData    { get; private set; }
        public InteropBitmap    BitmapSource { get; private set; }
        public int              BytesPerPixel{ get; private set; }
        public uint             ByteLength   { get; private set; }
        public ushort           Height       { get; private set; }
        public ushort           Width        { get; private set; }
        public PixelFormat      PixelFormat  { get; private set; }
        public readonly object  LockObject = new object();

        public unsafe uint*     ImageUIntPtr    { get; private set; }
        public unsafe ushort*   ImageWordPtr    { get; private set; }
        public unsafe byte*     ImageBytePtr    { get; private set; }
        public uint             Stride          { get; private set; }

        [Obsolete("Discarded - not fast enough so should not be used")]
        [System.Runtime.CompilerServices.IndexerName("TheItem")]
        public uint this[int x, int y]   // Indexer declaration
        {
            get {
                int p = (int) ((x * BitmapSource.Width * BytesPerPixel) + (y * BytesPerPixel));
                return ImageUIntPtr[p];
            }
        }

        #region Creating and Disposing

        private BitmapSurface()
        {
        }

        internal BitmapSurface(ushort width, ushort height, PixelFormat pixelFormat)
        {
            PixelFormat = pixelFormat;
            BytesPerPixel = ((byte)pixelFormat & (byte)PixelFormat.BppFormatMask) >> (byte)PixelFormat.BppFormatOffs;
            Height = height;
            Width  = width;

            Stride = (uint)Width * (uint)BytesPerPixel;
            ByteLength = Stride * (uint)Height;

            // create memory section and map
            _section = CreateFileMapping(new IntPtr(-1), IntPtr.Zero, 0x04, 0, ByteLength, null);
            ImageData = MapViewOfFile(_section, 0xF001F, 0, 0, ByteLength);

            var mediaPixelFormat = BytesPerPixel == 2 ? PixelFormats.Bgr555 : BytesPerPixel == 4 ? PixelFormats.Bgr32 : PixelFormats.Default;
            BitmapSource = Imaging.CreateBitmapSourceFromMemorySection(_section, Width, Height, mediaPixelFormat, Width * BytesPerPixel, 0) as InteropBitmap;
            ImageUIntPtr = (uint*)ImageData;
            ImageWordPtr = (ushort*)ImageData;
            ImageBytePtr = (byte*)ImageData;
        }

        private unsafe void CreateFromBitmap(Bitmap sourceBitmap)
        {
            //throw new NotImplementedException("Untested!");
            lock (LockObject)
            {
                BitmapData bData = sourceBitmap.LockBits(new Rectangle(new System.Drawing.Point(), sourceBitmap.Size), ImageLockMode.ReadOnly, sourceBitmap.PixelFormat);
                var pbb = bData.Stride / bData.Width;
                if (pbb == BytesPerPixel) 
                { 
                    // First copy int64 as far as we can (faster than copying single bytes)
                    int copied = 0;
                    Int64* src64 = (Int64*)bData.Scan0.ToPointer();
                    Int64* dst64 = (Int64*)ImageData;

                    int srcHeight = sourceBitmap.Height * sourceBitmap.Width;
                    int dstHeight = Height * Width;
                    int maxLen = Math.Min(srcHeight, dstHeight);

                    int copyLength = maxLen - 8;
                    int i = 0;
                    while (copied < copyLength) {
                        dst64[i] = src64[i];
                        i++;
                        copied += 8;
                    }

                    // Then copy single bytes until end of data
                    byte* src8 = (byte*)bData.Scan0.ToPointer();
                    byte* dst8 = (byte*)ImageData;

                    i *= 8;
                    while (copied < ByteLength) {
                        dst8[i] = src8[i];
                        i++;
                        copied++;
                    }
                } else if (pbb == 4 && BytesPerPixel == 2)
                {
                    var trg = ImageWordPtr;
                    for (var y = 0; y < Height; ++y) {
                        var srs = (uint*)((byte*)bData.Scan0.ToPointer() + y * bData.Stride);
                        for (var x = 0; x < Width; ++x, ++srs, ++trg) {
                            if (*srs <= 0x00FFFFFF) {
                                *trg = 0;
                                continue;
                            }
                            *trg = (ushort)(((*srs & 0x00F80000) >> 9) | ((*srs & 0x0000F800) >> 6) | ((*srs & 0x000000F8) >> 3));
                            if (*trg > 0x0000)
                                *trg |= 0x8000;
                        }
                    }
                }


            }
        }

        private unsafe void CreateFromPBytes(ushort sourceWidth, ushort sourceHeight, byte* sourceBytes)
        {
            lock (LockObject) {
                var maxLen = Math.Min((uint)sourceHeight * (uint)sourceWidth, ByteLength);
                CopyMemory(ImageData, (IntPtr) sourceBytes, maxLen);
            }
        }

        internal BitmapSurface(ISurface surface) : this(surface.Width, surface.Height, surface.PixelFormat)
        {
            if (surface is BitmapSurface)
                CreateFromPBytes(Width, Height, (surface as BitmapSurface).ImageBytePtr);
            else
                throw new NotImplementedException();
        }

        internal BitmapSurface(Bitmap bitmap, PixelFormat pixelFormat = PixelFormat.Bpp16A1R5G5B5) : this((ushort)bitmap.Width, (ushort)bitmap.Height, pixelFormat)
        {
            CreateFromBitmap(bitmap);
        }

        internal BitmapSurface(Image image) : this(new Bitmap(image))
        {
        }

        internal BitmapSurface(Stream stream) : this(new Bitmap(stream))
        {
        }

        internal BitmapSurface(string filepath) : this(new Bitmap(filepath))
        {
        }

        internal BitmapSurface(BitmapSource bitmap, PixelFormat pixelFormat = PixelFormat.Bpp16A1R5G5B5) : this((ushort)bitmap.Width, (ushort)bitmap.Height, pixelFormat)
        {
            using (MemoryStream outStream = new MemoryStream()) {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmap));
                enc.Save(outStream);
                var _bitmap = new Bitmap(outStream);
                CreateFromBitmap(_bitmap);
            }
        }

        internal BitmapSurface(ushort width, ushort height, byte* bytes, PixelFormat pixelFormat = PixelFormat.Bpp16A1R5G5B5) : this(width, height, pixelFormat)
        {
            CreateFromPBytes(width, height, bytes);
        }

        internal BitmapSurface(ushort width, ushort height, byte[] bytes, PixelFormat pixelFormat = PixelFormat.Bpp16A1R5G5B5) : this(width, height, pixelFormat)
        {
            fixed(byte* _bytes = bytes) {
                CreateFromPBytes(width, height, _bytes);
            }
        }



        ~BitmapSurface()
        {
            Dispose();
        }
        private bool _disposed = false;

        public void Dispose()
        {
            _disposed = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            _disposed = true;
            lock (LockObject)
            {
                if (disposing)
                {
                    // free managed resources
                }
                // free native resources if there are any.
                if (ImageData != IntPtr.Zero) {
                    UnmapViewOfFile(ImageData);
                    ImageData = IntPtr.Zero;
                }
                if (_section != IntPtr.Zero) {
                    CloseHandle(_section);
                    _section = IntPtr.Zero;
                }
            }
        }

        #endregion

        
        


        

       



        public BitmapSurface Clone()
        {
            lock (LockObject)
            {
                // Create new bitmap
                var bitmap = new BitmapSurface(Width, Height, PixelFormat);
                // Copy data into new bitmap
                BitBlt(bitmap);

                return bitmap;
            }
        }
        /*
        public static Bitmap CreateFromFile(string file)
        {
            using (System.Drawing.Image image = System.Drawing.Image.FromFile(file))
            {
                using (System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(image))
                {

                    System.Drawing.Imaging.PixelFormat format = bmp.PixelFormat;
                    Bitmap ret = new Bitmap(image.Width, image.Height, PixelFormats.Bgr32);

                    BitmapData data = bmp.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                                                   ImageLockMode.ReadOnly,
                                                   format);
                    int stride = data.Stride;
                    int offset = stride - image.Width * ret.BytesPerPixel;
                    unsafe
                    {
                        byte* src = (byte*)data.Scan0.ToPointer();
                        byte* dst = (byte*)ret.ImageData;

                        int mp = image.Height * image.Width * ret.BytesPerPixel;
                        for (int p = 0; p < mp; p++)
                        {
                            dst[p] = src[p];
                        }
                    }
                    return ret;

                }
            }
        }

        public static Bitmap CreateFromStream(Stream file)
        {
            using (System.Drawing.Image image = System.Drawing.Image.FromStream(file))
            {
                using (System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(image))
                {

                    System.Drawing.Imaging.PixelFormat format = bmp.PixelFormat;
                    Bitmap ret = new Bitmap(image.Width, image.Height, PixelFormats.Bgr32);
                    BitmapData data = bmp.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                                                   ImageLockMode.ReadOnly,
                                                   format);
                    int stride = data.Stride;
                    int offset = stride - image.Width * ret.BytesPerPixel;
                    unsafe
                    {
                        byte* src = (byte*)data.Scan0.ToPointer();
                        byte* dst = (byte*)ret.ImageData;

                        int mp = image.Height * image.Width * ret.BytesPerPixel;
                        for (int p = 0; p < mp; p++)
                        {
                            dst[p] = src[p];
                        }
                    }
                    return ret;
                }
            }
        }
        */

        private static byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            using (MemoryStream ms = new MemoryStream()) {
                imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                return ms.ToArray();
            }
        }

        public void SavePNG(string filename)
        {
            lock (LockObject) {
                using (var fileStream = new FileStream(filename, FileMode.Create)) {
                    SavePNG(fileStream);
                }
            }
        }

        public void SavePNG(Stream stream)
        {
            lock (LockObject) {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(BitmapSource));
                encoder.Save(stream);
            }
        }

        public System.Drawing.Bitmap CloneToBitmap()
        {
            lock (LockObject) {
                return System.Drawing.Image.FromHbitmap(ImageData);
            }
        }

        public void Blur(int xBlur, int yBlur)
        {
            BitmapSurface srcImage = Clone();
            uint* srcIntPtr = srcImage.ImageUIntPtr;
            uint* dstIntPtr = ImageUIntPtr;

            int xfrom = (xBlur / 2) * -1;
            int xto = xfrom + xBlur;
            int yfrom = (yBlur / 2) * -1;
            int yto = yfrom + yBlur;

            for (int x = 0; x < Height; x++) {
                for (int y = 0; y < Width; y++) {
                    // Look around
                    long avg = 0;
                    int count = 0;
                    for (int xa = xfrom; xa < xto; xa++) {
                        for (int ya = yfrom; ya < yto; ya++) {
                            if (x + xa >= 0 && x + xa < Height && y + ya >= 0 && y + ya < Width) {
                                int p = ((x + xa) * Width) + (y + ya);
                                avg += srcIntPtr[p];
                                count++;
                            }
                        }
                    }
                    int pd = (x  * Width) + y ;
                    dstIntPtr[pd] = (uint)(avg / count);
                }
            }

            srcImage.Dispose();
        }

        public void SwapColor32(uint oldColor, uint newColor)
        {         
            for (uint x = 0; x < Height; x++) {
                for (uint y = 0; y < Width; y++) {
                    uint p = (x * Width) + y;
                    if (ImageUIntPtr[p] == oldColor)
                        ImageUIntPtr[p] = newColor;
                }
            }
        }
    


        /// <summary>
        /// Get array of image data, slow...
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            var ret = new byte[ByteLength];
            lock (LockObject)
                Marshal.Copy(ImageData, ret, 0, ret.Length);
            return ret;
        }

        public byte[] GetHash()
        {
            // TODO: remove Array cast
            if (_hash == null)
                _hash = _shaM.ComputeHash(ToArray());
            //return BitConverter.ToString(Hash);
            return _hash;
        }
        private byte[] _hash = null;
        private static SHA256Managed _shaM = new SHA256Managed();
    }




    //internal class DirectDrawSurface : ISurface
    //{
    // Yea, we need it later   
    //}
}
