﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EssenceUDK.Platform;
using EssenceUDK.Platform.DataTypes;
using EssenceUDK.Platform.DataTypes.Factories;
using EssenceUDK.Platform.Factories;
using EssenceUDK.Platform.MiscHelper.Components.MultiStruct;
using EssenceUDK.Platform.UtilHelpers;

namespace EssenceUDK.Tools
{
    class Program
    {
        private static string usage = ""
            + "\n  This software is part of EssenceUDK project "
            + "\n  <http://dev.uoquint.ru/projects/essence-udk>"
            + String.Format("\n   EssenceUDK.Tool      version: {0}", Assembly.GetExecutingAssembly().GetName().Version)
            + String.Format("\n   EssenceUDK.Platform  version: {0}", Assembly.GetAssembly(typeof(UODataManager)).GetName().Version)
            + "\n"
            + String.Format("\nUsage: run \"{0}\" with command.", AppDomain.CurrentDomain.FriendlyName)
            + "\n"
            + "\nCommands:"
            + "\n"
            + "\n --fsutil    - Operation over file system (rename, moving, deleteing files)"
            + "\n   Arguments:"
            + "\n     [-pfx <string>] [-min <number>] [-max <number>] [-ren <beg|end>]"
            + "\n     [-mov <number>] [-len <number>] [-hex] [-del] <in_dir> <out_dir>"
            + "\n       -pfx <string>  Prefix for file names: I, L, T, G and others."
            + "\n       -min <number>  Ignore all files with ID < number."
            + "\n       -max <number>  Ignore all files with ID > number."
            + "\n       -ren <beg|end> Rename all files from end or start without skipping ID."
            + "\n       -mov <number>  Shift each result ID by specified value."
            + "\n       -len <number>  Count of digits in output names (default 5 and 4 for hex)"
            + "\n       -hex           Save file names in HEX format."
            + "\n       -del           Delete file from <in_dir> after coping ti <out_dir>."
            + "\n       <in_dir>       Path to input folder."
            + "\n       <out_dir>      Path to output folder."
            + "\n"
            + "\n --create    - Create new index or not index *.mul file with specidied size"
            + "\n   Arguments:"
            + "\n     <entry_count> <entry_size> <mulfile>"
            + "\n     <entry_count> <idxfile> <mulfile>"
            + "\n"
            + "\n --resize    - Increase/decrease *.mul file length"
            + "\n   Arguments:"
            + "\n     <entry_count> $MULCON$"
            + "\n"
            + "\n --defrag    - Defragmentation index *.mul file"
            + "\n   Arguments:"
            + "\n     $MULCON$"
            + "\n"
            + "\n --export    - Export data blocks and save them as *.mde files."
            + "\n   Arguments:"
            + "\n     <dirpath> $MULCON$"
            + "\n"
            + "\n --import    - Import data blocks from *.mde files to *.mul file."
            + "\n   Arguments:"
            + "\n     <dirpath> $MULCON$"
            + "\n"
            + "\n --merger    - Make binary merge for 2 *.mul files. All not existing entries in"
            + "\n               second file will be replaced from 1st, and all conflict entries"
            + "\n               will be exported to *.mde files in specified folders."
            + "\n   Arguments:"
            + "\n     <dirpath> $MULCON$ <dirpath> $MULCON$"
            + "\n"
            + "\n --convtd    - Make conversation from new tiledta format(HS) to old one and back"
            + "\n   Arguments:"
            + "\n     <dstfile> <newformat> <mulfile>"
            + "\n       <newformat>  - true/false, true means converting from new format to old"
            + "\n"
            + "\n --convmt    - Make conversation from new multi format(HS) to old one and back"
            + "\n   Arguments:"
            + "\n     <newformat> <idxfile> <mulfile>"
            + "\n       <newformat>  - true/false, true means converting from new format to old"
            + "\n"
            + "\n --copyid    - Coping entries with specified numbers to new position"
            + "\n   Arguments:"
            + "\n     $LIST#2$ $MULCON$"
            + "\n"
            + "\n --moveid    - Moving entries with specified numbers to new position"
            + "\n   Arguments:"
            + "\n     $LIST#2$ $MULCON$"
            + "\n"
            + "\n --remove    - Delete entries with specified numbers in index *.mul file"
            + "\n   Arguments:"
            + "\n     $LIST#1$ $MULCON$"
            + "\n"
            + "\n --facetm    - Operation over facets (map, statics, facets)"
            + "\n   Arguments:"
            + "\n   <folder> <uodata> <uoopts> $SUBCMD$"
            + "\n     <folder> - folder with data files"
            + "\n     <uodata> - client type flags (for HS - 0x0462, else - 0x0062)"
            + "\n     <uoopts> - array of map indexes and map sizes, where values are separeted"
            + "\n               by \"|\" (for original HS maps:" 
            + "\n               \"0|896|512|1|896|512|2|288|200|3|320|256|4|181|181|5|160|512\")"
            + "\n     $SUBCMD$ - subcommand, see list below:"
            + "\n"
            + "\n     -replid    - Replace all land tiles with specified Index by new Index"
            + "\n      Arguments:"
            + "\n        $MPAREA$ $LIST#2$"
            + "\n"
            + "\n     -reptid    - Replace all item tiles with specified Index by new Index"
            + "\n      Arguments:"
            + "\n        $MPAREA$ $LIST#2$"
            + "\n"
            + "\n     -rephid    - Replace all item colors with specified Index by new Index"
            + "\n      Arguments:"
            + "\n        $MPAREA$ $LIST#2$"
            + "\n"
            + "\n"
            + "\nAllias:"
            + "\n"
            + "\n  $MULCON$  ->  <index_offset> <entry_length> <idxfile> <mulfile>"
            + "\n  $MULCON$  ->  <byte_offset> <entry_length> <entry_size> <mulfile>"
            + "\n  $MULCON$  ->  <byte_offset> <entry_length> <entry_head_size>"
            + "\n                <entry_item_size> <entry_item_count> <mulfile>"
            + "\n                   <entry_length> - if 0 then will be set up to the end of file"
            + "\n  $LIST#1$  ->  <index>"
            + "\n  $LIST#1$  ->  <txtfile>"
            + "\n  $LIST#2$  ->  <srs_index> <dst_index>"
            + "\n  $LIST#2$  ->  <txtfile>"
            + "\n  $MPAREA$  ->  <map_index> <block_x1> <block_y1> <block_x2> <block_y2>"
            + "\n                  <block_x2> == 0 or <block_y2> == 0 means width-1 and height-1"
            + "\n"
            + "\n"
            + "\nNotes:"
            + "\n"
            + "\n- Before merge or defrag operation to make them more effective recomended to"
            + "\n  resave all data by Fiddler. This action prevent situations when same data"
            + "\n  will have different byte sequences."
            + "\n- *.mde files are binary data of specified index with name in ?????? format,"
            + "\n  where ?????? is number of entry. For index *.mul files first 4 bytes are"
            + "\n  extra value from *.idx file."
            + "\n- text file for $LIST#1$ allias must contain list of numbers separated with"
            + "\n  line break."
            + "\n- text file for $LIST#2$ allias must contain list of pair numbers separated"
            + "\n  with line break. Numbers in pairs can be separated with space, comma, \"->\""
            + "\n  If second number not present instead of it will be used incremented by 1"
            + "\n  last second value. If second value wasn't specifer befor exception occurs"
            + "\n- All numbers can be ither in decimal or in hex style, for last case they"
            + "\n  have to start wirh \"0x\" specifier."
            + "\n- All paths can be ither full or related for setting working directory"
            + "\n"
            + "\nInfo about muls:"
            + "\n"
            + "\n | file name    |  entry size | entry count  |"
            + "\n |--------------|-------------|--------------|"
            + "\n | tiledata.mul |   836 / 964 |          512 | - land tiles"
            + "\n | tiledata.mul | 1188 / 1316 | 512/1024/2048| - item tiles(off:428032/493568)"
            + "\n | animdata.mul |         548 |    items / 8 |"
            + "\n | radarcol.mul |           2 | land + items |"
            + "\n | hues.mul     |         708 |          375 |"
            + "\n | light.mul    |           - |          100 |"
            + "\n | sound.mul    |           - |         4096 |"
            + "\n | multi.mul    |           - |         8192 |"
            + "\n | map##.mul    |         196 | width*height | - sizes in blocks (8x8 tiles)"
            + "\n"
            + "\n | file name    | header | item size | item count |"
            + "\n |--------------|--------|-----------|------------|"
            + "\n | tiledata.mul |      4 |   26 / 30 |         32 | - land tiles"
            + "\n | tiledata.mul |      4 |   37 / 41 |         32 | - item tiles"
            + "\n | animdata.mul |      4 |        68 |          8 |"
            + "\n | hues.mul     |      4 |        88 |          8 |"
            + "\n"
            + "\n Length of index based muls can be geted as *.idx length / 12"
            + "\n"
            ;

        static void Main(string[] args)
        {
            #if !DEBUG
            try {
            #endif
                int from = 1;
                if (String.Compare(args[0], "--fsutil", true) == 0) {
                    var srsdir = String.Empty;
                    var trgdir = String.Empty;
                    var imgcls = String.Empty;
                    var lminid = 0x0000;
                    var lmaxid = 0xFFFF;
                    var lcount = 0;
                    var trimsp = (int)0;
                    var shmove = (int)0;
                    var outhex = false;
                    var outdel = false;

                    args = args.Select(a => a.ToLower()).Skip(1).ToArray();
                    var enumerator = args.GetEnumerator();
                    while (enumerator.MoveNext()) {
                        var arg = enumerator.Current as String;
                        if (arg == "-pfx" && enumerator.MoveNext()) {
                            imgcls = enumerator.Current as String;
                        } else

                        if (arg == "-min" && enumerator.MoveNext()) {
                            arg = enumerator.Current as String;
                            lminid = arg.StartsWith("0x") ? Int32.Parse(arg.Substring(2), NumberStyles.AllowHexSpecifier) : Int32.Parse(arg, NumberStyles.None);
                        } else
                        if (arg == "-max" && enumerator.MoveNext()) {
                            arg = enumerator.Current as String;
                            lmaxid = arg.StartsWith("0x") ? Int32.Parse(arg.Substring(2), NumberStyles.AllowHexSpecifier) : Int32.Parse(arg, NumberStyles.None);
                        } else

                        if (arg == "-ren" && enumerator.MoveNext()) {
                            arg = enumerator.Current as String;
                            trimsp = (arg == "beg") ? 1 : (arg == "end") ? -1 : 0;
                        } else

                        if (arg == "-mov" && enumerator.MoveNext()) {
                            arg = enumerator.Current as String;
                            if ((shmove = arg[0] == '-' ? -1 : arg[0] == '+' ? +1 : 0) != 0)
                                arg = arg.Substring(1); else shmove = +1;
                            shmove *= arg.StartsWith("0x") ? Int32.Parse(arg.Substring(2), NumberStyles.AllowHexSpecifier) : Int32.Parse(arg, NumberStyles.None);
                        } else

                        if (arg == "-len" && enumerator.MoveNext()) {
                            arg = enumerator.Current as String;
                            lcount = arg.StartsWith("0x") ? Int32.Parse(arg.Substring(2), NumberStyles.AllowHexSpecifier) : Int32.Parse(arg, NumberStyles.None);
                        } else
                        if (arg == "-hex") {
                            outhex = true;
                        } else
                        if (arg == "-del") {
                            outdel = true;
                        } else

                        if (String.IsNullOrWhiteSpace(srsdir)) {
                            srsdir = GetFullPath(arg);
                        } else
                        if (String.IsNullOrWhiteSpace(trgdir)) {
                            trgdir = GetFullPath(arg);
                        } else

                        continue;
                    }
                    if (lcount == 0)
                        lcount = outhex ? 4 : 5;
 
                    if (String.IsNullOrWhiteSpace(srsdir) || String.IsNullOrWhiteSpace(trgdir)) 
                        throw new ArgumentException();

                    var searchPattern = new Regex(@"[\\/][" + imgcls + @"](0x[A-F0-9\-]{4}|[0-9\-]{5})\.(bmp|png|tif|tiff|gif)$", RegexOptions.IgnoreCase);
                    var lfiles = Directory.GetFiles(srsdir, "*", SearchOption.TopDirectoryOnly).Where(f => searchPattern.IsMatch(f)).Select(f => f.Substring(f.LastIndexOf('\\') + 1).ToLower()).ToArray();
                    var entries = lfiles.Select(f => {
                        var _ext = f.Substring(f.LastIndexOf(@"."));
                        var _nam = f.Substring(0, f.Length - _ext.Length);
                        var _hex = _nam.Contains("0x");
                        var _str = _hex ? _nam.LastIndexOf('x') + 1 : _nam.Reverse().SkipWhile(c => !Char.IsDigit(c)).Count();
                        var _len = f.Length - _ext.Length - _str;
                        var _idx = UInt32.Parse(f.Substring(_str, _len), _hex ? NumberStyles.AllowHexSpecifier : NumberStyles.None);
                        return new FileDesc(_ext, _hex, (ushort)_len, (ushort)_idx);
                    }).ToList();
                    entries = entries.Where(e => e.OrigIndex >= lminid && e.OrigIndex <= lmaxid).ToList();
                    entries.Sort(FileDesc.CompareFileDescByOrigIndex);

                    if (trimsp > 0)
                        for (int idx = (int)entries.First().OrigIndex, i = 0; i < entries.Count; ++i)
                            entries[i].SetMoveIndex(idx++);
                    if (trimsp < 0)
                        for (int idx = (int)entries.Last().OrigIndex, i = entries.Count - 1; i >= 0; --i)
                            entries[i].SetMoveIndex(idx--);
                    if (shmove != 0)
                        for (int i = 0; i < entries.Count; ++i)
                            entries[i].SetMoveIndex(entries[i].OrigIndex + shmove);

                    enumerator = entries.GetEnumerator();
                    while (enumerator.MoveNext()) {
                        var entry = (FileDesc)enumerator.Current;
                        var path1 = Path.Combine(srsdir, String.Format("{0}{1}{2}", imgcls.ToUpper(), String.Format((entry.HexFormat ? "0x{0:X" : "{0:D") + entry.OrigCount + "}", entry.OrigIndex), entry.Extension));
                        var path2 = Path.Combine(trgdir, String.Format("{0}{1}{2}", imgcls.ToUpper(), String.Format((outhex ? "0x{0:X" : "{0:D") + entry.OrigCount + "}", entry.MoveIndex), entry.Extension));
                        File.Copy(path1, path2);
                        if (outdel)
                            File.Delete(path1);
                    }
                } else

                if (String.Compare(args[0], "--create", true) == 0) {
                    ResetProcessStatus(1, "Creating container ");
                    var leng = StringToUint(args[from++]);
                    uint size = 0; string fidx = null;
                    try { size = StringToUint(args[from++]); }
                    catch (Exception e) { --from; fidx = GetFullPath(args[from++]); }
                    var fmul = GetFullPath(args[from++]);
                    var smul = new FileStream(fmul, FileMode.Create, FileAccess.Write, FileShare.Read, 0x1000, false);
                    if (fidx == null) {
                        smul.SetLength(leng * size);
                    } else {
                        smul.SetLength(4);
                        var sidx = new FileStream(fidx, FileMode.Create, FileAccess.Write, FileShare.Read, 0x1000, false);
                        var identry = new byte[12] { 0xFF, 0xFF, 0xFF, 0xFF,   0xFF, 0xFF, 0xFF, 0xFF,   0x00, 0x00, 0x00, 0x00 };
                        for (int i = 0; i < leng; ++i)
                            sidx.Write(identry, 0, 12);
                        sidx.Flush();
                        sidx.Close();
                    }
                    smul.Flush();
                    smul.Close();
                    UpdateProcessStatus(1);
                } else
                if (String.Compare(args[0], "--resize", true) == 0) {
                    var leng = StringToUint(args[from++]);
                    var mulc = GetMulContainer(args, ref from);
                    ResetProcessStatus(mulc.EntryLength, "Resizing container ");
                    mulc.Resize(leng);
                    UpdateProcessStatus(mulc.EntryLength);
                } else
                if (String.Compare(args[0], "--defrag", true) == 0) {
                    var mulc = GetMulContainer(args, ref from);
                    ResetProcessStatus(mulc.EntryLength, "Defragmentating container ");
                    mulc.Defrag();
                    UpdateProcessStatus(mulc.EntryLength);
                } else
                if (String.Compare(args[0], "--export", true) == 0) {
                    var fold = GetFullPath(args[from++]);
                    var mulc = GetMulContainer(args, ref from);
                    ResetProcessStatus(mulc.EntryLength * mulc.EntryItemsCount, "Export data ");
                    for (uint it = 0, i = 0; i < mulc.EntryLength; ++i) {
                        if (!mulc.IsValid(i))
                            continue;
                        for (uint c = 0; c < mulc.EntryItemsCount; ++c) {
                            var id = i * mulc.EntryItemsCount + c;
                            var file = Path.Combine(fold, String.Format("{0:000000}.mde", id));
                            var stream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Read, 0x1000, false);
                            if (mulc.IsIndexBased)
                                stream.Write(Utils.StructToBuff(mulc.GetExtra(i)), 0, 4);
                            var data = mulc[id, mulc.EntryItemsCount > 1];
                            stream.Write(data, 0, data.Length);
                            stream.Flush();
                            stream.Close();
                            UpdateProcessStatus(++it);
                        }
                        
                    }
                } else
                if (String.Compare(args[0], "--import", true) == 0) {
                    var fold = GetFullPath(args[from++]);
                    var mulc = GetMulContainer(args, ref from);
                    var fils = Directory.GetFiles(fold, "??????.mde", SearchOption.TopDirectoryOnly);
                    uint it = 0;
                    ResetProcessStatus((uint)fils.Length, "Import data ");
                    foreach (var file in fils) {
                        uint i;
                        if (!uint.TryParse(Path.GetFileNameWithoutExtension(file), out i))
                            continue;
                        var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 0x1000, false);
                        if (mulc.IsIndexBased) {
                            var edat = new byte[4];
                            stream.Read(edat, 0, 4);
                            mulc.SetExtra(i, Utils.BuffToStruct<uint>(edat, 0, 1)[0]);
                        }
                        var data = new byte[stream.Length - stream.Position];
                        stream.Read(data, 0, data.Length);
                        mulc[i, mulc.EntryItemsCount > 1] = data;
                        UpdateProcessStatus(++it);
                    }
                } else
                if (String.Compare(args[0], "--merger", true) == 0) {
                    var fol1 = GetFullPath(args[from++]);
                    var mul1 = GetMulContainer(args, ref from);
                    var fol2 = GetFullPath(args[from++]);
                    var mul2 = GetMulContainer(args, ref from);
                    if (mul1.EntryItemsCount != mul2.EntryItemsCount || mul1.EntryLength != mul2.EntryLength)
                        throw new Exception("Both mul containers must be same type.");
                    var cmpr = Math.Min(mul1.EntryLength, mul2.EntryLength);
                    var item = mul1.EntryItemsCount > 1;
                    ResetProcessStatus(cmpr * mul1.EntryItemsCount, "Merging data ");
                    for (uint it = 0, i = 0; i < cmpr; ++i) {
                        byte[] dat1 = null; 
                        byte[] dat2 = null;
                        if (!mul1.IsValid(i) || !mul2.IsValid(i)) {
                            if (mul1.IsValid(i))
                                dat1 = mul1[i];
                            else if (mul2.IsValid(i))
                                dat2 = mul2[i];
                            else {
                                it += mul1.EntryItemsCount;
                                continue;
                            }
                        }
                        for (uint c = 0; c < mul1.EntryItemsCount; ++c) {
                            var id = i * mul1.EntryItemsCount + c;
                            dat1 = dat1 ?? mul1[id, item];
                            dat2 = dat2 ?? mul2[id, item];
                            if (dat1 != null && dat2 != null && Utils.ArrayIdentical(dat1, dat2)) {
                                ++it;
                                continue;
                            }
                            for (int m = 1; m < 3; ++m) {
                                var data = (m == 1) ? dat1 : dat2;
                                if (data == null)
                                    continue;
                                var mulc = (m == 1) ? mul1 : mul2;
                                var file = Path.Combine((m == 1) ? fol1 : fol2, String.Format("{0:000000}.mde", i));
                                var stream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Read, 0x1000, false);
                                if (mulc.IsIndexBased)
                                    stream.Write(Utils.StructToBuff(mulc.GetExtra(i)), 0, 4);
                                stream.Write(data, 0, data.Length);
                                stream.Flush();
                                stream.Close();
                            }
                            dat1 = dat2 = null;
                            UpdateProcessStatus(++it);
                        }
                    }   
                } else
                if (String.Compare(args[0], "--convtd", true) == 0) {
                    var file = GetFullPath(args[from++]);
                    var fnew = Boolean.Parse(args[from++]);
                    var mulf = GetFullPath(args[from++]);
                    var virt = ContainerFactory.CreateVirtualMul(null, mulf);
                    var land = ContainerFactory.CreateMul(virt, fnew ? 964u : 836u, 0, 0x4000 >> 5);
                    var item = ContainerFactory.CreateMul(virt, fnew ? 1316u : 1188u, (0x4000 >> 5) * (fnew ? 964u : 836u), 0);
                    var tile = new[] {land, item};
                    var dest = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Read, 0x1000, false);
                    var cons = new byte[4] {0x00, 0x00, 0x00, 0x00};
                    ResetProcessStatus(land.EntryLength + item.EntryLength, "Converting tiledata ");
                    for (int it = 0, t = 0; t < 2; ++t) {
                        var size = (t == 0) ? (fnew ? 30 : 26) : (fnew ? 41 : 37);
                        for (uint i = 0; i < tile[t].EntryLength; ++i) {
                            var data = tile[t][i];
                            dest.Write(data, 0, 4);
                            if (fnew) // from new to old
                                for (int offs = 4, e = 0; e < 32; ++e, offs += size) {
                                    dest.Write(data, offs, 4);
                                    dest.Write(data, offs + 8, size - 8);
                                }
                            else // from old to new
                                for (int offs = 4, e = 0; e < 32; ++e, offs += size) {
                                    dest.Write(data, offs, 4);
                                    dest.Write(cons, 0, 4);
                                    dest.Write(data, offs + 4, size - 4);
                                }
                            UpdateProcessStatus((uint)++it);
                        } 
                    }
                    dest.Flush();
                    dest.Close();
                } else
                if (String.Compare(args[0], "--convmt", true) == 0) {
                    var fnew = Boolean.Parse(args[from++]);
                    var idxf = GetFullPath(args[from++]);
                    var mulf = GetFullPath(args[from++]);
                    var mult = ContainerFactory.CreateMul(idxf, mulf);
                    var cons = new byte[4] {0x00, 0x00, 0x00, 0x00};
                    ResetProcessStatus(mult.EntryLength, "Converting multi ");
                    for (uint it = 0, i = 0; i < mult.EntryLength; ++i) {
                        var dbuf = mult[i];
                        var data = new byte[(dbuf.Length / (fnew ? 16 : 12)) * (fnew ? 12 : 16)];
                        if (fnew) // from new to old
                            for (int t = 0, s = 0; s < dbuf.Length; s += 16, t += 12) {
                                Array.Copy(dbuf, s, data, t, 12);
                            }
                        else // from old to new
                            for (int t = 0, s = 0; s < dbuf.Length; s += 12, t += 16) {
                                Array.Copy(dbuf, s, data, t, 12);
                                Array.Copy(cons, 0, data, t + 12, 4);
                            }
                        mult[i] = data;
                        UpdateProcessStatus(++it);
                    }
                } else

                // --------------------------------------------------------------------------
                if (String.Compare(args[0], "--copyid", true) == 0) {
                    var list = GetMultiDimentionList(args, ref from);
                    var mulc = GetMulContainer(args, ref from);
                    var virt = mulc.EntryItemsCount > 1;
                    ResetProcessStatus((uint)list.Count, "Copying entries ");
                    for (int it = 0, i = 0; i < list.Count; ++i) {
                        var sors = list[i][0];
                        var dest = list[i][1];
                        if (mulc.IsIndexBased)
                            mulc.SetExtra(dest, mulc.GetExtra(sors));
                        mulc[dest, virt] = mulc[sors, virt];
                        UpdateProcessStatus((uint)++it);
                    }
                } else
                if (String.Compare(args[0], "--moveid", true) == 0) {
                    var list = GetMultiDimentionList(args, ref from);
                    var mulc = GetMulContainer(args, ref from);
                    var virt = mulc.EntryItemsCount > 1;
                    ResetProcessStatus((uint)list.Count, "Moving entries ");
                    for (int it = 0, i = 0; i < list.Count; ++i) {
                        var sors = list[i][0];
                        var dest = list[i][1];
                        if (mulc.IsIndexBased)
                            mulc.SetExtra(dest, mulc.GetExtra(sors));
                        mulc[dest, virt] = mulc[sors, virt];
                        if (mulc.IsIndexBased)
                            mulc[sors] = null;
                        UpdateProcessStatus((uint)++it);
                    }
                } else
                if (String.Compare(args[0], "--remove", true) == 0) {
                    var list = GetSingleDimentionList(args, ref from);
                    var mulc = GetMulContainer(args, ref from);
                    ResetProcessStatus((uint)list.Count, "Deleting entries ");
                    for (int it = 0, i = 0; i < list.Count; ++i) {
                        mulc[list[i]] = null;
                        UpdateProcessStatus((uint)++it);
                    }
                } else

                // --------------------------------------------------------------------------
                if (String.Compare(args[0], "--facetm", true) == 0) {
                    var folder = GetFullPath(args[from++]);
                    var uodata = (UODataType)StringToUint(args[from++]);
                    var uoopta = args[from++].Split(new []{'|'}, StringSplitOptions.RemoveEmptyEntries).Select(s => (ushort)StringToUint(s)).ToArray();
                    var uoopts = new UODataOptions();
                    for (int a = 0; a < uoopta.Length / 3; a+=3)
                        uoopts.majorFacet[uoopta[a]] = new FacetDesc(String.Format("Facet0{0}", uoopta[a]), uoopta[a+1], uoopta[a+2], uoopta[a+1], uoopta[a+2]);
                    var manager = new UODataManager(new Uri(folder), uodata, Language.English, uoopts, true);
                    var dwriter = manager.DataFactory as IDataFactoryWriter;

                    var frarg = from++;
                    if (String.Compare(args[frarg], "-replid", true) == 0) {
                        byte m; uint x1, y1, x2, y2;
                        var  f = GetMapFacet(args, ref from, manager, out m, out x1, out y1, out x2, out y2);
                        var list = GetMultiDimentionList(args, ref from);
                        ResetProcessStatus((x2 - x1 + 1) * (y2 - y1 + 1), "Replacing land tiles in map ");
                        for (uint it = 1, x = x1; x <= x2; ++x) {
                            for (uint y = y1; y <= y2; ++y, ++it) {
                                var i= f.GetBlockId(x, y);
                                var b = f[i];
                                var c = false;
                                for (uint k = 0; k < 64; ++k) {
                                    var srs = b[k].Land.TileId;
                                    var ent = list.FirstOrDefault(e => e[0] == srs);
                                    if (ent == null)
                                        continue;
                                    b[k].Land.TileId = (ushort)ent[1];
                                    c = true;
                                }
                                if (c) {
                                    var d = b.GetData();
                                    dwriter.SetMapBlock((byte)m, i, d);
                                }
                                UpdateProcessStatus(it);
                                b.Dispose(); 
                            }
                        }
                    } else
                    if (String.Compare(args[frarg], "-reptid", true) == 0) {
                        byte m; uint x1, y1, x2, y2;
                        var  f = GetMapFacet(args, ref from, manager, out m, out x1, out y1, out x2, out y2);
                        var list = GetMultiDimentionList(args, ref from);
                        ResetProcessStatus((x2 - x1 + 1) * (y2 - y1 + 1), "Replacing item tiles in map ");
                        for (uint it = 1, x = x1; x <= x2; ++x) {
                            for (uint y = y1; y <= y2; ++y, ++it) {
                                var i= f.GetBlockId(x, y);
                                var b = f[i];
                                if (b == null) {
                                    UpdateProcessStatus(it);
                                    continue;
                                }
                                var c = false;
                                for (uint k = 0; k < 64; ++k) {
                                    for (int t = 0; t < b[k].Count; ++t) {
                                        var srs = b[k][t].TileId;
                                        var ent = list.FirstOrDefault(e => e[0] == srs);
                                        if (ent == null)
                                            continue;
                                        b[k][t].TileId = (ushort)ent[1];
                                        c = true;
                                    } 
                                }
                                if (c) {
                                    var d = b.GetData();
                                    dwriter.SetMapBlock((byte)m, i, d);
                                }
                                UpdateProcessStatus(it);
                                b.Dispose();
                            }
                        }
                    } else
                    if (String.Compare(args[frarg], "-rephid", true) == 0) {
                        byte m; uint x1, y1, x2, y2;
                        var  f = GetMapFacet(args, ref from, manager, out m, out x1, out y1, out x2, out y2);
                        var list = GetMultiDimentionList(args, ref from);
                        ResetProcessStatus((x2 - x1 + 1) * (y2 - y1 + 1), "Replacing item colors in map ");
                        for (uint it = 1, x = x1; x <= x2; ++x) {
                            for (uint y = y1; y <= y2; ++y, ++it) {
                                var i= f.GetBlockId(x, y);
                                var b = f[i];
                                if (b == null) {
                                    UpdateProcessStatus(it);
                                    continue;
                                }
                                var c = false;
                                for (uint k = 0; k < 64; ++k) {
                                    for (int t = 0; t < b[k].Count; ++t) {
                                        var srs = b[k][t].Palette;
                                        var ent = list.FirstOrDefault(e => e[0] == srs);
                                        if (ent == null)
                                            continue;
                                        b[k][t].Palette = (ushort)ent[1];
                                        c = true;
                                    } 
                                }
                                if (c) {
                                    var d = b.GetData();
                                    dwriter.SetMapBlock((byte)m, i, d);
                                }
                                UpdateProcessStatus(it);
                                b.Dispose();
                            }
                        }
                    } else

                        throw new Exception();

                } else


                // --------------------------------------------------------------------------
                if ((String.Compare(args[0], "--help", true) == 0) || (String.Compare(args[0], "-help", true) == 0) 
                    || (String.Compare(args[0], "--h", true) == 0) || (String.Compare(args[0], "-h", true) == 0)
                    || (String.Compare(args[0], "--?", true) == 0) || (String.Compare(args[0], "-?", true) == 0)
                    || (String.Compare(args[0], "\\?", true) == 0) || (String.Compare(args[0], "/?", true) == 0)
                    || (String.Compare(args[0], "help", true) == 0)) {
                    Console.WriteLine(usage);
                } else
                    throw new Exception();
            #if !DEBUG
            } catch(Exception e) {
                Console.WriteLine("Proccess aborted: Ither bad agruments or something real very very bad happened..");
                Console.WriteLine("Run application wiht \"--help\" argumnet for additional info.");              
                var exception = e;
                while (exception != null) {
                    Console.WriteLine("\n\nError: {0}\n\n{1}", exception.Message, exception.StackTrace);
                    exception = exception.InnerException;
                }
            }
            #endif
        }


        private class FileDesc // : IEnumerable<FileDesc>
        {
            public string Extension { get; private set; }
            public bool   HexFormat { get; private set; }
            public ushort OrigIndex { get; private set; }
            public ushort OrigCount { get; private set; }
            public ushort MoveIndex { get; private set; }

            internal FileDesc(string ext, bool hex, ushort len, ushort idx)
            {
                Extension = ext;
                HexFormat = hex;
                OrigIndex = idx;
                OrigCount = len;
                MoveIndex = idx;
            }

            internal void SetMoveIndex(int idx)
            {
                MoveIndex = (ushort)idx;
            }

            internal static int CompareFileDescByOrigIndex(FileDesc l, FileDesc r)
            {
                return (l.OrigIndex < r.OrigIndex) ? -1 : (l.OrigIndex == r.OrigIndex) ? 0 : +1;
            }

            internal static int CompareFileDescByMoveIndex(FileDesc l, FileDesc r)
            {
                return (l.MoveIndex < r.MoveIndex) ? -1 : (l.MoveIndex == r.MoveIndex) ? 0 : +1;
            }
        }


        private static readonly char[] separators = new[] {' ', '\t', '-', '>', '<', '=', ','};

        private static List<uint> GetSingleDimentionList(string[] args, ref int from)
        {
            List<uint> list = null;
            try {
                var f = from;
                var id1 = UInt32.Parse(args[f++]);
                list = new List<uint>(new [] {id1});
                from += 1;
            } catch(Exception e) {
                var file = GetFullPath(args[from++]); 
                var lins = File.ReadAllLines(file).Where(s => !String.IsNullOrWhiteSpace(s)).Select(s => s.Trim(' ', '\t')).ToList();
                list = new List<uint>(lins.Count);
                for (int i = 0; i < lins.Count; ++i) {
                    var word = lins[i].Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    list.Add(StringToUint(word[0]));
                }
            }
            return list;
        }

        private static List<uint[]> GetMultiDimentionList(string[] args, ref int from)
        {
            List<uint[]> list = null;
            try {
                var f = from;
                var id1 = UInt32.Parse(args[f++]);
                var id2 = UInt32.Parse(args[f++]);
                list = new List<uint[]>(new uint[][] { new [] {id1, id2} });
                from += 2;
            } catch(Exception e) {
                var file = GetFullPath(args[from++]); 
                var lins = File.ReadAllLines(file).Where(s => !String.IsNullOrWhiteSpace(s)).Select(s => s.Trim(' ', '\t')).ToList();
                list = new List<uint[]>(lins.Count);
                var prev = 0xFFFFFFFFu;
                for (int i = 0; i < lins.Count; ++i) {
                    var word = lins[i].Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    if (word.Length == 1) {
                        if (prev == 0xFFFFFFFFu)
                            throw new Exception();
                        ++prev;
                    } else {
                        prev = StringToUint(word[1]);
                    }
                    list.Add(new[] { StringToUint(word[0]), prev });
                }
            }
            return list;
        }

        private static IMapFacet GetMapFacet(string[] args, ref int from, UODataManager manager, out byte m, out uint x1, out uint y1, out uint x2, out uint y2)
        {
            m = (byte)StringToUint(args[from++]);
            var f = manager.GetMapFacet(m);
            x1 = StringToUint(args[from++]);
            y1 = StringToUint(args[from++]);
            x2 = StringToUint(args[from++]);
            y2 = StringToUint(args[from++]);
            if (x2 == 0) x2 = f.Width - 1;
            if (y2 == 0) y2 = f.Height - 1;
            return f;
        }

        private static IDataContainer GetMulContainer(string[] args, ref int from)
        {
            try {
                var  soff = StringToUint(args[from++]);
                var  leng = StringToUint(args[from++]);
                uint size; string fidx = null;
                if (!uint.TryParse(args[from++], out size)) {
                    --from;
                    fidx = GetFullPath(args[from++]);
                    size = 0;
                } else {
                    uint count;
                    if (!uint.TryParse(args[from++], out count)) {
                        --from;
                    } else {
                        var head = size;
                        size = count;
                        count = StringToUint(args[from++]);
                        var path = GetFullPath(args[from++]);
                        if ((soff == 0) && (leng == 0)) {
                            return ContainerFactory.CreateMul(head, size, count, path);
                        } else {
                            var virt = ContainerFactory.CreateVirtualMul(null, path);
                            return ContainerFactory.CreateMul(virt, head, size, count, soff, leng);
                        }
                    }
                }
                var  fmul = GetFullPath(args[from++]);

                if ((soff == 0) && (leng == 0)) {
                    return size == 0
                        ? ContainerFactory.CreateMul(fidx, fmul)
                        : ContainerFactory.CreateMul(size, fmul);
                } else {
                    var virt = ContainerFactory.CreateVirtualMul(fidx, fmul);
                    return size == 0
                        ? ContainerFactory.CreateMul(virt, soff, leng)
                        : ContainerFactory.CreateMul(virt, size, soff, leng);
                }
            } catch(Exception e) {
                return null;
            }
        }

        private static string GetFullPath(string path)
        {
            if (path == null)
                return null;
            path = path.TrimEnd('\\', '/', ' ', '\t');
            if (path.Length >= 2 && path[1] == ':')
                return path;
            if (String.IsNullOrWhiteSpace(path))
                return Environment.CurrentDirectory;
            return Path.Combine(Environment.CurrentDirectory, path);
        }

        private static uint StringToUint(string value)
        {
            var val = value.Trim(' ', '\t', '\r', '\n');
            if (String.IsNullOrEmpty(val))
                throw new Exception();

            if (val.StartsWith("0x", true, null))
                return UInt32.Parse(val.Substring(2), NumberStyles.AllowHexSpecifier);
            else
                return UInt32.Parse(val);
        }

        private static int  curleft = 0, workset = 0, lastupd = 0, maxiter = 0;
        private static void UpdateProcessStatus(uint curIteration)
        {
            if ((Environment.TickCount - lastupd > 100) || (curIteration == maxiter)) {
                //GC.Collect();
                workset = (int)(Environment.WorkingSet >> 20);
                lastupd = Environment.TickCount;

                Console.SetCursorPosition(curleft, Console.CursorTop);
                Console.Write("{0,6:0.00}% > RAM: {1} MiB", (100f * curIteration) / maxiter, workset);
            }
        }

        private static void ResetProcessStatus(uint allIteration, string format, params object[] args)
        {
            Console.Write(format, args);
            curleft = Console.CursorLeft;
            workset = lastupd = 0;
            maxiter = (int)allIteration;
        }

    }
}
