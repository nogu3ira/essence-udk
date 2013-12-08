﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using EssenceUDK.Platform.DataTypes;

namespace EssenceUDK.Platform.Factories
{
    public unsafe class BaseSysFactory
    {
        internal BaseSysFactory()
        {
        }

        //internal static BaseSysFactory Instance { get { return _Instance ?? (_Instance = new BaseSysFactory()); } }
        //private static BaseSysFactory _Instance;

        public ISurface CreateSurface(IImageSurface surface)
        {
            return new BitmapSurface(surface);
        }

        public ISurface CreateSurface(BitmapSource bitmap)
        {
            return new BitmapSurface(bitmap);
        }

        public ISurface CreateSurface(Bitmap bitmap)
        {
            return new BitmapSurface(bitmap);
        }

        public ISurface CreateSurface(Image image)
        {
            return new BitmapSurface(image);
        }

        public ISurface CreateSurface(Stream stream)
        {
            return new BitmapSurface(stream);
        }

        public ISurface CreateSurface(ushort width, ushort height, PixelFormat pixelFormat = PixelFormat.Bpp16A1R5G5B5)
        {
            return new BitmapSurface(width, height, pixelFormat);
        }

        public ISurface CreateSurface(ushort width, ushort height, byte[] bytes, PixelFormat pixelFormat = PixelFormat.Bpp16A1R5G5B5)
        {
            return new BitmapSurface(width, height, bytes, pixelFormat);
        }

        public ISurface CreateSurface(ushort width, ushort height, byte* bytes, PixelFormat pixelFormat = PixelFormat.Bpp16A1R5G5B5)
        {
            return new BitmapSurface(width, height, bytes, pixelFormat);
        }

        public ISurface CreateSurface(string filepath)
        {
            return new BitmapSurface(filepath);
        }
    
    }
}
