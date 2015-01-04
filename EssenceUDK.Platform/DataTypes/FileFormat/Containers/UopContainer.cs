﻿// Base on Wyatt algoritm published on www.ruosi.org

//* idxLength variable was added for compatibility with legacy code for art (see art.cs)
//* At the moment the only UOP file having entries with extra field is gumpartlegacy.uop,
//* and it's two dwords in the beginning of the entry.
//* It's possible that UOP can include some entries with unknown hash: not really unknown for me, but
//* not useful for reading legacy entries. That's why i removed unknown hash exception throwing from this code
﻿
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using EssenceUDK.Platform.UtilHelpers;

namespace EssenceUDK.Platform.DataTypes.FileFormat.Containers
{
    //TODO: make IDataContainer interface realisation
    internal class UopContainer : IDataContainer
    {
        internal readonly string FNameUop;
        internal readonly FileStream StreamUop;

        private FileHeader Header;
        [StructLayout(LayoutKind.Sequential, Size = 28, Pack = 1)]
        private unsafe struct FileHeader {
            internal       uint   Format;   // 0x0050594D - MYP (Mythic Package?)
            internal       uint   Version;
            internal       uint   Signature;
            internal      ulong   Offset;   // First Group offset
            internal       uint   Capacity; // Group capacity (i.e. max _files in 1 Group)
            internal       uint   Count;    // Total _files count
        }

        private FileGroup[] Files;
        private unsafe struct FileGroup {
            internal       uint   Count;  // Number of _files in this Group
            internal      ulong   Offset; // Next Group offset
            internal FileEntry[] Entries;
        }

        [StructLayout(LayoutKind.Sequential, Size = 34, Pack = 1)]
        private unsafe struct FileEntry {
            internal      ulong   Offset;
            internal       uint   HeaderLength;
            internal       uint   CompressedLength;
            internal       uint   DecompressedLength;
            internal      ulong   Hash;
            internal       uint   Adler32;
            internal     ushort   Flags;
        }

        private IndexEntry[] IdxTable;
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct IndexEntry {
            internal uint Offset;
            internal uint Length;
            internal uint Uncomp;
            internal uint Append; 
        }

        internal UopContainer(string uopFile, bool realTime, string extension = null, bool hasAppend = false)
        {
            StreamUop = new FileStream(FNameUop = uopFile, FileMode.Open, realTime ? FileAccess.ReadWrite : FileAccess.Read, FileShare.Read, 0x10000, false);
            Header = Utils.ArrayRead<FileHeader>(StreamUop, 1)[0];
            if (Header.Format != 0x0050594D)
                throw new FileFormatException("Bad UOP file.");

            IdxTable = new IndexEntry[Header.Count]; // sure it's bad for addining files but now our goal is just reading

            // now we need to compute hashes...
            // thow i realy don't know patterns for animation hashes, so we make dirty loading for such files
            Dictionary<ulong, int> hashes = new Dictionary<ulong, int>();
            if (extension != null) {
                FileInfo fi = new FileInfo(uopFile);
                string uopPattern = fi.Name.Replace(fi.Extension, "").ToLowerInvariant();
                for (int i = 0; i < IdxTable.Length; i++) {
                    string entryName = string.Format("build/{0}/{1:D8}{2}", uopPattern, i, extension ?? String.Empty);
                    // also maybe we use smth like build/animationframe/000741/22.bin but it also not work
                    ulong hash = HashFileName(entryName);

                    if (!hashes.ContainsKey(hash))
                        hashes.Add(hash, i);
                }
            }
            
            // let's simply read all groups with all tables
            var tables = new List<FileGroup>(1024);
            var offset = Header.Offset;
            while (offset != 0 && offset != 0xFFFFFFFF) {
                StreamUop.Seek((long)offset, SeekOrigin.Begin);
                FileGroup file;
                file.Count   = Utils.ArrayRead<uint>(StreamUop, 1)[0];
                offset = file.Offset = Utils.ArrayRead<ulong>(StreamUop, 1)[0];
                file.Entries = Utils.ArrayRead<FileEntry>(StreamUop, (int)file.Count);
                tables.Add(file);
            }
            Files = tables.ToArray();

            // Now we transfer file's entries to index's entries
            int ind = 0;
            foreach (var group in Files)
                foreach (var table in group.Entries) {
                    if (table.Offset == 0) continue;
int idx = ind++;
                    if (extension == null || hashes.TryGetValue(table.Hash, out idx)) {
                        if (idx < 0 || idx > IdxTable.Length)
                            throw new IndexOutOfRangeException("hashes dictionary and files collection have different count of entries!");

                        IdxTable[idx].Offset = (uint)(table.Offset + table.HeaderLength);
                        IdxTable[idx].Length = table.Flags == 1 ? table.CompressedLength : table.DecompressedLength;
                        IdxTable[idx].Uncomp = table.DecompressedLength;

                        if (hasAppend) {
                            StreamUop.Seek(IdxTable[idx].Offset, SeekOrigin.Begin);
                            byte[] extra  = Utils.ArrayRead<byte>(StreamUop, 8);

                            ushort extra1 = (ushort)((extra[3] << 24) | (extra[2] << 16) | (extra[1] << 8) | extra[0]);
                            ushort extra2 = (ushort)((extra[7] << 24) | (extra[6] << 16) | (extra[5] << 8) | extra[4]);

                            IdxTable[idx].Offset += 8;
                            IdxTable[idx].Append = (uint)(extra1 << 16 | extra2);
                        }
                    }
                }

            //var ext1 = BruteforceExtension(0, 2600);
            return;
        }

        /// <summary>
        /// Method for calculating entry hash by it's name.
        /// Taken from Mythic.Package.dll
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static ulong HashFileName(string s)
        {
            uint eax, ecx, edx, ebx, esi, edi;

            eax = ecx = edx = ebx = esi = edi = 0;
            ebx = edi = esi = (uint)s.Length + 0xDEADBEEF;

            int i = 0;

            for (i = 0; i + 12 < s.Length; i += 12) {
                edi = (uint)((s[i +  7] << 24) | (s[i +  6] << 16) | (s[i + 5] << 8) | s[i + 4]) + edi;
                esi = (uint)((s[i + 11] << 24) | (s[i + 10] << 16) | (s[i + 9] << 8) | s[i + 8]) + esi;
                edx = (uint)((s[i +  3] << 24) | (s[i +  2] << 16) | (s[i + 1] << 8) | s[i])     - esi;

                edx = (edx + ebx) ^ (esi >> 28) ^ (esi <<  4);
                esi += edi;
                edi = (edi - edx) ^ (edx >> 26) ^ (edx <<  6);
                edx += esi;
                esi = (esi - edi) ^ (edi >> 24) ^ (edi <<  8);
                edi += edx;
                ebx = (edx - esi) ^ (esi >> 16) ^ (esi << 16);
                esi += edi;
                edi = (edi - ebx) ^ (ebx >> 13) ^ (ebx << 19);
                ebx += esi;
                esi = (esi - edi) ^ (edi >> 28) ^ (edi <<  4);
                edi += ebx;
            }

            if (s.Length - i > 0) {
                switch (s.Length - i) {
                    case 12:    esi += (uint)s[i + 11] << 24;   goto case 11;
                    case 11:    esi += (uint)s[i + 10] << 16;   goto case 10;
                    case 10:    esi += (uint)s[i +  9] <<  8;   goto case  9;
                    case  9:    esi += (uint)s[i +  8];         goto case  8;
                    case  8:    edi += (uint)s[i +  7] << 24;   goto case  7;
                    case  7:    edi += (uint)s[i +  6] << 16;   goto case  6;
                    case  6:    edi += (uint)s[i +  5] <<  8;   goto case  5;
                    case  5:    edi += (uint)s[i +  4];         goto case  4;
                    case  4:    ebx += (uint)s[i +  3] << 24;   goto case  3;
                    case  3:    ebx += (uint)s[i +  2] << 16;   goto case  2;
                    case  2:    ebx += (uint)s[i +  1] <<  8;   goto case  1;
                    case  1:    ebx += (uint)s[i];              break;
                }

                esi = (esi ^ edi) - ((edi >> 18) ^ (edi << 14));
                ecx = (esi ^ ebx) - ((esi >> 21) ^ (esi << 11));
                edi = (edi ^ ecx) - ((ecx >>  7) ^ (ecx << 25));
                esi = (esi ^ edi) - ((edi >> 16) ^ (edi << 16));
                edx = (esi ^ ecx) - ((esi >> 28) ^ (esi <<  4));
                edi = (edi ^ edx) - ((edx >> 18) ^ (edx << 14));
                eax = (esi ^ edi) - ((edi >>  8) ^ (edi << 24));

                return ((ulong)edi << 32) | eax;
            }

            return ((ulong)esi << 32) | eax;
        }

        private string BruteforceExtension(int minId = 0, int maxId = -1, bool fixPoint = true)
        {
            var hashes = new List<ulong>(0x4000);
            foreach (var group in Files)
                foreach (var table in group.Entries)
                    hashes.Add(table.Hash);

            FileInfo fi = new FileInfo(FNameUop);
            string uopPattern = fi.Name.Replace(fi.Extension, "").ToLowerInvariant();

            var chars = new[] {'.','-','_','a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z'};

            string extension, entryname;
            //for (int i = minId; i < (maxId = maxId < 0 ? minId == 0 ? 0x8000 : minId + 1 : maxId); ++i)
            //{
            //    for (var c1 = 0; c1 < (fixPoint ? 1 : chars.Length); ++c1)
            //        for (var c2 = 0; c2 < chars.Length; ++c2)
            //            for (var c3 = 0; c3 < chars.Length; ++c3)
            //                for (var c4 = 0; c4 < chars.Length; ++c4) {
            //                    extension = new string(new [] { chars[c1], chars[c2], chars[c3], chars[c4]});
            //                    entryname = string.Format("build/{0}/{1:D8}{2}", uopPattern, i, extension);                                
            //                    if (hashes.Contains(HashFileName(entryname)))
            //                        return entryname;
            //                }
                
            //}


            for (int i = minId; i < (maxId = maxId < 0 ? minId == 0 ? 0x8000 : minId + 1 : maxId); ++i)
            {
                for (var f = 0; f < 100; ++f) {
                    entryname = string.Format("build/animationframe/{0:D6}/{1:D2}.bin", i, f);
                    if (hashes.Contains(HashFileName(entryname)))
                        return entryname;
                    entryname = string.Format("build/animationframe1/{0:D6}/{1:D2}.bin", i, f);
                    if (hashes.Contains(HashFileName(entryname)))
                        return entryname;
                    entryname = string.Format("build/animationframe/{0:D4}/{1:D4}.bin", i, f);
                    if (hashes.Contains(HashFileName(entryname)))
                        return entryname;
                    entryname = string.Format("build/animationframe1/{0:D4}/{1:D4}.bin", i, f);
                    if (hashes.Contains(HashFileName(entryname)))
                        return entryname;
                    entryname = string.Format("build/animationframe/{0:D4}/{1:D2}/{1:D2}.bin", i, f);
                    if (hashes.Contains(HashFileName(entryname)))
                        return entryname;
                    entryname = string.Format("build/animationframe1/{0:D4}/{1:D2}/{1:D2}.bin", i, f);
                    if (hashes.Contains(HashFileName(entryname)))
                        return entryname;
                }
                
            }

            return null;
        }


        bool IDataContainer.IsValid(uint id)
        {
            if (id >= (this as IDataContainer).EntryLength)
                return false;
            if (IdxTable != null && (IdxTable[id].Offset == 0xFFFFFFFF || IdxTable[id].Length == 0x00000000 || IdxTable[id].Length == 0xFFFFFFFF))
                return false;
            return true;
        }
        
        uint IDataContainer.EntryLength {
            get { return (uint)IdxTable.Length; }
        }

        bool IDataContainer.IsIndexBased { get { return false; } }

        uint IDataContainer.GetExtra(uint id) { return 0; }

        void IDataContainer.SetExtra(uint id, uint value) { }

        byte[] IDataContainer.this[uint id] {
            get { return Read(id); }
            set { Write(id, value); }
        }

        T IDataContainer.Read<T>(uint id, uint offset)
        {
            throw new NotImplementedException();
        }

        T[] IDataContainer.ReadAll<T>(uint id, uint offset)
        {
            throw new NotImplementedException();
        }

        T[] IDataContainer.Read<T>(uint fromId, uint offset, uint count)
        {
            throw new NotImplementedException();
        }

        private byte[] Read(uint id)
        {
            if (id >= IdxTable.Length)
                return null;
            if (IdxTable[id].Offset == 0xFFFFFFFF || IdxTable[id].Length == 0xFFFFFFFF)
                return null;
            StreamUop.Seek(IdxTable[id].Offset, SeekOrigin.Begin);
            var comprdata = Utils.ArrayRead<byte>(StreamUop, (int)IdxTable[id].Length);
            var uncomdata = IdxTable[id].Length == IdxTable[id].Uncomp ? comprdata
                          : new ZipNativeCompressor().Decompress(comprdata, IdxTable[id].Uncomp);
            return uncomdata;
        }


        void IDataContainer.Write<T>(uint id, uint offset, T data)
        {
            throw new NotImplementedException();
        }

        void IDataContainer.Write<T>(uint id, uint offset, T[] data, uint sfrom, uint count)
        {
            throw new NotImplementedException();
        }

        private void Write(uint id, byte[] rawdata)
        {
            if (id >= (this as IDataContainer).EntryLength || (IdxTable == null && rawdata != null))
                throw new ArgumentOutOfRangeException();
            throw new NotImplementedException();
        }


        void IDataContainer.Replace(uint id1, uint id2)
        {
            throw new NotImplementedException();
        }

        void IDataContainer.Delete(uint id)
        {
            throw new NotImplementedException();
        }

        void IDataContainer.Resize(uint entries)
        {
            throw new NotImplementedException();
        }

        void IDataContainer.Defrag()
        {
            throw new NotImplementedException();
        }


    }
}
