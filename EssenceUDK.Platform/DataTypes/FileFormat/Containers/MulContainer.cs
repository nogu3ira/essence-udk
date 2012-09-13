using System;
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
        internal readonly uint _EntryLength;
        internal readonly string FNameIdx, FNameMul;
        internal readonly FileStream StreamIdx, StreamMul;

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

        private IndexEntry[] IdxTable;
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct IndexEntry {
            internal uint Offset;
            internal uint Length;
            internal uint Append;
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

        byte[] IDataContainer.this[uint id] {
            get { return Read(id);  }
            set { Write(id, value); }
        }

        private byte[] Read(uint id)
        {
            if (id >= _EntryLength)
                throw new ArgumentOutOfRangeException();
            if (IdxTable == null) {
                StreamMul.Seek(id * EntrySize, SeekOrigin.Begin);
                return Utils.ArrayRead<byte>(StreamMul, (int)EntrySize);             
            } else {
                StreamMul.Seek(IdxTable[id].Offset, SeekOrigin.Begin);
                return Utils.ArrayRead<byte>(StreamMul, (int)IdxTable[id].Length);
            }
        }

        private void Write(uint id, byte[] rawdata)
        {
            if (id >= _EntryLength || (IdxTable == null && rawdata != null && rawdata.Length != EntrySize))
                throw new ArgumentOutOfRangeException();
            throw new NotImplementedException();
        }

        internal void Replace(uint id1, uint id2)
        {
            throw new NotImplementedException();
        }

        internal void Delete(uint id)
        {
            throw new NotImplementedException();
        }

        internal void Defrag()
        {
            throw new NotImplementedException();
        }
    }
}
