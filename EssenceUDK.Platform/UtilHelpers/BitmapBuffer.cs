﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;


namespace WPFInterop
//namespace EssenceUDK.Platform.UtilHelpers
{
    public class BitmapBuffer
    {
        private static readonly MethodInfo _lockmethod;
        private static readonly MethodInfo _getStrideMethod;
        private static readonly MethodInfo _getBufferMethod;

        private readonly BitmapSource _bitmapImage = null;

        private object _wicImageHandle = null;

        private object _wicImageLock = null;

        private uint _bufferSize = 0;

        private IntPtr _bufferPointer = IntPtr.Zero;

        private uint _stride = 0;

        private int _width;

        private int _height;

        /// <summary>
        /// Static constructor
        /// </summary>
        static BitmapBuffer()
        {
            /* Get the internal nested class that holds some of 
             * the native functions for WIC */
            Type wicBitmapNativeMethodsClass = Type.GetType("MS.Win32.PresentationCore.UnsafeNativeMethods+WICBitmap, PresentationCore, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            /* Get the methods of all the static methods in the class */
            MethodInfo[] info = wicBitmapNativeMethodsClass.GetMethods(BindingFlags.Static | BindingFlags.NonPublic);

            /* This method looks good */
            _lockmethod = info[0];

            /* Get the internal nested class that holds some of the
             * other native functions for WIC */
            Type wicLockMethodsClass = Type.GetType("MS.Win32.PresentationCore.UnsafeNativeMethods+WICBitmapLock, PresentationCore, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            /* Get all the native methods into our array */
            MethodInfo[] lockMethods = wicLockMethodsClass.GetMethods(BindingFlags.Static | BindingFlags.NonPublic);

            /* Our method to get the stride value of the image */
            _getStrideMethod = lockMethods[0];

            /* This function will return to us
             * the buffer pointer and size */
            _getBufferMethod = lockMethods[1];
        }

        public BitmapBuffer(BitmapSource Image)
        {
            /* Keep reference to our bitmap image */
            _bitmapImage = Image;

            /* Get around the STA deal */
            _bitmapImage.Dispatcher.Invoke(
                DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate
                        {
                            /* Cache our width and height */
                            _width = _bitmapImage.PixelWidth;
                            _height = _bitmapImage.PixelHeight;
                            return null;
                        },
                null);

            /* Retrieve and store our WIC handle to the bitmap */
            SetWICHandle();

            /* Set the buffer pointer */
            SetBufferInfo();
        }

        /// <summary>
        /// The pointer to the BitmapImage's native buffer
        /// </summary>
        public IntPtr BufferPointer
        {
            get
            {
                /* Set the buffer pointer */
                SetBufferInfo();

                return _bufferPointer;
            }
        }

        /// <summary>
        /// The size of BitmapImage's native buffer
        /// </summary>
        public uint BufferSize
        {
            get { return _bufferSize; }
        }

        /// <summary>
        /// The stride of BitmapImage's native buffer
        /// </summary>
        public uint Stride
        {
            get { return _stride; }
        }

        private void SetBufferInfo()
        {
            int hr = 0;

            /* The rectangle of the buffer we are
             * going to request */
            Int32Rect rect = new Int32Rect();

            rect.Width = _width;
            rect.Height = _height;

            /* Populate the arguments to pass to the function */
            object[] args = new object[] { _wicImageHandle, rect, 2, _wicImageHandle };

            /*Execute our static Lock() method */
            hr = (int)_lockmethod.Invoke(null, args);

            /* argument[3] is our "out" pointer to the lock handle
             * it is set by our last method invoke call */
            _wicImageLock = args[3];

            /* Fill in our arguments */
            args = new object[] { _wicImageLock, _stride };

            /* Execute the stride method */
            _getStrideMethod.Invoke(null, args);

            /* Grab out or byref value for the stride */
            _stride = (uint)args[1];

            /* Fill in our arguments */
            args = new object[] { _wicImageLock, _bufferSize, _bufferPointer };

            /* Run our method */
            hr = (int)_getBufferMethod.Invoke(null, args);

            _bufferSize = (uint)args[1];
            _bufferPointer = (IntPtr)args[2];

            DisposeLockHandle();
        }

        private void DisposeLockHandle()
        {
            MethodInfo close = _wicImageLock.GetType().GetMethod("Close");
            MethodInfo dispose = _wicImageLock.GetType().GetMethod("Dispose");

            close.Invoke(_wicImageLock, null);
            dispose.Invoke(_wicImageLock, null);
        }

        private void SetWICHandle()
        {
            /* Get the type of bitmap image */
            Type bmpType = typeof(BitmapSource);

            /* Use reflection to get the private property WicSourceHandle */
            FieldInfo fInfo = bmpType.GetField("_wicSource",
                                               BindingFlags.NonPublic
                                             | BindingFlags.Instance);

            /* Retrieve the WIC handle from our BitmapImage instance */
            _wicImageHandle = fInfo.GetValue(_bitmapImage);
        }

    }
}