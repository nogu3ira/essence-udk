﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using EssenceUDK.Platform.DataTypes;
using EssenceUDK.Platform.Factories;

namespace EssenceUDK.Platform.TileEngine
{
    public class FacetRender
    {
        private UODataManager dataManager;
        private TilesComparer tileComparer;
        private List<IEntryMapTile> tileCache;

        internal FacetRender(UODataManager manager)
        {
            dataManager = manager;
            if (dataManager.DataFactory == null)
                throw new NullReferenceException("DataFactory wasn't initialized.");

            tileCache = new List<IEntryMapTile>(128);
            tileComparer = new TilesComparer(dataManager);

            InitFlatRender();
            InitObliqueRender();
        }

        internal class TilesComparer : IComparer<IEntryMapTile>
        {
            private UODataManager dataManager;
            internal sbyte MinFilterZ { get; set; }
            internal sbyte MaxFilterZ { get; set; }

            public TilesComparer(UODataManager manager)
            {
                dataManager = manager;
                MinFilterZ = -128;
                MaxFilterZ = +127;
            }

            public int Compare(IEntryMapTile l, IEntryMapTile r)
            {
                //  0 ->  l = r
                // -1 ->  l < r
                // +1 ->  l > r
                var pl = CalcPrior(l);
                var pr = CalcPrior(r);

                if (pl == pr)
                    return 0;
                return pl > pr ? +1 : -1;
            }

            private int CalcPrior(IEntryMapTile e)
            {
                int prior = e.Altitude;
                var t = e as IItemMapTile;
                if (t != null) {
                    var dt = dataManager.GetItemTile(e.TileId);

                    if (dt.Height > 0)
                        ++prior;
                    //if (dl.Flags.HasFlag(TileFlag.Wet))
                    //    --pl;
                    if (dt.Flags.HasFlag(TileFlag.Surface))
                        --prior;
                    if (dt.Flags.HasFlag(TileFlag.Background))
                        --prior;
                    if (dt.Flags.HasFlag(TileFlag.Foliage))
                        ++prior;
                    if (dt.Flags.HasFlag(TileFlag.Wall) || dt.Flags.HasFlag(TileFlag.Window))
                        ++prior;
                } else
                    prior -= 5;

                return prior;
            }

            private static ushort[] nodraw_lands = { 0x0002, 0x01AF, 0x01B0, 0x01B1, 0x01B2, 0x01B3, 0x01B4, 0x01B5 };
            private static ushort[] nodraw_items = { 0x0001, 0x2198, 0x2199, 0x219A, 0x219B, 0x219C, 0x219D, 0x219E, 
                                                     0x219F, 0x21A0, 0x21A1, 0x21A2, 0x21A3, 0x21A4, 0x21BC, 0x5690, 0x0490};

            internal bool IsValid(IEntryMapTile e)
            {
                var t = e as IItemMapTile;
                if (t != null) {
                    if (nodraw_items.Contains(e.TileId))
                        return false;
                } else {
                    if (nodraw_lands.Contains(e.TileId))
                        return false;
                }

                return (e.Altitude >= MinFilterZ) && (e.Altitude <= MaxFilterZ);
            }
        }

        private delegate void DrawTexm(ISurface srs, sbyte z, sbyte zl, sbyte zr, sbyte zd, ref ISurface dst, int cx, int cy);
        private delegate void DrawTile(ISurface srs, sbyte z, ushort hue, byte alpha, ref ISurface dst, int cx, int cy);
        private void DrawBlock(IMapBlock mapblock, uint x, uint y, IMapBlock[][] block, uint b, ref ISurface dest, int dest_cx, int dest_cy, DrawTile drawtile, DrawTexm drawtexm)
        {
            var maptile = mapblock[x, y]; 

            tileCache.Clear();
            if (tileComparer.IsValid(maptile.Land))
                tileCache.Add(maptile.Land);
            for (int i = 0; i < maptile.Count; ++i)
                if (tileComparer.IsValid(maptile[i]))
                    tileCache.Add(maptile[i]);
            tileCache.Sort(tileComparer);

            for (int t = 0; t < tileCache.Count; ++t) {
                var tile = tileCache[t];
                if (tile as ILandMapTile != null) {
                    var landtile = dataManager.GetLandTile(tile.TileId);
                    if (!landtile.IsValid)
                        continue;

                    sbyte ul, ur, ud;

                    if (y < 7) {
                        ul = mapblock[x, y + 1u].Land.Altitude;
                    } else {
                        ul = block[0][b+1][x,0].Land.Altitude;
                    }
                    if (x < 7) {
                        ur = mapblock[x + 1u, y].Land.Altitude;
                    } else {
                        ur = block[1][b][0,y].Land.Altitude;
                    }
                    if (y < 7 && x < 7) {
                        ud = mapblock[x + 1u, y + 1u].Land.Altitude;
                    } else if (x != 7) {
                        ud = block[0][b+1][x+1u,0].Land.Altitude;
                    } else if (y != 7) {
                        ud = block[1][b][0,y+1u].Land.Altitude;
                    } else {
                        ud = block[1][b+1][0,0].Land.Altitude;
                    }

                    var tl = ((int)tile.Altitude - ul);
                    var tr = ((int)tile.Altitude - ur);
                    var td = ((int)tile.Altitude - ud);

                    if ((landtile.Texture != null) && ((td != 0) || (tl != 0) || (tr != 0))) {
                        drawtexm(landtile.Texture, tile.Altitude, ul, ur, ud, ref dest, dest_cx, dest_cy);
                    } else {
                        drawtile(landtile.Surface, tile.Altitude, 0, 0xFF, ref dest, dest_cx, dest_cy);
                    }
                    continue;
                }

                var itemtile = dataManager.GetItemTile(tile.TileId);
                if (itemtile.IsValid)
                    drawtile(itemtile.Surface, tile.Altitude, 0, itemtile.Flags.HasFlag(TileFlag.Translucent)
                                                    ? (byte)alphavl : (byte)0xFF, ref dest, dest_cx, dest_cy);
            }
        }

        private delegate void DrawBlocks(ref ISurface dest, int cx, int cy, IMapBlock[][] block, uint b1, uint b2, byte x1, byte x2, byte y1, byte y2);
        private void DrawFacet(int icx, int icy, int bxdw, int bxdh, int bydw, int bydh, byte map, short sealvl, ref ISurface dest, byte range, ushort tx, ushort ty, sbyte minz, sbyte maxz, DrawBlocks drawblocks)
        {
            if (dest.PixelFormat != PixelFormat.Bpp16A1R5G5B5 && dest.PixelFormat != PixelFormat.Bpp16X1R5G5B5)
                throw new ArgumentException("Only 16 bpp surfaces with pixel format A1R5G5B5 or X1R5G5B5 are supported.");

            var facet = dataManager.GetMapFacet(map);
            tileComparer.MinFilterZ = minz;
            tileComparer.MaxFilterZ = maxz;

            var tx1 = tx - range;
            var ty1 = ty - range;
            var tx2 = tx + range;
            var ty2 = ty + range;

            var bx0 = (uint)(tx  / 8);
            var by0 = (uint)(ty  / 8);
            var bx1 = (uint)(tx1 / 8);
            var by1 = (uint)(ty1 / 8);
            var bx2 = (uint)(tx2 / 8);
            var by2 = (uint)(ty2 / 8);

            var x1 = (byte)(tx1 % 8);
            var y1 = (byte)(ty1 % 8);
            var x2 = (byte)(tx2 % 8);
            var y2 = (byte)(ty2 % 8);
            int dest_cx, dest_cy;

            IMapBlock[][] blocks = new IMapBlock[2][];
            blocks[0] = new IMapBlock[2+by2-by1];
            blocks[1] = new IMapBlock[2+by2-by1];
            for (uint i = 0, by = by1; by <= by2+1; ++by, ++i)
                blocks[1][i] = facet[facet.GetBlockId(bx1, by)];

            lock (dest) {
                //bx1 = bx2 = bx0;
                //by1 = by2 = by0;
                //x1 = y1 = 0;
                //x2 = y2 = 7;

                for (uint bx = bx1; bx <= bx2; ++bx) {
                    Array.Copy(blocks[1], blocks[0], 2 + by2 - by1);
                    for (uint i = 0, by = by1; by <= by2+1; ++by, ++i)
                        blocks[1][i] = facet[facet.GetBlockId(bx+1, by)];

                    var ox1 = bx == bx1 ? x1 : (byte)0;
                    var ox2 = bx == bx2 ? x2 : (byte)7;
                    dest_cx = (int)(icx + bxdw * (bx - bx0) - bxdh * (by1 - by0));
                    dest_cy = (int)(icy + bydw * (bx - bx0) + bydh * (by1 - by0));
                    drawblocks(ref dest, dest_cx, dest_cy, blocks, 0, by2 - by1, ox1, ox2, y1, y2);
                    //DrawCross(dest, dest_cx, dest_cy, 0xFC1F, 5);
                }
            }         
        }

        #region Notes

        // DrawTexm ( 
        //      ISurface    srs         - sourse surface of 16 bit texture 64x64 or 128x128
        //      sbyte       z           - altitude of tile (i.e. upper corner)
        //      sbyte       zl          - z - Altitude of left corner (tile with coords x,y+1)
        //      sbyte       zr          - z - Altitude of right corner (tile with coords x+1,y)
        //      sbyte       zd          - z - Altitude of bottom corner (tile with coords x+1,y+1)
        //      ISurface    dst         - destenation surface to draw
        //      int         cx          - x position on dest surface of bottom tile corner (i.e. [dest_rx] -> [sors_x = srs.width /2])
        //      int         cy          - y position on dest surface of bottom tile corner (i.e. [dest_by] -> [sors_y = srs.height-1])
        // )
        //
        // DrawTile ( 
        //      ISurface    srs         - sourse surface of 16 bit texture 64x64 or 128x128
        //      sbyte       z           - altitude of tile (i.e. upper corner)
        //      ushort      hue         - palette index in hues.mul (0 - for non color)
        //      sbyte       alpha       - alpha chanel for source surface (optimesed for alphavl const)
        //      ISurface    dst         - destenation surface to draw
        //      int         cx          - x position on dest surface of bottom tile corner (i.e. [dest_rx] -> [sors_x = srs.width /2])
        //      int         cy          - y position on dest surface of bottom tile corner (i.e. [dest_by] -> [sors_y = srs.height-1])
        // )
        //
        // DrawBlock (
        //      IMapBlock   mapblock    - curent block
        //      uint        x           - x coordinate of center tile in center block
        //      uint        y           - y coordinate of center tile in center block
        //      IMapBlock[2][] block    - column of blocks (x = const), where x of first column is same as for center block, and for next column x = x+1
        //      uint        b           - index of curent block (i.e. mapblock == block[0][b])
        //      ISurface    dest        - destenation surface to draw
        //      int         dest_cx     - x position of bottom tile corner (i.e. [dest_rx] -> [sors_x = srs.width /2])
        //      int         dest_cy     - y position of bottom tile corner (i.e. [dest_by] -> [sors_y = srs.height-1])
        //      DrawTile    drawtile    - delegate for draw tile
        //      DrawTexm    drawtexm    - delegate for draw texture
        // )
        //
        // DrawBlocks (
        //      ISurface    dest        - destenation surface to draw
        //      int         cx          - x position on screen for bottom tile(x=0,y=0) corner of first block
        //      int         cy          - y position on screen for bottom tile(x=0,y=0) corner of first block
        //      IMapBlock[2][] block    - 
        //      uint        b1          - index of first processing block from <block> variable
        //      uint        b2          - index of last  processing block from <block> variable
        //      byte        x1          - process tiles with x >= x1
        //      byte        x2          - process tiles with x <= x2
        //      byte        y1          - process tiles in block [0][b1] with y >= y1
        //      byte        y2          - process tiles in block [0][b2] with y <= y2
        // )
        //
        // DrawFacet (
        //      int         icx         - x position on screen for bottom tile(x=0,y=0) corner of center block
        //      int         icy         - y position on screen for bottom tile(x=0,y=0) corner of center block
        //      int         bxdw        - 
        //      int         bxdh        - 
        //      int         bydw        - 
        //      int         bydh        - 
        //      byte        map         - map index
        //      sbyte       sealvl      - sea level (for moving result image to get better result)
        //      ISurface    dest        - destenation surface to draw
        //      byte        range       - distance in tiles to draw
        //      ushort      tx          - target X coordinate of center tile
        //      ushort      ty          - target Y coordinate of center tile
        //      sbyte       minz        - minimum altitude for drawing tiles
        //      sbyte       maxz        - maximum altitude for drawing tiles
        //      DrawBlocks  drawblocks  - delegate for drawing blocks
        // )


        #endregion

        // Just for debuging purposes (red - 0xFC00, green - 0x83E0, blue - 0x801F, yellow - 0xFFE0, azure - 0x83FF, purple - 0xFC1F)
        private unsafe void DrawCross(ISurface dest, int cx, int cy, ushort color, ushort cs, bool locked = true)
        {
            if (!locked)
                lock (dest) {
                    DrawCross(dest, cy, cx, color, cs, true);
                }
            else {
                var dst_strd = dest.Stride >> 1;
                ushort* dst_pixl;

                if (cx >= 0 && cx < dest.Width)
                    for (int y = cy - cs; y <= cy + cs; ++y) {
                        if (y < 0 || y >= dest.Height)
                            continue;
                        dst_pixl = dest.ImageWordPtr + y*dst_strd + cx;
                        *dst_pixl = color;
                    }

                if (cy >= 0 && cy < dest.Height)
                    for (int x = cx - cs; x <= cx + cs; ++x) {
                        if (x < 0 || x >= dest.Width)
                            continue;
                        dst_pixl = dest.ImageWordPtr + cy*dst_strd + x;
                        *dst_pixl = color;
                    }
            }
        }


        #region Flat Render

        private static double scale = 0.549;
        private static double angle = -Math.PI / 4.0;
        private static double sin45 = Math.Sin(angle);
        private static double cos45 = Math.Cos(angle);


        private static int      srsw = 400;
        private static int      srsh = 500;

        private static int     srlx;
        private static int     srly;
        private static int[][] srsx = null;
        private static int[][] srsy = null;
        private static FlatPoint[][] srsp = null;
        private class FlatPoint {
            internal short X1, X2, Y1, Y2;
            internal short   YOffset;
            internal short[] XOffset;
            internal short[] XLength;
            internal FlatPoint(int sh, int sw)
            {
                var sx_l = -sw/2;
                    var sx_r = +sw/2-1;
                    var sy_u = -(sh-1);

                    var p = this;
                    p.X1 = (short)(((double)sx_l * cos45 - (double)sy_u * sin45) * scale);
                    p.X2 = (short)(((double)sx_r * cos45 - (double)( 0) * sin45) * scale);
                    p.Y1 = (short)(((double)sx_r * sin45 + (double)sy_u * cos45) * scale);
                    p.Y2 = (short)(((double)sx_l * sin45 + (double)( 0) * cos45) * scale);

                    var x1y = (short)(((double)sx_l * sin45 + (double)sy_u * cos45) * scale);
                    var x2y = (short)(((double)sx_r * sin45 + (double)( 0) * cos45) * scale);
                    //var y1x = (short)(((double)sx_r * cos45 + (double)sy_u * sin45) * scale);
                    //var y2x = (short)(((double)sx_l * cos45 + (double)( 0) * sin45) * scale);

                    var off = (short)(((double)sx_r * cos45 - (double)sy_u * sin45) * scale);
                    var len = (short)1;

                    //var pxl = p.X2 - p.X1 + 1;
                    var pyl = p.Y2 - p.Y1 + 1;
                    //p.XRunOff = new short[pyl];
                    p.XOffset = new short[pyl];
                    p.XLength = new short[pyl];
                    p.YOffset = (short)(p.Y1 - srly);

                    off -= (short)srlx;
                    x1y -= (short)p.Y1;
                    x2y -= (short)p.Y1;
                    var inc_off = (short)-1;
                    var inc_len = (short)+2;
                    for (int y = 0; y < pyl; ++y) {
                        p.XOffset[y] = off;
                        p.XLength[y] = len;

                        if (y == x1y) {
                            inc_off = (short)+1;
                            inc_len -= 2;
                        }
                        if (y == x2y) {
                            inc_len -= 2;
                        }

                        off += inc_off;
                        len += inc_len;
                    
                        var _y = p.YOffset +y;
                        var _x = p.XOffset[y];
                        var _l = p.XLength[y];
                        for (int c = _x; c < _x + _l; ++c) {
                            var _sx = srsx[_y][c];
                            var _sy = srsy[_y][c];

                            if (_sx == 0xDEAD)
                                srsx[_y][c] = sx_l;

                            if (_sy == 0xDEAD)
                                srsy[_y][c] = sy_u;

                            if (_sx < sx_l)
                                srsx[_y][c] = sx_l;// + 1;
                            if (_sx > sx_r)
                                srsx[_y][c] = sx_r;// - 1;
                            if (_sy < sy_u)
                                srsy[_y][c] = sy_u;//+ 1;
                            if (_sy > 0)
                                srsy[_y][c] = 0;

                            #if DEBUG
                            //if (_sx < sx_l || _sx > sx_r || _sy < sy_u) {    
                            //    Debug.Assert(true, "Fuck it's out of sors!!!!");
                            //}   
                            #endif
                        }

                        
                    }
            }
        }

        private static void InitFlatRender()
        {
            if (srsp != null)
                return;

            // Coordinates for rotated max size surface
            var dx1 = (int)(((double)(-srsw / 2) * cos45 - (double)(-srsh) * sin45) * scale);
            var dx2 = (int)(((double)(+srsw / 2) * cos45 - (double)(    0) * sin45) * scale);
            var dy1 = (int)(((double)(+srsw / 2) * sin45 + (double)(-srsh) * cos45) * scale);
            var dy2 = (int)(((double)(-srsw / 2) * sin45 + (double)(    0) * cos45) * scale);
            var dxl = dx2 - dx1 + 2;
            var dyl = dy2 - dy1 + 1;

            srlx = dx1;
            srly = dy1;
            srsx = new int[dyl][];
            srsy = new int[dyl][];
            for (int dy = 0; dy < dyl; ++dy) {
                srsx[dy] = new int[dxl];
                srsy[dy] = new int[dxl];
                //Array.Clear(srsx[dy], 0, dxl);
                //Array.Clear(srsy[dy], 0, dxl);
                for (int dx = 0; dx < dxl; ++dx) {
                    srsx[dy][dx] = 0xDEAD;
                    srsy[dy][dx] = 0xDEAD;
                }
            }

            for (int sy = 0; sy >= -srsh; --sy) {
                for (int sx = 0; sx >= -srsw/2; --sx) {
                    label_process_sx_sy: 
                    var dx = (int)(((double)sx * cos45 - (double)sy * sin45) * scale) - dx1;
                    var dy = (int)(((double)sx * sin45 + (double)sy * cos45) * scale) - dy1;
                    //if (srsx[dy][dx] == 0xDEAD || Math.Abs(sx) < Math.Abs(srsx[dy][dx]))
                        srsx[dy][dx] = sx;
                    //if (srsy[dy][dx] == 0xDEAD || Math.Abs(sy) < Math.Abs(srsy[dy][dx]))
                        srsy[dy][dx] = sy;


                    bool bx1 = dx > 0;
                    bool bx2 = dx < dxl-1;
                    bool by1 = dy > 0;
                    bool by2 = dy < dyl - 1;

                    if (by1) {
                        if (srsx[dy-1][dx] == 0xDEAD)
                            srsx[dy-1][dx] = sx+1;    
                        if (srsy[dy-1][dx] == 0xDEAD)
                            srsy[dy-1][dx] = sy+1;
                    }
                    if (by2) {
                        if (srsx[dy+1][dx] == 0xDEAD)
                            srsx[dy+1][dx] = sx-1;
                        if (srsy[dy+1][dx] == 0xDEAD)
                            srsy[dy+1][dx] = sy-1;
                    }
                    if (bx1) {
                        if (srsx[dy][dx-1] == 0xDEAD)
                            srsx[dy][dx-1] = sx-1;
                        if (srsy[dy][dx-1] == 0xDEAD)
                            srsy[dy][dx-1] = sy+1;
                    }   
                    if (bx2) {
                        if (srsx[dy][dx+1] == 0xDEAD)
                            srsx[dy][dx+1] = sx+1;
                        if (srsy[dy][dx+1] == 0xDEAD)
                            srsy[dy][dx+1] = sy-1;
                    }
                    
                    if (by1 && bx1) {
                        if (srsx[dy-1][dx-1] == 0xDEAD)
                            srsx[dy-1][dx-1] = sx;
                        if (srsy[dy-1][dx-1] == 0xDEAD)
                            srsy[dy-1][dx-1] = sy+1;
                    }
                    if (by1 && bx2) {
                        if (srsx[dy-1][dx+1] == 0xDEAD)
                            srsx[dy-1][dx+1] = sx+1;
                        if (srsy[dy-1][dx+1] == 0xDEAD)
                            srsy[dy-1][dx+1] = sy;
                    }
                    if (by2 && bx1) {
                        if (srsx[dy+1][dx-1] == 0xDEAD)
                            srsx[dy+1][dx-1] = sx-1;
                        if (srsy[dy+1][dx-1] == 0xDEAD)
                            srsy[dy+1][dx-1] = sy;
                    }
                    if (by2 && bx2) {
                        if (srsx[dy+1][dx+1] == 0xDEAD)
                            srsx[dy+1][dx+1] = sx;
                        if (srsy[dy+1][dx+1] == 0xDEAD)
                            srsy[dy+1][dx+1] = sy-1;
                    }

                    if (sx < 0) {
                        sx = -sx;
                        goto label_process_sx_sy;
                    }
                    if (sx > 0)
                        sx = -sx;
                }
                
            }

            srsp = new FlatPoint[srsh][];
            for (int sh = srsh-1; sh > 0; --sh) {
                srsp[sh] = new FlatPoint[srsw];
                Array.Clear(srsp[sh], 0, srsw);
            }
        }

        private class Edge {
            internal int cX1, cY1;
            internal int cX2, cY2;
            internal int tX1, tY1;
            internal int tX2, tY2;
            internal Edge(int cx1, int cy1, int cx2, int cy2, int tx1, int ty1, int tx2, int ty2)
            {
                if (cy1 < cy2) {
                    cX1 = cx1;
                    cY1 = cy1;
                    cX2 = cx2;
                    cY2 = cy2;
                    tX1 = tx1;
                    tY1 = ty1;
                    tX2 = tx2;
                    tY2 = ty2;
                } else {
                    cX1 = cx2;
                    cY1 = cy2;
                    cX2 = cx1;
                    cY2 = cy1;
                    tX1 = tx2;
                    tY1 = ty2;
                    tX2 = tx1;
                    tY2 = ty1;
                }
            }
        }

        private class Span {
            internal int cX1, cX2;
            internal int tX1, tY1;
            internal int tX2, tY2;
            internal Span(int cx1, int cx2, int tx1, int ty1, int tx2, int ty2)
            {
                if (cx1 < cx2) {
                    cX1 = cx1;
                    cX2 = cx2;
                    tX1 = tx1;
                    tY1 = ty1;
                    tX2 = tx2;
                    tY2 = ty2;
                } else {
                    cX1 = cx2;
                    cX2 = cx1;
                    tX1 = tx2;
                    tY1 = ty2;
                    tX2 = tx1;
                    tY2 = ty1;
                }
            }
        }

        private unsafe void DrawSpan(ISurface dst, ISurface srs, Span span, int y)
        {
            int xdiff = span.cX2 - span.cX1;
            if (xdiff == 0)
                return;

            //Color colordiff = span.Color2 - span.Color1;

            float factor = 0.0f;
            float factorStep = 1.0f / (float)xdiff;

            // draw each pixel in the span
            ushort* dest = dst.ImageWordPtr + y*(dst.Stride >> 1) + span.cX1;
            ushort* sors = srs.ImageWordPtr + span.tY1*(srs.Stride >> 1) + span.tX1;

            int scxdiff = span.cX2 - span.cX1;
            int stxdiff = span.tX2 - span.tX1;
            int stydiff = span.tY2 - span.tY1;

            var stxstep = (float)stxdiff / scxdiff;
            var stystep = (float)stydiff / scxdiff;
            var stxcord = 0f;
            var stycord = 0f;

            for(int x = span.cX1; x < span.cX2; ++x) {

                sors = srs.ImageWordPtr + (span.tY1 + (int)stycord) * (srs.Stride >> 1) + (span.tX1 + (int)stxcord);
                stycord += stystep;
                stxcord += stxstep;
                //SetPixel(x, y, span.Color1 + (colordiff * factor));
                //sors = srs.ImageWordPtr;
                
                // It's dirty hack to solve problem with brihtness of textures
                // We need here some kind of interpolation or light modifier
                *dest++ = tbcolor[*sors & 0x7FFF];
                //*dest++ = *sors;
                
                factor += factorStep;
            }
        }

        private void DrawSpan(ISurface dst, ISurface srs, Edge edge1, Edge edge2)
        {
            // calculate difference between the y coordinates
            // of the first edge and return if 0
            float e1ydiff = (float)(edge1.cY2 - edge1.cY1);
            if (e1ydiff == 0.0f)
                return;

            // calculate difference between the y coordinates
            // of the second edge and return if 0
            float e2ydiff = (float)(edge2.cY2 - edge2.cY1);
            if (e2ydiff == 0.0f)
                return;

            // calculate differences between the x coordinates
            // and colors of the points of the edges
            float e1xdiff = (float)(edge1.cX2 - edge1.cX1);
            float e2xdiff = (float)(edge2.cX2 - edge2.cX1);

            float t1ydiff = (float)(edge1.tY2 - edge1.tY1);
            float t2ydiff = (float)(edge2.tY2 - edge2.tY1);
            float t1xdiff = (float)(edge1.tX2 - edge1.tX1);
            float t2xdiff = (float)(edge2.tX2 - edge2.tX1);

            //Color e1colordiff = (edge1.Color2 - edge1.Color1);
            //Color e2colordiff = (edge2.Color2 - edge2.Color1);


            // calculate factors to use for interpolation
            // with the edges and the step values to increase
            // them by after drawing each span
            float factor1 = (float)(edge2.cY1 - edge1.cY1) / e1ydiff;
            float factorStep1 = 1.0f / e1ydiff;
            float factor2     = 0.0f;
            float factorStep2 = 1.0f / e2ydiff;


        float tex_y1factor = t1ydiff / e2ydiff;
        float tex_y2factor = t2ydiff / e2ydiff;
        float tex_y1step   = 0.0f;
        float tex_y2step   = 0.0f;

            // loop through the lines between the edges and draw spans
            for(int y = edge2.cY1; y < edge2.cY2; y++) {
                // create and draw span
                Span span = new Span(edge1.cX1 + (int)(e1xdiff * factor1),
                                     edge2.cX1 + (int)(e2xdiff * factor2),

                                     edge1.tX1 + (int)(t1xdiff * factor1),
                                     edge1.tY1 + (int)(t1ydiff * factor1),
                                     edge2.tX1 + (int)(t2xdiff * factor2),
                                     edge2.tY1 + (int)(t2ydiff * factor2)
                                     );
                      //edge2.Color1 + (e2colordiff * factor2),
                      //edge1.Color1 + (e1colordiff * factor1),);
                DrawSpan(dst, srs, span, y);


            tex_y1factor += tex_y1step;
            tex_y2factor += tex_y2step;

                // increase factors
                factor1 += factorStep1;
                factor2 += factorStep2;

            }
        }

        private unsafe void DrawFlatTrng(ISurface srs, int s1x, int s1y, int s2x, int s2y, int s3x, int s3y,
                                     ref ISurface dst, int d1x, int d1y, int d2x, int d2y, int d3x, int d3y)
        {
            Edge[] edges = {
                new Edge(d1x, d1y, d2x, d2y, s1x, s1y, s2x, s2y),
                new Edge(d2x, d2y, d3x, d3y, s2x, s2y, s3x, s3y),
                new Edge(d3x, d3y, d1x, d1y, s3x, s3y, s1x, s1y)
            };

            int maxLength = 0;
            int longEdge  = 0;

            // find edge with the greatest length in the y axis
            for(int i = 0; i < 3; i++) {
                int length = edges[i].cY2 - edges[i].cY1;
                if(length > maxLength) {
                    maxLength = length;
                    longEdge = i;
                }
            }

            int shortEdge1 = (longEdge + 1) % 3;
            int shortEdge2 = (longEdge + 2) % 3;

            // draw spans between edges; the long edge can be drawn
            // with the shorter edges to draw the full triangle
            DrawSpan(dst, srs, edges[longEdge], edges[shortEdge1]);
            DrawSpan(dst, srs, edges[longEdge], edges[shortEdge2]);
        }

        private unsafe void DrawFlatTexm(ISurface srs, sbyte z, sbyte zl, sbyte zr, sbyte zd, ref ISurface dst, int cx, int cy)
        {
            int czu =  z * 14142 / 10000;
            int czl = zl * 14142 / 10000;
            int czr = zr * 14142 / 10000;
            int czd = zd * 14142 / 10000;

            int czu_x = cx - czu - 16;
            int czu_y = cy - czu - 16;
            int czl_x = cx - czl - 16;
            int czl_y = cy - czl -  0;
            int czr_x = cx - czr -  0;
            int czr_y = cy - czr - 16;
            int czd_x = cx - czd -  0;
            int czd_y = cy - czd -  0;

            lock (srs) {
                var sxy = srs.Width - 1;
                DrawFlatTrng(srs, 0, sxy,   0,   0, sxy, 0, ref dst, czl_x, czl_y, czu_x, czu_y, czr_x, czr_y);
                DrawFlatTrng(srs, 0, sxy, sxy, sxy, sxy, 0, ref dst, czl_x, czl_y, czd_x, czd_y, czr_x, czr_y);
            }


            //if (czu_y >= czd_y || czu_y >= czl_y || czr_y >= czd_y || czr_y >= czl_y ||
            //    czu_x >= czr_x || czu_x >= czd_x || czl_x >= czr_x || czl_x >= czd_x )
            //    return;

            //var tlu_x = (float)(czl_y - czu_y) / (czu_x - czl_x);
            //var tld_x = (float)(czl_y - czd_y) / (czd_x - czl_x);
            //var tru_x = (float)(czr_y - czd_y) / (czu_x - czr_x);
            //var trd_x = (float)(czr_y - czd_y) / (czd_x - czr_x);


        }

        private unsafe void DrawFlatTile(ISurface srs, sbyte z, ushort hue, byte alpha, ref ISurface dst, int cx, int cy)
        {
            int z_offset = z * 14142 / 10000;
            int z_cx = cx - z_offset;
            int z_cy = cy - z_offset;

            int dst_width = dst.Width;
            int dst_heigh = dst.Height;
            int srs_width = srs.Width;
            int srs_heigh = srs.Height;

            var p = srsp[srs_heigh][srs_width] ?? (srsp[srs_heigh][srs_width] = new FlatPoint(srs_heigh, srs_width));
            int del_x1 = z_cx + p.X1;
            int del_x2 = z_cx + p.X2;
            int del_y1 = z_cy + p.Y1;
            int del_y2 = z_cy + p.Y2;
            
            int dst_x1 = Math.Max(0,            del_x1);
            int dst_x2 = Math.Min(dst_width-1,  del_x2);
            int dst_y1 = Math.Max(0,            del_y1);
            int dst_y2 = Math.Min(dst_heigh-1,  del_y2);
            if (dst_x1 > dst_x2 || dst_y1 > dst_y2)
                return;

            var off_y1 = dst_y1 - del_y1;
            var off_y2 = p.Y2 - p.Y1 + 1 + (dst_y2 - del_y2);
            //var off_x1 = dst_x1 - del_x1;
            //var off_x2 = p.X2 - p.X1 + 1 + (dst_x2 - del_x2);

            lock (srs) {
                int srs_r, srs_g, srs_b, dst_r, dst_g, dst_b;

                var dst_strd = dst.Stride >> 1;
                var dst_line = dst.ImageWordPtr + z_cy * dst_strd + z_cx;
                var srs_strd = srs.Stride >> 1;
                var srs_line = srs.ImageWordPtr + (srs_heigh-1) * srs_strd + srs_width / 2;
                ushort* dst_pixl, srs_pixl;

                int[] srs_x, srs_y;
                short ol, ox, oy  = (short)(p.YOffset + off_y1);
                short xi, xz_srlx = (short)(srlx + z_cx);
                dst_line += (oy + srly - 1)*dst_strd + srlx;

                if (alpha == alphavl) {
                    //--off_y2;
                    for (int dy = off_y1; dy < off_y2; ++dy, ++oy) {
                        ox = p.XOffset[dy];
                        ol = p.XLength[dy];

                        //ox += 1;
                        //ol -= 2;

                        xi = (short)(xz_srlx + ox);
                        if (xi < 0) {
                            ox -= xi;
                            ol += xi;
                            xi = 0;
                        }
                        xi += (short)(ol - 1);
                        if (xi >= dst_width) {
                            ol -= (short)(xi + 1 - dst_width);
                        }
                        if (ol < 1)
                            continue;
                    
                        dst_pixl = (dst_line += dst_strd) + ox;
                        srs_x = srsx[oy];
                        srs_y = srsy[oy]; 

                        for (int c = 0; c < ol; ++c, ++ox, ++dst_pixl) {
                            /*
                            if (c == 0) {
                                *dst_pixl  = 0xFC00;
                                continue;
                            }
                            if (c == 1) {
                                *dst_pixl  = 0x83E0; 
                                continue;
                            }
                            if ( dy == off_y1) {
                                *dst_pixl  = 0xFFE0;
                                continue;
                            }
                            if ( c == 14) {
                                *dst_pixl  = 0x83FF;
                                continue;
                            }
                            if ( c == 15) {
                                *dst_pixl  = 0xFC1F; 
                                continue;
                            }*/

                            //srs_pixl = srs_line + srs_y[ox-1] * srs_strd + srs_x[ox-1];
                            //if (*srs_pixl == 0x0000)
                            //    continue;

                            srs_pixl = srs_line + srs_y[ox] * srs_strd + srs_x[ox];
                            if (*srs_pixl == 0x0000)
                                continue;

                            srs_r = (*srs_pixl & 0x7C00) >> 10;
                            srs_g = (*srs_pixl & 0x03E0) >> 5;
                            srs_b = (*srs_pixl & 0x001F);

                            dst_r = (*dst_pixl & 0x7C00) >> 10;
                            dst_g = (*dst_pixl & 0x03E0) >> 5;
                            dst_b = (*dst_pixl & 0x001F);

                            *dst_pixl = (ushort)((tbalpha[srs_r][dst_r] << 10) | (tbalpha[srs_g][dst_g] << 5) | tbalpha[srs_b][dst_b]);
                        }
                    }
                } else {
                    for (int dy = off_y1; dy < off_y2; ++dy, ++oy) {
                        ox = p.XOffset[dy];
                        ol = p.XLength[dy];

                        xi = (short)(xz_srlx + ox);
                        if (xi < 0) {
                            ox -= xi;
                            ol += xi;
                            xi = 0;
                        }
                        xi += (short)(ol - 1);
                        if (xi >= dst_width) {
                            ol -= (short)(xi + 1 - dst_width);
                        }
                        if (ol < 1)
                            continue;
                    
                        dst_pixl = (dst_line += dst_strd) + ox;
                        srs_x = srsx[oy];
                        srs_y = srsy[oy]; 

                        for (int c = 0; c < ol; ++c, ++ox, ++dst_pixl) {
                            srs_pixl = srs_line + srs_y[ox] * srs_strd + srs_x[ox];
                            if (*srs_pixl == 0x0000)
                                continue;

                            *dst_pixl = *srs_pixl;
                        }
                    }
                }
            }
        }

        private void DrawFlatBlock(ref ISurface dest, int cx, int cy, IMapBlock[][] block, uint b1, uint b2, byte x1 = 0, byte x2 = 7, byte y1 = 0, byte y2 = 7)
        {
            int dest_bx, dest_by;
            int dest_dx = cx + (x1 << 4) - 16;
            for (byte x = x1; x <= x2; ++x) {
                dest_bx = (dest_dx += 16);
                dest_by = cy;
                for (uint b = b1; b <= b2; ++b) {
                    var mapblok = block[0][b];
                    var by1 = b == b1 ? y1 : (byte)0;
                    var by2 = b == b2 ? y2 : (byte)7;
                    dest_by += (by1<<4);

                    for (byte y = by1; y <= by2; ++y) {
                        dest_by += 16;
                        DrawBlock(mapblok, x, y, block, b, ref dest, dest_bx, dest_by, DrawFlatTile, DrawFlatTexm);
                    }   
                }
            }
        }

        public void DrawFlatMap(byte map, short sealvl, ref ISurface dest, byte range, ushort tx, ushort ty, sbyte minz = -128, sbyte maxz = +127)
        {
            var sea = 14142*sealvl/10000;
            var icx = (int)(dest.Width /2 + 8 - 16*(tx%8 - 0) + sea);
            var icy = (int)(dest.Height/2 - 8 - 16*(ty%8 - 0) + sea);

            //lock (dest)
            //{
                //var srs = dataManager.GetItemTile(0x0512);
                //var z = (sbyte)0;
                //var cx = dest.Width /2;
                //var cy = dest.Height/2;            
                //DrawFlatTile(srs.Surface, z, 0, 0xFF, ref dest, cx, cy);
                //DrawFlatTile(dataManager.GetItemTile(0x0137).Surface, z, 0, 0xFF, ref dest, cx + 100, cy);
                //DrawFlatTile(dataManager.GetItemTile(0x0133).Surface, z, 0, 0xFF, ref dest, cx + 200, cy);
                //DrawCross(dest, cx, cy, 0xFFE0, 5);
                //DrawCross(dest, cx, cy, 0xFC1F, 3);
                //return;
            //}

            DrawFacet(icx, icy, 128, 0, 0, 128, map, sealvl, ref dest, range, tx, ty, minz, maxz, DrawFlatBlock);  

            #if DEBUG
                DrawCross(dest, icx + 96 - sea, icy + 112 - sea, 0xFFE0, 5);
                DrawCross(dest, icx + 96 - sea, icy + 112 - sea, 0xFC1F, 3);
                DrawCross(dest, dest.Width / 2 - 8, dest.Height / 2 - 8, 0x83E0, 3);
                DrawCross(dest, dest.Width / 2 - 8, dest.Height / 2 - 8, 0x83FF, 5);
            #endif
        }

        #endregion


        #region Oblique Render

        private static byte      alphavl = 0xCC;
        private static byte[][]  tbalpha = null;
        private static ushort[]  tbcolor = null;
        private static Point[][] texm064 = null;
        private static Point[][] texm128 = null;
        private struct Point {
            internal int X, Y;
            internal Point(int x, int y) { X = x; Y = y; }
        }

        private static void InitObliqueRender()
        {
            if (texm064 != null && texm128 != null)
                return;

            var dst_a = 0xFF - alphavl;
            tbalpha = new byte[0x20][];
            for (int s = 0; s < 32; ++s) {
                tbalpha[s] = new byte[0x20];
                for (int d = 0; d < 32; ++d)
                    tbalpha[s][d] = (byte)(  ((d * dst_a) + (s * alphavl)) / 0xFF  );
            }


            texm064 = new Point[44][];
            texm128 = new Point[44][];
            for (int dx = 0; dx < 44; ++dx) {
                texm064[dx] = new Point[ 96].Select(p => new Point(0xDEAD, 0xDEAD)).ToArray();
                texm128[dx] = new Point[192].Select(p => new Point(0xDEAD, 0xDEAD)).ToArray();
            }

            for (int sx = 0; sx < 64; ++sx) {
                for (int sy = 0; sy < 64; ++sy) {
                    var dx = (int)( ((double)sy *sin45 + (double)sx *cos45) / 2d + 22);
                    var dy = (int)( ((double)sy *cos45 - (double)sx *sin45) );
                    if (dx >= 0 && dx < 44 && dy > 0 && dy < 96)
                            texm064[dx][dy] = new Point(sx,sy);
                }
            }

            for (int sx = 0; sx < 128; ++sx) {
                for (int sy = 0; sy < 128; ++sy) {
                    var dx = (int)( ((double)sy *sin45 + (double)sx *cos45 ) / 4d + 22 );
                    var dy = (int)( ((double)sy *cos45 - (double)sx *sin45  + 4d) );
                    if (dx >= 0 && dx < 44 && dy > 0 && dy < 192)
                        texm128[dx][dy] = new Point(sx,sy);
                }
            }

            for (int dx = 0; dx < 44; ++dx) {
                texm064[dx] = texm064[dx].Where(p => p.X != 0xDEAD && p.Y != 0xDEAD).ToArray();
                texm128[dx] = texm128[dx].Where(p => p.X != 0xDEAD && p.Y != 0xDEAD).ToArray();
            }

            tbcolor = new ushort[0x8000];
            var col = new int[32];
            for (int c = 0; c < 32; ++c)
                col[c] = (c * 9000) / 10000;
            for (int c = 0, r = 0; r < 32; ++r)
                for (int g = 0; g < 32; ++g)
                    for (int b = 0; b < 32; ++b, ++c)
                        tbcolor[c] = (ushort)((col[r] << 10) | (col[g] << 5) | (col[b]));
        }

        //private unsafe void Interpolate()

        private unsafe void DrawObliqueLine(ushort* dest, uint dstoff, int dstlen, int dstbeg, int dstcnt, ushort* sors, uint srsoff, Point[] line, int srslen)
        {
            int numberpart = srslen / dstlen;
            int fractpart  = srslen % dstlen;

            dest += dstbeg * dstoff;
            int y = dstbeg * numberpart;
            int e = dstbeg * fractpart;
            while (e >= dstlen){
                e -= dstlen;
                ++y;
            }
            
            while (dstcnt-- > 0) {
                *dest = *(sors + line[y].Y * srsoff + line[y].X);

                // It's dirty hack to solve problem with brihtness of textures
                // We need here some kind of interpolation or light modifier
                *dest = tbcolor[*dest & 0x7FFF];


                dest += dstoff;
                y += numberpart;
                e += fractpart;
                if (e >= dstlen) {
                    e -= dstlen;
                    ++y;
                }
            }
        }

        private unsafe void DrawObliqueLine(ushort* dest, uint dstoff, int dst_height, int cy, int yu, int yd, ushort* sors, uint srsoff, Point[] line)
        {
            if (yu > yd)
                return;
            var p = dest + yu * dstoff; 
            var l = 1 + yd - yu;
            var s = Math.Max(0, -(cy + yu));
            var k = l - s - Math.Max(0, 1 + (cy + yd) - dst_height);
            if (k > 0)
                DrawObliqueLine(p, dstoff, l, s, k, sors, srsoff, line, line.Length);
        }

        private unsafe void DrawObliqueTexm(ISurface srs, sbyte z, sbyte zl, sbyte zr, sbyte zd, ref ISurface dst, int cx, int cy)
        {
            var srs_xmin = Math.Max( 0, 22 - cx);
            var srs_xmax = Math.Min(45, 22 - cx + dst.Width);
            if (srs_xmax < 0 || srs_xmin > srs_xmax || srs_xmin > 45)
                return;

            int czu =  -(z << 2) - 44;
            int czl = -(zl << 2) - 22;
            int czr = -(zr << 2) - 22;
            int czd = -(zd << 2);

            var dst_ymin = cy + Math.Min(Math.Min(czl, czr), Math.Min(czu, czd));
            var dst_ymax = cy + Math.Max(Math.Max(czl, czr), Math.Max(czu, czd));
            if (dst_ymin > dst.Height || dst_ymax < 0)
                return;

            var srs_lmin =      srs_xmin;
            var srs_lmax = Math.Min(22,      srs_xmax);
            var srs_rmin = 45 - srs_xmax;
            var srs_rmax = Math.Min(22, 45 - srs_xmin);

            var tlu = (czl - czu) / 22f;
            var tld = (czl - czd) / 22f;
            var tru = (czr - czu) / 22f;
            var trd = (czr - czd) / 22f;

            lock (srs) {
                var dst_strd = dst.Stride >> 1;
                var dst_line = dst.ImageWordPtr + (cy) * dst_strd + cx;

                var srs_strd = srs.Stride >> 1;
                var srs_line = srs.ImageWordPtr;

                int yu, yd;
                var pre_texm = srs.Height == 64 ? texm064 : texm128;
                var dst_ldat = dst_line - 23 + srs_lmin;
                var dst_rdat = dst_line + 23 - srs_rmin;

                if (srs_xmin < 24 && srs_xmax > 22)
                    DrawObliqueLine(dst_line, dst_strd, dst.Height, cy, czu, czd, srs_line, srs_strd, pre_texm[22]);

                for (int x = srs_lmin; x < srs_lmax; ++x) {
                    yu = (int)(czl - tlu * x);
                    yd = (int)(czl - tld * x);
                    DrawObliqueLine(++dst_ldat, dst_strd, dst.Height, cy, yu, yd, srs_line, srs_strd, pre_texm[x]);
                }
                for (int x = srs_rmin; x < srs_rmax; ++x) {
                    yu = (int)(czr - tru * x);
                    yd = (int)(czr - trd * x);
                    DrawObliqueLine(--dst_rdat, dst_strd, dst.Height, cy, yu, yd, srs_line, srs_strd, pre_texm[43 - x]);
                }
            }
        }

        private unsafe void DrawObliqueTile(ISurface srs, sbyte z, ushort hue, byte alpha, ref ISurface dst, int cx, int cy)
        {
            lock (srs)
            {
                var dst_xdel = srs.Width >> 1;
                var dst_ydel = srs.Height + 4 * z;

                var srs_xmin = Math.Max(0, dst_xdel - cx);
                var srs_xmax = Math.Min(dst.Width - cx + dst_xdel, srs.Width);
                var srs_ymin = Math.Max(0, dst_ydel - cy);
                var srs_ymax = Math.Min(Math.Max(0, dst.Height - cy + dst_ydel - srs_ymin), srs.Height);

                var dst_strd = dst.Stride >> 1;
                var dst_line = dst.ImageWordPtr + (cy - dst_ydel + srs_ymin) * dst_strd + cx - dst_xdel + srs_xmin;
                var dst_pixl = dst_line;

                var srs_strd = srs.Stride >> 1;
                var srs_line = srs.ImageWordPtr + srs_ymin * srs_strd + srs_xmin;
                var srs_pixl = srs_line;

                if (alpha == 0xFF)
                    for (int sy = srs_ymin; sy < srs_ymax; ++sy) {
                        for (int sx = srs_xmin; sx < srs_xmax; ++sx, ++srs_pixl, ++dst_pixl) {
                            if (*srs_pixl == 0x0000)
                                continue;

                            *dst_pixl = *srs_pixl;
                        }
                        srs_pixl = (srs_line += srs_strd);
                        dst_pixl = (dst_line += dst_strd);
                    }
                else if (alpha == alphavl) {
                    int srs_r, srs_g, srs_b, dst_r, dst_g, dst_b;
                    for (int sy = srs_ymin; sy < srs_ymax; ++sy) {
                        for (int sx = srs_xmin; sx < srs_xmax; ++sx, ++srs_pixl, ++dst_pixl) {
                            if (*srs_pixl == 0x0000)
                                continue;

                            srs_r = (*srs_pixl & 0x7C00) >> 10;
                            srs_g = (*srs_pixl & 0x03E0) >> 5;
                            srs_b = (*srs_pixl & 0x001F);

                            dst_r = (*dst_pixl & 0x7C00) >> 10;
                            dst_g = (*dst_pixl & 0x03E0) >> 5;
                            dst_b = (*dst_pixl & 0x001F);

                            *dst_pixl = (ushort)((tbalpha[srs_r][dst_r] << 10) | (tbalpha[srs_g][dst_g] << 5) | tbalpha[srs_b][dst_b]);
                                        
                        }
                        srs_pixl = (srs_line += srs_strd);
                        dst_pixl = (dst_line += dst_strd);
                    }
                } else if (alpha != 0) {
                    int srs_r, srs_g, srs_b, dst_r, dst_g, dst_b, dst_a = 255 - alpha;
                    for (int sy = srs_ymin; sy < srs_ymax; ++sy) {
                        for (int sx = srs_xmin; sx < srs_xmax; ++sx, ++srs_pixl, ++dst_pixl) {
                            if (*srs_pixl == 0x0000)
                                continue;

                            srs_r = (*srs_pixl & 0x7C00) >> 10;
                            srs_g = (*srs_pixl & 0x03E0) >> 5;
                            srs_b = (*srs_pixl & 0x001F);

                            dst_r = (*dst_pixl & 0x7C00) >> 10;
                            dst_g = (*dst_pixl & 0x03E0) >> 5;
                            dst_b = (*dst_pixl & 0x001F);

                            *dst_pixl = (ushort)(    ((((dst_r * dst_a) + (srs_r * alpha)) / 255) << 10)
                                                   | ((((dst_g * dst_a) + (srs_g * alpha)) / 255) << 5)
                                                   | ((((dst_b * dst_a) + (srs_b * alpha)) / 255)));
                                        
                        }
                        srs_pixl = (srs_line += srs_strd);
                        dst_pixl = (dst_line += dst_strd);
                    }
                }              
            }
        }
  
        private void DrawObliqueBlock(ref ISurface dest, int cx, int cy, IMapBlock[][] block, uint b1, uint b2, byte x1 = 0, byte x2 = 7, byte y1 = 0, byte y2 = 7)
        {
            int dest_rx, dest_by;
            for (byte x = x1; x <= x2; ++x) {
                dest_rx = cx + 22*x;
                dest_by = cy + 22*x;
                for (uint b = b1; b <= b2; ++b) {
                    var mapblok = block[0][b];
                    var by1 = b == b1 ? y1 : (byte)0;
                    var by2 = b == b2 ? y2 : (byte)7;
                    dest_rx -= 22*by1;
                    dest_by += 22*by1;

                    for (byte y = by1; y <= by2; ++y) { 
                        DrawBlock(mapblok, x, y, block, b, ref dest, dest_rx, dest_by, DrawObliqueTile, DrawObliqueTexm);
                        dest_rx -= 22;
                        dest_by += 22;
                    }   
                }
            }
        }

        public unsafe void DrawObliqueMap(byte map, short sealvl, ref ISurface dest, byte range, ushort tx, ushort ty, sbyte minz = -128, sbyte maxz = +127)
        {
            var icx = (int)(dest.Width /2 - 22*(tx%8 - 0) + 22*(ty%8 - 0));
            var icy = (int)(dest.Height/2 - 22*(tx%8 - 3) - 22*(ty%8 - 3) + 4*sealvl - 132);

            DrawFacet(icx, icy, 176, 176, 176, 176, map, sealvl, ref dest, range, tx, ty, minz, maxz, DrawObliqueBlock);

            #if DEBUG
                DrawCross(dest, icx, icy + 308 - 4 * sealvl, 0xFFE0, 5);
                DrawCross(dest, icx, icy + 308 - 4 * sealvl, 0xFC1F, 3);
                DrawCross(dest, dest.Width / 2, dest.Height / 2, 0x83E0, 3);
                DrawCross(dest, dest.Width / 2, dest.Height / 2, 0x83FF, 5);
            #endif
        }

        #endregion


        

        public void DrawRadarMap(byte map, ref ISurface dest, ushort x, ushort w, ushort y, ushort h)
        {

        }

        public void DrawMiniMap(byte map, ref ISurface dest, ushort x, ushort w, ushort y, ushort h)
        {

        }

    }
}
