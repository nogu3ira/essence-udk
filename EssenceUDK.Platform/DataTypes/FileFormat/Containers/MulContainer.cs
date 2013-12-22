﻿﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using EssenceUDK.Platform.UtilHelpers;

namespace EssenceUDK.Platform.DataTypes.FileFormat.Containers
{
    internal class MulContainer : IDataContainer
    {
        internal readonly uint EntrySize;
        internal readonly uint _EntryLength, _EntryOff;
        internal readonly string FNameIdx, FNameMul;
        internal readonly FileStream StreamIdx, StreamMul;

        //private bool IsVirtual { get { return (_Parent != null || _ChieldCount > 0); } }
        private  readonly bool IsVirtual = false;
        private  readonly MulContainer _Parent = null;
        private  byte _ChieldCount = 0;

        internal MulContainer(uint entrySize, string mulFile, bool realTime)
        {
            StreamMul = new FileStream(FNameMul = mulFile, FileMode.Open, realTime ? FileAccess.ReadWrite : FileAccess.Read, FileShare.Read, 0x10000, false);
            StreamIdx = null;
            EntrySize = entrySize;
            IdxTable = null;
            _EntryLength = (uint)(StreamMul.Length / EntrySize);
        }

        internal MulContainer(string idxFile, string mulFile, bool realTime)
        { 
            StreamMul = new FileStream(FNameMul = mulFile, FileMode.Open, realTime ? FileAccess.ReadWrite : FileAccess.Read, FileShare.Read, 0x10000, false);
            StreamIdx = new FileStream(FNameIdx = idxFile, FileMode.Open, realTime ? FileAccess.ReadWrite : FileAccess.Read, FileShare.Read, 192, false);
            IdxTable = Utils.ArrayRead<IndexEntry>(StreamIdx, (int)StreamIdx.Length / 12);
            EntrySize = 0;
            _EntryLength = (uint)IdxTable.Length;
        }

        internal MulContainer(MulContainer container, uint entryoff, uint entries = 0, uint entrySize = 0)
        {
            _Parent   = container;
            ++container._ChieldCount;
            FNameMul  = container.FNameMul; 
            StreamMul = container.StreamMul;
            EntrySize = entrySize;
            _EntryOff = entryoff;

            if (container.StreamIdx != null && entrySize == 0) {
                FNameIdx = container.FNameIdx;
                StreamIdx = container.StreamIdx;
                StreamIdx.Position = 12 * entryoff;
                _EntryLength = entries == 0 ? (uint)StreamIdx.Length / 12 - entryoff : entries;
                IdxTable = Utils.ArrayRead<IndexEntry>(StreamIdx, (int)_EntryLength);
            } else if (container.StreamIdx == null && entrySize != 0) {
                FNameIdx  = null;
                StreamIdx = null;
                IdxTable  = null;
                _EntryLength = entries == 0 ? ((uint)StreamMul.Length - entryoff) / EntrySize : entries;
            }
        }

        internal static MulContainer GetVirtual(string idxFile, string mulFile, bool realTime)
        {
            var container = new MulContainer(realTime, idxFile, mulFile);
            return container;
            //TODO: we need to remember opend streams for virtual containers
        }

        private MulContainer(bool realTime, string idxFile, string mulFile)
        {
            IsVirtual = true;
            StreamMul = new FileStream(FNameMul = mulFile, FileMode.Open, realTime ? FileAccess.ReadWrite : FileAccess.Read, FileShare.Read, 0x10000, false);
            if (!String.IsNullOrEmpty(idxFile))
                StreamIdx = new FileStream(FNameIdx = idxFile, FileMode.Open, realTime ? FileAccess.ReadWrite : FileAccess.Read, FileShare.Read, 192, false);
            else { StreamIdx = null; }
        }

        // TODO: Dispose object

        private IndexEntry[] IdxTable;
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct IndexEntry {
            internal uint Offset;
            internal uint Length;
            internal uint Append;
        }

        uint IDataContainer.GetExtra(uint id)
        {
            return IdxTable[id].Append;
        }

        void IDataContainer.SetExtra(uint id, uint value)
        {
            IdxTable[id].Append = value;
        }

        private void WriteIdx(uint id)
        {
            if (IdxTable == null) return;
            StreamIdx.Seek(IdxTable[id].Offset, SeekOrigin.Begin);
            //_Stream.Write();
            throw new NotImplementedException();
            StreamIdx.Flush();
        }

        private void FlushIdx()
        {
            if (IdxTable == null) return;
            StreamIdx.Seek(0, SeekOrigin.Begin);
            Utils.ArrayWrite(StreamIdx, IdxTable, 0, IdxTable.Length);
            StreamIdx.Flush();
        }

        uint IDataContainer.EntryLength {
            get { return _EntryLength; }
        }

        bool IDataContainer.IsValid(uint id)
        {
            if (id >= _EntryLength)
                return false;
            if (IdxTable != null && (IdxTable[id].Offset == 0xFFFFFFFF || IdxTable[id].Length == 0x00000000 || IdxTable[id].Length == 0xFFFFFFFF))
                return false;
            return true;
        }

        byte[] IDataContainer.this[uint id] {
            get { return Read(id);  }
            set { Write(id, value); }
        }

        T IDataContainer.Read<T>(uint id, uint offset)
        {
            return (this as IDataContainer).Read<T>(id, offset, 1)[0];
        }

        T[] IDataContainer.Read<T>(uint fromId, uint offset, uint count)
        {
            if (IdxTable != null)
                throw new Exception();
            StreamMul.Seek(_EntryOff + fromId*EntrySize + offset, SeekOrigin.Begin);
            var arr = Utils.ArrayRead<T>(StreamMul, (int)count);
            return arr;
        }

        private byte[] Read(uint id)
        {
            if (id >= _EntryLength)
                return null;
                //throw new ArgumentOutOfRangeException();
            if (IdxTable == null) {
                StreamMul.Seek(_EntryOff + id*EntrySize, SeekOrigin.Begin);
                return Utils.ArrayRead<byte>(StreamMul, (int)EntrySize);             
            } else {
                if (IdxTable[id].Offset == 0xFFFFFFFF || IdxTable[id].Length == 0xFFFFFFFF)
                    return null;
                StreamMul.Seek(IdxTable[id].Offset, SeekOrigin.Begin);
                return Utils.ArrayRead<byte>(StreamMul, (int)IdxTable[id].Length);
            }
        }

        void IDataContainer.Write<T>(uint id, uint offset, T data)
        {
            if (id >= _EntryLength || (IdxTable == null && Marshal.SizeOf(typeof(T)) != EntrySize))
                throw new ArgumentOutOfRangeException();
            (this as IDataContainer).Write<T>(id, offset, new T[] { data }, 0, 1 ); 
        }

        void IDataContainer.Write<T>(uint id, uint offset, T[] data, uint sfrom, uint count)
        {
            if (!StreamMul.CanWrite || (StreamIdx != null && !StreamIdx.CanWrite))
                throw new AccessViolationException();
            if (id >= _EntryLength || (IdxTable == null && data != null && Marshal.SizeOf(typeof(T)) != EntrySize))
                throw new ArgumentOutOfRangeException();

            if (IdxTable == null) {
                StreamMul.Seek(_EntryOff + id * EntrySize + offset, SeekOrigin.Begin);
            } else {
                var length = IdxTable[id].Length;
                IdxTable[id].Length = (uint)(offset + data.Length * Marshal.SizeOf(typeof(T)));
                if (IdxTable[id].Offset != 0xFFFFFFFF && length != 0xFFFFFFFF && length >= IdxTable[id].Length)
                    StreamMul.Seek(_EntryOff + IdxTable[id].Offset + offset, SeekOrigin.Begin);
                else {
                    StreamMul.Seek(_EntryOff + IdxTable[id].Offset, SeekOrigin.Begin);
                    var rawdata = new byte[offset];
                    StreamMul.Read(rawdata, 0, (int)offset);
                    IdxTable[id].Offset = (uint)StreamMul.Seek(0, SeekOrigin.End);
                    StreamMul.Write(rawdata, 0, (int)offset);
                }
                StreamIdx.Seek(id * Marshal.SizeOf(typeof(IndexEntry)), SeekOrigin.Begin);
                Utils.ArrayWrite<IndexEntry>(StreamIdx, IdxTable, (int)id, 1);
            }

            Utils.ArrayWrite<T>(StreamMul, data, (int)sfrom, (int)count);
        }

        private unsafe void Write(uint id, byte[] rawdata)
        {
            if (!StreamMul.CanWrite || (StreamIdx != null && !StreamIdx.CanWrite))
                throw new AccessViolationException("Can't write data to mul file");
            if (id >= _EntryLength || (IdxTable == null && rawdata != null && rawdata.Length != EntrySize))
                throw new ArgumentOutOfRangeException();

            if (IdxTable == null) {
                StreamMul.Seek((_EntryOff + id) * EntrySize, SeekOrigin.Begin);
            } else {
                var length = IdxTable[id].Length;
                IdxTable[id].Length = (uint)rawdata.Length;
                if (IdxTable[id].Offset != 0xFFFFFFFF && length != 0xFFFFFFFF && length >= IdxTable[id].Length)
                    StreamMul.Seek(IdxTable[id].Offset, SeekOrigin.Begin);
                else
                    IdxTable[id].Offset = (uint)StreamMul.Seek(0, SeekOrigin.End);
                StreamIdx.Seek((_EntryOff + id) * sizeof(IndexEntry), SeekOrigin.Begin);
                Utils.ArrayWrite<IndexEntry>(StreamIdx, IdxTable, (int)id, 1);
                StreamIdx.Flush();
            }

            Utils.ArrayWrite<byte>(StreamMul, rawdata, 0, (int)IdxTable[id].Length);
            StreamMul.Flush();
        }

        internal void Replace(uint id1, uint id2)
        {
            throw new NotImplementedException();
        }

        internal void Delete(uint id)
        {
            throw new NotImplementedException();
        }

        internal void Resize(uint entries)
        {
            throw new NotImplementedException();
        }

        internal void Defrag()
        {
            throw new NotImplementedException();
        }
    }
}
