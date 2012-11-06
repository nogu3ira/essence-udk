﻿﻿﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
﻿using Microsoft.Win32;

namespace EssenceUDK.Platform.UtilHelpers
{
    public static class Utils
    {
        #region Arrays and Structures Helpers

        public static bool ArrayIdentical<T>(T[] a1, T[] a2) where T : struct
        {
            if (a1.Length != a2.Length)
                return false;

            for (int i = 0; i < a1.Length; i++)
                if (!a1[i].Equals(a2[i]))
                    return false;

            return true;
        }

        public unsafe static bool ArrayIdentical(byte* a1, byte* a2, int count, bool nullterminate = true)
        {
            for (int c = 0; c < count; ++c)
            {
                if ((*a1 == 0 && *a2 != 0) || (*a1 != 0 && *a2 == 0))
                    return false;
                if (nullterminate && *a1 == 0 && *a2 == 0)
                    break;
                if (*a1++ != *a2++)
                    return false;
            }

            return true;
        }

        public static T[] BuffToStruct<T>(byte[] arr, int offset = 0, int count = 1) where T : struct
        {
            int sizeOfT = Marshal.SizeOf(typeof(T));
            GCHandle gch = GCHandle.Alloc(arr, GCHandleType.Pinned);
            IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(arr, offset);
            T[] ret = new T[count];
            while (count-- > 0)
                ret[count] = (T)Marshal.PtrToStructure(ptr + sizeOfT * count, typeof(T));
            gch.Free();
            return ret;
        }

        public static byte[] StructToBuff<T>(T value) where T : struct
        {
            byte[] arr = new byte[Marshal.SizeOf(value)];
            GCHandle gch = GCHandle.Alloc(arr, GCHandleType.Pinned);
            IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(arr, 0);
            Marshal.StructureToPtr(value, ptr, true);
            gch.Free();
            return arr;
        }

        public static byte[] StructToBuff<T>(T[] value) where T : struct
        {
            int sizeOfT = Marshal.SizeOf(typeof(T));
            byte[] arr = new byte[sizeOfT * value.Length];
            GCHandle gch = GCHandle.Alloc(arr, GCHandleType.Pinned);
            IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(arr, 0);

            int count = 0;
            while (++count < value.Length)
                Marshal.StructureToPtr(value[count], ptr + sizeOfT * count, true);
            gch.Free();
            return arr;
        }

        /// <summary>
        /// Writes a part of an array to a file stream as quickly as possible,
        /// without making any additional copies of the data.
        /// </summary>
        /// <typeparam name="T">The type of the array elements.</typeparam>
        /// <param name="fs">The file stream to which to write.</param>
        /// <param name="array">The array containing the data to write.</param>
        /// <param name="offset">The offset of the start of the data in the array to write.</param>
        /// <param name="count">The number of array elements to write.</param>
        /// <exception cref="IOException">Thrown on error. See inner exception for <see cref="Win32Exception"/></exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Runtime.InteropServices.SafeHandle.DangerousGetHandle")]
        public static void ArrayWrite<T>(FileStream fs, T[] array, int offset, int count) where T : struct
        {
            int sizeOfT = Marshal.SizeOf(typeof(T));
            GCHandle gcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);

            try {
                uint bytesWritten;
                uint bytesToWrite = (uint)(count * sizeOfT);

                if (!WriteFile(fs.SafeFileHandle.DangerousGetHandle(), 
                    new IntPtr(gcHandle.AddrOfPinnedObject().ToInt64() + (offset * sizeOfT)), bytesToWrite, out bytesWritten, IntPtr.Zero)) {
                    throw new IOException("Unable to write file.", new Win32Exception(Marshal.GetLastWin32Error()));
                }
                Debug.Assert(bytesWritten == bytesToWrite);
            } finally {
                gcHandle.Free();
            }
        }

        /// <summary>
        /// Reads array data from a file stream as quickly as possible,
        /// without making any additional copies of the data.
        /// </summary>
        /// <typeparam name="T">The type of the array elements.</typeparam>
        /// <param name="fs">The file stream from which to read.</param>
        /// <param name="count">The number of elements to read.</param>
        /// <returns>
        /// The array of elements that was read. This may be less than the number that was
        /// requested if the end of the file was reached. It may even be empty.
        /// NOTE: There may still be data left in the file, even if not all the requested
        /// elements were returned - this happens if the number of bytes remaining in the
        /// file is less than the size of the array elements.
        /// </returns>
        /// <exception cref="IOException">Thrown on error. See inner exception for <see cref="Win32Exception"/></exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Runtime.InteropServices.SafeHandle.DangerousGetHandle")]
        public static T[] ArrayRead<T>(FileStream fs, int count) where T : struct
        {
            int sizeOfT = Marshal.SizeOf(typeof(T));

            long bytesRemaining = fs.Length - fs.Position;
            long wantedBytes = count * sizeOfT;
            long bytesAvailable = Math.Min(bytesRemaining, wantedBytes);
            long availableValues = bytesAvailable / sizeOfT;
            long bytesToRead = (availableValues * sizeOfT);

            if ((bytesRemaining < wantedBytes) && ((bytesRemaining - bytesToRead) > 0)) {
                Debug.WriteLine("Requested data exceeds available data and partial data remains in the file.", "Dmr.Common.IO.Arrays.FastRead(fs,count)");
            }

            T[] result = new T[availableValues];
            GCHandle gcHandle = GCHandle.Alloc(result, GCHandleType.Pinned);

            try {
                uint bytesRead = 0;
                if(!ReadFile(fs.SafeFileHandle.DangerousGetHandle(),
                    gcHandle.AddrOfPinnedObject(), (uint)bytesToRead, out bytesRead, IntPtr.Zero)) {
                    throw new IOException("Unable to read file.", new Win32Exception(Marshal.GetLastWin32Error()));
                }
                Debug.Assert(bytesRead == bytesToRead);
            } finally {
                gcHandle.Free();
            }

            return result;
        }

        /// <summary>
        /// Reads array data from a file stream as quickly as possible,
        /// without making any additional copies of the data.
        /// </summary>
        /// <typeparam name="T">The type of the array elements.</typeparam>
        /// <param name="fs">The file stream from which to read.</param>
        /// <param name="array">The array into which to put the read elements.</param>
        /// <param name="offset">The starting offset in the array to put the read elements.</param>
        /// <param name="count">The number of elements to read into the array.</param>
        /// <returns>
        /// The number of elements that were read. This may be less than the number that was
        /// requested if the end of the file was reached. It may even be zero.
        /// NOTE: There may still be data left in the file, even if not all the requested
        /// elements were returned - this happens if the number of bytes remaining in the
        /// file is less than the size of the array elements.
        /// </returns>
        /// <exception cref="IOException">Thrown on error. See inner exception for <see cref="Win32Exception"/></exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Runtime.InteropServices.SafeHandle.DangerousGetHandle")]
        public static int FastRead<T>(FileStream fs, T[] array, int offset, int count) where T : struct
        {
            int sizeOfT = Marshal.SizeOf(typeof(T));

            long bytesRemaining = fs.Length - fs.Position;
            long wantedBytes = count * sizeOfT;
            long bytesAvailable = Math.Min(bytesRemaining, wantedBytes);
            long availableValues = bytesAvailable / sizeOfT;
            long bytesToRead = (availableValues * sizeOfT);

            if ((bytesRemaining < wantedBytes) && ((bytesRemaining - bytesToRead) > 0)) {
                Debug.WriteLine("Requested data exceeds available data and partial data remains in the file.", "Dmr.Common.IO.Arrays.FastRead(fs,array,offset,count)");
            }

            GCHandle gcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);

            try {
                uint bytesRead = 0;

                if (!ReadFile(fs.SafeFileHandle.DangerousGetHandle(),
                    new IntPtr(gcHandle.AddrOfPinnedObject().ToInt64() + (offset * sizeOfT)), (uint)bytesToRead, out bytesRead, IntPtr.Zero)) {
                    throw new IOException("Unable to read file.", new Win32Exception(Marshal.GetLastWin32Error()));
                }

                Debug.Assert(bytesRead == bytesToRead);
            } finally {
                gcHandle.Free();
            }

            return (int)availableValues;
        }

        [SuppressMessage("Microsoft.Interoperability", "CA1415:DeclarePInvokesCorrectly")]
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WriteFile(IntPtr hFile, IntPtr lpBuffer, uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten, IntPtr lpOverlapped);

        [SuppressMessage("Microsoft.Interoperability", "CA1415:DeclarePInvokesCorrectly")]
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ReadFile(IntPtr hFile, IntPtr lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

        #endregion

        #region Registry

        public static T GetRegistryKey<T>(string key, string name)
        {
            if (typeof(T) == typeof(string))
                return (T)((object)GetRegistryKey(key, name, String.Empty));
            else
                return GetRegistryKey<T>(key, name, DynamicExecutor.CreateInstance<T>());
        }

        public static T GetRegistryKey<T>(string key, string name, T defvalue)
        {            
            var skey = Registry.LocalMachine.OpenSubKey(String.Format(@"SOFTWARE{0}\{1}", Environment.Is64BitOperatingSystem ? @"\Wow6432Node" : String.Empty, key));
            try {
                if (skey == null)
                    return defvalue;
                var sval = skey.GetValue(name, defvalue);
                var styp = skey.GetValueKind(name);
                if (styp == RegistryValueKind.String && typeof(T) == typeof(string))
                    return (T)sval;
                if (styp == RegistryValueKind.DWord && typeof(T) == typeof(uint))
                    return (T)sval;
                if (styp == RegistryValueKind.QWord && typeof(T) == typeof(UInt64))
                    return (T)sval;
                if (styp == RegistryValueKind.None || sval == null)
                    return defvalue;
            } catch {;}
            return defvalue;
        }

        #endregion
    }
}
