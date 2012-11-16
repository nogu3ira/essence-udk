using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using EssenceUDK.MapMaker.Elements.ColorArea;
using EssenceUDK.MapMaker.Elements.ColorArea.ColorArea;

namespace EssenceUDK.MapMaker.MapMaking
{
    public class BitmapReader 
    {
        //public Color[] BitmapColors { get; private set; }

        public BitmapReader(string bitmapLocation, Boolean altitude)
        {
            try
            {
                //Cache(bitmapLocation, altitude);
            }
            catch(Exception)
            {
            }
        }

        ///// <summary>
        ///// Just cache the bmp in ram
        ///// </summary>
        ///// <param name="location"></param>
        ///// <param name="altitude"> </param>
        //private void Cache(string location, bool altitude)
        //{
        //    using (var bitmap = new Bitmap(location))
        //    {
               
        //            BitmapColors = new Color[bitmap.Width*bitmap.Height];
        //            BitmapColors.Initialize();
                
        //             Lock the bitmap's bits.  
        //            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                    
        //            lock the bitmap bits
        //            BitmapData bmpData;
                    
        //            bmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

        //             Get the address of the first line.
        //            var ptr = bmpData.Scan0;

        //             Declare an array to hold the bytes of the bitmap.
        //            var bytes = bmpData.Stride*bitmap.Height;
        //            var rgbValues = new byte[bytes];


        //             Copy the RGB values into the array.
        //            Marshal.Copy(ptr, rgbValues, 0, bytes);

        //            var stride = bmpData.Stride;
                    
        //            for (var coulmn = bmpData.Height - 1; coulmn >= 0; coulmn--)
        //            {
        //                for (var row = 0; row < bmpData.Width; row++)
        //                {
        //                    BitmapColors[(coulmn * (bmpData.Width)) + row] = Color.FromRgb((rgbValues[(coulmn * stride) + (row * 3) + 2]),
        //                                                               rgbValues[(coulmn*stride) + (row*3) + 1],
        //                                                               rgbValues[(coulmn*stride) + (row*3)]);
        //                }
        //            }
        //    }
        //}


        /// <summary>
        /// 
        /// </summary>
        /// <param name="collectionAreaColor"></param>
        /// <param name="location"></param>
        /// <exception cref="ExecutionEngineException"></exception>
        /// <returns></returns>
        public static AreaColor[] ProduceMap(CollectionAreaColor collectionAreaColor, string location)
        {
            AreaColor[] areaColors = null;
            using (var bitmap = new Bitmap(location))
            {


                areaColors = new AreaColor[bitmap.Width * bitmap.Height];
                
                // Lock the bitmap's bits.  
                var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

                //lock the bitmap bits
                BitmapData bmpData;

                bmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                // Get the address of the first line.
                var ptr = bmpData.Scan0;

                // Declare an array to hold the bytes of the bitmap.
                var bytes = bmpData.Stride * bitmap.Height;
                var rgbValues = new byte[bytes];

                var list = new List<String>();
                // Copy the RGB values into the array.
                Marshal.Copy(ptr, rgbValues, 0, bytes);

                var stride = bmpData.Stride;

                for (var coulmn = bmpData.Height - 1; coulmn >= 0; coulmn--)
                {
                    for (var row = 0; row < bmpData.Width; row++)
                    {
                        areaColors[(coulmn*(bmpData.Width)) + row] =
                            collectionAreaColor.FindByByteArray(new[]
                                                                    {
                                                                        rgbValues[(coulmn*stride) + (row*3) + 2],
                                                                        rgbValues[(coulmn*stride) + (row*3) + 1],
                                                                        rgbValues[(coulmn*stride) + (row*3)]
                                                                    });

                        if (areaColors[(coulmn*(bmpData.Width)) + row] != null) continue;

                        var str = "Color =" + 
                            System.Windows.Media.Color.FromRgb(
                                rgbValues[(coulmn * stride) + (row * 3) + 2],
                                rgbValues[(coulmn * stride) + (row * 3) + 1],
                                rgbValues[(coulmn * stride) + (row * 3)]) 
                            + " not found.";
                            
                        if(!list.Contains(str))
                        {
                            list.Add(str);
                        }
                    }
                }

                if(list.Count>0)
                {
                    var message = list.Aggregate("", (current, str) => current + (str + '\n'));
                    throw new Exception(message);
                }
            }

            return areaColors;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public static sbyte[] Altitude(string location)
        {
            sbyte[] array;
            using (var bitmap = new Bitmap(location))
            {

                array = new sbyte[bitmap.Width * bitmap.Height];
                array.Initialize();

                // Lock the bitmap's bits.  
                var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

                //lock the bitmap bits
                BitmapData bmpData;

                bmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                // Get the address of the first line.
                var ptr = bmpData.Scan0;

                // Declare an array to hold the bytes of the bitmap.
                var bytes = bmpData.Stride * bitmap.Height;
                var rgbValues = new byte[bytes];


                // Copy the RGB values into the array.
                Marshal.Copy(ptr, rgbValues, 0, bytes);

                var stride = bmpData.Stride;

                for (var coulmn = bmpData.Height - 1; coulmn >= 0; coulmn--)
                {
                    for (var row = 0; row < bmpData.Width; row++)
                    {
                        var red = rgbValues[(coulmn*stride) + (row*3) + 2];
                        var green = rgbValues[(coulmn*stride) + (row*3) + 1];
                        var blue = rgbValues[(coulmn*stride) + (row*3)];

                        if(red==green && red==blue)
                        {
                            var partialresult = red - 128;
                            if(partialresult >sbyte.MaxValue)
                                partialresult = sbyte.MaxValue;
                            if(partialresult<sbyte.MinValue)
                                partialresult = sbyte.MinValue;

                            array[(coulmn*(bmpData.Width)) + row] = (sbyte) partialresult;
                            continue;
                        }

                        if(red == blue && green == 0)
                        {
                            var partialresult = red - 128;
                            if (partialresult > sbyte.MaxValue)
                                partialresult = sbyte.MaxValue;
                            if (partialresult < sbyte.MinValue)
                                partialresult = sbyte.MinValue;

                            array[(coulmn * (bmpData.Width)) + row] = (sbyte)partialresult;
                            continue;
                        }

                        if(green>0 && blue == 0 && red == 0)
                        {
                            var partialresult = green - 128;
                            if (partialresult > sbyte.MaxValue)
                                partialresult = sbyte.MaxValue;
                            if (partialresult < sbyte.MinValue)
                                partialresult = sbyte.MinValue;

                            array[(coulmn * (bmpData.Width)) + row] = (sbyte)partialresult;
                            continue;
                        }

                        if(blue >0 && red == 0 && green ==0)
                        {
                            var partialresult = blue - 128;
                            if (partialresult > sbyte.MaxValue)
                                partialresult = sbyte.MaxValue;
                            if (partialresult < sbyte.MinValue)
                                partialresult = sbyte.MinValue;

                            array[(coulmn * (bmpData.Width)) + row] = (sbyte)partialresult;
                            continue;
                        }

                        array[(coulmn*(bmpData.Width)) + row] = 0;


                    }

                }
                rgbValues = null;

            }
            return array;
        }


    }
}
