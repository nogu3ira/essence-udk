﻿﻿using System;
﻿using System.Collections;
﻿using System.Collections.Generic;
﻿using System.Diagnostics;
﻿using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
﻿using System.Windows.Ink;
﻿using EssenceUDK.Platform.UtilHelpers;
﻿using EssenceUDK.Resources.Libraries.MiscUtil.Collections.Extensions;
﻿using ICSharpCode.SharpZipLib.Checksums;

namespace EssenceUDK.Platform.DataTypes.FileFormat.Containers
{
    internal class MulContainer : IDataContainer
    {
        internal readonly uint EntrySize;       // entry size for non index data
        private uint _EntryLength, _EntryOff;   // number of entries and ither start position of data for not index data or index shift for index data
        private readonly uint _EntryHeaderSize, _EntryItemSize, _EntryItemsCount; // this is hack for non index muls, which store entries by blocks with header
                                                                                  // Note, single supported method for using item indexs is IDataContainer.this[uint id, bool item] 
        internal readonly string FNameIdx, FNameMul;
        internal readonly FileStream StreamIdx, StreamMul;

        //private bool IsVirtual { get { return (_Parent != null || _ChieldCount > 0); } }
        private  readonly bool IsVirtual = false;       // virtual container is illegal, we need it as parent for preventing duplicating streams
        private  readonly MulContainer _Parent = null;  // parent can be only virtual container
        private  List<MulContainer> _Chields = null;    // chields have only virtual containers

        bool IDataContainer.IsIndexBased { get { return IdxTable != null; } }
        uint IDataContainer.EntryHeaderSize { get { return _EntryHeaderSize; } }
        uint IDataContainer.EntryItemsCount { get { return _EntryItemsCount; } }

        internal MulContainer(uint entrySize, string mulFile, bool realTime)
        {
            StreamMul = new FileStream(FNameMul = mulFile, FileMode.Open, realTime ? FileAccess.ReadWrite : FileAccess.Read, FileShare.Read, 0x10000, false);
            StreamIdx = null;
            EntrySize = entrySize;
            IdxTable = null;
            _EntryLength = (uint)(StreamMul.Length / EntrySize);
            _EntryHeaderSize = 0;
            _EntryItemsCount = 1;
            _EntryItemSize = EntrySize;
        }

        internal MulContainer(uint entryHeaderSize, uint entryItemSize, uint entryItemsCount, string mulFile, bool realTime)
        {
            StreamMul = new FileStream(FNameMul = mulFile, FileMode.Open, realTime ? FileAccess.ReadWrite : FileAccess.Read, FileShare.Read, 0x10000, false);
            StreamIdx = null;
            EntrySize = entryHeaderSize + entryItemSize * entryItemsCount;
            IdxTable = null;
            _EntryLength = (uint)(StreamMul.Length / EntrySize);
            _EntryHeaderSize = entryHeaderSize;
            _EntryItemsCount = entryItemsCount;
            _EntryItemSize = entryItemSize;
        }

        internal MulContainer(string idxFile, string mulFile, bool realTime)
        { 
            StreamMul = new FileStream(FNameMul = mulFile, FileMode.Open, realTime ? FileAccess.ReadWrite : FileAccess.Read, FileShare.Read, 0x10000, false);
            StreamIdx = new FileStream(FNameIdx = idxFile, FileMode.Open, realTime ? FileAccess.ReadWrite : FileAccess.Read, FileShare.Read, 192, false);
            IdxTable = Utils.ArrayRead<IndexEntry>(StreamIdx, (int)StreamIdx.Length / 12);
            EntrySize = 0;
            _EntryLength = (uint)IdxTable.Length;
            _EntryHeaderSize = 0;
            _EntryItemsCount = 1;
            _EntryItemSize = 0;
        }

        internal MulContainer(MulContainer container, uint entryoff, uint entries = 0, uint entrySize = 0)
        {
            if (container == null || !container.IsVirtual)
                throw new ArgumentException("Mul container is null or not virtual.");
            _Parent   = container;
            _Parent._Chields.Add(this);
            FNameMul  = container.FNameMul; 
            StreamMul = container.StreamMul;
            EntrySize = entrySize;
            _EntryOff = entryoff;

            if (container.StreamIdx != null && (entrySize == 0 || entrySize == 12)) {
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
            _EntryHeaderSize = 0;
            _EntryItemsCount = 1;
            _EntryItemSize = 0;
        }

        internal MulContainer(MulContainer container, uint entryHeaderSize, uint entryItemSize, uint entryItemsCount, uint entryoff, uint entries = 0)
            : this(container, entryoff, entries, entryHeaderSize + entryItemSize * entryItemsCount)
        {
            if ((container as IDataContainer).IsIndexBased)
                throw new ArgumentException("Index based mul container can't have items.");
            _EntryHeaderSize = entryHeaderSize;
            _EntryItemsCount = entryItemsCount;
            _EntryItemSize = entryItemSize;
        }

        internal static MulContainer GetParent(MulContainer container)
        {
            return container._Parent;
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
            _Chields  = new List<MulContainer>();
            StreamMul = new FileStream(FNameMul = mulFile, FileMode.Open, realTime ? FileAccess.ReadWrite : FileAccess.Read, FileShare.Read, 0x10000, false);
            if (!String.IsNullOrEmpty(idxFile))
                StreamIdx = new FileStream(FNameIdx = idxFile, FileMode.Open, realTime ? FileAccess.ReadWrite : FileAccess.Read, FileShare.Read, 192, false);
            else { StreamIdx = null; }
            _EntryHeaderSize = 0;
            _EntryItemsCount = 1;
            _EntryItemSize = 0;
        }

        // TODO: Dispose object

        private IndexEntry[] IdxTable;
        [StructLayout(LayoutKind.Sequential, Size = 12, Pack = 1)]
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

        private unsafe void WriteIdx(uint id)
        {
            if (IdxTable == null) return;
            StreamIdx.Seek((_EntryOff + id) * sizeof(IndexEntry), SeekOrigin.Begin);
            Utils.ArrayWrite<IndexEntry>(StreamIdx, IdxTable, (int)id, 1);
            //StreamIdx.Flush();
        }

        private unsafe void FlushIdx()
        {
            if (IdxTable == null) return;
            StreamIdx.Seek(_EntryOff * sizeof(IndexEntry), SeekOrigin.Begin);
            Utils.ArrayWrite(StreamIdx, IdxTable, 0, (int)_EntryLength);
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

        byte[] IDataContainer.this[uint id, bool item] {
            get {
                if (!item)
                    return Read(id);
                if (StreamIdx != null)
                    throw new NotSupportedException("Index based mul container can't have items.");
                var eid = (id / _EntryItemsCount);
                var dat = Read(eid);
                var res = new byte[_EntryItemSize];
                Array.Copy(dat, _EntryHeaderSize + (id % _EntryItemsCount) * _EntryItemSize, res, 0, _EntryItemSize);
                return res;
            }
            set {
                if (!item) {
                    Write(id, value);
                    return;
                }
                if (StreamIdx != null)
                    throw new NotSupportedException("Index based mul container can't have items.");
                var eid = (id / _EntryItemsCount);
                var dat = Read(eid);
                Array.Copy(value, 0, dat, _EntryHeaderSize + (id % _EntryItemsCount) * _EntryItemSize, _EntryItemSize);
                Write(eid, dat);
            }
        }

        T IDataContainer.Read<T>(uint id, uint offset)
        {
            return (this as IDataContainer).Read<T>(id, offset, 1)[0];
        }

        T[] IDataContainer.ReadAll<T>(uint id, uint offset)
        {
            if (IdxTable != null)
                return (this as IDataContainer).Read<T>(id, offset, (uint)((IdxTable[id].Length - offset) / Marshal.SizeOf(typeof(T))));
            else
                return (this as IDataContainer).Read<T>(id, offset, (uint)((StreamMul.Length - offset) / Marshal.SizeOf(typeof(T))));
        }

        T[] IDataContainer.Read<T>(uint fromId, uint offset, uint count)
        {
            if (IdxTable != null) {
                if (IdxTable[fromId].Offset == 0xFFFFFFFF || IdxTable[fromId].Length == 0x00000000 || IdxTable[fromId].Length == 0xFFFFFFFF)
                    return new T[0];
                StreamMul.Seek(IdxTable[fromId].Offset + offset, SeekOrigin.Begin);
                var arr = Utils.ArrayRead<T>(StreamMul, (int)count);
                return arr;
            } else {
                StreamMul.Seek(_EntryOff + fromId*EntrySize + offset, SeekOrigin.Begin);
                var arr = Utils.ArrayRead<T>(StreamMul, (int)count);
                return arr;
            }
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
            if (id >= _EntryLength || (IdxTable == null && data != null && Marshal.SizeOf(typeof(T)) != (EntrySize - offset) / data.Length))
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
                StreamMul.Seek(_EntryOff + id * EntrySize, SeekOrigin.Begin);
            } else {
                if (rawdata != null) {
                    var length = IdxTable[id].Length;
                    IdxTable[id].Length = (uint)rawdata.Length;
                    if (IdxTable[id].Offset != 0xFFFFFFFF && length != 0xFFFFFFFF && length >= IdxTable[id].Length)
                        StreamMul.Seek(IdxTable[id].Offset, SeekOrigin.Begin);
                    else
                        IdxTable[id].Offset = (uint)StreamMul.Seek(0, SeekOrigin.End);
                } else
                    (this as IDataContainer).Delete(id);
                StreamIdx.Seek((_EntryOff + id) * sizeof(IndexEntry), SeekOrigin.Begin);
                Utils.ArrayWrite<IndexEntry>(StreamIdx, IdxTable, (int)id, 1);
                StreamIdx.Flush();
            }
            if (rawdata != null) {
                Utils.ArrayWrite<byte>(StreamMul, rawdata, 0, rawdata.Length);
                StreamMul.Flush();
            }
        }

        void IDataContainer.Replace(uint id1, uint id2)
        {
            if (IsVirtual)
                throw new MethodAccessException("Only non virtual mul containers supports replacing.");

            if (id1 >= _EntryLength || id2 >= _EntryLength)
                return;

            if (IdxTable == null) {
                var srsdata1 = Read(id1);
                var srsdata2 = Read(id2);
                Write(id1, srsdata2);
                Write(id2, srsdata1);
            } else {
                var  srsentry = IdxTable[id1];
                IdxTable[id1] = IdxTable[id2];
                IdxTable[id2] = srsentry;
                WriteIdx(id1);
                WriteIdx(id2);
            }
        }

        void IDataContainer.Delete(uint id)
        {
            if (IdxTable == null || IsVirtual) {
                throw new MethodAccessException("Only non virtual mul containers with idx tables support deleting data.");
            }

            IdxTable[id].Offset = 0xFFFFFFFFu;
            IdxTable[id].Length = 0xFFFFFFFFu;
        }

        unsafe void IDataContainer.Resize(uint entries)
        {
            if (IsVirtual)
                throw new MethodAccessException("Only non virtual mul containers is possible to resize.");
            else if (_EntryLength == entries)
                return;

            var stream = StreamIdx ?? StreamMul;
            int delt = ((int)entries - (int)_EntryLength) * (StreamIdx == null ? (int)EntrySize : sizeof(IndexEntry));
            var from = (StreamIdx == null)
                     ? (_EntryOff + _EntryLength * EntrySize)
                     : ((_EntryOff + _EntryLength) * sizeof(IndexEntry));

            int size = 0x1000;
            var data = new byte[size];

            if (delt < 0) { // decrease size
                stream.Seek(from, SeekOrigin.Begin);
                while (size >= data.Length) {
                    size = stream.Read(data, 0, data.Length);
                    stream.Seek(delt - size, SeekOrigin.Current);
                    stream.Write(data, 0, size);
                    stream.Seek(-delt, SeekOrigin.Current);
                }
                stream.SetLength(stream.Length + delt);
            } else { // increase size (delt > 0)
                int next = (int)(stream.Length - from);
                stream.SetLength(stream.Length + delt);
                stream.Seek(-delt, SeekOrigin.End);
                while (size >= data.Length) {
                    size = Math.Min(next, data.Length);
                    next -= size;
                    stream.Seek(-size, SeekOrigin.Current);
                    stream.Read(data, 0, size);
                    stream.Seek(+delt - size, SeekOrigin.Current);
                    stream.Write(data, 0, size);
                    stream.Seek(-delt - size, SeekOrigin.Current);
                }
            }

            if (_Parent != null) {
                _Parent._Chields.ForEach(c => {
                    if (c._EntryOff > _EntryOff) {
                        c._EntryOff = (uint)((int)c._EntryOff + ((StreamIdx == null) ? delt : ((int)entries - (int)_EntryLength)));
                    }
                });
            }

            if (StreamIdx != null) {
                Array.Resize(ref IdxTable, (int)entries);
                for (uint id = _EntryLength; id < entries; ++id) {
                    (this as IDataContainer).Delete(id);
                    (this as IDataContainer).SetExtra(id, 0);
                }
            }

            _EntryLength = entries;
            if (StreamIdx != null)
                FlushIdx();
        }

        void IDataContainer.Defrag()
        {
            if (StreamIdx == null || IsVirtual) {
                throw new MethodAccessException("Only non virtual mul containers with idx tables is possible to defrag.");
            }

            var crc32 = new Crc32();
            var hash1 = new Hashtable();
            var hash2 = new Hashtable();

            uint eidx = 0U, fpos = 0U;
            var  list = IdxTable.Select(e => new IndexEntry { Append = eidx++, Offset = e.Offset, Length = e.Length }).OrderBy(e => e.Offset).ToList();
            var  maxl = IdxTable.Max(e => ((ulong)e.Offset + (ulong)e.Length) < (ulong)StreamMul.Length ? e.Length : 0);
            var  bufi = new byte[Math.Min(maxl << 1, StreamMul.Length)];

            StreamMul.Seek(0, SeekOrigin.Begin);
            var back = Utils.ArrayRead<byte>(StreamMul, bufi.Length);


                                                                    var curleft = Console.CursorLeft;
                                                                    var process = Process.GetCurrentProcess();
                                                                    var workset = 0U;
                                                                    var lastupd = 0;
            for (int i = 0; i < _EntryLength; ++i) {
                                                                    if (Environment.TickCount - lastupd > 100)
                                                                    {
                                                                        //GC.Collect();
                                                                        workset = (uint)(Environment.WorkingSet >> 20);
                                                                        lastupd = Environment.TickCount;

                                                                        Console.SetCursorPosition(curleft, Console.CursorTop);
                                                                        Console.Write("{0,7} < {1,6:0.00}% > RAM: {2} MiB", i, (100f * i) / _EntryLength, workset);
                                                                    }


                eidx = list[i].Append;
                if (IdxTable[eidx].Offset == 0xFFFFFFFF || IdxTable[eidx].Length == 0x00000000 || IdxTable[eidx].Length == 0xFFFFFFFF)
                    continue;

                var hkey = IdxTable[eidx].Offset;
                var hlst = hash1[hkey];
                if (hlst != null) {
                    IdxTable[eidx].Offset = IdxTable[(uint)hlst].Offset;
                    continue;
                } else
                    hash1.Add(hkey, eidx);

                if ((IdxTable[eidx].Offset + IdxTable[eidx].Length) >= back.Length) {
                    StreamMul.Seek(IdxTable[eidx].Offset, SeekOrigin.Begin);
                    Utils.FastRead(StreamMul, bufi, 0, (int)IdxTable[eidx].Length);
                } else {
                    Array.Copy(back, IdxTable[eidx].Offset, bufi, 0, IdxTable[eidx].Length);
                }

                crc32.Reset();
                crc32.Update(bufi);
                hkey = (uint)crc32.Value;
                hlst = hash2[hkey];
                if (hlst != null) {
                    var data = Read((uint)hlst);
                    if (Utils.ArrayIdentical(data, bufi, 0, Math.Max(data.Length, (int)IdxTable[eidx].Length))) {
                        IdxTable[eidx].Offset = IdxTable[(uint)hlst].Offset;
                        continue;
                    }
                } else
                    hash2.Add(hkey, eidx);

                IdxTable[eidx].Offset = fpos;
                fpos += IdxTable[eidx].Length;
                StreamMul.Seek(IdxTable[eidx].Offset, SeekOrigin.Begin);
                Utils.ArrayWrite(StreamMul, bufi, 0, (int)IdxTable[eidx].Length);
            }
            StreamMul.SetLength(fpos);
            StreamMul.Flush();
            FlushIdx();
        }
    }
}
