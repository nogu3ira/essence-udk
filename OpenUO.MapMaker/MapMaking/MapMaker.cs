using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using OpenUO.MapMaker.Elements.BaseTypes.ComplexTypes.Enum;
using OpenUO.MapMaker.Elements.ColorArea;
using OpenUO.MapMaker.Elements.ColorArea.ColorArea;
using OpenUO.MapMaker.Elements.Items;
using OpenUO.MapMaker.Elements.Items.ItemText;
using OpenUO.MapMaker.Elements.Items.ItemsTransition;
using OpenUO.MapMaker.Elements.Textures;
using OpenUO.MapMaker.Elements.Textures.TextureTransition;
using OpenUO.MapMaker.Elements.Textures.TexureCliff;

namespace OpenUO.MapMaker.MapMaking
{
    public class MapMaker
    {
        #region Fields
        public static Random Random { get; set; }
        public int MinX = 2;
        public int MinY = 2;
        private readonly int _stride;
        private Color[] _bitmap;
        private Color[] _bitmapZ;
        #endregion

        #region props
        #region scripts

        #region scripts for areas
        public CollectionAreaColor CollectionAreaColor { get; set; }
        //public CollectionAreaColor ColorAreasCoast { get; set; }
        //public CollectionAreaColorMountains ColorMountainsAreas { get; set; }
        #endregion

        //#region scripts for Items
        //public CollectionAreaItems Items { get; set; }
        //public CollectionAreaTransitionItemCoast ItemsCoasts { get; set; }
        //public CollectionAreaTransitionItems ItemsSmooth { get; set; }
        //#endregion

        #region Textures
        public CollectionAreaTexture TextureAreas { get; set; }
        //public CollectionAreaTransitionTexture TxtureSmooth { get; set; }
        //public CollectionAreaTransitionCliffTexture CollectionAreaCliffs { get; set; }
        #endregion

        #endregion

        #region Matrix for making

        /// <summary>
        /// cache of the bitmap map file
        /// </summary>
        public Color[] BitmapMap { get { return _bitmap; } set { _bitmap = value; } }
        /// <summary>
        /// cache of the bitmap Z file
        /// </summary>
        public Color[] BitmapMapZ { get { return _bitmapZ; } set { _bitmapZ = value; } }

        ///// <summary>
        ///// temp map used for mountains and smooth
        ///// </summary>
        //private readonly int[] _MapOcc;

        ///// <summary>
        ///// Calculated altitude of the map
        ///// </summary>
        //private readonly int[] _MapAlt;

        ///// <summary>
        ///// Id Texture of the map
        ///// </summary>
        //private readonly int[] _MapID;

        ///// <summary>
        ///// items of the map
        ///// </summary>
        //private readonly List<Item>[] _AddItemMap;

        /// <summary>
        /// all the tiles of the map
        /// </summary>
        private readonly MapObject[] _mapObjects;

        ///// <summary>
        ///// temp copy of the map
        ///// </summary>
        //private readonly Color[] _Tmp;

        #endregion

        /// <summary>
        /// directory where you're going to write your mul files
        /// </summary>
        public string MulDirectory { get; set; }

        /// <summary>
        /// index of the map that you're going to make
        /// </summary>
        public int mapIndex { get; set; }

        /// <summary>
        /// max x of your map
        /// </summary>
        private readonly int _X;

        /// <summary>
        /// max y of your map
        /// </summary>
        private readonly int _Y;


        /// <summary>
        /// if you want to process the Z automatically or not
        /// </summary>
        public Boolean AutomaticZMode { get; set; }

        #endregion

        #region ctor
        /// <summary>
        /// Class costructor
        /// </summary>
        /// <param name="map">map cached previusly</param>
        /// <param name="alt">map altitude cached</param>
        /// <param name="x">max x of the map</param>
        /// <param name="y">max y of the map</param>
        /// <param name="index">index of the map</param>
        public MapMaker(Color[] map, Color[] alt, int x, int y, int index)
        {

            BitmapMap = map;
            BitmapMapZ = alt;
            var x1 = x + 10;
            var y1 = y + 10;
            var lenght = x1 * y1;
            #region InitArrays
            //_MapOcc = new int[lenght];
            //_MapOcc.Initialize();

            //_MapAlt = new int[lenght];
            //_MapAlt.Initialize();

            //_MapID = new int[x1*y1];
            //_MapID.Initialize();

            //_AddItemMap = new List<Item>[lenght];
            //_AddItemMap.Initialize();

            ////_Tmp = new Color[lenght];
            ////_Tmp.Initialize();

            _mapObjects = new MapObject[lenght];

            for (int i = 0; i < _mapObjects.Length; i++)
            {
                _mapObjects[i] = new MapObject();
            }
            #endregion

            _X = x;
            _Y = y;

            MulDirectory = "";
            mapIndex = index;
            _stride = _X;
            Random = new Random(DateTime.Now.Millisecond);

            AutomaticZMode = true;
        }

        #endregion

        #region Methods

        #region Make

        /// <summary>
        /// main method to build the map
        /// </summary>
        public void Bmp2Map()
        {

            #region initialize data search
            CollectionAreaColor.InitializeSeaches();
            foreach (var VARIABLE in CollectionAreaColor.List)
            {
                VARIABLE.InizializeSearches();
            }
            TextureAreas.InitializeSeaches();
            #endregion

            if (AutomaticZMode)
                Mountain();

            for (var x = MinX; x < _X - 1; x++)
                for (var y = MinY; y < _Y - 1; y++)
                {
                    var coordinates = MakeIndexesDirections(x, y, 1, 1);
                    var areacolorcoordinates = new AreaColorCoordinates(CollectionAreaColor, coordinates, BitmapMap);
                    var buildMapCoordinates = new MapObjectCoordinates(coordinates, _mapObjects);

                    PlaceTextures(areacolorcoordinates, buildMapCoordinates);
                    MakeCoast(areacolorcoordinates, buildMapCoordinates);
                    //TextureTranstion(coordinates, areacolorcoordinates, buildMapCoordinates);
                    //MakeCliffs(coordinates,areacolorcoordinates,buildMapCoordinates);
                    //ItemsTransations(coordinates,areacolorcoordinates,buildMapCoordinates);
                    if (!AutomaticZMode)
                        ProcessZ(AutomaticZMode, areacolorcoordinates, buildMapCoordinates, coordinates);
                }
            //_directions = null;
            if (AutomaticZMode)
                ProcessZ(AutomaticZMode, null, null, new Coordinates(1, 1, 0, 0, 0));

            //SetItem();

            WriteStatics();

            WriteMUL();

        }

        #endregion

        #region MapInit

        /// <summary>
        /// I Init the array of the maps files
        /// </summary>
        //private void BMP2MUL()
        //{
        //    int x, y;
        //    //rrggbb

        //    for (x = 0; x < _X; x++)
        //        for (y = 0; y < _Y; y++)
        //        {
        //            var location = CalculateZone(x, y);
        //            //_MapID = 168;
        //            //_MapAlt[location] = -5;

        //            _mapObjects[location].Texture = 168; // set Default-Texture (water)
        //            _mapObjects[location].Altitude = -5;

        //            var area = ColorAreas.FindByColor(BitmapMap[location]);
        //            //var coast = ColorAreasCoast.FindByColor(BitmapMap[location]);

        //            if (coast == null && area != null)
        //            {
        //                //_MapID[location] = RandomTexture(area.Index);
        //                //_MapAlt[location] = Random.Next(area.Low, area.Hight);
        //                _mapObjects[location].Texture = (short)RandomTexture(area.TextureIndex);
        //                _mapObjects[location].Altitude = (sbyte)Random.Next(area.Min, area.Max);
        //                continue;
        //            }


        //            if (coast != null)
        //            {
        //                //_MapID[location] = RandomTexture(coast.Index);
        //                //_MapAlt[location] = Random.Next(coast.Low,coast.Hight);
        //                _mapObjects[location].Texture = (short) RandomTexture(area.TextureIndex);
        //                _mapObjects[location].Altitude = (sbyte) Random.Next(area.Min, area.Max);
        //                continue;
        //            }
        //        }
        //}
        private void PlaceTextures(AreaColorCoordinates areaColorCoordinates, MapObjectCoordinates mapObjectCoordinates)
        {

            //_MapID = 168;
            //_MapAlt[location] = -5;

            mapObjectCoordinates.Center.Altitude = -5;

            //var coast = ColorAreasCoast.FindByColor(BitmapMap[location]);

            if (areaColorCoordinates.Center != null)
            {
                //_MapID[location] = RandomTexture(area.Index);
                //_MapAlt[location] = Random.Next(area.Low, area.Hight);
                mapObjectCoordinates.Center.Texture = (short)RandomTexture(areaColorCoordinates.Center.TextureIndex);
                mapObjectCoordinates.Center.Altitude = (sbyte)Random.Next(areaColorCoordinates.Center.Min, areaColorCoordinates.Center.Max);
            }
        }


        /// <summary>
        /// it's used to process the map automatically
        /// </summary>
        /// <param name="mode">mode of how you want to process the map 0 for following the map, 1 to calculate automatically</param>
        /// <param name="areas"> </param>
        /// <param name="mapObjectCoordinates"> </param>
        /// <param name="coordinates"> </param>
        private void ProcessZ(bool mode, AreaColorCoordinates areas, MapObjectCoordinates mapObjectCoordinates, Coordinates coordinates)
        {
            if (!mode)
            {
                //_MapAlt[location] = BitmapMapZ[location].B-128;
                #region Standard GrayScale
                var alt = BitmapMapZ[coordinates.Center].B - 128;


                if (alt > sbyte.MaxValue)
                    alt = sbyte.MaxValue;
                if (alt < sbyte.MinValue)
                    alt = sbyte.MinValue;
                #endregion //Standard

                mapObjectCoordinates.Center.Altitude = (sbyte)alt;
            }
            else
            {
                int x;
                for (x = MinX; x < _X; x++)
                {
                    int y;
                    for (y = MinY; y < _Y; y++)
                    {
                        var location = CalculateZone(x, y, _stride);

                        if (BitmapMapZ[location] != Colors.Black)
                        {
                            //_MapAlt[location] += BitmapMapZ[location].R-128;
                            _mapObjects[location].Altitude += (sbyte)(BitmapMapZ[location].R - 128);

                            var mnt = CollectionAreaColor.FindByColor(BitmapMap[location]);
                            if (mnt != null || mnt.Type != TypeColor.Moutains)
                            {
                                if (mnt.ModeAutomatic)
                                {
                                    //_MapAlt[location] -= BitmapMapZ[location].R-128;
                                    _mapObjects[location].Altitude -= (sbyte)(BitmapMapZ[location].R - 128);
                                    continue;
                                }
                                //if (_MapAlt[location] > 127 && BitmapMapZ[location].R > 0)
                                //    _MapAlt[location] = Random.Next(120, 125);
                                if (_mapObjects[location].Altitude > 127 && BitmapMapZ[location].R > 0)
                                    _mapObjects[location].Altitude = (sbyte)Random.Next(120, 125);
                                continue;
                            }
                        }
                        //if (_MapAlt[location] > 127)
                        //{
                        //    _MapAlt[location] = Random.Next(120, 125);
                        //}
                        if (_mapObjects[location].Altitude > 127)
                            _mapObjects[location].Altitude = (sbyte)(Random.Next(120, 125));
                    }
                }
            }
        }

        #endregion

        #region Transations

        #region mountains

        /// <summary>
        /// method to init the mountains textures
        /// </summary>
        private void Mountain()
        {
            byte chk = 1;
            //rrggbb

            for (int x = 0; x < _X; x++)
            {
                for (int y = 0; y < _Y; y++)
                {
                    var location = CalculateZone(x, y, _stride);
                    var area = CollectionAreaColor.FindByColor(BitmapMap[location]);
                    if (area != null && area.Type == TypeColor.Moutains)
                        _mapObjects[location].Occupied = (byte)area.Index;
                }
            }
            for (int x = MinX; x < _X; x++)
                for (int y = MinY; y < _Y; y++)
                {
                    chk = 1;
                    var mountset = CollectionAreaColor.FindByColor(BitmapMap[CalculateZone(x, y, _stride)]);
                    if (mountset == null || mountset.Type != TypeColor.Moutains) continue;

                    var coord = MakeIndexesDirections(x, y, 1, 1);
                    var mapcoord = new MapObjectCoordinates(coord, _mapObjects);
                    //_MapID[location] = RandomTexture(mountset.IndexMountainGroup);
                    //_MapAlt[location] = Random.Next(mountset.List.First().From, mountset.List.First().To);
                    mapcoord.Center.Texture = (short)RandomTexture(mountset.TextureIndex);
                    mapcoord.Center.Altitude =
                        (sbyte)Random.Next(mountset.List.First().From, mountset.List.First().To);
                    chk = (byte)mountset.TextureIndex;
                    if (!mountset.ModeAutomatic) continue;

                    for (int index = 0; index < mountset.List.Count; index++)
                    {
                        var i = index * 2;
                        var cirlce = mountset.List[index];
                        //if (_MapOcc[directions[(int)Directions.North]] != 0 && _MapOcc[directions[(int)Directions.NorthEast]] != 0 && _MapOcc[directions[(int)Directions.East]] != 0 &&
                        //    _MapOcc[directions[(int)Directions.SouthEast]] != 0 && _MapOcc[directions[(int)Directions.South]] != 0 && _MapOcc[directions[(int)Directions.SouthWest]] != 0 &&
                        //    _MapOcc[directions[(int)Directions.West]] != 0 && _MapOcc[directions[(int)Directions.NorthWest]] != 0)
                        //{
                        //    _MapAlt[location] = Random.Next(cirlce.From, cirlce.To);
                        //    if (_MapAlt[location] > 127)
                        //        _MapAlt[location] = Random.Next(120, 125);
                        //    if (mountset.ColorMountain != Color.Black && (_MapAlt[location] > 115 || i > 8))
                        //        _MapOcc[location] = chk;
                        //}
                        //if (mapcoord.North.Occupied != 0 && mapcoord.NorthEast.Occupied != 0 && _mapObjects[directions[(int)Directions.East]].Occupied != 0 &&
                        //    _mapObjects[directions[(int)Directions.SouthEast]].Occupied != 0 && _mapObjects[directions[(int)Directions.South]].Occupied != 0 && _mapObjects[directions[(int)Directions.SouthWest]].Occupied != 0 &&
                        //    _mapObjects[directions[(int)Directions.West]].Occupied != 0 && _mapObjects[directions[(int)Directions.NorthWest]].Occupied != 0)

                        if (mapcoord.List.Any(c => c.Occupied == 0)) continue;

                        mapcoord.Center.Altitude = (sbyte)Random.Next(cirlce.From, cirlce.To);
                        if (mapcoord.Center.Altitude > 127)
                            mapcoord.Center.Altitude = (sbyte)(Random.Next(120, 125));
                        if (mountset.ColorTopMountain != Colors.Black && (mapcoord.Center.Altitude > 115 || i > 8))
                            mapcoord.Center.Occupied = (byte)chk;
                    }
                }

            for (int x = MinX; x < _X; x++)
                for (int y = MinY; y < _Y; y++)
                {
                    var location = CalculateZone(x, y, _stride);
                    //if (_MapOcc[location] == 20)
                    //    _MapOcc[location] = 0;
                    //if (_MapOcc[location] <= 0) continue;
                    if (_mapObjects[location].Occupied == byte.MaxValue)
                        _mapObjects[location].Occupied = 0;
                    if (_mapObjects[location].Occupied <= 0) continue;

                    //var mountset = ColorMountainsAreas.FindMountainById(new Id() {Value = _MapOcc[location]});
                    var mountset =
                        CollectionAreaColor.FindByIndex(_mapObjects[location].Occupied);
                    BitmapMap[location] = mountset.Color;
                    //_MapID[location] = RandomTexture(mountset.IndexMountainGroup);
                    //_MapOcc[location] = 0;

                    _mapObjects[location].Texture = (short)RandomTexture(mountset.TextureIndex);
                    _mapObjects[location].Occupied = 0;
                }
        }

        #endregion

        #region Texture Transations

        /// <summary>
        /// transitions from a kind of terrain to anotherkind
        /// </summary>
        /// <param name="coordinates"> </param>
        /// <param name="areaColorCoordinates"> </param>
        /// <param name="mapObjectCoordinates"> </param>
        private void TextureTranstion(Coordinates coordinates, AreaColorCoordinates areaColorCoordinates, MapObjectCoordinates mapObjectCoordinates)
        {
            //var listSmooth = TxtureSmooth.FindFromByColor(BitmapMap[CalculateZone(x, y)]);
            var transitionList = areaColorCoordinates.Center.TextureTransitions;
            if (!transitionList.Any())
                return;
            Color A = BitmapMap[coordinates.Center];
            int special = 0;
            int z = 0;

            //if (_MapOcc[_directions[(int)Directions.Location]] == 0)
            if (mapObjectCoordinates.Center.Occupied == 0)
            {
                // x = nicht def.

                //Border
                //xB
                //Ax
                //int x1 = x + 1;
                //int y1 = y - 1;
                //if (BitmapMap[CalculateZone(x1, y1)] != A)
                if (BitmapMap[coordinates.NorthEast] != A)
                {

                    //var transition = GetTransationTexture(transitionList, coordinates.NorthEast);
                    var transition = areaColorCoordinates.Center.FindTransitionTexture(BitmapMap[coordinates.NorthEast]);
                    //_MapID[_directions[(int)Directions.Location]] = RandomFromList(smoothT.Border.Forth.List);
                    //z = _MapAlt[_directions[(int)Directions.NorthEast]] + _MapAlt[_directions[(int)Directions.SouthWest]];
                    special = 2;
                    if (transition != null)
                    {
                        mapObjectCoordinates.Center.Texture =
                            (short)RandomFromList(transition.BorderSouthWest.List);
                        z = mapObjectCoordinates.NorthEast.Altitude +
                            mapObjectCoordinates.SouthWest.Altitude;
                    }
                }
                //x1 = x - 1;
                //y1 = y -1;
                //Bx
                //xA
                //if (BitmapMap[_directions[(int)Directions.NorthWest]] != A)
                if (BitmapMap[coordinates.NorthWest] != A)
                {
                    var transition = areaColorCoordinates.Center.FindTransitionTexture(BitmapMap[coordinates.NorthWest]);
                    //var smoothT = Smooth(listSmooth, x - 1, y - 1);

                    //_MapID[_directions[(int)Directions.Location]] = RandomFromList(smoothT.Border.Third.List);
                    if (transition != null)
                    {
                        special = 1;
                        mapObjectCoordinates.Center.Texture = (short)RandomFromList(transition.BorderSouthEast.List);
                        //z = _MapAlt[_directions[(int)Directions.NorthWest]] + _MapAlt[_directions[(int)Directions.SouthEast]];
                        //_mapObjects[_directions[(int)Directions.Center]].Texture = (short)RandomFromList(smoothT.Border.CollectionThird.List);
                        //z = _mapObjects[_directions[(int)Directions.NorthWest]].Altitude +
                        //    _mapObjects[_directions[(int)Directions.SouthEast]].Altitude;

                        z = mapObjectCoordinates.NorthWest.Altitude +
                            mapObjectCoordinates.SouthEast.Altitude;
                    }
                }
                //GA
                //BG
                //x1 = x - 1;
                //y1 = y + 1;
                //if (BitmapMap[_directions[(int)Directions.SouthWest]] != A)
                if (BitmapMap[coordinates.SouthWest] != A)
                {
                    var transation = areaColorCoordinates.Center.FindTransitionTexture(BitmapMap[coordinates.SouthWest]);
                    //???? controllare, possibile errore
                    //_MapID[_directions[(int)Directions.Location]] = RandomFromList(smoothT.Border.Second.List);
                    if (transation != null)
                    {
                        special = 2;
                        //z = _MapAlt[_directions[(int)Directions.SouthWest]] + _MapAlt[_directions[(int)Directions.NorthEast]];



                        //_mapObjects[_directions[(int)Directions.Center]].Texture = (short)RandomFromList(smoothT.Border.CollectionSecond.List);
                        //z = _mapObjects[_directions[(int)Directions.SouthWest]].Altitude +
                        //    _mapObjects[_directions[(int)Directions.NorthEast]].Altitude;



                        _mapObjects[coordinates.Center].Texture = (short)RandomFromList(transation.BorderSouthEast.List);
                        z = _mapObjects[coordinates.SouthWest].Altitude +
                            _mapObjects[coordinates.NorthEast].Altitude;
                    }

                }
                //Ax
                //xB
                //x1 = x + 1;
                //y1 = y + 1;
                //if (BitmapMap[_directions[(int)Directions.SouthEast]] != A)
                if (BitmapMap[coordinates.SouthEast] != A)
                {
                    //var smoothT = Smooth(listSmooth, x + 1, y + 1);
                    //_MapID[_directions[(int)Directions.Location]] = RandomFromList(smoothT.Border.First.List);
                    //z = _MapAlt[_directions[(int)Directions.SouthEast]] + _MapAlt[_directions[(int)Directions.NorthWest]];
                    //_mapObjects[_directions[(int)Directions.Center]].Texture = (short)RandomFromList(smoothT.Border.CollectionFirst.List);
                    //z = _mapObjects[_directions[(int)Directions.SouthEast]].Altitude +
                    //    _mapObjects[_directions[(int)Directions.NorthWest]].Altitude;

                    var transation = areaColorCoordinates.Center.FindTransitionTexture(BitmapMap[coordinates.SouthEast]);
                    if (transation != null)
                    {
                        special = 2;
                        mapObjectCoordinates.Center.Texture = (short)RandomFromList(transation.BorderNorthWest.List);
                        z = _mapObjects[coordinates.SouthEast].Altitude +
                            _mapObjects[coordinates.NorthWest].Altitude;
                    }
                }

                //Line
                // B
                //xAx
                //y1 = y - 1;
                //if (BitmapMap[_directions[(int)Directions.North]] != A)
                if (BitmapMap[coordinates.North] != A)
                {
                    //var smoothT = Smooth(listSmooth, x, y - 1);
                    var transation = areaColorCoordinates.Center.FindTransitionTexture(BitmapMap[coordinates.North]);
                    if (transation != null)
                    {
                        //_MapID[_directions[(int)Directions.Location]] = RandomFromList(smoothT.Line.Third.List);
                        special = 1;
                        //z = _MapAlt[_directions[(int)Directions.North]] + _MapAlt[_directions[(int)Directions.South]];
                        //_mapObjects[_directions[(int)Directions.Center]].Texture = (short)RandomFromList(smoothT.Line.CollectionThird.List);
                        //z = _mapObjects[_directions[(int)Directions.North]].Altitude +
                        //    _mapObjects[_directions[(int)Directions.South]].Altitude;
                        mapObjectCoordinates.Center.Texture = (short)RandomFromList(transation.LineSouth.List);
                        z = mapObjectCoordinates.North.Altitude +
                            mapObjectCoordinates.South.Altitude;
                    }
                }


                //xAx
                // B
                ////y1 = y + 1;
                //if (BitmapMap[_directions[(int)Directions.South]] != A)
                if (BitmapMap[coordinates.South] != A)
                {
                    //var smoothT = Smooth(listSmooth, x, y + 1);
                    var transation = areaColorCoordinates.Center.FindTransitionTexture(BitmapMap[coordinates.South]);
                    if (transation != null)
                    {
                        //_MapID[_directions[(int)Directions.Location]] = RandomFromList(smoothT.Line.First.List);
                        special = 2;
                        //z = _MapAlt[_directions[(int)Directions.South]] + _MapAlt[_directions[(int)Directions.North]];
                        //_mapObjects[_directions[(int)Directions.Center]].Texture = (short)RandomFromList(smoothT.Line.CollectionFirst.List);
                        //z = _mapObjects[_directions[(int)Directions.South]].Altitude +
                        //    _mapObjects[_directions[(int)Directions.North]].Altitude;
                        mapObjectCoordinates.Center.Texture = (short)RandomFromList(transation.LineNorth.List);
                        z = mapObjectCoordinates.South.Altitude +
                            mapObjectCoordinates.North.Altitude;
                    }
                }
                //x
                //AB
                //x
                //x1 = x + 1;
                //if (BitmapMap[_directions[(int)Directions.East]] != A)
                if (BitmapMap[coordinates.East] != A)
                {
                    //    var smoothT = Smooth(listSmooth, x + 1, y);
                    var transation = areaColorCoordinates.Center.FindTransitionTexture(BitmapMap[coordinates.East]);
                    if (transation != null)
                    {
                        //_MapID[_directions[(int)Directions.Location]] = RandomFromList(smoothT.Line.Forth.List);
                        special = 2;
                        //z = _MapAlt[_directions[(int)Directions.East]] + _MapAlt[_directions[(int)Directions.West]];
                        //_mapObjects[_directions[(int)Directions.Center]].Texture = (short)RandomFromList(smoothT.Line.CollectionForth.List);
                        //z = _mapObjects[_directions[(int)Directions.East]].Altitude +
                        //    _mapObjects[_directions[(int)Directions.West]].Altitude;
                        _mapObjects[coordinates.Center].Texture =
                            (short)RandomFromList(transation.LineEast.List);
                        z = _mapObjects[coordinates.East].Altitude +
                            _mapObjects[coordinates.West].Altitude;
                    }
                }
                // x
                //BA
                // x
                //x1 = x - 1;
                //if (BitmapMap[_directions[(int)Directions.West]] != A)
                if (BitmapMap[coordinates.West] != A)
                {
                    //var smoothT = GetTransationTexture(listSmooth, x - 1, y);

                    var transation = areaColorCoordinates.Center.FindTransitionTexture(BitmapMap[coordinates.East]);
                    if (transation != null)
                    {
                        //_MapID[_directions[(int)Directions.Location]] = RandomFromList(smoothT.Line.Second.List);
                        special = 1;
                        //z = _MapAlt[_directions[(int)Directions.West]] + _MapAlt[_directions[(int)Directions.East]];
                        //_mapObjects[_directions[(int)Directions.Center]].Texture = (short)RandomFromList(smoothT.Line.CollectionSecond.List);
                        //z = _mapObjects[_directions[(int)Directions.West]].Altitude +
                        //    _mapObjects[_directions[(int)Directions.East]].Altitude;
                        _mapObjects[coordinates.Center].Texture =
                            (short)RandomFromList(transation.LineWest.List);
                        z = _mapObjects[coordinates.West].Altitude +
                            _mapObjects[coordinates.East].Altitude;
                    }
                }

                //Edge
                //B
                //AB
                //x1 = x + 1;
                ////y1 = y - 1;
                //if (BitmapMap[_directions[(int)Directions.East]] != A && BitmapMap[_directions[(int)Directions.North]] != A)

                if (BitmapMap[coordinates.East] != A && BitmapMap[coordinates.North] != A)
                {
                    //var smoothT = GetTransationTexture(listSmooth, x, y - 1);
                    var transation = areaColorCoordinates.Center.FindTransitionTexture(BitmapMap[coordinates.NorthEast]);
                    if (transation != null)
                    {
                        //_MapID[_directions[(int)Directions.Location]] = RandomFromList(smoothT.Edge.Forth.List);
                        special = 2;
                        //z = _MapAlt[_directions[(int)Directions.NorthEast]] + _MapAlt[_directions[(int)Directions.SouthWest]];
                        //_mapObjects[_directions[(int)Directions.Center]].Texture = (short)RandomFromList(smoothT.Edge.CollectionForth.List);
                        //z = _mapObjects[_directions[(int)Directions.NorthEast]].Altitude +
                        //    _mapObjects[_directions[(int)Directions.SouthWest]].Altitude;
                        mapObjectCoordinates.Center.Texture =
                            (short)RandomFromList(transation.EdgeSouthWest.List);
                        z = mapObjectCoordinates.NorthEast.Altitude +
                            mapObjectCoordinates.SouthWest.Altitude;
                    }
                }
                // B
                //BA
                //y1 = y - 1;
                //x1 = x - 1;
                //if (BitmapMap[_directions[(int)Directions.West]] != A && BitmapMap[_directions[(int)Directions.North]] != A)
                if (BitmapMap[coordinates.West] != A && BitmapMap[coordinates.North] != A)
                {
                    //var smoothT = GetTransationTexture(listSmooth, x, y - 1);
                    var transation = areaColorCoordinates.Center.FindTransitionTexture(BitmapMap[coordinates.NorthWest]);
                    //_MapID[CalculateZone(x,y)] = RandomFromList(smoothT.Edge.Third.List);
                    if (transation != null)
                    {
                        special = 1;
                        //z = _MapAlt[_directions[(int)Directions.NorthWest]] + _MapAlt[_directions[(int)Directions.SouthEast]];
                        //_mapObjects[_directions[(int)Directions.Center]].Texture = (short)RandomFromList(smoothT.Edge.CollectionThird.List);
                        //z = _mapObjects[_directions[(int)Directions.NorthEast]].Altitude +
                        //    _mapObjects[_directions[(int)Directions.SouthWest]].Altitude;
                        mapObjectCoordinates.Center.Texture =
                            (short)RandomFromList(transation.EdgeSouthEast.List);
                        z = mapObjectCoordinates.NorthEast.Altitude +
                            mapObjectCoordinates.SouthWest.Altitude;
                    }
                }
                //BA
                // B
                //x1 = x - 1;
                //y1 = y + 1;
                //if (BitmapMap[_directions[(int)Directions.West]] != A && BitmapMap[_directions[(int)Directions.South]] != A)
                if (BitmapMap[coordinates.West] != A && BitmapMap[coordinates.South] != A)
                {
                    //var smoothT = GetTransationTexture(listSmooth, x, y + 1);
                    var transation = areaColorCoordinates.Center.FindTransitionTexture(BitmapMap[coordinates.SouthWest]);
                    //_MapID[CalculateZone(x,y)] = RandomFromList(smoothT.Edge.Second.List);
                    if (transation != null)
                    {
                        special = 2;
                        //z = _MapAlt[_directions[(int)Directions.SouthWest]] + _MapAlt[_directions[(int)Directions.NorthEast]];
                        //_mapObjects[_directions[(int)Directions.Center]].Texture = (short)RandomFromList(smoothT.Edge.CollectionSecond.List);
                        //z = _mapObjects[_directions[(int)Directions.SouthWest]].Altitude +
                        //    _mapObjects[_directions[(int)Directions.NorthEast]].Altitude;

                        mapObjectCoordinates.Center.Texture = (short)RandomFromList(transation.EdgeNorthEast.List);
                        z = mapObjectCoordinates.SouthWest.Altitude +
                            mapObjectCoordinates.NorthEast.Altitude;
                    }
                }
                //AB
                //B
                //x1 = x + 1;
                //y1 = y + 1;
                //if (BitmapMap[_directions[(int)Directions.East]] != A && BitmapMap[_directions[(int)Directions.South]] != A)

                if (BitmapMap[coordinates.East] != A && BitmapMap[coordinates.South] != A)
                {
                    //var smoothT = GetTransationTexture(listSmooth, x, y + 1);

                    var transation = areaColorCoordinates.Center.FindTransitionTexture(BitmapMap[coordinates.SouthEast]);
                    //_MapID[_directions[(int)Directions.Location]] = RandomFromList(smoothT.Edge.First.List);
                    if (transation != null)
                    {
                        special = 2;
                        //z = _MapAlt[_directions[(int)Directions.SouthEast]] + _MapAlt[_directions[(int)Directions.NorthWest]];
                        //_mapObjects[_directions[(int)Directions.Center]].Texture = (short)RandomFromList(smoothT.Edge.CollectionFirst.List);
                        //z = _mapObjects[_directions[(int)Directions.SouthEast]].Altitude +
                        //    _mapObjects[_directions[(int)Directions.NorthWest]].Altitude;

                        mapObjectCoordinates.Center.Texture = (short)RandomFromList(transation.EdgeNorthWest.List);
                        z = mapObjectCoordinates.SouthEast.Altitude +
                            mapObjectCoordinates.NorthWest.Altitude;
                    }

                }
                if (special > 0)
                    //_MapOcc[_directions[(int)Directions.Location]] = 1;
                    //_mapObjects[_directions[(int)Directions.Center]].Occupied = 1;
                    mapObjectCoordinates.Center.Occupied = 1;

                //if (BitmapMapZ[_directions[(int)Directions.Center]] == Colors.Black)
                if (BitmapMapZ[coordinates.Center] == Colors.Black)
                {
                    //_MapAlt[_directions[(int)Directions.Location]] = z / 2;
                    //_mapObjects[_directions[(int)Directions.Center]].Altitude = (sbyte)(z / 2);
                    _mapObjects[coordinates.Center].Altitude = (sbyte)(z / 2);
                }

            }
        }

        /// <summary>
        /// utility find function
        /// </summary>
        /// <param name="listsmooth">list of all smooths</param>
        /// <param name="location"> </param>
        /// <returns></returns>
        private AreaTransitionTexture GetTransationTexture(IList<AreaTransitionTexture> listsmooth, int location)
        {
            return listsmooth.FirstOrDefault(s => s.ColorTo == BitmapMap[location]) ??
                                  listsmooth.FirstOrDefault();
        }
        #endregion

        #region Items Transations
        /// <summary>
        /// function to make the translations with items
        /// </summary>
        /// <param name="coordinates"> </param>
        /// <param name="areaColorCoordinates"> </param>
        /// <param name="mapObjectCoordinates"> </param>
        void ItemsTransations(Coordinates coordinates, AreaColorCoordinates areaColorCoordinates, MapObjectCoordinates mapObjectCoordinates)
        {
            //int special = 0;

            int zlev = 0;
            var item = new ItemClone();

            if (areaColorCoordinates.Center.TransitionItems.Count == 0)
                return;

            var colors = areaColorCoordinates.List.Select(o => o.Color);
            AreaTransitionItem k = null;

            foreach (var color in colors)
            {
                k = areaColorCoordinates.Center.FindTransationItemByColor(color);
                if (k != null) break;
            }
            if (k == null) return;


            //if ((_AddItemMap[_directions[(int)Directions.Location]] == null || _AddItemMap[_directions[(int)Directions.Location]].Count == 0) && _MapOcc[_directions[(int)Directions.Location]] == 0)
            if (mapObjectCoordinates.Center.Items == null && mapObjectCoordinates.Center.Occupied == 0)
            {
                //Border
                //GB
                //xG
                if (BitmapMap[coordinates.NorthEast] != areaColorCoordinates.Center.Color)
                {
                    var transation = areaColorCoordinates.Center.FindTransationItemByColor(areaColorCoordinates.NorthEast.Color);
                    //7
                    if (transation != null)
                        item = new ItemClone() { Id = RandomFromList(transation.BorderSouthWest.List) };
                }
                //BG
                //Gx
                if (BitmapMap[coordinates.NorthWest] != areaColorCoordinates.Center.Color)
                {
                    var transation = areaColorCoordinates.Center.FindTransationItemByColor(areaColorCoordinates.NorthWest.Color);
                    //6
                    if (transation != null)
                        item = new ItemClone { Id = RandomFromList(transation.BorderSouthEast.List) };
                }
                //Gx
                //BG
                if (BitmapMap[coordinates.SouthWest] != areaColorCoordinates.Center.Color)
                {
                    var transation = areaColorCoordinates.Center.FindTransationItemByColor(areaColorCoordinates.SouthWest.Color);
                    //5
                    if (transation != null)
                        item = new ItemClone { Id = RandomFromList(transation.BorderNorthEast.List) };
                }
                //xG
                //GB
                if (BitmapMap[coordinates.SouthEast] != areaColorCoordinates.Center.Color)
                {
                    var transation = areaColorCoordinates.Center.FindTransationItemByColor(areaColorCoordinates.SouthEast.Color);
                    //4
                    if (transation != null)
                        item = new ItemClone { Id = RandomFromList(transation.BorderNorthWest.List) };
                }

                //Line
                // B
                //GxG
                if (BitmapMap[coordinates.North] != areaColorCoordinates.Center.Color)
                {
                    var transation = areaColorCoordinates.Center.FindTransationItemByColor(areaColorCoordinates.North.Color);
                    //2
                    if (transation != null) item = new ItemClone { Id = RandomFromList(transation.LineSouth.List) };
                }
                //GxG
                // B
                if (BitmapMap[coordinates.South] != areaColorCoordinates.Center.Color)
                {
                    var transation = areaColorCoordinates.Center.FindTransationItemByColor(areaColorCoordinates.South.Color);
                    //0
                    if (transation != null) item = new ItemClone { Id = RandomFromList(transation.LineNorth.List) };
                }
                //G
                //xB
                //G
                if (BitmapMap[coordinates.East] != areaColorCoordinates.Center.Color)
                {
                    var transation = areaColorCoordinates.Center.FindTransationItemByColor(areaColorCoordinates.East.Color);
                    //3
                    if (transation != null) item = new ItemClone { Id = RandomFromList(transation.LineWest.List) };
                }
                // G
                //Bx
                // G
                if (BitmapMap[coordinates.West] != areaColorCoordinates.Center.Color)
                {
                    var transation = areaColorCoordinates.Center.FindTransationItemByColor(areaColorCoordinates.West.Color);
                    //1
                    if (transation != null) item = new ItemClone { Id = RandomFromList(transation.LineEast.List) };
                }

                //Edge
                //B
                //xB
                if (BitmapMap[coordinates.East] != areaColorCoordinates.Center.Color && BitmapMap[coordinates.North] != areaColorCoordinates.Center.Color)
                {
                    var transation = areaColorCoordinates.Center.FindTransationItemByColor(areaColorCoordinates.NorthEast.Color);
                    //11
                    if (transation != null) item = new ItemClone { Id = RandomFromList(transation.EdgeSouthWest.List) };
                }
                // B
                //Bx
                if (BitmapMap[coordinates.West] != areaColorCoordinates.Center.Color && BitmapMap[coordinates.North] != areaColorCoordinates.Center.Color)
                {
                    var transation = areaColorCoordinates.Center.FindTransationItemByColor(areaColorCoordinates.NorthWest.Color);
                    //10
                    if (transation != null) item = new ItemClone { Id = RandomFromList(transation.EdgeSouthEast.List) };
                }
                //Bx
                // B
                if (BitmapMap[coordinates.West] != areaColorCoordinates.Center.Color && BitmapMap[coordinates.South] != areaColorCoordinates.Center.Color)
                {
                    var transation = areaColorCoordinates.Center.FindTransationItemByColor(areaColorCoordinates.SouthWest.Color);
                    //9
                    if (transation != null) item = new ItemClone { Id = RandomFromList(transation.EdgeNorthEast.List) };
                }
                //xB
                //B
                if (BitmapMap[coordinates.East] != areaColorCoordinates.Center.Color && BitmapMap[coordinates.South] != areaColorCoordinates.Center.Color)
                {
                    var transation = areaColorCoordinates.Center.FindTransationItemByColor(areaColorCoordinates.SouthEast.Color);
                    //8
                    if (transation != null) item = new ItemClone { Id = RandomFromList(transation.EdgeNorthWest.List) };
                }

                //if (item.Id == 0)
                //    item.Id = smoothItem.Border.List[5 % 4].List[0];


                var coast = areaColorCoordinates.Center;

                if (coast.Type == TypeColor.Water)
                {
                    zlev = Random.Next(coast.Min, coast.Max);
                    //item.Z = _MapAlt[_directions[(int)Directions.Location]] + zlev;
                    item.Z = (sbyte)(mapObjectCoordinates.Center.Altitude + zlev);
                }

                //if (_AddItemMap[_directions[(int)Directions.Location]] == null)
                //    _AddItemMap[_directions[(int)Directions.Location]] = new List<Item>();
                if (item.Id != 0)
                    if (mapObjectCoordinates.Center.Items == null)
                        mapObjectCoordinates.Center.Items = new List<ItemClone>();

                //_AddItemMap[_directions[(int)Directions.Location]].Add(item);
                if (item.Id != 0)
                    mapObjectCoordinates.Center.Items.Add(item);
            }
        }

        #endregion //Items Transations

        #endregion

        #region Coasts


        ///// <summary>
        ///// function to use if you didn't make a good coast bmp
        ///// </summary>
        //void CoastBMP()
        //{
        //    int x, y;
        //    Color A, B;
        //    int z = 2;

        //        for (x = Math.Max(2, MinX); x < _X; x++)
        //            for (y = Math.Max(2, MinY); y < _Y; y++)
        //            {
        //                A = ColorAreasCoast.List[0].Color;
        //                B = ColorAreasCoast.List[1].Color;
        //                if (BitmapMap[CalculateZone(x,y)] == A)
        //                {
        //                    //Seiten
        //                    if (BitmapMap[CalculateZone(x, y - z)] != A && BitmapMap[_directions[(int)Directions.West]] == A && BitmapMap[_directions[(int)Directions.East]] == A) _Tmp[CalculateZone(x, y)] = B;
        //                    if (BitmapMap[CalculateZone(x, y + z)] != A && BitmapMap[_directions[(int)Directions.West]] == A && BitmapMap[_directions[(int)Directions.East]] == A) _Tmp[CalculateZone(x, y)] = B;
        //                    if (BitmapMap[CalculateZone(x + z, y)] != A && BitmapMap[_directions[(int)Directions.North]] == A && BitmapMap[_directions[(int)Directions.South]] == A) _Tmp[CalculateZone(x, y)] = B;
        //                    if (BitmapMap[CalculateZone(x - z, y)] != A && BitmapMap[_directions[(int)Directions.North]] == A && BitmapMap[_directions[(int)Directions.South]] == A) _Tmp[CalculateZone(x, y)] = B;
        //                    //Kanten
        //                    if (BitmapMap[CalculateZone(x + z, y - z)] != A && BitmapMap[_directions[(int)Directions.East]] == A && BitmapMap[_directions[(int)Directions.North]] == A) _Tmp[CalculateZone(x, y)] = B;
        //                    if (BitmapMap[CalculateZone(x - z, y - z)] != A && BitmapMap[_directions[(int)Directions.West]] == A && BitmapMap[_directions[(int)Directions.North]] == A) _Tmp[CalculateZone(x, y)] = B;
        //                    if (BitmapMap[CalculateZone(x - z, y + z)] != A && BitmapMap[_directions[(int)Directions.West]] == A && BitmapMap[_directions[(int)Directions.South]] == A) _Tmp[CalculateZone(x, y)] = B;
        //                    if (BitmapMap[CalculateZone(x + z, y + z)] != A && BitmapMap[_directions[(int)Directions.East]] == A && BitmapMap[_directions[(int)Directions.South]] == A) _Tmp[CalculateZone(x, y)] = B;
        //                    //ECKEN
        //                    if (BitmapMap[CalculateZone(x + z, y)] != A && BitmapMap[CalculateZone(x, y - z)] != A) _Tmp[CalculateZone(x, y)] = B;
        //                    if (BitmapMap[CalculateZone(x - z, y)] != A && BitmapMap[CalculateZone(x, y - z)] != A) _Tmp[CalculateZone(x, y)] = B;
        //                    if (BitmapMap[CalculateZone(x - z, y)] != A && BitmapMap[CalculateZone(x, y + z)] != A) _Tmp[CalculateZone(x, y)] = B;
        //                    if (BitmapMap[CalculateZone(x + z, y)] != A && BitmapMap[CalculateZone(x, y + z)] != A) _Tmp[CalculateZone(x, y)] = B;
        //                }
        //            }


        //        for (x = Math.Max(2, MinX); x < _X; x++)
        //            for (y = Math.Max(2, MinY); y < _Y; y++)
        //                if (_Tmp[CalculateZone(x,y)] != Color.Black)
        //                    BitmapMap[CalculateZone(x,y)] = _Tmp[CalculateZone(x,y)];


        //}

        /// <summary>
        /// Make Coast method, it search if the given parameter is a coast or not
        /// </summary>
        private void MakeCoast(AreaColorCoordinates areaColorCoordinates, MapObjectCoordinates mapObjectCoordinates)
        {
            //Color Water = BitmapMap[_directions[(int)Directions.Center]];
            //int ID = 0;
            //if (!ItemsCoasts.FindCoastByColor(Water))
            //    return;
            int ID = 0;
            if (areaColorCoordinates.Center.Type != TypeColor.WaterCoast)
                return;

            if (areaColorCoordinates.List.All(o => o.Type == TypeColor.WaterCoast || o.Type == TypeColor.Water)) //&& areaColorCoordinates.List.All(area => area.Color == areaColorCoordinates.Center.Color))
            {
                PlaceCoastItem(areaColorCoordinates.Center.Coasts.Coast.Texture, mapObjectCoordinates, areaColorCoordinates);
                return;
            }

            Boolean found = false;


            #region Line
            //Line
            // LLL 
            // WxW
            //if (BitmapMap[_directions[(int)Directions.North]] == i.Ground.Color && BitmapMap[_directions[(int)Directions.West]] == Water && BitmapMap[_directions[(int)Directions.East]] == Water)
            if (
                areaColorCoordinates.North.Type != TypeColor.WaterCoast
                && areaColorCoordinates.West.Type == TypeColor.WaterCoast
                && areaColorCoordinates.East.Type == TypeColor.WaterCoast
                //&& areaColorCoordinates.NorthEast.Type != TypeColor.WaterCoast
                //&& areaColorCoordinates.NorthWest.Type != TypeColor.WaterCoast
                )
            {
                //num 2 
                //_MapID[_directions[(int)Directions.Location]] = RandomFromList(i.Ground.Lines[2 / 4].List[2 % 4].List);
                //_mapObjects[_directions[(int)Directions.Center]].Texture = (short)RandomFromList(i.Ground.LineSouth.List); ;
                //_MapAlt[_directions[(int)Directions.Location]] = -15;
                //_mapObjects[_directions[(int)Directions.Center]].Altitude = -15;
                //_mapObjects[_directions[(int)Directions.Center]].Occupied = 1;
                //ID = RandomFromList(i.Coast.LineNorth.List);
                ID = RandomFromList(areaColorCoordinates.North.Coasts.Coast.LineSouth.List);

                var texture= (short)RandomFromList(areaColorCoordinates.North.Coasts.Ground.LineSouth.List);
                mapObjectCoordinates.Center.Texture = texture;
                mapObjectCoordinates.Center.Altitude = -15;
                mapObjectCoordinates.Center.Occupied = 1;
                PlaceCoastItem(ID, mapObjectCoordinates, areaColorCoordinates);
                return;

            }
            // WxW
            //  L 
            //if (BitmapMap[_directions[(int)Directions.South]] == i.Ground.Color && BitmapMap[_directions[(int)Directions.West]] == Water && BitmapMap[_directions[(int)Directions.East]] == Water)
            if (
                areaColorCoordinates.South.Type != TypeColor.WaterCoast
                && areaColorCoordinates.West.Type == TypeColor.WaterCoast
                && areaColorCoordinates.East.Type == TypeColor.WaterCoast
                //&& areaColorCoordinates.SouthEast.Type != TypeColor.WaterCoast
                //&& areaColorCoordinates.SouthWest.Type != TypeColor.WaterCoast
                )
            {
                //num 0
                //_MapID[_directions[(int)Directions.Location]] = RandomFromList(i.Ground.Lines[0 / 4].List[0 % 4].List);
                //ID = RandomFromList(i.Coast.LineSouth.List);
                //_MapAlt[_directions[(int)Directions.Location]] = -15;
                //_mapObjects[_directions[(int)Directions.North]].Occupied = 1;
                //_mapObjects[_directions[(int)Directions.Center]].Altitude = -15;
                //_mapObjects[_directions[(int)Directions.Center]].Texture = (short)RandomFromList(i.Ground.LineNorth.List);
                mapObjectCoordinates.North.Occupied = 1;
                mapObjectCoordinates.Center.Altitude = -15;
                var texture =
                    (short)RandomFromList(areaColorCoordinates.South.Coasts.Ground.LineNorth.List);
                mapObjectCoordinates.Center.Texture = texture;
                ID = RandomFromList(areaColorCoordinates.South.Coasts.Coast.LineNorth.List);
                PlaceCoastItem(ID, mapObjectCoordinates, areaColorCoordinates);
                return;
            }
            // WL
            // xL 
            // WL
            //if (BitmapMap[_directions[(int)Directions.East]] == i.Ground.Color && BitmapMap[_directions[(int)Directions.North]] == Water && BitmapMap[_directions[(int)Directions.South]] == Water)
            if (
                areaColorCoordinates.East.Type != TypeColor.WaterCoast
                && areaColorCoordinates.North.Type == TypeColor.WaterCoast
                && areaColorCoordinates.South.Type == TypeColor.WaterCoast
                //&& areaColorCoordinates.SouthEast.Type != TypeColor.WaterCoast
                //&& areaColorCoordinates.NorthEast.Type != TypeColor.WaterCoast
                )
            {
                //3
                //_MapID[_directions[(int)Directions.Location]] = RandomFromList(i.Ground.Lines[3 / 4].List[3 % 4].List);
                //ID = RandomFromList(i.Coast.LineWest.List);
                ////_MapAlt[_directions[(int)Directions.Location]] = -15;
                //_mapObjects[_directions[(int)Directions.Center]].Texture = (short)RandomFromList(i.Ground.LineEast.List);
                //_mapObjects[_directions[(int)Directions.Center]].Altitude = -15;
                //_mapObjects[_directions[(int)Directions.East]].Occupied = 1;

                ID = RandomFromList(areaColorCoordinates.East.Coasts.Coast.LineWest.List);
                var texture = (short)RandomFromList(areaColorCoordinates.East.Coasts.Ground.LineWest.List);
                mapObjectCoordinates.Center.Texture = texture;
                mapObjectCoordinates.Center.Altitude = -15;
                mapObjectCoordinates.Center.Occupied = 1;
                PlaceCoastItem(ID, mapObjectCoordinates, areaColorCoordinates);
                return;
            }
            //  W
            // Lx
            //  W
            //if (BitmapMap[_directions[(int)Directions.West]] == i.Ground.Color && BitmapMap[_directions[(int)Directions.North]] == Water && BitmapMap[_directions[(int)Directions.South]] == Water)
            if (areaColorCoordinates.West.Type != TypeColor.WaterCoast
                && areaColorCoordinates.North.Type == TypeColor.WaterCoast
                && areaColorCoordinates.South.Type == TypeColor.WaterCoast
                //&& areaColorCoordinates.SouthWest.Type != TypeColor.WaterCoast
                //&& areaColorCoordinates.NorthWest.Type != TypeColor.WaterCoast
                )
            {
                //1
                ////_MapID[_directions[(int)Directions.Location]] = RandomFromList(i.Ground.Lines[1 / 4].List[1 % 4].List);
                //ID = RandomFromList(i.Coast.LineEast.List);
                ////if (neu.spezial != 2)
                ////    MapAlt(x, y) = RandomNum(-4, 2);
                ////_MapAlt[_directions[(int)Directions.Location]] = -15;
                ////_MapOcc[_directions[(int)Directions.Location]] = 1;
                //_mapObjects[_directions[(int)Directions.Center]].Texture = (short)RandomFromList(i.Ground.LineWest.List);
                //_mapObjects[_directions[(int)Directions.Center]].Altitude = (sbyte)Random.Next(-4, 2);
                //_mapObjects[_directions[(int)Directions.Center]].Occupied = 1;


                ID = RandomFromList(areaColorCoordinates.West.Coasts.Coast.LineEast.List);
                var texture = (short)RandomFromList(areaColorCoordinates.West.Coasts.Ground.LineWest.List);
                mapObjectCoordinates.Center.Texture = texture;
                mapObjectCoordinates.Center.Altitude = (sbyte)Random.Next(-4, 2);
                mapObjectCoordinates.Center.Occupied = 1;
                PlaceCoastItem(ID, mapObjectCoordinates, areaColorCoordinates);
                return;

            }
            #endregion // Line

            #region  Edge

            //Edge
            // LL
            // xL
            if (
                areaColorCoordinates.NorthEast.Type != TypeColor.WaterCoast
                && areaColorCoordinates.East.Type != TypeColor.WaterCoast
                && areaColorCoordinates.North.Type != TypeColor.WaterCoast
                )
            {
                ID = RandomFromList(areaColorCoordinates.NorthEast.Coasts.Coast.EdgeSouthWest.List);
                var texture = (short)RandomFromList(areaColorCoordinates.NorthEast.Coasts.Ground.EdgeSouthWest.List);
                mapObjectCoordinates.Center.Texture = texture;
                mapObjectCoordinates.Center.Altitude = -15;
                mapObjectCoordinates.Center.Occupied = 1;
                PlaceCoastItem(ID, mapObjectCoordinates, areaColorCoordinates);
                return;
            }
            // LL
            // Lx
            if (
                areaColorCoordinates.NorthWest.Type != TypeColor.WaterCoast
                && areaColorCoordinates.West.Type != TypeColor.WaterCoast
                && areaColorCoordinates.North.Type != TypeColor.WaterCoast
                )
            {
                ID = RandomFromList(areaColorCoordinates.NorthWest.Coasts.Coast.EdgeSouthEast.List);
                var texture = (short)RandomFromList(areaColorCoordinates.NorthWest.Coasts.Ground.EdgeSouthEast.List);
                mapObjectCoordinates.Center.Texture = texture;
                mapObjectCoordinates.Center.Altitude = -15;
                mapObjectCoordinates.Center.Occupied = 1;
                PlaceCoastItem(ID, mapObjectCoordinates, areaColorCoordinates);
                return;
            }
            // Lx
            // LL
            if (
                areaColorCoordinates.SouthWest.Type != TypeColor.WaterCoast
                && areaColorCoordinates.West.Type != TypeColor.WaterCoast
                && areaColorCoordinates.South.Type != TypeColor.WaterCoast
                )
            {
                ID = RandomFromList(areaColorCoordinates.SouthWest.Coasts.Coast.EdgeNorthEast.List);
                var texture = (short)RandomFromList(areaColorCoordinates.SouthWest.Coasts.Ground.EdgeNorthEast.List);
                mapObjectCoordinates.Center.Texture = texture;
                mapObjectCoordinates.Center.Altitude = -15;
                mapObjectCoordinates.Center.Occupied = 1;
                PlaceCoastItem(ID, mapObjectCoordinates, areaColorCoordinates);
                return;
            }
            // xL
            // LL
            if (
                areaColorCoordinates.SouthEast.Type != TypeColor.WaterCoast
                && areaColorCoordinates.East.Type != TypeColor.WaterCoast
                && areaColorCoordinates.South.Type != TypeColor.WaterCoast
                )
            {
                ID = RandomFromList(areaColorCoordinates.SouthEast.Coasts.Coast.EdgeNorthWest.List);
                var texture = (short)RandomFromList(areaColorCoordinates.SouthEast.Coasts.Ground.EdgeNorthWest.List);
                mapObjectCoordinates.Center.Texture = texture;
                mapObjectCoordinates.Center.Altitude = -15;
                mapObjectCoordinates.Center.Occupied = 1;
                PlaceCoastItem(ID, mapObjectCoordinates, areaColorCoordinates);
                return;
            }

            #endregion //Edge

            #region Border
            //Border
            // WL
            // xW
            if (
                areaColorCoordinates.NorthEast.Type != TypeColor.WaterCoast 
                && areaColorCoordinates.East.Type == TypeColor.WaterCoast
                && areaColorCoordinates.North.Type == TypeColor.WaterCoast
                )
            {
                ID = RandomFromList(areaColorCoordinates.NorthEast.Coasts.Coast.BorderSouthEast.List);
                var texture= (short)RandomFromList(areaColorCoordinates.NorthEast.Coasts.Ground.BorderNorthEast.List);
                if (texture == 100)
                {
                    int a = 0;
                    a++;
                }
                mapObjectCoordinates.Center.Texture = texture;
                mapObjectCoordinates.Center.Altitude = -15;
                mapObjectCoordinates.Center.Occupied = 1;
                PlaceCoastItem(ID, mapObjectCoordinates, areaColorCoordinates);
                return;
            }
            // LW
            // Wx
            if (
                areaColorCoordinates.NorthWest.Type != TypeColor.WaterCoast 
                && areaColorCoordinates.West.Type == TypeColor.WaterCoast
                && areaColorCoordinates.North.Type == TypeColor.WaterCoast
                )
            {
                ID = RandomFromList(areaColorCoordinates.NorthWest.Coasts.Coast.BorderSouthWest.List);
                var texture= (short)RandomFromList(areaColorCoordinates.NorthWest.Coasts.Ground.BorderNorthWest.List);

                mapObjectCoordinates.Center.Texture = texture;
                mapObjectCoordinates.Center.Altitude = -15;
                mapObjectCoordinates.Center.Occupied = 1;

                PlaceCoastItem(ID, mapObjectCoordinates, areaColorCoordinates);
                return;

            }
            // Wx
            // LW
            if (
                areaColorCoordinates.SouthWest.Type != TypeColor.WaterCoast
                && areaColorCoordinates.West.Type == TypeColor.WaterCoast
                && areaColorCoordinates.South.Type == TypeColor.WaterCoast
                )
            {
                ID = RandomFromList(areaColorCoordinates.SouthWest.Coasts.Coast.BorderNorthWest.List);
                var texture = (short)RandomFromList(areaColorCoordinates.SouthWest.Coasts.Ground.BorderSouthWest.List);
                mapObjectCoordinates.Center.Texture = texture;
                mapObjectCoordinates.Center.Altitude = -15;
                mapObjectCoordinates.SouthWest.Occupied = 1;
                PlaceCoastItem(ID, mapObjectCoordinates, areaColorCoordinates);
                return;
            }
            // xW
            // WL
            if (
                areaColorCoordinates.SouthEast.Type != TypeColor.WaterCoast
                && areaColorCoordinates.East.Type == TypeColor.WaterCoast
                && areaColorCoordinates.South.Type == TypeColor.WaterCoast
                )
            {
                ID = RandomFromList(areaColorCoordinates.SouthEast.Coasts.Coast.BorderNorthEast.List);
                var texture = (short)RandomFromList(areaColorCoordinates.SouthEast.Coasts.Ground.BorderSouthEast.List);
                mapObjectCoordinates.Center.Texture = texture;
                mapObjectCoordinates.Center.Altitude = -15;
                mapObjectCoordinates.Center.Occupied = 1;
                PlaceCoastItem(ID, mapObjectCoordinates, areaColorCoordinates);
                return;
            }

            #endregion //Border



        }

        private void PlaceCoastItem(int id, MapObjectCoordinates mapObjectCoordinates, AreaColorCoordinates areaColorCoordinates)
        {
            //if (off > 1)
            //    off = 1;
            if(id == 0)
            {
                int a = 2;
                a++;
            }

            // first revision i know of (ghoulsblade,28.07.2010) : http://zwischenwelt.org/trac/irisserver/browser/tools/uotc/src/coast.cpp?rev=4
            // uotc: additem	*Add[XBORDER+5][YBORDER+5];
            var item = new ItemClone() { Id = id };
            //if (_AddItemMap[_directions[(int)Directions.Location]] == null) _AddItemMap[_directions[(int)Directions.Location]] = new List<Item>();
            //_AddItemMap[_directions[(int)Directions.Location]].Add(item); // uotc : additem
            if (mapObjectCoordinates.Center.Items == null) mapObjectCoordinates.Center.Items = new List<ItemClone>();
            mapObjectCoordinates.Center.Items.Add(item);

            //SiENcE mod
            if (!AutomaticZMode)
            {
                //item.Z = _MapAlt[CalculateZone(1, 1)];
                item.Z = _mapObjects[CalculateZone(1, 1, _stride)].Altitude;
            }
            else
            {
                //item.Z = _MapAlt[_directions[(int)Directions.Location]] + Random.Next(coast.Low, coast.Hight);		// Zuweisung der Coastitemshöhe über die Farbe
                item.Z = (sbyte)(mapObjectCoordinates.Center.Altitude +
                         Random.Next(areaColorCoordinates.Center.Min, areaColorCoordinates.Center.Max));
            }
        }

        #endregion

        #region CollectionAreaCliffs

        /// <summary>
        /// method to make cliff
        /// </summary>
        /// <param name="coordinates"> </param>
        /// <param name="Areacoordinates"> </param>
        /// <param name="mapObjectCoordinates"> </param>
        private void MakeCliffs(Coordinates coordinates, AreaColorCoordinates Areacoordinates, MapObjectCoordinates mapObjectCoordinates)
        {
            if (Areacoordinates.Center.Type != TypeColor.Cliff) return;
            //_MapAlt[_directions[(int)Directions.Location]] = 0;									
            mapObjectCoordinates.Center.Altitude = 0;

            //**********************
            //*       Line         *
            //**********************

            //  ? 
            // CXC
            //  ? 
            //if (BitmapMap[_directions[(int)Directions.West]] == CollectionAreaCliffs.Color && BitmapMap[_directions[(int)Directions.East]] == CollectionAreaCliffs.Color)
            //{
            //    SetCliff(x, y, x, y - 1, x, y + 1, (DirectionCliff)0);
            //}
            if (Areacoordinates.West.Type == TypeColor.Cliff && Areacoordinates.East.Type == TypeColor.Cliff)
            {
                var areaTransitionCliffTexture =
                    Areacoordinates.
                    North.
                    TransitionCliffTextures.
                    FirstOrDefault(c => c.Directions == DirectionCliff.NorthSouth
                    && c.ColorTo == BitmapMap[coordinates.South]);

                if (areaTransitionCliffTexture != null)
                    mapObjectCoordinates.Center.Texture = (short)RandomFromList(areaTransitionCliffTexture.List);
            }
            //  C
            // ?X?
            //  C
            if (Areacoordinates.North.Type == TypeColor.Cliff && Areacoordinates.South.Type == TypeColor.Cliff)
            {
                var areaTransitionCliffTexture =
                    Areacoordinates.
                    West.
                    TransitionCliffTextures.
                    FirstOrDefault
                    (c => c.Directions == DirectionCliff.WestEast
                        && c.ColorTo == BitmapMap[coordinates.East]);
                if (areaTransitionCliffTexture != null)
                    mapObjectCoordinates.Center.Texture = (short)RandomFromList(areaTransitionCliffTexture.List);
            }


            //**********************
            //* Anfang und Ende    *
            //**********************

            //  ! 
            // ?X?
            //  C
            if (Areacoordinates.South.Type == TypeColor.Cliff && Areacoordinates.North.Type != TypeColor.Cliff)
            {
                var areaTransitionCliffTexture =
                    Areacoordinates.East.TransitionCliffTextures.
                    FirstOrDefault(c => c.Directions == DirectionCliff.NorthEnd && c.ColorTo == Areacoordinates.West.Color);
                if (areaTransitionCliffTexture != null)
                    mapObjectCoordinates.Center.Texture = (short)RandomFromList(areaTransitionCliffTexture.List);

            }

            //  ? 
            // CX!
            //  ?
            if (Areacoordinates.West.Type == TypeColor.Cliff && Areacoordinates.East.Type != TypeColor.Cliff)
            {
                //SetCliff(x, y, x, y - 1, x, y + 1, (DirectionCliff)3);
                var areaTransitionCliffTexture = Areacoordinates.North.TransitionCliffTextures.
                    FirstOrDefault(c => c.Directions == DirectionCliff.EastEnd && c.ColorTo == Areacoordinates.South.Color);
                if (areaTransitionCliffTexture != null)
                    mapObjectCoordinates.Center.Texture = (short)RandomFromList(areaTransitionCliffTexture.List);

            }


            //  C 
            // ?X?
            //  !
            if (Areacoordinates.South.Type == TypeColor.Cliff && Areacoordinates.North.Type != TypeColor.Cliff)
            {
                //SetCliff(x, y, x - 1, y, x + 1, y, (DirectionCliff)4);
                var areaTransitionCliffTexture = Areacoordinates.East.TransitionCliffTextures.
                    FirstOrDefault(c => c.Directions == DirectionCliff.SouthEnd &&
                        c.ColorTo == Areacoordinates.West.Color);
                if (areaTransitionCliffTexture != null)
                    mapObjectCoordinates.Center.Texture = (short)RandomFromList(areaTransitionCliffTexture.List);

            }


            //  ? 
            // !XC
            //  ?
            if (Areacoordinates.East.Type == TypeColor.Cliff && Areacoordinates.West.Type != TypeColor.Cliff)
            {
                var areaTransitionCliffTexture = Areacoordinates.South.TransitionCliffTextures.
                    FirstOrDefault(c => c.Directions == DirectionCliff.WestEnd
                        && c.ColorTo == Areacoordinates.North.Color);
                if (areaTransitionCliffTexture != null)
                    mapObjectCoordinates.Center.Texture = (short)RandomFromList(areaTransitionCliffTexture.List);
            }

            //**********************
            //* Rundungen          *
            //**********************

            //  C 
            // CX
            //   ?
            if (Areacoordinates.West.Type == TypeColor.Cliff && Areacoordinates.North.Type == TypeColor.Cliff)
            {
                var areaTransitionCliffTexture = Areacoordinates.SouthEast.TransitionCliffTextures.
                    FirstOrDefault(c => c.Directions == DirectionCliff.NorthWestRounding &&
                        c.ColorTo == Areacoordinates.NorthWest.Color);
                if (areaTransitionCliffTexture != null)
                    mapObjectCoordinates.Center.Texture = (short)RandomFromList(areaTransitionCliffTexture.List);
            }

            //  C 
            //  XC
            // ?
            if (Areacoordinates.East.Type == TypeColor.Cliff && Areacoordinates.North.Type == TypeColor.Cliff)
            {
                var areaTransitionCliffTexture = Areacoordinates.Center.TransitionCliffTextures.
                    FirstOrDefault(
                    c => c.Directions == DirectionCliff.NorthEastRounding
                        && c.ColorTo == Areacoordinates.NorthEast.Color);
                if (areaTransitionCliffTexture != null)
                    mapObjectCoordinates.Center.Texture = (short)RandomFromList(areaTransitionCliffTexture.List);
            }

            // ? 
            //  XC
            //  C
            if (Areacoordinates.East.Type == TypeColor.Cliff && Areacoordinates.South.Type == TypeColor.Cliff)
            {
                //SetCliff(x, y, x - 1, y - 1, -500, 0, (DirectionCliff)8);
                var areaTransitionCliffTexture = Areacoordinates.NorthWest.TransitionCliffTextures.
                    FirstOrDefault(
                    c => c.Directions == DirectionCliff.SouthEastRounding
                        && c.ColorTo == Areacoordinates.SouthEast.Color);
                if (areaTransitionCliffTexture != null)
                    mapObjectCoordinates.Center.Texture = (short)RandomFromList(areaTransitionCliffTexture.List);
            }

            //   ?
            // CX
            //  C
            if (Areacoordinates.West.Type == TypeColor.Cliff && Areacoordinates.North.Type == TypeColor.Cliff)
            {
                //SetCliff(x, y, x + 1, y - 1, -500, 0, (DirectionCliff)9);
                var areaTransitionCliffTexture = Areacoordinates.NorthEast.TransitionCliffTextures.
                    FirstOrDefault(
                    c => c.Directions == DirectionCliff.SouthWestRounding
                        && c.ColorTo == Areacoordinates.SouthWest.Color);
                if (areaTransitionCliffTexture != null)
                    mapObjectCoordinates.Center.Texture = (short)RandomFromList(areaTransitionCliffTexture.List);
            }
        }

        ///// <summary>
        ///// method to handle the dirty work
        ///// </summary>
        ///// <param name="x">x param</param>
        ///// <param name="y">y param</param>
        ///// <param name="x1">x1 direction changed</param>
        ///// <param name="y1">y1 direction changed</param>
        ///// <param name="x2">x2 direction changed</param>
        ///// <param name="y2">y2 direction changed</param>
        ///// <param name="directions">direction thatn you need</param>
        //private void SetCliff(int x, int y, int x1, int y1, int x2, int y2, DirectionCliff directions)
        //{
        //    AreaTransitionCliffTexture cliff = null;

        //    if (x2 != -500)
        //    {
        //        cliff = CollectionAreaCliffs.FindFromByColor(BitmapMap[CalculateZone(x1, y1)]).FirstOrDefault(
        //            c => c.Directions == directions);
        //    }
        //    else
        //    {
        //        cliff =
        //            CollectionAreaCliffs.FindFromByColor(BitmapMap[CalculateZone(x1, y1)]).FirstOrDefault(
        //                c => c.ColorTo == BitmapMap[CalculateZone(x2, y2)] && c.Directions == directions) ??
        //            CollectionAreaCliffs.FindFromByColor(BitmapMap[CalculateZone(x1, y1)]).FirstOrDefault(
        //                c => c.Directions == directions);
        //    }

        //    if (cliff != null)
        //    {
        //        //_MapID[CalculateZone(x,y)] = RandomFromList(cliff.List);
        //        _mapObjects[CalculateZone(x, y)].Texture = (short)RandomFromList(cliff.List);
        //    }
        //}

        #endregion


        #region Items



        /// <summary>
        /// method to set items in the map
        /// </summary>
        void SetItem()
        {
            int x, y, z = 0;

            for (x = MinX; x < _X; x++)
                for (y = MinY; y < _Y; y++)
                {
                    var location = CalculateZone(x, y, _stride);
                    //if (_MapOcc[location] == 0) 
                    if (_mapObjects[location].Occupied == 0)
                    {
                        var itemgroups = CollectionAreaColor.FindByColor(BitmapMap[location]).Items;
                        if (itemgroups != null && itemgroups.List.Count > 0)
                        {
                            var group = itemgroups.List[Random.Next(0, itemgroups.List.Count)];
                            var random = Random.Next(0, 100);
                            if (random > group.Percent) continue;

                            var tmp_item = group.List.First();
                            if (group.List.Count > 1)
                            {
                                z = tmp_item.Z;
                            }

                            foreach (SingleItem item in group.List)
                            {
                                var locationshift = CalculateZone(x + item.X, y + item.Y, _stride);
                                //if (_AddItemMap[locationshift] == null)
                                //    _AddItemMap[locationshift] = new List<Item>();
                                if (_mapObjects[locationshift].Items == null)
                                    _mapObjects[locationshift].Items = new List<ItemClone>();
                                var itemclone = new ItemClone(item);

                                //_AddItemMap[locationshift].Add(itemclone);
                                _mapObjects[locationshift].Items.Add(itemclone);
                                if (tmp_item == item)
                                {
                                    //itemclone.Z = (_MapAlt[locationshift] +
                                    //          _MapAlt[CalculateZone(x + item.X + 1, y + item.Y)] +
                                    //          _MapAlt[CalculateZone(x + item.X, y + item.Y + 1)] +
                                    //          _MapAlt[CalculateZone(x + item.X + 1, y + item.Y + 1)]) / 4 + item.Z;
                                    itemclone.Z = (sbyte)((_mapObjects[locationshift].Altitude +
                                              _mapObjects[CalculateZone(x + item.X + 1, y + item.Y, _stride)].Altitude +
                                              _mapObjects[CalculateZone(x + item.X, y + item.Y + 1, _stride)].Altitude +
                                              _mapObjects[CalculateZone(x + item.X + 1, y + item.Y + 1, _stride)].Altitude) / 4 + item.Z);
                                }
                                else
                                {
                                    //itemclone.Z = (_MapAlt[CalculateZone(x + tmp_item.X, y + tmp_item.Y)] +
                                    //          _MapAlt[CalculateZone(x + tmp_item.X + 1, y + tmp_item.Y)] +
                                    //          _MapAlt[CalculateZone(x + tmp_item.X, y + tmp_item.Y + 1)] +
                                    //          _MapAlt[CalculateZone(x + tmp_item.X + 1, y + tmp_item.Y + 1)]) / 4 + tmp_item.Z + z;
                                    itemclone.Z = (sbyte)((_mapObjects[CalculateZone(x + tmp_item.X, y + tmp_item.Y, _stride)].Altitude +
                                              _mapObjects[CalculateZone(x + tmp_item.X + 1, y + tmp_item.Y, _stride)].Altitude +
                                              _mapObjects[CalculateZone(x + tmp_item.X, y + tmp_item.Y + 1, _stride)].Altitude +
                                              _mapObjects[CalculateZone(x + tmp_item.X + 1, y + tmp_item.Y + 1, _stride)].Altitude) / 4 + tmp_item.Z + z);

                                }
                            }
                        }
                    }
                }
        }

        #endregion

        #region Mul Handers

        /// <summary>
        /// method to write statics
        /// </summary>
        private void WriteStatics()
        {
            int blockx, blocky, x, y, items;
            short color;
            byte x2, y2;
            sbyte waterlevel = -120;
            Int32 length = 0, start;

            var staidx = new FileStream(Path.Combine(MulDirectory, string.Format("staidx{0}.mul", mapIndex)), FileMode.OpenOrCreate);
            var statics = new FileStream(Path.Combine(MulDirectory, string.Format("statics{0}.mul", mapIndex)), FileMode.OpenOrCreate);
            var statics0 = new BinaryWriter(statics);
            var staidx0 = new BinaryWriter(staidx);

            items = 0;
            start = 0;

            using (staidx)
            {
                using (statics)
                {
                    using (statics0)
                    {
                        using (staidx0)
                        {
                            for (blockx = 0; blockx < (_X / 8); ++blockx)
                            {
                                for (blocky = 0; blocky < (_Y / 8); ++blocky)
                                {
                                    length = 0;
                                    for (y = (8 * blocky); y < (8 * (blocky + 1)); y++)
                                    {
                                        for (x = (8 * blockx); x < (8 * (blockx + 1)); x++)
                                        {
                                            x2 = (byte)(x % 8);
                                            y2 = (byte)(y % 8);
                                            var local = CalculateZone(x, y, _stride);
                                            //if (_AddItemMap[CalculateZone(x,y)] != null)
                                            if (_mapObjects[local].Items != null)
                                            {
                                                //foreach (var item in _AddItemMap[CalculateZone(x,y)])
                                                foreach (var item in _mapObjects[local].Items)
                                                {
                                                    statics0.Write((ushort)item.Id);
                                                    statics0.Write((byte)x2);
                                                    statics0.Write((byte)y2);
                                                    statics0.Write((sbyte)item.Z);
                                                    statics0.Write((Int16)item.Hue);
                                                    length += 7;
                                                    items++;
                                                }
                                            }
                                        }

                                    }

                                    staidx0.Write(start);
                                    start += length;
                                    staidx0.Write(length);
                                    staidx0.Write((Int32)1);


                                }
                            }
                            statics0.Flush();
                            staidx0.Flush();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// method to write map.mul
        /// </summary>
        void WriteMUL()
        {
            int blockx, blocky, x, y;
            int empty = 0;
            int grey = 0x0244;

            var mapmul = new FileStream(Path.Combine(MulDirectory, "map" + mapIndex + ".mul"), FileMode.OpenOrCreate);
            var map0 = new BinaryWriter(mapmul);
            using (mapmul)
            {
                using (map0)
                {
                    for (blockx = 0; blockx < (_X / 8); ++blockx)
                    {
                        for (blocky = 0; blocky < (_Y / 8); ++blocky)
                        {
                            map0.Write((int)1);//header
                            for (y = (8 * blocky); y < (8 * (blocky + 1)); y++)
                            {
                                for (x = (8 * blockx); x < (8 * (blockx + 1)); x++)
                                {
                                    var local = CalculateZone(x, y, _stride);

                                    //var id = _MapID[CalculateZone(x,y)];
                                    //var z = _MapAlt[CalculateZone(x,y)];
                                    var id = _mapObjects[local].Texture;
                                    var z = _mapObjects[local].Altitude;
                                    if ((id < 0) || (id >= 0x4000))
                                        id = 0;

                                    map0.Write((short)id);//writes tid
                                    map0.Write((sbyte)z);//writes Z
                                }
                            }
                        }
                    }
                    map0.Flush();
                }

            }
        }

        #endregion

        #region utility methods

        /// <summary>
        /// it takes a random member in a list of int
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private int RandomFromList(IList<int> list)
        {
            if (list.Count == 0)
                return 0;
            int number = Random.Next(0, list.Count - 1);
            return list[number];
        }

        private int RandomTexture(int index)
        {
            var textures = TextureAreas.FindByIndex(index);
            return textures == null ? 0 : RandomFromList(textures.List);
        }

        /// <summary>
        /// coordinate for x and y in linear matrix
        /// </summary>
        /// <param name="x">x coord</param>
        /// <param name="y">y coord</param>
        /// <returns></returns>
        public static int CalculateZone(int x, int y, int stride)
        {
            return (y * stride) + x;
        }

        /// <summary>
        /// array of precalculated x and y in a linear matrix
        /// </summary>
        /// <param name="x">x coord</param>
        /// <param name="y">y coord</param>
        /// <param name="shift">shifting parameter</param>
        /// <returns>array of params</returns>
        private Coordinates MakeIndexesDirections(int x, int y, int shiftX, int shiftY)
        {
            return new Coordinates(shiftX, shiftY, x, y, _stride);
        }



        #endregion

        #endregion
    }

    /// <summary>
    /// Directions for array
    /// </summary>
    internal enum Directions
    {
        Center,
        North,
        South,
        East,
        West,
        NorthWest,
        NorthEast,
        SouthWest,
        SouthEast
    }

    internal struct Coordinates
    {
        private readonly int[] _array;
        //private readonly int _center;
        //private readonly int _north;
        //private readonly int _south;
        //private readonly int _east;
        //private readonly int _west;
        //private readonly int _northEast;
        //private readonly int _northWest;
        //private readonly int _southEast;
        //private readonly int _southWest;
        //private readonly int _x;
        //private readonly int _y;

        //public int Center { get { return _center; } }
        //public int North { get { return _north; } }
        //public int South { get { return _south; } }
        //public int East { get { return _east; } }
        //public int West { get { return _west; } }
        //public int NorthEast { get { return _northEast; } }
        //public int NorthWest { get { return _northWest; } }
        //public int SouthEast { get { return _southEast; } }
        //public int SouthWest { get { return _southWest; } }
        //public int X { get { return _x; } }
        //public int Y { get { return _y; } }
        public int Center { get { return _array[0]; } }
        public int North { get { return _array[1]; } }
        public int South { get { return _array[2]; } }
        public int East { get { return _array[3]; } }
        public int West { get { return _array[4]; } }
        public int NorthEast { get { return _array[5]; } }
        public int NorthWest { get { return _array[6]; } }
        public int SouthEast { get { return _array[7]; } }
        public int SouthWest { get { return _array[8]; } }
        public int X { get { return _array[9]; } }
        public int Y { get { return _array[10]; } }

        public int[] List { get { return _array; } }

        public Coordinates(int shiftX, int shiftY, int x, int y, int stride)
        {
            _array = new int[11];
            _array[0] = MapMaker.CalculateZone(x, y, stride);
            _array[1] = MapMaker.CalculateZone(x, y - shiftY, stride); ;
            _array[2] = MapMaker.CalculateZone(x, y + shiftY, stride);
            _array[3] = MapMaker.CalculateZone(x + shiftX, y, stride);
            _array[4] = MapMaker.CalculateZone(x - shiftX, y, stride);
            _array[5] = MapMaker.CalculateZone(x - shiftX, y - shiftY, stride);
            _array[6] = MapMaker.CalculateZone(x + shiftX, y - shiftY, stride);
            _array[7] = MapMaker.CalculateZone(x - shiftX, y + shiftY, stride);
            _array[8] = MapMaker.CalculateZone(x + shiftX, y + shiftY, stride);
            _array[9] = x;
            _array[10] = y;
            //_x = x;
            //_y = y;
            //_center = MapMaker.CalculateZone(x, y, stride);
            //_north = MapMaker.CalculateZone(x, y - shiftY, stride);
            //_south = MapMaker.CalculateZone(x, y + shiftY, stride);
            //_east = MapMaker.CalculateZone(x + shiftX, y, stride);
            //_west = MapMaker.CalculateZone(x - shiftX, y, stride);
            //_northWest = MapMaker.CalculateZone(x - shiftX, y - shiftY, stride);
            //_northEast = MapMaker.CalculateZone(x + shiftX, y - shiftY, stride);
            //_southWest = MapMaker.CalculateZone(x - shiftX, y + shiftY, stride);
            //_southEast = MapMaker.CalculateZone(x + shiftX, y + shiftY, stride);

        }
    }

    internal class AreaColorCoordinates
    {
        private readonly AreaColor _center;
        private readonly AreaColor _north;
        private readonly AreaColor _south;
        private readonly AreaColor _east;
        private readonly AreaColor _west;
        private readonly AreaColor _northEast;
        private readonly AreaColor _northWest;
        private readonly AreaColor _southEast;
        private readonly AreaColor _southWest;

        public AreaColor Center { get { return _center; } }
        public AreaColor North { get { return _north; } }
        public AreaColor South { get { return _south; } }
        public AreaColor East { get { return _east; } }
        public AreaColor West { get { return _west; } }
        public AreaColor NorthEast { get { return _northEast; } }
        public AreaColor NorthWest { get { return _northWest; } }
        public AreaColor SouthEast { get { return _southEast; } }
        public AreaColor SouthWest { get { return _southWest; } }

        public AreaColor[] List { get; set; }

        public AreaColorCoordinates(CollectionAreaColor collection, Coordinates coordinates, Color[] map)
        {
            _center = collection.FindByColor(map[coordinates.Center]);
            _north = collection.FindByColor(map[coordinates.North]);
            _south = collection.FindByColor(map[coordinates.South]);
            _east = collection.FindByColor(map[coordinates.East]);
            _west = collection.FindByColor(map[coordinates.West]);
            _northWest = collection.FindByColor(map[coordinates.NorthWest]);
            _northEast = collection.FindByColor(map[coordinates.NorthEast]);
            _southWest = collection.FindByColor(map[coordinates.SouthWest]);
            _southEast = collection.FindByColor(map[coordinates.SouthEast]);

            List = new[] { _center, _north, _south, _east, _west, _northEast, _northWest, _southEast, _southWest };
        }
    }

    internal class MapObjectCoordinates
    {
        private readonly MapObject _center;
        private readonly MapObject _north;
        private readonly MapObject _south;
        private readonly MapObject _east;
        private readonly MapObject _west;
        private readonly MapObject _northEast;
        private readonly MapObject _northWest;
        private readonly MapObject _southEast;
        private readonly MapObject _southWest;

        public MapObject Center { get { return _center; } }
        public MapObject North { get { return _north; } }
        public MapObject South { get { return _south; } }
        public MapObject East { get { return _east; } }
        public MapObject West { get { return _west; } }
        public MapObject NorthEast { get { return _northEast; } }
        public MapObject NorthWest { get { return _northWest; } }
        public MapObject SouthEast { get { return _southEast; } }
        public MapObject SouthWest { get { return _southWest; } }

        public MapObject[] List { get; set; }

        public MapObjectCoordinates(Coordinates coordinates, MapObject[] map)
        {
            _center = map[coordinates.Center];
            _north = map[coordinates.North];
            _south = map[coordinates.South];
            _east = map[coordinates.East];
            _west = map[coordinates.West];
            _northWest = map[coordinates.NorthWest];
            _northEast = map[coordinates.NorthEast];
            _southWest = map[coordinates.SouthWest];
            _southEast = map[coordinates.SouthEast];

            List = new[] { _center, _north, _south, _east, _west, _northEast, _northWest, _southEast, _southWest };
        }
    }

    internal class MapObject
    {
        public byte Occupied;
        public short Texture;
        public sbyte Altitude;
        public List<ItemClone> Items;

        public MapObject()
        {
            Occupied = 0;
            Texture = 0;
            Altitude = 0;
            Items = null;
        }
    }

    internal struct ItemClone
    {
        private int _x;
        private int _y;
        private int _z;
        private int _hue;
        private int _id;

        public int X
        {
            get { return _x; }
            set { _x = value; }
        }

        public int Y { get { return _y; } set { _y = value; } }

        public int Id { get { return _id; } set { _id = value; } }

        public int Z { get { return _z; } set { _z = value; } }

        public int Hue { get { return _hue; } set { _hue = value; } }

        public ItemClone(SingleItem item)
        {
            _x = item.X;
            _y = item.Y;
            _z = item.Z;
            _id = item.Id;
            _hue = item.Hue;
        }

        public SingleItem ToSingleItem()
        {
            return new SingleItem() { Id = Id, Hue = _hue, X = X, Y = Y, Z = Z };
        }
    }

}
