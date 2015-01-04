using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
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
            + "\n"
            + "\nUsage: run EssenceUDK.Tool with command."
            + "\n"
            + "\nCommands:"
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
            + "\n     -reptid    - Replace all static tiles with specified Index by new Index"
            + "\n      Arguments:"
            + "\n        $MPAREA$ $LIST#2$"
            + "\n"
            + "\n     -rephid    - Replace all static colors with specified Index by new Index"
            + "\n      Arguments:"
            + "\n        $MPAREA$ $LIST#2$"
            + "\n"
            + "\n"
            + "\nAllias:"
            + "\n"
            + "\n  $MULCON$  ->  <byte_offset> <entry_length> <entry_size> <mulfile>"
            + "\n  $MULCON$  ->  <byte_offset> <entry_length> <idxfile> <mulfile>"
            + "\n  $LIST#1$  ->  <index>"
            + "\n  $LIST#1$  ->  <txtfile>"
            + "\n  $LIST#2$  ->  <srs_index> <dst_index>"
            + "\n  $LIST#2$  ->  <txtfile>"
            + "\n  $MPAREA$  ->  <map_index> <block_x1> <block_y1> <block_x2> <block_y2>"
            + "\n"
            + "\n"
            + "\nNotes:"
            + "\n"
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
            + "\n"
            + "\nInfo about muls:"
            + "\n| file name    |  entry size | entry count  |"
            + "\n|--------------|------------|--------------|"
            + "\n| tiledata.mul |   836 / 964 |          512 | - land tiles"
            + "\n| tiledata.mul | 1188 / 1316 | 512/1024/2046| - item tiles(off:428032/493568)"
            + "\n| animdata.mul |         548 |    items / 8 |"
            + "\n| radarcol.mul |           2 | land + items |"
            + "\n| hues.mul     |         708 |          375 |"
            + "\n"
            ;

        // "--create <entry_count> <entry_size> <mulfile>"
        // "--create <entry_count> <idxfile> <mulfile>"
        // "--resize <entry_count> $MULCON$"
        // "--defrag $MULCON$"
        // "--export <dirpath> $MULCON$"
        // "--import <dirpath> $MULCON$"
        // "--merger <dirpath> $MULCON$ <dirpath> $MULCON$"
        // "--convtd <dstfile> <newformat> <mulfile>"

        // "--copyid $LIST#2$ $MULCON$"
        // "--moveid $LIST#2$ $MULCON$"
        // "--remove $LIST#1$ $MULCON$"

        // "--facetm <folder> <uodata> <uoopts> $MAP_COMMAND$"

        // $MAP_COMMAND$ -?
        // -replid $MPAREA$ $LIST#2$
        // -reptid $MPAREA$ $LIST#2$
        // -rephid $MPAREA$ $LIST#2$

        // "$MULCON$  ->  <byte_offset> <entry_length> <entry_size> <mulfile>"
        // "$MULCON$  ->  <byte_offset> <entry_length> <idxfile> <mulfile>"
        // "$LIST#1$  ->  <index>"
        // "$LIST#1$  ->  <txtfile>"
        // "$LIST#2$  ->  <srs_index> <dst_index>"
        // "$LIST#2$  ->  <txtfile>"
        // "$MPAREA$  ->  <map_index> <block_x1> <block_y1> <block_x2> <block_y2>"

        static void Main(string[] args)
        {
            try {
                int from = 1;
                if (String.Compare(args[0], "--create", true) == 0) {
                    ResetProcessStatus(1, "Creating container ");
                    var leng = Convert.ToUInt32(args[from++]);
                    uint size = 0; string fidx = null;
                    try { size = Convert.ToUInt32(args[from++]); }
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
                    var leng = Convert.ToUInt32(args[from++]);
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
                    ResetProcessStatus(mulc.EntryLength, "Export data ");
                    for (uint it = 0, i = 0; i < mulc.EntryLength; ++i) {
                        if (!mulc.IsValid(i))
                            continue;
                        var file = Path.Combine(fold, String.Format("{0:000000}.mde", i));
                        var stream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Read, 0x1000, false);
                        if (mulc.IsIndexBased)
                            stream.Write(Utils.StructToBuff(mulc.GetExtra(i)), 0, 4);
                        var data = mulc[i];
                        stream.Write(data, 0, data.Length);
                        stream.Flush();
                        stream.Close();
                        UpdateProcessStatus(++it);
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
                        mulc[i] = data;
                        UpdateProcessStatus(++it);
                    }
                } else
                if (String.Compare(args[0], "--merger", true) == 0) {
                    var fol1 = GetFullPath(args[from++]);
                    var mul1 = GetMulContainer(args, ref from);
                    var fol2 = GetFullPath(args[from++]);
                    var mul2 = GetMulContainer(args, ref from);
                    var cmpr = Math.Min(mul1.EntryLength, mul2.EntryLength);
                    ResetProcessStatus(cmpr, "Merging data ");
                    for (uint it = 0, i = 0; i < cmpr; ++i) {
                        byte[] dat1 = null; 
                        byte[] dat2 = null;                  
                        if (!mul1.IsValid(i) || !mul2.IsValid(i)) {
                            if (mul1.IsValid(i))
                                dat1 = mul1[i];
                            else if (mul2.IsValid(i))
                                dat2 = mul2[i];
                            else
                                continue;
                        } else {
                            dat1 = mul1[i];
                            dat2 = mul2[i];
                            if (Utils.ArrayIdentical(dat1, dat2))
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
                        UpdateProcessStatus(++it);
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
                        }
                        UpdateProcessStatus((uint)++it);
                    }
                    dest.Flush();
                    dest.Close();
                } else

                // --------------------------------------------------------------------------
                if (String.Compare(args[0], "--copyid", true) == 0) {
                    var list = GetMultiDimentionList(args, ref from);
                    var mulc = GetMulContainer(args, ref from);
                    ResetProcessStatus((uint)list.Count, "Copying entries ");
                    for (int it = 0, i = 0; i < list.Count; ++i) {
                        var sors = list[i][0];
                        var dest = list[i][1];
                        if (mulc.IsIndexBased)
                            mulc.SetExtra(dest, mulc.GetExtra(sors));
                        mulc[dest] = mulc[sors];
                        UpdateProcessStatus((uint)++it);
                    }
                } else
                if (String.Compare(args[0], "--moveid", true) == 0) {
                    var list = GetMultiDimentionList(args, ref from);
                    var mulc = GetMulContainer(args, ref from);
                    ResetProcessStatus((uint)list.Count, "Moving entries ");
                    for (int it = 0, i = 0; i < list.Count; ++i) {
                        var sors = list[i][0];
                        var dest = list[i][1];
                        if (mulc.IsIndexBased)
                            mulc.SetExtra(dest, mulc.GetExtra(sors));
                        mulc[dest] = mulc[sors];
                        if (mulc.IsIndexBased)
                            mulc.Delete(sors);
                        UpdateProcessStatus((uint)++it);
                    }
                } else
                if (String.Compare(args[0], "--remove", true) == 0) {
                    var list = GetSingleDimentionList(args, ref from);
                    var mulc = GetMulContainer(args, ref from);
                    ResetProcessStatus((uint)list.Count, "Deleting entries ");
                    for (int it = 0, i = 0; i < list.Count; ++i) {
                        mulc.Delete(list[i]);
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
                            for (uint y = y1; y <= y2; ++y) {
                                var i= f.GetBlockId(x, y);
                                var b = f[i];
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
                            for (uint y = y1; y <= y2; ++y) {
                                var i= f.GetBlockId(x, y);
                                var b = f[i];
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
            } catch(Exception e) {
                Console.WriteLine("Proccess aborted: Ither bad agruments or something real very very bad happened..");
                Console.WriteLine("Run application wiht \"--help\" argumnet for additional info.");
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
                var  soff = Convert.ToUInt32(args[from++]);
                var  leng = Convert.ToUInt32(args[from++]);
                uint size; string fidx = null;
                if (!uint.TryParse(args[from++], out size)) {
                    fidx = GetFullPath(args[from++]);
                    size = 0;
                }
                var  fmul = GetFullPath(args[from++]);

                if (soff == 0 && leng == 0) {
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
            if (path.Length >= 2 && path[1] == ':')
                return path;
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
            if (Environment.TickCount - lastupd > 100 || curIteration == maxiter) {
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
