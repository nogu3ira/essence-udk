using System;
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


        protected WriteableBitmap Bitmap;

        internal BitmapSurface(WriteableBitmap bitmap)
        {
            Bitmap = bitmap;
        }

    }

    //internal class DirectDrawSurface : ISurface
    //{
    // Yea, we need it later   
    //}
}
