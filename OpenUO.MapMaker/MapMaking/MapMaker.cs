using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using EssenceUDK.MapMaker.Elements;
using EssenceUDK.MapMaker.Elements.ColorArea;
using EssenceUDK.MapMaker.Elements.ColorArea.ColorArea;
using EssenceUDK.MapMaker.Elements.Items.ItemText;
using EssenceUDK.MapMaker.Elements.Items.ItemsTransition;
using EssenceUDK.MapMaker.Elements.Textures;
using EssenceUDK.MapMaker.Elements.Textures.TextureTransition;
using EssenceUDK.MapMaker.Elements.Textures.TexureCliff;

namespace EssenceUDK.MapMaker.MapMaking
{
    public class MapMaker
    {
        #region Fields

        public static Random Random { get; set; }
        public int MinX = 1;
        public int MinY = 1;
        private readonly int _stride;
        //private Color[] _bitmap;
        private sbyte[] _bitmapZ;
        private double _progressPerc;

        private AreaColor[] _bitmapAreaColor;

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
        ///// <summary>
        ///// Class costructor
        ///// </summary>
        ///// <param name="map">map cached previusly</param>
        ///// <param name="alt">map altitude cached</param>
        ///// <param name="x">max x of the map</param>
        ///// <param name="y">max y of the map</param>
        ///// <param name="index">index of the map</param>
        //public MapMaker(Color[] map, Color[] alt, int x, int y, int index)
        //{

        //    _bitmap = map;
        //    var x1 = x + 10;
        //    var y1 = y + 10;
        //    var lenght = x1 * y1;
        //    #region InitArrays

        //    _mapObjects = new MapObject[lenght];
        //    _bitmapZ = new sbyte[lenght];
        //    for (int i = 0; i < alt.Length; i++)
        //    {
        //        _bitmapZ[i] = CalculateHeightValue(alt[i]);

        //    }

        //    for (int i = 0; i < _mapObjects.Length; i++)
        //    {
        //        _mapObjects[i] = new MapObject();
        //    }
        //    #endregion

        //    _X = x;
        //    _Y = y;

        //    MulDirectory = "";
        //    mapIndex = index;
        //    _stride = _X;
        //    Random = new Random(DateTime.Now.Millisecond);

        //    AutomaticZMode = true;
        //}


        public MapMaker(sbyte[] altitude, AreaColor[] colors, int x, int y, int index)
        {
            var x1 = x + 10;
            var y1 = y + 10;
            var lenght = x1 * y1;


            #region Init Arrays

            _mapObjects = new MapObject[lenght];
            _bitmapZ = new sbyte[lenght];
            _bitmapAreaColor = new AreaColor[lenght];


            for (int i = 0; i < altitude.Length; i++)
            {
                _bitmapZ[i] = altitude[i];
                _bitmapAreaColor[i] = colors[i];
            }

            for (int i = 0; i < lenght; i++)
            {
                _mapObjects[i] = new MapObject();
            }

            #endregion //init arrays


            _X = x;
            _Y = y;
            MulDirectory = "";
            mapIndex = index;
            _stride = _X;
            Random = new Random(DateTime.Now.Millisecond);
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

            //if (AutomaticZMode)
            //{
            //    OnProgressText(new ProgressEventArgs(){PayLoad = "Making Mountains"});
            //    Mountain();
                
            //}
            var count = Environment.ProcessorCount;
            var maptasks = new Task[count];
            OnProgressText(new ProgressEventArgs() { PayLoad = "Making Map" });

            for (int i = 0; i < count; i++)
            {
                var minX = i == 0 ? MinX : (_X / count) * (i)-1;
                var MaxX = i<count?(_X/count)*(i+1):_X;
               
                maptasks[i]= new Task(()=>BuildMapThread(MaxX, _Y, minX, MinY));
                maptasks[i].Start();
            }
            Task.WaitAll(maptasks);

            if (!AutomaticZMode)
            {
                _bitmapZ = null;
            }
            if (AutomaticZMode)
            {
                OnProgressText(new ProgressEventArgs(){PayLoad = "Making Altitude"});
                ProcessZ(AutomaticZMode, null, new Coordinates(0,0,0,0,0));
            }
            _bitmapZ = null;

            //SetItem();
            OnProgressText(new ProgressEventArgs() { PayLoad = "Writing files" });
            try
            {
                Task writeMul = new Task(WriteMUL);
                Task writestatic = new Task(WriteStatics);
                Task[] tasks = new[] { writeMul, writestatic };
                writeMul.Start();
                writestatic.Start();
                Task.WaitAll(tasks);
            }
            catch (Exception e)
            {
                
                throw e;
            }
            

            OnProgressText(new ProgressEventArgs(){PayLoad = "Done"});
        }

        #endregion

        #region MapInit

        private void BuildMapThread(int X, int Y, int minX, int minY)
        {
            for (var x = minX; x < X - 1; x++)
            {
                
                for (var y = minY; y < Y - 1; y++)
                {
                    Random=new Random(DateTime.UtcNow.Millisecond);
                    var coordinates = MakeIndexesDirections(x, y, 1, 1);
                    var areacolorcoordinates = new AreaColorCoordinates(coordinates, _bitmapAreaColor);
                    var buildMapCoordinates = new MapObjectCoordinates(coordinates, _mapObjects);
                    if (AutomaticZMode)
                        Mountain(areacolorcoordinates, buildMapCoordinates, coordinates);

                    MakeCoastUolStyle(areacolorcoordinates, buildMapCoordinates, coordinates);
                    TextureTranstion(coordinates, areacolorcoordinates, buildMapCoordinates);
                    MakeCliffs(coordinates, areacolorcoordinates, buildMapCoordinates);
                    ItemsTransations(coordinates, areacolorcoordinates, buildMapCoordinates);
                    PlaceTextures(areacolorcoordinates, buildMapCoordinates, coordinates);

                    if (!AutomaticZMode)
                        ProcessZ(AutomaticZMode, buildMapCoordinates, coordinates);
                }
            }
            float percent1 = (100 * (X - minX)) / (_X);
            _progressPerc += percent1;
            OnProgressText(new ProgressEventArgs() { PayLoad = "Making Map", Progress = (byte)Math.Round(_progressPerc) });
        }


        private void PlaceTextures(AreaColorCoordinates areaColorCoordinates, MapObjectCoordinates mapObjectCoordinates, Coordinates coordinates)
        {
            if (areaColorCoordinates.Center == null) return;

            if (mapObjectCoordinates.Center.Texture == 0)
                mapObjectCoordinates.Center.Texture = (short)RandomTexture(areaColorCoordinates.Center.TextureIndex);
        }


        /// <summary>
        /// it's used to process the map automatically
        /// </summary>
        /// <param name="mode">mode of how you want to process the map 0 for following the map, 1 to calculate automatically</param>
        /// <param name="mapObjectCoordinates"> </param>
        /// <param name="coordinates"> </param>
        private void ProcessZ(bool mode, MapObjectCoordinates mapObjectCoordinates, Coordinates coordinates)
        {
            if (!mode)
            {
                mapObjectCoordinates.Center.Altitude = _bitmapZ[coordinates.Center];
            }
            else
            {
                int x;
                for (x = MinX; x < _X - 1; x++)
                {
                    Random = new Random(DateTime.UtcNow.Millisecond);
                    byte percent1 = (byte)((100 * x) / (_X));
                    OnProgressText(new ProgressEventArgs(){PayLoad = "Making Altitude",Progress = percent1});
                    int y;
                    for (y = MinY; y < _Y - 1; y++)
                    {
                        var location = CalculateZone(x, y, _stride);
                        var area = _bitmapAreaColor[location];
                        if (_mapObjects[location].Altitude == 0)
                            _mapObjects[location].Altitude += (sbyte)Random.Next(area.Min, area.Max);
                        if (_mapObjects[location].Altitude >= 120)
                            _mapObjects[location].Altitude = (sbyte)(Random.Next(120, 125));
                        var z = _bitmapZ[location];
                        _mapObjects[location].Altitude = (sbyte)(_mapObjects[location].Altitude + z);
                        if(_mapObjects[location].Altitude==0)
                        {
                            int a=0;
                            a++;
                        }
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
        private void Mountain(AreaColorCoordinates areacoord, MapObjectCoordinates mapObjectCoordinates, Coordinates coord)
        {
            //byte chk = 1;
            //rrggbb

            //for (int x = MinX; x < _X - 1; x++)
            //    for (int y = MinY; y < _Y - 1; y++)
            //    {
                    //var coord = MakeIndexesDirections(x, y, 1, 1);

                    //var areacoord = new AreaColorCoordinates(coord, _bitmapAreaColor);

                    if (areacoord.Center == null || areacoord.Center.Type != TypeColor.Moutains) return;

                    //var mapcoord = new MapObjectCoordinates(coord, _mapObjects);

                    mapObjectCoordinates.Center.Altitude =
                        (sbyte)Random.Next(areacoord.Center.Min, areacoord.Center.Max);
                    if (!areacoord.Center.ModeAutomatic) return;

                    for (int index = 0; index < areacoord.Center.List.Count; index++)
                    {
                        var cirlce = areacoord.Center.List[index];

                        var areacircles =
                            new AreaColorCoordinates(new Coordinates(index, index, coord.X, coord.Y, _stride),
                                                     _bitmapAreaColor);

                        if (areacircles.List == null)
                        {
                            break;
                        }
                        if (areacircles.List.Any(c => c == null || c.Type != TypeColor.Moutains))
                        {
                            break;
                        }

                        mapObjectCoordinates.Center.Altitude = (sbyte)Random.Next(cirlce.From, cirlce.To);
                        if (mapObjectCoordinates.Center.Altitude > 127)
                            mapObjectCoordinates.Center.Altitude = (sbyte)(Random.Next(120, 125));
                        if (index >= (areacoord.Center.List.Count / 3) * 2 && areacoord.Center.IndexColorTopMountain != 0)
                        {
                            var area = _bitmapAreaColor[coord.Center];
                            area = CollectionAreaColor.FindByIndex(area.IndexColorTopMountain);
                            mapObjectCoordinates.Center.Texture = (short)RandomTexture(area.TextureIndex);
                            _bitmapAreaColor[coord.Center] = CollectionAreaColor.FindByColor(area.ColorTopMountain);
                        }
                    //}
                }

            //for (int x = MinX; x < _X; x++)
            //    for (int y = MinY; y < _Y; y++)
            //    {
            //        var location = CalculateZone(x, y, _stride);
            //        if (_mapObjects[location].Occupied != 30) continue;

            //        var mapobject = _mapObjects[location];
            //        var area = _bitmapAreaColor[location];
            //        area = CollectionAreaColor.FindByIndex(area.IndexColorTopMountain);
            //        mapobject.Texture = (short)RandomTexture(area.TextureIndex);

            //        _bitmapAreaColor[location] = CollectionAreaColor.FindByColor(area.ColorTopMountain);
            //    }
        }

        #endregion //mountains

        #region Texture Transations

        /// <summary>
        /// transitions from a kind of terrain to anotherkind
        /// </summary>
        /// <param name="coordinates"> </param>
        /// <param name="areaColorCoordinates"> </param>
        /// <param name="mapObjectCoordinates"> </param>
        private void TextureTranstion(Coordinates coordinates, AreaColorCoordinates areaColorCoordinates, MapObjectCoordinates mapObjectCoordinates)
        {
            var transitionList = areaColorCoordinates.Center.TextureTransitions;
            if (!transitionList.Any())
                return;
            if (areaColorCoordinates.List.All(o => o.Color == areaColorCoordinates.Center.Color))
                return;
            var colors = areaColorCoordinates.List.Select(o => o.Color);
            AreaTransitionTexture texture = null;
            foreach (var color in colors)
            {
                texture = areaColorCoordinates.Center.FindTransitionTexture(color);
                if (texture != null)
                    break;
            }
            if (texture == null)
                return;

            int special = 0;
            int z = 0;

            if (mapObjectCoordinates.Center.Occupied != 0) return;

            #region Line
            //Line
            // B
            //xAx
            //y1 = y - 1;
            if (
                areaColorCoordinates.North.Color != areaColorCoordinates.Center.Color
                && areaColorCoordinates.East.Color != areaColorCoordinates.North.Color
                && areaColorCoordinates.West.Color != areaColorCoordinates.North.Color
                )
            {
                //var smoothT = Smooth(listSmooth, x, y - 1);
                var transation = areaColorCoordinates.Center.FindTransitionTexture(areaColorCoordinates.North.Color);
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
            if (
                areaColorCoordinates.South.Color != areaColorCoordinates.Center.Color
                && areaColorCoordinates.East.Color != areaColorCoordinates.South.Color
                && areaColorCoordinates.West.Color != areaColorCoordinates.South.Color
                )
            {
                //var smoothT = Smooth(listSmooth, x, y + 1);
                //var transation = areaColorCoordinates.Center.FindTransitionTexture(_bitmap[coordinates.South]);
                var transation = areaColorCoordinates.Center.FindTransitionTexture(areaColorCoordinates.South.Color);
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
            if (
                areaColorCoordinates.East.Color != areaColorCoordinates.Center.Color
                && areaColorCoordinates.North.Color != areaColorCoordinates.East.Color
                && areaColorCoordinates.South.Color != areaColorCoordinates.East.Color
                )
            {
                //    var smoothT = Smooth(listSmooth, x + 1, y);
                //var transation = areaColorCoordinates.Center.FindTransitionTexture(_bitmap[coordinates.East]);

                var transation = areaColorCoordinates.Center.FindTransitionTexture(areaColorCoordinates.East.Color);
                if (transation != null)
                {
                    //_MapID[_directions[(int)Directions.Location]] = RandomFromList(smoothT.Line.Forth.List);
                    special = 2;
                    //z = _MapAlt[_directions[(int)Directions.East]] + _MapAlt[_directions[(int)Directions.West]];
                    //_mapObjects[_directions[(int)Directions.Center]].Texture = (short)RandomFromList(smoothT.Line.CollectionForth.List);
                    //z = _mapObjects[_directions[(int)Directions.East]].Altitude +
                    //    _mapObjects[_directions[(int)Directions.West]].Altitude;
                    mapObjectCoordinates.Center.Texture =
                        (short)RandomFromList(transation.LineWest.List);
                    z = mapObjectCoordinates.East.Altitude +
                        mapObjectCoordinates.West.Altitude;
                }
            }
            // x
            //BA
            // x
            //x1 = x - 1;
            //if (_bitmap[_directions[(int)Directions.West]] != A)
            if (
                areaColorCoordinates.West.Color != areaColorCoordinates.Center.Color
                && areaColorCoordinates.West.Color != areaColorCoordinates.North.Color
                && areaColorCoordinates.West.Color != areaColorCoordinates.South.Color
                )
            {
                //var smoothT = GetTransationTexture(listSmooth, x - 1, y);

                var transation = areaColorCoordinates.Center.FindTransitionTexture(areaColorCoordinates.West.Color);
                if (transation != null)
                {
                    //_MapID[_directions[(int)Directions.Location]] = RandomFromList(smoothT.Line.Second.List);
                    special = 1;
                    //z = _MapAlt[_directions[(int)Directions.West]] + _MapAlt[_directions[(int)Directions.East]];
                    //_mapObjects[_directions[(int)Directions.Center]].Texture = (short)RandomFromList(smoothT.Line.CollectionSecond.List);
                    //z = _mapObjects[_directions[(int)Directions.West]].Altitude +
                    //    _mapObjects[_directions[(int)Directions.East]].Altitude;
                    mapObjectCoordinates.Center.Texture =
                        (short)RandomFromList(transation.LineEast.List);
                    z = mapObjectCoordinates.West.Altitude +
                        mapObjectCoordinates.East.Altitude;
                }
            }

            #endregion //Line

            #region Border
            //Border
            //xB
            //Ax
            if (
                areaColorCoordinates.NorthEast.Color != areaColorCoordinates.Center.Color
                && areaColorCoordinates.East.Color != areaColorCoordinates.NorthEast.Color
                && areaColorCoordinates.North.Color != areaColorCoordinates.NorthEast.Color
                )
            {

                //var transition = areaColorCoordinates.Center.FindTransitionTexture(_bitmap[coordinates.NorthEast]);

                var transition = areaColorCoordinates.Center.FindTransitionTexture(areaColorCoordinates.NorthEast.Color);
                special = 2;
                if (transition != null)
                {
                    mapObjectCoordinates.Center.Texture =
                        (short)RandomFromList(transition.BorderSouthWest.List);
                    z = mapObjectCoordinates.NorthEast.Altitude +
                        mapObjectCoordinates.SouthWest.Altitude;
                }
            }

            //Bx
            //xA
            if (
                areaColorCoordinates.NorthWest.Color != areaColorCoordinates.Center.Color
                && areaColorCoordinates.West.Color != areaColorCoordinates.NorthWest.Color
                && areaColorCoordinates.North.Color != areaColorCoordinates.NorthWest.Color
                )
            {
                var transition = areaColorCoordinates.Center.FindTransitionTexture(areaColorCoordinates.NorthWest.Color);

                if (transition != null)
                {
                    special = 1;
                    mapObjectCoordinates.Center.Texture =
                        (short)RandomFromList(transition.BorderSouthEast.List);

                    z = mapObjectCoordinates.NorthWest.Altitude +
                        mapObjectCoordinates.SouthEast.Altitude;
                }
            }

            //GA
            //BG
            if (
                areaColorCoordinates.SouthWest.Color != areaColorCoordinates.Center.Color
                && areaColorCoordinates.West.Color != areaColorCoordinates.SouthWest.Color
                && areaColorCoordinates.South.Color != areaColorCoordinates.SouthWest.Color
                )
            {
                var transation = areaColorCoordinates.Center.FindTransitionTexture(areaColorCoordinates.SouthWest.Color);
                if (transation != null)
                {
                    special = 2;

                    mapObjectCoordinates.Center.Texture = (short)RandomFromList(transation.BorderNorthEast.List);
                    z = mapObjectCoordinates.NorthWest.Altitude +
                        mapObjectCoordinates.NorthEast.Altitude;
                }

            }
            //Ax
            //xB
            if (
                areaColorCoordinates.SouthEast.Color != areaColorCoordinates.Center.Color
                && areaColorCoordinates.East.Color != areaColorCoordinates.SouthEast.Color
                && areaColorCoordinates.South.Color != areaColorCoordinates.SouthEast.Color
                )
            {

                //var transation = areaColorCoordinates.Center.FindTransitionTexture(_bitmap[coordinates.SouthEast]);

                var transation = areaColorCoordinates.Center.FindTransitionTexture(areaColorCoordinates.SouthEast.Color);

                if (transation != null)
                {
                    special = 2;
                    mapObjectCoordinates.Center.Texture = (short)RandomFromList(transation.BorderNorthWest.List);
                    z = _mapObjects[coordinates.SouthEast].Altitude +
                        _mapObjects[coordinates.NorthWest].Altitude;
                }
            }

            #endregion // Border

            #region Edge
            //Edge
            //B
            //AB

            if (
                areaColorCoordinates.NorthEast.Color != areaColorCoordinates.Center.Color
                && areaColorCoordinates.North.Color == areaColorCoordinates.NorthEast.Color
                && areaColorCoordinates.NorthEast.Color == areaColorCoordinates.East.Color
                )
            {
                var transition =
                    areaColorCoordinates.Center.FindTransitionTexture(areaColorCoordinates.NorthEast.Color);
                if (transition != null)
                {
                    special = 2;
                    mapObjectCoordinates.Center.Texture =
                        (short)RandomFromList(transition.EdgeSouthWest.List);
                    z = mapObjectCoordinates.NorthEast.Altitude +
                        mapObjectCoordinates.SouthWest.Altitude;
                }
            }

            // B
            //BA
            if (
                areaColorCoordinates.NorthWest.Color != areaColorCoordinates.Center.Color
                && areaColorCoordinates.North.Color == areaColorCoordinates.NorthWest.Color
                && areaColorCoordinates.NorthWest.Color == areaColorCoordinates.West.Color
                )
            {
                var transation =
                    areaColorCoordinates.Center.FindTransitionTexture(areaColorCoordinates.West.Color);
                if (transation != null)
                {
                    special = 1;
                    mapObjectCoordinates.Center.Texture =
                        (short)RandomFromList(transation.EdgeSouthEast.List);
                    z = mapObjectCoordinates.NorthEast.Altitude +
                        mapObjectCoordinates.SouthWest.Altitude;
                }
            }

            //BA
            // B
            if (
                areaColorCoordinates.SouthWest.Color != areaColorCoordinates.Center.Color
                && areaColorCoordinates.South.Color == areaColorCoordinates.SouthWest.Color
                && areaColorCoordinates.SouthWest.Color == areaColorCoordinates.West.Color
                )
            {
                var transation =
                    areaColorCoordinates.Center.FindTransitionTexture(areaColorCoordinates.SouthWest.Color);
                if (transation != null)
                {
                    special = 2;

                    mapObjectCoordinates.Center.Texture = (short)RandomFromList(transation.EdgeNorthEast.List);
                    z = mapObjectCoordinates.SouthWest.Altitude +
                        mapObjectCoordinates.NorthEast.Altitude;
                }
            }

            //AB
            //B
            if (
                areaColorCoordinates.SouthEast.Color != areaColorCoordinates.Center.Color
                && areaColorCoordinates.South.Color == areaColorCoordinates.SouthEast.Color
                && areaColorCoordinates.East.Color == areaColorCoordinates.SouthEast.Color
                )
            {
                var transation =
                    areaColorCoordinates.Center.FindTransitionTexture(areaColorCoordinates.SouthEast.Color);
                if (transation != null)
                {
                    special = 2;

                    mapObjectCoordinates.Center.Texture = (short)RandomFromList(transation.EdgeNorthWest.List);
                    z = mapObjectCoordinates.SouthEast.Altitude +
                        mapObjectCoordinates.NorthWest.Altitude;
                }

            }
            if (special > 0)
                mapObjectCoordinates.Center.Occupied = 1;

            if (_bitmapZ[coordinates.Center] == 128)
            {
                _mapObjects[coordinates.Center].Altitude = (sbyte)(z / 2);
            }
            #endregion Edge

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
            if (mapObjectCoordinates.Center.Items != null || mapObjectCoordinates.Center.Occupied != 0) return;
            //int special = 0;
            var transitionList = areaColorCoordinates.Center.TransitionItems;
            if (!transitionList.Any())
                return;
            if (areaColorCoordinates.List.All(o => o.Color == areaColorCoordinates.Center.Color))
                return;

            var colors = areaColorCoordinates.List.Select(o => o.Color);
            AreaTransitionItem k = null;

            foreach (var color in colors)
            {
                k = areaColorCoordinates.Center.FindTransationItemByColor(color);
                if (k != null) break;
            }
            if (k == null) return;

            int zlev = 0;
            var item = new ItemClone();


            #region Border
            //Border
            //GB
            //xG
            if (areaColorCoordinates.NorthEast.Color != areaColorCoordinates.Center.Color)
            {
                var transation = areaColorCoordinates.Center.FindTransationItemByColor(areaColorCoordinates.NorthEast.Color);
                //7
                if (transation != null)
                    item = new ItemClone() { Id = RandomFromList(transation.BorderSouthWest.List) };
            }
            //BG
            //Gx
            if (areaColorCoordinates.NorthWest.Color != areaColorCoordinates.Center.Color)
            {
                var transation = areaColorCoordinates.Center.FindTransationItemByColor(areaColorCoordinates.NorthWest.Color);
                //6
                if (transation != null)
                    item = new ItemClone { Id = RandomFromList(transation.BorderSouthEast.List) };
            }
            //Gx
            //BG
            if (areaColorCoordinates.SouthWest.Color != areaColorCoordinates.Center.Color)
            {
                var transation = areaColorCoordinates.Center.FindTransationItemByColor(areaColorCoordinates.SouthWest.Color);
                //5
                if (transation != null)
                    item = new ItemClone { Id = RandomFromList(transation.BorderNorthEast.List) };
            }
            //xG
            //GB
            if (areaColorCoordinates.SouthEast.Color != areaColorCoordinates.Center.Color)
            {
                var transation = areaColorCoordinates.Center.FindTransationItemByColor(areaColorCoordinates.SouthEast.Color);
                //4
                if (transation != null)
                    item = new ItemClone { Id = RandomFromList(transation.BorderNorthWest.List) };
            }
            #endregion //Border


            #region Line
            //Line
            // B
            //GxG
            if (areaColorCoordinates.North.Color != areaColorCoordinates.Center.Color)
            {
                var transation = areaColorCoordinates.Center.FindTransationItemByColor(areaColorCoordinates.North.Color);
                //2
                if (transation != null) item = new ItemClone { Id = RandomFromList(transation.LineSouth.List) };
            }
            //GxG
            // B
            if (areaColorCoordinates.South.Color != areaColorCoordinates.Center.Color)
            {
                var transation = areaColorCoordinates.Center.FindTransationItemByColor(areaColorCoordinates.South.Color);
                //0
                if (transation != null) item = new ItemClone { Id = RandomFromList(transation.LineNorth.List) };
            }
            //G
            //xB
            //G
            if (areaColorCoordinates.East.Color != areaColorCoordinates.Center.Color)
            {
                var transation = areaColorCoordinates.Center.FindTransationItemByColor(areaColorCoordinates.East.Color);
                //3
                if (transation != null) item = new ItemClone { Id = RandomFromList(transation.LineWest.List) };
            }
            // G
            //Bx
            // G
            if (areaColorCoordinates.West.Color != areaColorCoordinates.Center.Color)
            {
                var transation = areaColorCoordinates.Center.FindTransationItemByColor(areaColorCoordinates.West.Color);
                //1
                if (transation != null) item = new ItemClone { Id = RandomFromList(transation.LineEast.List) };
            }
            #endregion //Line


            #region Edge
            //Edge
            //B
            //xB
            if (
                areaColorCoordinates.East.Color != areaColorCoordinates.Center.Color
                && areaColorCoordinates.North.Color != areaColorCoordinates.Center.Color
                )
            {
                var transation = areaColorCoordinates.Center.FindTransationItemByColor(areaColorCoordinates.NorthEast.Color);
                //11
                if (transation != null) item = new ItemClone { Id = RandomFromList(transation.EdgeSouthWest.List) };
            }
            // B
            //Bx
            if (
                areaColorCoordinates.West.Color != areaColorCoordinates.Center.Color &&
                areaColorCoordinates.North.Color != areaColorCoordinates.Center.Color
                )
            {
                var transation = areaColorCoordinates.Center.FindTransationItemByColor(areaColorCoordinates.NorthWest.Color);
                //10
                if (transation != null) item = new ItemClone { Id = RandomFromList(transation.EdgeSouthEast.List) };
            }
            //Bx
            // B
            if (
                areaColorCoordinates.West.Color != areaColorCoordinates.Center.Color
                && areaColorCoordinates.South.Color != areaColorCoordinates.Center.Color
                )
            {
                var transation = areaColorCoordinates.Center.FindTransationItemByColor(areaColorCoordinates.SouthWest.Color);
                //9
                if (transation != null) item = new ItemClone { Id = RandomFromList(transation.EdgeNorthEast.List) };
            }
            //xB
            //B
            if (
                areaColorCoordinates.East.Color != areaColorCoordinates.Center.Color
                && areaColorCoordinates.South.Color != areaColorCoordinates.Center.Color
                )
            {
                var transation = areaColorCoordinates.Center.FindTransationItemByColor(areaColorCoordinates.SouthEast.Color);
                //8
                if (transation != null) item = new ItemClone { Id = RandomFromList(transation.EdgeNorthWest.List) };
            }

            #endregion //Edge

            if (item.Id == 0) return;

            var coast = areaColorCoordinates.Center;

            if (coast.Type == TypeColor.Water)
            {
                zlev = Random.Next(coast.Min, coast.Max);
                item.Z = (sbyte)(mapObjectCoordinates.Center.Altitude + zlev);
            }


            if (mapObjectCoordinates.Center.Items == null)
                mapObjectCoordinates.Center.Items = new List<ItemClone>();
            mapObjectCoordinates.Center.Items.Add(item);
        }

        #endregion //Items Transations

        #endregion //Transations

        #region Coasts Old Style

        /// <summary>
        /// Make Coast method, it search if the given parameter is a coast or not
        /// </summary>
        private void MakeCoast(AreaColorCoordinates areaColorCoordinates, MapObjectCoordinates mapObjectCoordinates, Coordinates coordinates)
        {
            //Color Water = _bitmap[_directions[(int)Directions.Center]];
            //int ID = 0;
            //if (!ItemsCoasts.FindCoastByColor(Water))
            //    return;
            int id;
            if (areaColorCoordinates.Center.Type != TypeColor.WaterCoast)
                return;

            if (areaColorCoordinates.List.All(o => o.Type == TypeColor.WaterCoast || o.Type == TypeColor.Water)) //&& areaColorCoordinates.List.All(area => area.Color == areaColorCoordinates.Center.Color))
            {
                if (AutomaticZMode)
                    PlaceCoastItem(areaColorCoordinates.Center.Coasts.Coast.Texture, mapObjectCoordinates, areaColorCoordinates, -5);
                //test
                else
                {
                    PlaceCoastItem(areaColorCoordinates.Center.Coasts.Coast.Texture, mapObjectCoordinates, areaColorCoordinates, -5, true, coordinates);
                }
                return;
            }


            #region Line
            //Line
            //  L 
            // WxW
            if (
                areaColorCoordinates.North.Type != TypeColor.WaterCoast
                && areaColorCoordinates.West.Type == TypeColor.WaterCoast
                && areaColorCoordinates.East.Type == TypeColor.WaterCoast
                &&
                (areaColorCoordinates.NorthEast.Type != TypeColor.WaterCoast
                || areaColorCoordinates.NorthWest.Type != TypeColor.WaterCoast)
                )
            {
                id = RandomFromList(areaColorCoordinates.North.Coasts.Coast.LineSouth.List);
                var texture = (short)RandomFromList(areaColorCoordinates.North.Coasts.Ground.LineSouth.List);
                mapObjectCoordinates.Center.Occupied = 1;


                mapObjectCoordinates.Center.Texture = texture;
                if (AutomaticZMode || (!AutomaticZMode && _bitmapZ[coordinates.North] != _bitmapZ[coordinates.Center]))
                {
                    mapObjectCoordinates.South.Altitude = -15;
                    mapObjectCoordinates.Center.Altitude = (sbyte)Random.Next(-4, 2);
                    PlaceCoastItem(id, mapObjectCoordinates, areaColorCoordinates, 0);
                }
                else
                {
                    mapObjectCoordinates.Center.Altitude = (sbyte)Random.Next(-4, 2);
                    PlaceCoastItem(id, mapObjectCoordinates, areaColorCoordinates, 0, true, coordinates);
                }

            }

            if (areaColorCoordinates.South.Type != TypeColor.WaterCoast //&& mapObjectCoordinates.South.Occupied == 0
                && areaColorCoordinates.West.Type == TypeColor.WaterCoast
                && areaColorCoordinates.East.Type == TypeColor.WaterCoast
                &&
                (areaColorCoordinates.SouthEast.Type != TypeColor.WaterCoast
                || areaColorCoordinates.SouthWest.Type != TypeColor.WaterCoast)
                )
            {

                //mapObjectCoordinates.North.Occupied = 1;
                //mapObjectCoordinates.Center.Altitude = -15;

                var texture =
                   (short)RandomFromList(areaColorCoordinates.South.Coasts.Ground.LineNorth.List);
                mapObjectCoordinates.Center.Texture = texture;
                //id = RandomFromList(areaColorCoordinates.South.Coasts.Coast.LineNorth.List);
                //PlaceCoastItem(id, mapObjectCoordinates, areaColorCoordinates, -5, true);
                mapObjectCoordinates.North.Occupied = 1;
                mapObjectCoordinates.Center.Occupied = 1;

                if (AutomaticZMode || (!AutomaticZMode && _bitmapZ[coordinates.South] != _bitmapZ[coordinates.Center]))
                {
                    mapObjectCoordinates.Center.Altitude = -15;
                    mapObjectCoordinates.North.Altitude = -15;
                    mapObjectCoordinates.North.Occupied = 1;
                    mapObjectCoordinates.South.Altitude = (sbyte)Random.Next(-4, -2);
                    id = RandomFromList(areaColorCoordinates.South.Coasts.Coast.LineNorth.List);
                    return;

                }

                id = RandomFromList(areaColorCoordinates.South.Coasts.Coast.LineNorth.List);
                PlaceCoastItem(id, mapObjectCoordinates, areaColorCoordinates, 0, true, coordinates);
            }
            // W
            // xL 
            // W
            if (
                areaColorCoordinates.East.Type != TypeColor.WaterCoast //&& mapObjectCoordinates.East.Occupied == 0
                && areaColorCoordinates.North.Type == TypeColor.WaterCoast
                && areaColorCoordinates.South.Type == TypeColor.WaterCoast
                &&
                (areaColorCoordinates.NorthEast.Type != TypeColor.WaterCoast
                || areaColorCoordinates.SouthEast.Type != TypeColor.WaterCoast)
                )
            {
                id = RandomFromList(areaColorCoordinates.East.Coasts.Coast.LineWest.List);
                var texture = (short)RandomFromList(areaColorCoordinates.East.Coasts.Ground.LineWest.List);
                mapObjectCoordinates.Center.Texture = texture;
                mapObjectCoordinates.Center.Occupied = 1;

                if (AutomaticZMode || (!AutomaticZMode && _bitmapZ[coordinates.East] != _bitmapZ[coordinates.Center]))
                {
                    mapObjectCoordinates.Center.Altitude = -15;
                    mapObjectCoordinates.West.Altitude = -15;
                    mapObjectCoordinates.West.Occupied = 1;
                    //PlaceCoastItem(areaColorCoordinates.Center.Coasts.Coast.Texture, mapObjectCoordinates, areaColorCoordinates, -5,true,coordinates);
                }
                else
                {
                    PlaceCoastItem(id, mapObjectCoordinates, areaColorCoordinates, 0, true, coordinates);
                }
            }
            //  W
            // Lx
            //  W
            if (areaColorCoordinates.West.Type != TypeColor.WaterCoast// && mapObjectCoordinates.West.Occupied == 0
                && areaColorCoordinates.North.Type == TypeColor.WaterCoast
                && areaColorCoordinates.South.Type == TypeColor.WaterCoast
                &&
                (areaColorCoordinates.SouthWest.Type != TypeColor.WaterCoast
                || areaColorCoordinates.NorthWest.Type != TypeColor.WaterCoast)
                )
            {
                id = RandomFromList(areaColorCoordinates.West.Coasts.Coast.LineEast.List);
                var texture = (short)RandomFromList(areaColorCoordinates.West.Coasts.Ground.LineEast.List);
                mapObjectCoordinates.Center.Texture = texture;
                if (AutomaticZMode || (!AutomaticZMode && _bitmapZ[coordinates.West] != _bitmapZ[coordinates.Center]))
                {

                    mapObjectCoordinates.Center.Altitude = (sbyte)Random.Next(-4, 2);
                    mapObjectCoordinates.Center.Occupied = 1;
                    //test
                    if (mapObjectCoordinates.East.Altitude == 0)
                        mapObjectCoordinates.East.Altitude = -15;
                    mapObjectCoordinates.East.Occupied = 1;
                    PlaceCoastItem(id, mapObjectCoordinates, areaColorCoordinates);
                }
                else
                {
                    PlaceCoastItem(id, mapObjectCoordinates, areaColorCoordinates, 0, true, coordinates);
                }
                //id = RandomFromList(areaColorCoordinates.West.Coasts.Coast.LineEast.List);
                //var texture = (short)RandomFromList(areaColorCoordinates.West.Coasts.Ground.LineEast.List);
                //mapObjectCoordinates.Center.Texture = texture;
                //mapObjectCoordinates.Center.Altitude = (sbyte)Random.Next(-4, 2);
                //mapObjectCoordinates.Center.Occupied = 1;+
                ////test
                //if(mapObjectCoordinates.East.Altitude==0)
                //mapObjectCoordinates.East.Altitude = -15;
                //PlaceCoastItem(id, mapObjectCoordinates, areaColorCoordinates);

            }
            #endregion // Line

            #region Border
            //Border
            // WL
            // xW
            if (
                areaColorCoordinates.NorthEast.Type != TypeColor.WaterCoast && areaColorCoordinates.NorthEast.Type != TypeColor.Water
                && areaColorCoordinates.East.Type == TypeColor.WaterCoast
                && areaColorCoordinates.North.Type == TypeColor.WaterCoast
                )
            {

                id = RandomFromList(areaColorCoordinates.NorthEast.Coasts.Coast.BorderSouthWest.List);
                var texture = (short)RandomFromList(areaColorCoordinates.NorthEast.Coasts.Ground.BorderNorthEast.List);
                mapObjectCoordinates.Center.Texture = texture;
                mapObjectCoordinates.Center.Occupied = 1;

                if (AutomaticZMode || (!AutomaticZMode && _bitmapZ[coordinates.NorthEast] != _bitmapZ[coordinates.Center]))
                {
                    mapObjectCoordinates.Center.Altitude = -15;
                    PlaceCoastItem(id, mapObjectCoordinates, areaColorCoordinates, 0);
                }
                else
                {
                    PlaceCoastItem(id, mapObjectCoordinates, areaColorCoordinates, 0, true, coordinates);
                }

            }
            // LW
            // Wx
            if (
                areaColorCoordinates.NorthWest.Type != TypeColor.WaterCoast && areaColorCoordinates.NorthWest.Type != TypeColor.Water
                && areaColorCoordinates.West.Type == TypeColor.WaterCoast
                && areaColorCoordinates.North.Type == TypeColor.WaterCoast
                )
            {

                id = RandomFromList(areaColorCoordinates.NorthWest.Coasts.Coast.BorderSouthEast.List);
                var texture = (short)RandomFromList(areaColorCoordinates.NorthWest.Coasts.Ground.BorderNorthWest.List);

                mapObjectCoordinates.Center.Occupied = 1;
                mapObjectCoordinates.Center.Texture = texture;
                if (AutomaticZMode || (!AutomaticZMode && _bitmapZ[coordinates.NorthWest] != _bitmapZ[coordinates.Center]))
                {
                    mapObjectCoordinates.Center.Altitude = (sbyte)Random.Next(-2, 2);
                    PlaceCoastItem(id, mapObjectCoordinates, areaColorCoordinates, 0);
                    mapObjectCoordinates.SouthEast.Altitude = -15;
                }
                else
                {
                    PlaceCoastItem(id, mapObjectCoordinates, areaColorCoordinates, 0, true, coordinates);
                }

            }
            // Wx
            // LW
            if (
                areaColorCoordinates.SouthWest.Type != TypeColor.WaterCoast
                && areaColorCoordinates.West.Type == TypeColor.WaterCoast
                && areaColorCoordinates.South.Type == TypeColor.WaterCoast
                )
            {
                id = RandomFromList(areaColorCoordinates.SouthWest.Coasts.Coast.BorderNorthEast.List);
                var texture = (short)RandomFromList(areaColorCoordinates.SouthWest.Coasts.Ground.BorderSouthWest.List);
                mapObjectCoordinates.Center.Texture = texture;
                mapObjectCoordinates.SouthWest.Occupied = 1;


                if (AutomaticZMode || (!AutomaticZMode && _bitmapZ[coordinates.SouthWest] != _bitmapZ[coordinates.Center]))
                {
                    mapObjectCoordinates.Center.Altitude = -15;
                    PlaceCoastItem(id, mapObjectCoordinates, areaColorCoordinates, 0);
                }
                else
                {
                    PlaceCoastItem(id, mapObjectCoordinates, areaColorCoordinates, 0, true, coordinates);
                }

            }
            // xW
            // WL
            if (
                areaColorCoordinates.SouthEast.Type != TypeColor.WaterCoast
                && areaColorCoordinates.East.Type == TypeColor.WaterCoast
                && areaColorCoordinates.South.Type == TypeColor.WaterCoast
                )
            {
                mapObjectCoordinates.Center.Occupied = 1;
                var texture = (short)RandomFromList(areaColorCoordinates.SouthEast.Coasts.Ground.BorderSouthEast.List);
                mapObjectCoordinates.Center.Texture = texture;


                if (AutomaticZMode || (!AutomaticZMode && _bitmapZ[coordinates.SouthEast] >= _bitmapZ[coordinates.Center] + 15))
                {
                    mapObjectCoordinates.NorthWest.Altitude = -15;
                    mapObjectCoordinates.Center.Altitude = -15;
                    //PlaceCoastItem(areaColorCoordinates.Center.Coasts.Coast.Texture, mapObjectCoordinates, areaColorCoordinates, -5,true,coordinates);
                }
                else
                {
                    id = RandomFromList(areaColorCoordinates.SouthEast.Coasts.Coast.BorderNorthWest.List);
                    PlaceCoastItem(id, mapObjectCoordinates, areaColorCoordinates, 0, true, coordinates);
                }

            }

            #endregion //Border

            #region  Edge

            //Edge
            // LL
            // xL
            if (
                areaColorCoordinates.NorthEast.Type != TypeColor.WaterCoast// && mapObjectCoordinates.NorthEast.Occupied == 0
                && areaColorCoordinates.East.Type != TypeColor.WaterCoast
                && areaColorCoordinates.North.Type != TypeColor.WaterCoast
                )
            {
                var texture = (short)RandomFromList(areaColorCoordinates.NorthEast.Coasts.Ground.EdgeSouthWest.List);
                mapObjectCoordinates.Center.Texture = texture;
                mapObjectCoordinates.Center.Altitude = (sbyte)Random.Next(-3, 0);
                mapObjectCoordinates.Center.Occupied = 1;
                if (AutomaticZMode || (!AutomaticZMode && _bitmapZ[coordinates.NorthEast] != _bitmapZ[coordinates.Center]))
                {
                    //PlaceCoastItem(areaColorCoordinates.Center.Coasts.Coast.Texture, mapObjectCoordinates, areaColorCoordinates, 0,true,coordinates);
                }
                else
                {
                    id = RandomFromList(areaColorCoordinates.NorthEast.Coasts.Coast.EdgeSouthWest.List);
                    PlaceCoastItem(id, mapObjectCoordinates, areaColorCoordinates, -5, true, coordinates);
                }
            }
            // LL
            // Lx
            if (
                areaColorCoordinates.NorthWest.Type != TypeColor.WaterCoast// && mapObjectCoordinates.NorthWest.Occupied == 0
                && areaColorCoordinates.West.Type != TypeColor.WaterCoast
                && areaColorCoordinates.North.Type != TypeColor.WaterCoast
                )
            {
                //unico corretto
                id = RandomFromList(areaColorCoordinates.NorthWest.Coasts.Coast.EdgeSouthEast.List);
                var texture = (short)RandomFromList(areaColorCoordinates.NorthWest.Coasts.Ground.EdgeSouthEast.List);
                mapObjectCoordinates.Center.Texture = texture;
                mapObjectCoordinates.Center.Altitude = (sbyte)Random.Next(-3, 0); ;
                mapObjectCoordinates.Center.Occupied = 1;
                if (AutomaticZMode || (!AutomaticZMode && _bitmapZ[coordinates.NorthWest] != _bitmapZ[coordinates.Center]))
                {
                    PlaceCoastItem(id, mapObjectCoordinates, areaColorCoordinates, 0);

                }
                else
                {
                    PlaceCoastItem(id, mapObjectCoordinates, areaColorCoordinates, 0, true, coordinates);
                }

            }
            // Lx
            // LL
            if (
                areaColorCoordinates.SouthWest.Type != TypeColor.WaterCoast// && mapObjectCoordinates.SouthWest.Occupied == 0
                && areaColorCoordinates.West.Type != TypeColor.WaterCoast
                && areaColorCoordinates.South.Type != TypeColor.WaterCoast
                )
            {
                var texture = (short)RandomFromList(areaColorCoordinates.SouthWest.Coasts.Ground.EdgeNorthEast.List);
                mapObjectCoordinates.Center.Texture = texture;
                mapObjectCoordinates.Center.Altitude = (sbyte)Random.Next(-3, 0); ;
                mapObjectCoordinates.Center.Occupied = 1;
                if (AutomaticZMode || (!AutomaticZMode && _bitmapZ[coordinates.SouthWest] != _bitmapZ[coordinates.Center]))
                {
                    //PlaceCoastItem(areaColorCoordinates.Center.Coasts.Coast.Texture, mapObjectCoordinates, areaColorCoordinates, -5,true,coordinates);
                }
                else
                {
                    id = RandomFromList(areaColorCoordinates.SouthWest.Coasts.Coast.EdgeNorthEast.List);
                    PlaceCoastItem(id, mapObjectCoordinates, areaColorCoordinates, 0, true, coordinates);
                }
            }
            // xL
            // LL
            if (
                areaColorCoordinates.SouthEast.Type != TypeColor.WaterCoast // && mapObjectCoordinates.SouthEast.Occupied == 0
                && areaColorCoordinates.East.Type != TypeColor.WaterCoast
                && areaColorCoordinates.South.Type != TypeColor.WaterCoast
                )
            {
                var texture = (short)RandomFromList(areaColorCoordinates.SouthEast.Coasts.Ground.EdgeNorthWest.List);
                mapObjectCoordinates.Center.Texture = texture;
                mapObjectCoordinates.Center.Altitude = -9;
                mapObjectCoordinates.Center.Occupied = 1;
                if (AutomaticZMode || (!AutomaticZMode && _bitmapZ[coordinates.SouthEast] != _bitmapZ[coordinates.Center]))
                {
                    //PlaceCoastItem(areaColorCoordinates.Center.Coasts.Coast.Texture, mapObjectCoordinates, areaColorCoordinates, -5, true,coordinates);
                }
                else
                {
                    id = RandomFromList(areaColorCoordinates.SouthEast.Coasts.Coast.EdgeNorthWest.List);
                    PlaceCoastItem(id, mapObjectCoordinates, areaColorCoordinates, 0, true, coordinates);
                }
            }

            #endregion //Edge
        }

        private void PlaceCoastItem(
            int id,
            MapObjectCoordinates mapObjectCoordinates,
            AreaColorCoordinates areaColorCoordinates,
            int height = -5,
            bool test = false,
            Coordinates coordinates = default(Coordinates)
            )
        {
            if (id == 0)
            {
                int a = 0;
                a++;
                return;
            }

            var item = new ItemClone() { Id = id };

            if (!test)
            {
                if (mapObjectCoordinates.SouthEast.Items == null)
                {
                    mapObjectCoordinates.SouthEast.Items = new List<ItemClone> { item };
                }

                if (height == 0)
                    item.Z = (sbyte)(mapObjectCoordinates.SouthEast.Altitude +
                                        Random.Next(areaColorCoordinates.Center.Min, areaColorCoordinates.Center.Max));
                else
                {
                    item.Z = (sbyte)(mapObjectCoordinates.SouthEast.Altitude +
                                        height);
                }
            }
            else
            {
                if (mapObjectCoordinates.Center.Items == null)
                {
                    mapObjectCoordinates.Center.Items = new List<ItemClone> { item };
                }
                else
                {
                    mapObjectCoordinates.Center.Items.Add(item);
                }
                item.Z = _bitmapZ[coordinates.Center];

            }



        }

        #endregion

        #region Coasts UOL style


        private static bool PlaceObject(MapObjectCoordinates mapObjectCoordinates, sbyte altitude, int itemid, sbyte zItem, int texture)
        {
            mapObjectCoordinates.Center.Altitude = altitude;

            if (texture == 0)
            {
                int a = 0;
                a++;
            }
            if(mapObjectCoordinates.Center.Items==null)
            mapObjectCoordinates.Center.Items = new List<ItemClone>();

            mapObjectCoordinates.Center.Items.Add(new ItemClone { Id = itemid, Z = zItem });
            mapObjectCoordinates.Center.Occupied = 1;
            if (texture >= 0)
                mapObjectCoordinates.Center.Texture = (short)texture;
            return true;
        }

        #region Lines

        #region NS

        private static bool PlaceObjectNS(
            AreaColorCoordinates areaColorCoordinates,
            MapObjectCoordinates mapObjectCoordinates,
            Coordinates coordinates,
            TypeColor trueType,
            sbyte zItem,
            sbyte altitude,
            int itemid,
            int texture = -1
            )
        {
            if (areaColorCoordinates.North.Type != trueType
                || areaColorCoordinates.NorthWest.Type != trueType
                || areaColorCoordinates.NorthEast.Type != trueType
                || areaColorCoordinates.East.Type != trueType
                || areaColorCoordinates.West.Type != trueType)
                return false;

            if (areaColorCoordinates.South.Type != trueType || areaColorCoordinates.SouthEast.Type != trueType || areaColorCoordinates.SouthWest.Type != trueType)
            {

                if (areaColorCoordinates.SouthEast.Type == trueType && areaColorCoordinates.SouthEast.Type == trueType && areaColorCoordinates.South.Type != trueType)
                {
                    return PlaceObject(mapObjectCoordinates, altitude, itemid, zItem, texture);
                }
                return PlaceObject(mapObjectCoordinates, altitude, itemid, zItem, texture);
            }
            return false;
        }

        #endregion //NS

        #region SN

        private static bool PlaceObjectSN(
           AreaColorCoordinates areaColorCoordinates,
           MapObjectCoordinates mapObjectCoordinates,
           Coordinates coordinates,
           TypeColor trueType,
           sbyte zItem,
           sbyte altitude,
           int itemid,
            int texture = -1
           )
        {
            if (areaColorCoordinates.South.Type != trueType
                || areaColorCoordinates.SouthWest.Type != trueType
                || areaColorCoordinates.SouthEast.Type != trueType
                || areaColorCoordinates.East.Type != trueType
                || areaColorCoordinates.West.Type != trueType)
                return false;

            if (
                areaColorCoordinates.North.Type != trueType
                || areaColorCoordinates.NorthEast.Type != trueType
                || areaColorCoordinates.NorthWest.Type != trueType
                )
            {
                if (
                    areaColorCoordinates.North.Type != trueType
                    && areaColorCoordinates.NorthEast.Type != trueType
                    && trueType != TypeColor.WaterCoast
                    && areaColorCoordinates.NorthWest.Type == trueType
                    )
                {
                    return PlaceObject(mapObjectCoordinates, altitude, RandomFromList(areaColorCoordinates.Center.Coasts.Coast.BorderNorthEast.List), zItem, texture);
                }

                return PlaceObject(mapObjectCoordinates, altitude, itemid, zItem, texture);
            }
            return false;
        }

        #endregion //SN

        #region EW

        private static bool PlaceObjectEW(AreaColorCoordinates areaColorCoordinates,
            MapObjectCoordinates mapObjectCoordinates,
            Coordinates coordinates,
            TypeColor type,
            sbyte zItem,
            sbyte altitude,
            int itemid,
            int texture = -1
            )
        {
            if (
                areaColorCoordinates.NorthEast.Type != type
                || areaColorCoordinates.North.Type != type
                || areaColorCoordinates.East.Type != type
                || areaColorCoordinates.SouthEast.Type != type
                || areaColorCoordinates.South.Type != type
                )
                return false;

            if (areaColorCoordinates.West.Type != type || areaColorCoordinates.SouthWest.Type != type || areaColorCoordinates.NorthWest.Type != type)
            {
                if
                (
                areaColorCoordinates.SouthWest.Type == type
                && areaColorCoordinates.West.Type == type
                && areaColorCoordinates.NorthWest.Type != type
                && type != TypeColor.WaterCoast
                )
                {
                    return PlaceObject(mapObjectCoordinates, altitude,
                                       RandomFromList(areaColorCoordinates.Center.Coasts.Coast.BorderSouthWest.List),
                                       zItem, texture);
                }

                return PlaceObject(mapObjectCoordinates, altitude, itemid, zItem, texture);

            }

            return false;
        }

        #endregion //EW

        #region WE

        private static bool PlaceObjectWE(AreaColorCoordinates areaColorCoordinates,
            MapObjectCoordinates mapObjectCoordinates,
            Coordinates coordinates,
            TypeColor type,
            sbyte zItem,
            sbyte altitude,
            int itemid,
            int texture = -1
            )
        {
            if (areaColorCoordinates.West.Type != type
                || areaColorCoordinates.NorthWest.Type != type
                || areaColorCoordinates.SouthWest.Type != type
                || areaColorCoordinates.North.Type != type
                || areaColorCoordinates.South.Type != type
                )
                return false;

            if (areaColorCoordinates.East.Type != type
                || areaColorCoordinates.NorthEast.Type != type
                || areaColorCoordinates.SouthEast.Type != type)
            {
                return PlaceObject(mapObjectCoordinates, altitude, itemid, zItem, texture);
            }

            return false;
        }

        #endregion //WE

        #endregion //Lines

        #region Edges

        #region SouthWestEdge

        private static bool PlaceObjectSouthWestEdge(
            AreaColorCoordinates areaColorCoordinates,
            MapObjectCoordinates mapObjectCoordinates,
            Coordinates coordinates,
            TypeColor type,
            sbyte zItem,
            sbyte altitude,
            int itemid,
            int texture = -1
            )
        {
            if (areaColorCoordinates.North.Type != type || areaColorCoordinates.NorthEast.Type != type || areaColorCoordinates.East.Type != type)
                return false;

            if (areaColorCoordinates.South.Type != type || areaColorCoordinates.SouthEast.Type != type || areaColorCoordinates.SouthWest.Type != type)
                if (areaColorCoordinates.West.Type != type || areaColorCoordinates.NorthWest.Type != type)
                {
                    if (areaColorCoordinates.SouthWest.Type != type
                        && areaColorCoordinates.West.Type != type
                        && areaColorCoordinates.South.Type != type
                        && type != TypeColor.WaterCoast
                        && areaColorCoordinates.NorthWest.Type == type)
                    {
                        return PlaceObject(mapObjectCoordinates, altitude, RandomFromList(areaColorCoordinates.Center.Coasts.Coast.BorderSouthWest.List), zItem, texture);
                    }

                    return PlaceObject(mapObjectCoordinates, altitude, itemid, zItem, texture);
                }

            return false;
        }

        #endregion //SouthWestEdge

        #region NorthEastEdge

        private static bool PlaceObjectNorthEastEdge(
            AreaColorCoordinates areaColorCoordinates,
            MapObjectCoordinates mapObjectCoordinates,
            Coordinates coordinates,
            TypeColor type,
            sbyte zItem,
            sbyte altitude,
            int itemid,
            int texture = -1
            )
        {
            if (areaColorCoordinates.South.Type != type || areaColorCoordinates.SouthWest.Type != type || areaColorCoordinates.West.Type != type)
                return false;

            if (areaColorCoordinates.East.Type != type || areaColorCoordinates.SouthEast.Type != type)
                if (areaColorCoordinates.North.Type != type || areaColorCoordinates.NorthWest.Type != type || areaColorCoordinates.NorthEast.Type != type)
                {
                    if (areaColorCoordinates.North.Type == type
                        && areaColorCoordinates.NorthWest.Type == type
                        && areaColorCoordinates.East.Type == type
                        && areaColorCoordinates.NorthEast.Type != type
                        && type != TypeColor.WaterCoast)
                    {
                        return PlaceObject(mapObjectCoordinates, altitude,
                                    RandomFromList(areaColorCoordinates.Center.Coasts.Coast.BorderNorthEast.List), zItem, texture);
                    }

                    if (areaColorCoordinates.NorthWest.Type == type
                        && areaColorCoordinates.East.Type == type
                        && type != TypeColor.WaterCoast
                        && areaColorCoordinates.NorthEast.Type != type
                        && areaColorCoordinates.North.Type != type
                        )
                    {
                        return PlaceObject(mapObjectCoordinates, 0, itemid, zItem, texture);
                    }

                    return PlaceObject(mapObjectCoordinates, altitude, itemid, zItem, texture);
                }

            return false;
        }

        #endregion //NorthEastEdge

        #region SouthEastEdge

        private static bool PlaceObjectSouthEastEdge(
           AreaColorCoordinates areaColorCoordinates,
           MapObjectCoordinates mapObjectCoordinates,
           Coordinates coordinates,
           TypeColor type,
           sbyte zItem,
           sbyte altitude,
           int itemid,
            int texture = -1
           )
        {
            if (areaColorCoordinates.North.Type != type || areaColorCoordinates.NorthWest.Type != type || areaColorCoordinates.West.Type != type)
                return false;

            if (areaColorCoordinates.South.Type != type || areaColorCoordinates.SouthWest.Type != type)
                if (areaColorCoordinates.East.Type != type || areaColorCoordinates.NorthEast.Type != type || areaColorCoordinates.SouthEast.Type != type)
                {


                    if (areaColorCoordinates.SouthEast.Type == type
                        && areaColorCoordinates.SouthWest.Type == type
                        && areaColorCoordinates.NorthEast.Type == type
                        && areaColorCoordinates.East.Type != type
                        && type != TypeColor.WaterCoast)
                    {
                        return PlaceObject(mapObjectCoordinates, 0, itemid, zItem, texture);
                    }

                    return PlaceObject(mapObjectCoordinates, altitude, itemid, zItem, texture);
                }

            return false;
        }

        #endregion //SouthEastEdge

        #region NorthWestEdge

        private static bool PlaceObjectNorthWestEdge(
            AreaColorCoordinates areaColorCoordinates,
            MapObjectCoordinates mapObjectCoordinates,
            Coordinates coordinates,
            TypeColor type,
            sbyte zItem,
            sbyte altitude,
            int itemid,
            int texture = -1
            )
        {
            if (areaColorCoordinates.South.Type != type || areaColorCoordinates.SouthEast.Type != type || areaColorCoordinates.East.Type != type)
                return false;

            if (areaColorCoordinates.North.Type != type || areaColorCoordinates.NorthEast.Type != type || areaColorCoordinates.NorthWest.Type != type)
                if (areaColorCoordinates.West.Type != type || areaColorCoordinates.SouthWest.Type != type)
                {
                    return PlaceObject(mapObjectCoordinates, altitude, itemid, zItem, texture);
                }

            return false;
        }

        #endregion // NorthWestEdge

        #endregion //edges

        #region BORDER

        private static bool PlaceObjectBorder(
            AreaColorCoordinates areaColorCoordinates,
            MapObjectCoordinates mapObjectCoordinates,
            Coordinates coordinates,
            TypeColor type,
            sbyte zItem,
            sbyte altitude,
            int itemid,
            AreaColor border,
            int texture = -1
            )
        {
            if (areaColorCoordinates.List.Count(o => o != border && o.Type == type) != 8)
                return false;

            return PlaceObject(mapObjectCoordinates, altitude, itemid, zItem, texture);

        }

        #endregion //BORDER

        #region dobuleBorder

        private static bool PlaceDoubleBorder
            (
            AreaColorCoordinates areaColorCoordinates,
            MapObjectCoordinates mapObjectCoordinates,
            Coordinates coordinates,
            TypeColor type,
            sbyte zItem,
            sbyte altitude,
            int itemid,
            AreaColor border,
            AreaColor border2,
            int texture = -1
            )
        {
            if (areaColorCoordinates.List.Count(o => o.Type == type && o != border && o != border2) != 7)
                return false;

            return PlaceObject(mapObjectCoordinates, altitude, itemid, zItem, texture);

        }

        #endregion //doubleBorder

        #region Make coasts UOL style

        private void MakeCoastUolStyle
            (
            AreaColorCoordinates areaColorCoordinates,
            MapObjectCoordinates mapObjectCoordinates,
            Coordinates coordinates
            )
        {
            if (areaColorCoordinates.Center.Type == TypeColor.Water) return;


            if (areaColorCoordinates.List.All(o => o.Type != TypeColor.WaterCoast))
                return;

            if (areaColorCoordinates.List.All(o => o.Type == TypeColor.Water || o.Type == TypeColor.WaterCoast) && areaColorCoordinates.Center.Type == TypeColor.WaterCoast)
            {
                PlaceObject(mapObjectCoordinates, -15, areaColorCoordinates.Center.Coasts.Coast.Texture, -5,
                            RandomTexture(areaColorCoordinates.Center.TextureIndex));
                return;
            }

            #region WaterCoasts
            if (areaColorCoordinates.Center.Type == TypeColor.WaterCoast)
            {
                #region Borders
                if
                (
                    PlaceObjectBorder
                    (
                    areaColorCoordinates,
                    mapObjectCoordinates,
                    coordinates,
                    areaColorCoordinates.Center.Type,
                    -5,
                    -15,
                    areaColorCoordinates.Center.Coasts.Coast.Texture,
                    areaColorCoordinates.NorthWest
                    )
                )
                    return;

                if
                (
                    PlaceObjectBorder
                    (
                    areaColorCoordinates,
                    mapObjectCoordinates,
                    coordinates,
                    areaColorCoordinates.Center.Type,
                    -5,
                    -15,
                    areaColorCoordinates.Center.Coasts.Coast.Texture,
                    areaColorCoordinates.SouthEast
                    )
                )
                    return;

                if
                (
                    PlaceObjectBorder
                    (
                    areaColorCoordinates,
                    mapObjectCoordinates,
                    coordinates,
                    areaColorCoordinates.Center.Type,
                    -5,
                    -15,
                    areaColorCoordinates.Center.Coasts.Coast.Texture,
                    areaColorCoordinates.NorthEast
                    )
                )
                    return;

                if
                (
                    PlaceObjectBorder
                    (
                    areaColorCoordinates,
                    mapObjectCoordinates,
                    coordinates,
                    areaColorCoordinates.Center.Type,
                    -5,
                    -15,
                    areaColorCoordinates.Center.Coasts.Coast.Texture,
                    areaColorCoordinates.SouthWest
                    )
                )
                    return;
                #endregion //Borders

                #region Edges

                if
                (
                    PlaceObjectNorthWestEdge
                    (
                    areaColorCoordinates,
                    mapObjectCoordinates,
                    coordinates,
                    areaColorCoordinates.Center.Type,
                    -5,
                    -15,
                    areaColorCoordinates.Center.Coasts.Coast.Texture
                    )
                )
                    return;

                if
                (
                    PlaceObjectSouthWestEdge
                    (
                    areaColorCoordinates,
                    mapObjectCoordinates,
                    coordinates,
                    areaColorCoordinates.Center.Type,
                    -5,
                    -15,
                    areaColorCoordinates.Center.Coasts.Coast.Texture
                    )
                )
                    return;


                if
                (
                    PlaceObjectSouthEastEdge
                    (
                    areaColorCoordinates,
                    mapObjectCoordinates,
                    coordinates,
                    areaColorCoordinates.Center.Type,
                    -5,
                    -15,
                    areaColorCoordinates.Center.Coasts.Coast.Texture
                    )
                )
                    return;


                if
                (
                    PlaceObjectNorthEastEdge
                    (
                    areaColorCoordinates,
                    mapObjectCoordinates,
                    coordinates,
                    areaColorCoordinates.Center.Type,
                    -5,
                    -15,
                    areaColorCoordinates.Center.Coasts.Coast.Texture
                    )
                )
                    return;
                #endregion //edges

                #region Lines
                if
                (
                    PlaceObjectNS
                    (
                    areaColorCoordinates,
                    mapObjectCoordinates,
                    coordinates,
                    areaColorCoordinates.Center.Type,
                    -5,
                    -15,
                    areaColorCoordinates.Center.Coasts.Coast.Texture
                    )
                )
                    return;


                if
                (
                    PlaceObjectEW
                    (
                    areaColorCoordinates,
                    mapObjectCoordinates,
                    coordinates,
                    areaColorCoordinates.Center.Type,
                    -5,
                    -15,
                    areaColorCoordinates.Center.Coasts.Coast.Texture
                    )
                )
                    return;


                if
                (
                    PlaceObjectWE
                    (
                    areaColorCoordinates,
                    mapObjectCoordinates,
                    coordinates,
                    areaColorCoordinates.Center.Type,
                    -5,
                    -15,
                    areaColorCoordinates.Center.Coasts.Coast.Texture
                    )
                )
                    return;


                if
                (
                    PlaceObjectSN
                    (
                    areaColorCoordinates,
                    mapObjectCoordinates,
                    coordinates,
                    areaColorCoordinates.Center.Type,
                    -5,
                    -15,
                    areaColorCoordinates.Center.Coasts.Coast.Texture
                    )
                )
                    return;
                #endregion //Lines

                return;
            }
            #endregion //watercoasts

            #region casual

            #region BORDER

            #region NWB
            //NWB corretto
            if (
                PlaceObjectBorder(areaColorCoordinates,
                                  mapObjectCoordinates,
                                  coordinates,
                                  areaColorCoordinates.Center.Type,
                                  -5,
                                  -15,
                                  areaColorCoordinates.Center.Coasts.Coast.Texture,
                                  areaColorCoordinates.NorthWest,
                                  RandomFromList(areaColorCoordinates.Center.Coasts.Ground.EdgeNorthWest.List)))
            {
                mapObjectCoordinates.East.Altitude = -5;
                if (areaColorCoordinates.NorthEast.Type != TypeColor.WaterCoast)
                    mapObjectCoordinates.NorthEast.Altitude = -5;
                return;
            }
            #endregion

            #region NEB
            //NEB
            if (
                PlaceObjectBorder(
                    areaColorCoordinates,
                    mapObjectCoordinates,
                    coordinates,
                    areaColorCoordinates.Center.Type,
                    -5,
                    -15,
                    RandomFromList
                        (
                            areaColorCoordinates.Center.Coasts.Coast.EdgeNorthEast.List
                        )
                    ,
                    areaColorCoordinates.NorthEast,
                    RandomFromList
                        (areaColorCoordinates.Center.Coasts.Ground.EdgeNorthEast.List)
                    ))
                return;
            #endregion //NEB

            #region SEB
            //SEB
            if (
                PlaceObjectBorder(
                    areaColorCoordinates,
                    mapObjectCoordinates,
                    coordinates,
                    areaColorCoordinates.Center.Type,
                    -5,
                //test di verifica
                    (sbyte)Random.Next(areaColorCoordinates.Center.Min, areaColorCoordinates.Center.Max),
                    RandomFromList
                        (
                            areaColorCoordinates.Center.Coasts.Coast.EdgeSouthEast.List
                        )
                    ,
                    areaColorCoordinates.SouthEast,
                    RandomFromList
                        (areaColorCoordinates.Center.Coasts.Ground.EdgeSouthEast.List)
                    ))
                return;

            #endregion //SEB

            #region SWB
            //SWB
            if (
                PlaceObjectBorder(
                    areaColorCoordinates,
                    mapObjectCoordinates,
                    coordinates,
                    areaColorCoordinates.Center.Type,
                    -5,
                //test di verifica
                    (sbyte)Random.Next(areaColorCoordinates.Center.Min, areaColorCoordinates.Center.Max),
                    RandomFromList
                        (
                            areaColorCoordinates.Center.Coasts.Coast.EdgeSouthWest.List
                        )
                    ,
                    areaColorCoordinates.SouthWest,
                    RandomFromList
                        (areaColorCoordinates.Center.Coasts.Ground.EdgeSouthWest.List)
                    ))
                return;

            #endregion //SWB

            #endregion //BORDER


            #region Double Border
            //INS
            if (PlaceDoubleBorder(areaColorCoordinates, mapObjectCoordinates, coordinates,
                                  areaColorCoordinates.Center.Type, 0, 0, 0, areaColorCoordinates.SouthWest,
                                  areaColorCoordinates.NorthWest,
                                  RandomFromList(areaColorCoordinates.Center.Coasts.Ground.BorderSouthWest.List)))
                return;
            //IWE
            if (PlaceDoubleBorder(areaColorCoordinates, mapObjectCoordinates, coordinates,
                                  areaColorCoordinates.Center.Type, 0, 0, 0, areaColorCoordinates.NorthWest,
                                  areaColorCoordinates.SouthEast,
                                  RandomFromList(areaColorCoordinates.Center.Coasts.Ground.BorderSouthWest.List)))
                return;
            #endregion //DOUBLE BORDER


            #region Lines

            #region SN
            if (
                PlaceObjectSN(areaColorCoordinates,
                              mapObjectCoordinates,
                              coordinates,
                              areaColorCoordinates.Center.Type,
                              -5,
                              -15,
                              areaColorCoordinates.Center.Coasts.Coast.Texture,
                              RandomFromList(areaColorCoordinates.Center.Coasts.Ground.LineNorth.List)))
            {
                mapObjectCoordinates.South.Altitude += (sbyte) Random.Next(areaColorCoordinates.South.Min-2, areaColorCoordinates.South.Max-2);
                return;

            }
            #endregion //SN

            #region EW

            if (
                PlaceObjectEW(areaColorCoordinates,
                              mapObjectCoordinates,
                              coordinates,
                              areaColorCoordinates.Center.Type,
                              -5,
                              -15,
                              areaColorCoordinates.Center.Coasts.Coast.Texture,
                              RandomFromList(areaColorCoordinates.Center.Coasts.Ground.LineWest.List)))
            {

                return;
            }

            #endregion //EW

            #region WE

            if (
                PlaceObjectWE(areaColorCoordinates,
                              mapObjectCoordinates,
                              coordinates,
                              areaColorCoordinates.Center.Type,
                              -5,
                              (sbyte)Random.Next(areaColorCoordinates.Center.Min, areaColorCoordinates.Center.Max),
                              RandomFromList(areaColorCoordinates.Center.Coasts.Coast.LineEast.List),
                              RandomFromList(areaColorCoordinates.Center.Coasts.Ground.LineEast.List)))
            {
                return;

            }

            #endregion //WE

            #region NS

            if (
                PlaceObjectNS(areaColorCoordinates,
                              mapObjectCoordinates,
                              coordinates,
                              areaColorCoordinates.Center.Type,
                              -5,
                              (sbyte)Random.Next(areaColorCoordinates.Center.Min, areaColorCoordinates.Center.Max),
                              RandomFromList(areaColorCoordinates.Center.Coasts.Coast.LineSouth.List),
                              RandomFromList(areaColorCoordinates.Center.Coasts.Ground.LineSouth.List)))

                return;

            #endregion //NS

            #endregion //Lines


            #region Edges

            #region North East Edge

            if (PlaceObjectNorthEastEdge(
                areaColorCoordinates,
                mapObjectCoordinates,
                coordinates,
                areaColorCoordinates.Center.Type,
                -5,
                -15,
                areaColorCoordinates.Center.Coasts.Coast.Texture,
                RandomFromList(areaColorCoordinates.Center.Coasts.Ground.BorderNorthEast.List)))
            {
                if(areaColorCoordinates.NorthWest.Type!= TypeColor.WaterCoast)
                    PlaceObject(mapObjectCoordinates, -15,
                            RandomFromList(areaColorCoordinates.Center.Coasts.Coast.EdgeSouthWest.List), -5,-1);
                 
                mapObjectCoordinates.SouthEast.Altitude = -5;
                
                return;

            }

            #endregion //North East Edge

            #region South West Edge

            if (PlaceObjectSouthWestEdge(areaColorCoordinates,
                               mapObjectCoordinates,
                               coordinates,
                               areaColorCoordinates.Center.Type,
                               -5,
                               -15,
                               areaColorCoordinates.Center.Coasts.Coast.Texture,
                               RandomFromList(areaColorCoordinates.Center.Coasts.Ground.BorderSouthWest.List)))
            {
                return;

            }

            #endregion // South West Edge

            #region North West Edge

            if (PlaceObjectNorthWestEdge
                (areaColorCoordinates
                 , mapObjectCoordinates,
                 coordinates,
                 areaColorCoordinates.Center.Type,
                 -5,
                 -15,
                 areaColorCoordinates.Center.Coasts.Coast.Texture,
                 RandomFromList(areaColorCoordinates.Center.Coasts.Ground.BorderNorthWest.List)))
            {
                mapObjectCoordinates.SouthEast.Altitude = -5;
                return;
            }

            #endregion //North West Edge

            #region South East Edge

            if (PlaceObjectSouthEastEdge(areaColorCoordinates,
                               mapObjectCoordinates,
                               coordinates,
                               areaColorCoordinates.Center.Type,
                               -5,
                               (sbyte)Random.Next(areaColorCoordinates.Center.Min, areaColorCoordinates.Center.Max),
                               RandomFromList(areaColorCoordinates.Center.Coasts.Coast.BorderSouthEast.List),
                               RandomFromList(areaColorCoordinates.Center.Coasts.Ground.BorderSouthEast.List)))
                return;

            #endregion //South East Edge

            #endregion //Edges


            var coasts = areaColorCoordinates.List.FirstOrDefault(o => o.Type == TypeColor.WaterCoast);

            //_bitmap[coordinates.Center] = coasts.Color;
            _bitmapAreaColor[coordinates.Center] = coasts;
            PlaceObject(mapObjectCoordinates, -15, coasts.Coasts.Coast.Texture, -5, -1);

            #endregion //casual
        }

        #endregion // make coasts uol style


        #endregion // Coasts uol style

        #region CollectionAreaCliffs

        /// <summary>
        /// method to make cliff
        /// </summary>
        /// <param name="coordinates"> </param>
        /// <param name="Areacoordinates"> </param>
        /// <param name="mapObjectCoordinates"> </param>
        private static void MakeCliffs(Coordinates coordinates, AreaColorCoordinates Areacoordinates, MapObjectCoordinates mapObjectCoordinates)
        {
            if (Areacoordinates.Center.Type != TypeColor.Cliff) return;

            mapObjectCoordinates.Center.Altitude = 0;

            //**********************
            //*       Line         *
            //**********************


            if (Areacoordinates.North.Type == TypeColor.Cliff && Areacoordinates.South.Type == TypeColor.Cliff)
            {
                var areaTransitionCliffTexture =
                    Areacoordinates.West.TransitionCliffTextures.FirstOrDefault(
                        o => o.Directions == DirectionCliff.WestEast && o.ColorTo == Areacoordinates.East.Color);

                mapObjectCoordinates.Center.Texture = (short) RandomFromList(areaTransitionCliffTexture.List);
                return;
            }


            if (Areacoordinates.East.Type == TypeColor.Cliff && Areacoordinates.West.Type == TypeColor.Cliff)
            {
                var areaTransitionCliffTexture =
                    Areacoordinates.North.TransitionCliffTextures.FirstOrDefault(
                        o => o.Directions == DirectionCliff.NorthSouth && o.ColorTo == Areacoordinates.South.Color);

                mapObjectCoordinates.Center.Texture = (short) RandomFromList(areaTransitionCliffTexture.List);
                return;
            }


            ////**********************
            ////* Anfang und Ende    *
            ////**********************

            //  ! 
            // ?X?
            //  C
            if (Areacoordinates.South.Type == TypeColor.Cliff && Areacoordinates.North.Type != TypeColor.Cliff
                && Areacoordinates.East.Type!= TypeColor.Cliff && Areacoordinates.West.Type!= TypeColor.Cliff)
            {
                AreaTransitionCliffTexture areaTransitionCliffTexture = null;
                if (Areacoordinates.East.Type == TypeColor.Cliff)

                    areaTransitionCliffTexture =
                        Areacoordinates.West.TransitionCliffTextures.FirstOrDefault(
                            c => c.Directions == DirectionCliff.NorthEnd && c.ColorTo == Areacoordinates.North.Color);

                if (areaTransitionCliffTexture != null)
                    AddTexture(RandomFromList(areaTransitionCliffTexture.List), mapObjectCoordinates);
                return;
            }

            //  ? 
            // CX!
            //  ?
            if (Areacoordinates.West.Type == TypeColor.Cliff && Areacoordinates.East.Type != TypeColor.Cliff
                && Areacoordinates.North.Type!=TypeColor.Cliff && Areacoordinates.South.Type!=TypeColor.Cliff)
            {
                var areaTransitionCliffTexture = Areacoordinates.South.TransitionCliffTextures.
                    FirstOrDefault(
                        c => c.Directions == DirectionCliff.EastEnd && c.ColorTo == Areacoordinates.North.Color);

                if (areaTransitionCliffTexture != null)
                    AddTexture(RandomFromList(areaTransitionCliffTexture.List), mapObjectCoordinates);
                return;
            }


            //  C 
            // ?X?
            //  !
            if (Areacoordinates.South.Type != TypeColor.Cliff && Areacoordinates.North.Type == TypeColor.Cliff
                && Areacoordinates.East.Type!= TypeColor.Cliff && Areacoordinates.West.Type!=TypeColor.Cliff)
            {
                var areaTransitionCliffTexture = Areacoordinates.East.TransitionCliffTextures.
                    FirstOrDefault(c => c.Directions == DirectionCliff.SouthEnd &&
                                        c.ColorTo == Areacoordinates.West.Color);


                if (areaTransitionCliffTexture != null)
                    AddTexture(RandomFromList(areaTransitionCliffTexture.List), mapObjectCoordinates);
                return;
            }


            //  ? 
            // !XC
            //  ?
            if (Areacoordinates.East.Type == TypeColor.Cliff && Areacoordinates.West.Type != TypeColor.Cliff
                && Areacoordinates.North.Type!=TypeColor.Cliff&& Areacoordinates.South.Type!=TypeColor.Cliff)
            {
                var areaTransitionCliffTexture = Areacoordinates.South.TransitionCliffTextures.
                    FirstOrDefault(c => c.Directions == DirectionCliff.WestEnd
                                        && c.ColorTo == Areacoordinates.North.Color);

                if (areaTransitionCliffTexture != null)
                    AddTexture(RandomFromList(areaTransitionCliffTexture.List), mapObjectCoordinates);
                return;

            }



            //**********************
            //* Rundungen          *
            //**********************

            //  C 
            // CX
            //   ?
            if (Areacoordinates.West.Type == TypeColor.Cliff && Areacoordinates.North.Type == TypeColor.Cliff
                && Areacoordinates.NorthWest.Type!= TypeColor.Cliff)
            {
                var areaTransitionCliffTexture = Areacoordinates.NorthWest.TransitionCliffTextures.
                    FirstOrDefault(c => c.Directions == DirectionCliff.NorthWestRounding &&
                                        c.ColorTo == Areacoordinates.NorthWest.Color);

                if (areaTransitionCliffTexture != null)
                    AddTexture(RandomFromList(areaTransitionCliffTexture.List), mapObjectCoordinates);
                return;
            }

            //  C 
            //  XC
            // ?
            if (Areacoordinates.East.Type == TypeColor.Cliff && Areacoordinates.North.Type == TypeColor.Cliff && Areacoordinates.NorthEast.Type != TypeColor.Cliff)
            {
                var areaTransitionCliffTexture = Areacoordinates.NorthEast.TransitionCliffTextures.
                    FirstOrDefault(
                        c => c.Directions == DirectionCliff.NorthEastRounding
                             && c.ColorTo == Areacoordinates.NorthEast.Color);

                if (areaTransitionCliffTexture != null)
                    AddTexture(RandomFromList(areaTransitionCliffTexture.List), mapObjectCoordinates);
                return;
            }

            // ? 
            //  XC
            //  C
            if (Areacoordinates.East.Type == TypeColor.Cliff && Areacoordinates.South.Type == TypeColor.Cliff && Areacoordinates.SouthEast.Type!= TypeColor.Cliff)
            {
                var areaTransitionCliffTexture = Areacoordinates.SouthEast.TransitionCliffTextures.
                    FirstOrDefault(
                        c => c.Directions == DirectionCliff.SouthEastRounding
                             && c.ColorTo == Areacoordinates.SouthEast.Color);


                if (areaTransitionCliffTexture != null)
                    AddTexture(RandomFromList(areaTransitionCliffTexture.List), mapObjectCoordinates);
                return;
            }

            //   ?
            // CX
            //  C
            if (Areacoordinates.West.Type == TypeColor.Cliff && Areacoordinates.South.Type == TypeColor.Cliff && Areacoordinates.SouthWest.Type!= TypeColor.Cliff)
            {
                var areaTransitionCliffTexture = Areacoordinates.SouthWest.TransitionCliffTextures.
                    FirstOrDefault(
                        c => c.Directions == DirectionCliff.SouthWestRounding
                             && c.ColorTo == Areacoordinates.SouthWest.Color);

                if (areaTransitionCliffTexture != null)
                    AddTexture(RandomFromList(areaTransitionCliffTexture.List), mapObjectCoordinates);
                return;
            }
        }


        static void AddTexture(int texture, MapObjectCoordinates coordinates)
        {
            coordinates.Center.Texture = (short)texture;
        }

        #endregion //CollectionAreaCliffs

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

                    if (_mapObjects[location].Occupied != 0) continue;

                    var itemgroups = _bitmapAreaColor[location].Items;
                    if (itemgroups == null || itemgroups.List.Count <= 0) continue;

                    var group = itemgroups.List[Random.Next(0, itemgroups.List.Count)];
                    var random = Random.Next(0, 100);
                    if (random > @group.Percent) continue;

                    var tmp_item = @group.List.First();
                    if (@group.List.Count > 1)
                    {
                        z = tmp_item.Z;
                    }

                    foreach (SingleItem item in @group.List)
                    {
                        var locationshift = CalculateZone(x + item.X, y + item.Y, _stride);

                        if (_mapObjects[locationshift].Items == null)
                            _mapObjects[locationshift].Items = new List<ItemClone>();

                        var itemclone = new ItemClone(item);

                        _mapObjects[locationshift].Items.Add(itemclone);
                        if (tmp_item == item)
                        {
                            itemclone.Z = (sbyte)((_mapObjects[locationshift].Altitude +
                                                   _mapObjects[CalculateZone(x + item.X + 1, y + item.Y, _stride)].Altitude +
                                                   _mapObjects[CalculateZone(x + item.X, y + item.Y + 1, _stride)].Altitude +
                                                   _mapObjects[CalculateZone(x + item.X + 1, y + item.Y + 1, _stride)].Altitude) / 4 + item.Z);
                        }
                        else
                        {
                            itemclone.Z = (sbyte)((_mapObjects[CalculateZone(x + tmp_item.X, y + tmp_item.Y, _stride)].Altitude +
                                                   _mapObjects[CalculateZone(x + tmp_item.X + 1, y + tmp_item.Y, _stride)].Altitude +
                                                   _mapObjects[CalculateZone(x + tmp_item.X, y + tmp_item.Y + 1, _stride)].Altitude +
                                                   _mapObjects[CalculateZone(x + tmp_item.X + 1, y + tmp_item.Y + 1, _stride)].Altitude) / 4 + tmp_item.Z + z);

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

                                    var id = _mapObjects[local].Texture;
                                    var z = _mapObjects[local].Altitude;
                                    if ((id < 0) || (id >= 0x4000))
                                        id = 0;

                                    map0.Write(id);//writes tid
                                    map0.Write(z);//writes Z
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
        private static int RandomFromList(IList<int> list)
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

        internal static sbyte CalculateHeightValue(Color c)
        {
            var tmp = c.B - 128;

            if (tmp > sbyte.MaxValue)
                return sbyte.MaxValue;
            if (tmp < sbyte.MinValue)
                return sbyte.MinValue;

            return (sbyte)tmp;
        }


        /// <summary>
        /// array of precalculated x and y in a linear matrix
        /// </summary>
        /// <param name="x">x coord</param>
        /// <param name="y">y coord</param>
        /// <param name="shiftX"> </param>
        /// <param name="shiftY"> </param>
        /// <returns>array of params</returns>
        private Coordinates MakeIndexesDirections(int x, int y, int shiftX, int shiftY)
        {
            return new Coordinates(shiftX, shiftY, x, y, _stride);
        }



        #endregion

        #endregion //Methods

        #region Event

        public event EventHandler ProgressText;

        public void OnProgressText(EventArgs e)
        {
            EventHandler handler = ProgressText;
            if (handler != null) handler(this, e);
        }

        #endregion //event

    }

    #region Tool Classes and structures
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
        public int Center { get { return _array[(int)Directions.Center]; } }
        public int North { get { return _array[(int)Directions.North]; } }
        public int South { get { return _array[(int)Directions.South]; } }
        public int East { get { return _array[(int)Directions.East]; } }
        public int West { get { return _array[(int)Directions.West]; } }
        public int NorthEast { get { return _array[(int)Directions.NorthEast]; } }
        public int NorthWest { get { return _array[(int)Directions.NorthWest]; } }
        public int SouthEast { get { return _array[(int)Directions.SouthEast]; } }
        public int SouthWest { get { return _array[(int)Directions.SouthWest]; } }
        public int X { get { return _array[9]; } }
        public int Y { get { return _array[10]; } }

        public int[] List { get { return _array; } }

        public Coordinates(int shiftX, int shiftY, int x, int y, int stride)
        {
            _array = new int[11];
            _array[(int)Directions.Center] = MapMaker.CalculateZone(x, y, stride);
            _array[(int)Directions.North] = MapMaker.CalculateZone(x, y - shiftY, stride); ;
            _array[(int)Directions.South] = MapMaker.CalculateZone(x, y + shiftY, stride);
            _array[(int)Directions.East] = MapMaker.CalculateZone(x + shiftX, y, stride);
            _array[(int)Directions.West] = MapMaker.CalculateZone(x - shiftX, y, stride);
            _array[(int)Directions.NorthWest] = MapMaker.CalculateZone(x - shiftX, y - shiftY, stride);
            _array[(int)Directions.NorthEast] = MapMaker.CalculateZone(x + shiftX, y - shiftY, stride);
            _array[(int)Directions.SouthWest] = MapMaker.CalculateZone(x - shiftX, y + shiftY, stride);
            _array[(int)Directions.SouthEast] = MapMaker.CalculateZone(x + shiftX, y + shiftY, stride);
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

        //public AreaColorCoordinates(CollectionAreaColor collection, Coordinates coordinates, Color[] map)
        //{
        //    _center = collection.FindByColor(map[coordinates.Center]);
        //    _north = collection.FindByColor(map[coordinates.North]);
        //    _south = collection.FindByColor(map[coordinates.South]);
        //    _east = collection.FindByColor(map[coordinates.East]);
        //    _west = collection.FindByColor(map[coordinates.West]);
        //    _northWest = collection.FindByColor(map[coordinates.NorthWest]);
        //    _northEast = collection.FindByColor(map[coordinates.NorthEast]);
        //    _southWest = collection.FindByColor(map[coordinates.SouthWest]);
        //    _southEast = collection.FindByColor(map[coordinates.SouthEast]);

        //    List = new[] { _center, _north, _south, _east, _west, _northEast, _northWest, _southEast, _southWest };
        //}
        public AreaColorCoordinates(Coordinates coordinates, AreaColor[] map)
        {
            List = new AreaColor[9];

            List[(int)Directions.Center] = map[coordinates.Center];
            _center = List[(int)Directions.Center];

            List[(int)Directions.East] = map[coordinates.East];
            _east = List[(int)Directions.East];

            List[(int)Directions.North] = map[coordinates.North];
            _north = List[(int)Directions.North];

            List[(int)Directions.NorthEast] = map[coordinates.NorthEast];
            _northEast = List[(int)Directions.NorthEast];

            List[(int)Directions.NorthWest] = map[coordinates.NorthWest];
            _northWest = List[(int)Directions.NorthWest];

            List[(int)Directions.South] = map[coordinates.South];
            _south = List[(int)Directions.South];

            List[(int)Directions.SouthEast] = map[coordinates.SouthEast];
            _southEast = List[(int)Directions.SouthEast];

            List[(int)Directions.SouthWest] = map[coordinates.SouthWest];
            _southWest = List[(int)Directions.SouthWest];

            List[(int)Directions.West] = map[coordinates.West];
            _west = List[(int)Directions.West];


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

    internal struct MapZObjectCoordinates
    {
        private readonly sbyte[] _list;

        private readonly sbyte _center;
        private readonly sbyte _north;
        private readonly sbyte _south;
        private readonly sbyte _east;
        private readonly sbyte _west;
        private readonly sbyte _northEast;
        private readonly sbyte _northWest;
        private readonly sbyte _southEast;
        private readonly sbyte _southWest;

        public sbyte Center { get { return _center; } }
        public sbyte North { get { return _north; } }
        public sbyte South { get { return _south; } }
        public sbyte East { get { return _east; } }
        public sbyte West { get { return _west; } }
        public sbyte NorthEast { get { return _northEast; } }
        public sbyte NorthWest { get { return _northWest; } }
        public sbyte SouthEast { get { return _southEast; } }
        public sbyte SouthWest { get { return _southWest; } }

        public sbyte[] List { get { return _list; } }
        public MapZObjectCoordinates(Coordinates coordinates, Color[] map)
        {

            _center = MapMaker.CalculateHeightValue(map[coordinates.Center]);
            _north = MapMaker.CalculateHeightValue(map[coordinates.North]);
            _south = MapMaker.CalculateHeightValue(map[coordinates.South]);
            _east = MapMaker.CalculateHeightValue(map[coordinates.East]);
            _west = MapMaker.CalculateHeightValue(map[coordinates.West]);
            _northWest = MapMaker.CalculateHeightValue(map[coordinates.NorthWest]);
            _northEast = MapMaker.CalculateHeightValue(map[coordinates.NorthEast]);
            _southWest = MapMaker.CalculateHeightValue(map[coordinates.SouthWest]);
            _southEast = MapMaker.CalculateHeightValue(map[coordinates.SouthEast]);

            _list = new[] { _center, _north, _south, _east, _west, _northEast, _northWest, _southEast, _southWest };
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

    #endregion //Tool classes and structures

}
