using System.Collections.Generic;
using EssenceUDK.MapMaker.Elements.ColorArea.ColorArea;

namespace EssenceUDK.MapMaker.MapMaking
{
    public class MapObjectCoordinates
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


        public bool PlaceObject(AreaColorCoordinates areaColorCoordinates, sbyte altitude, int itemid,
                                sbyte zItem, int texture, bool normal = true)
        {

            var mapObject = !normal ? Center : SouthEast;
            if (mapObject.Occupied != 0 && (mapObject.Occupied != (byte)TypeColor.WaterCoast && itemid != (int)SpecialAboutItems.ClearAll))
                return true;

            if (itemid >= 0)
                mapObject.Items = new List<ItemClone> { new ItemClone { Id = itemid, Z = zItem } };
            if (itemid == (int)SpecialAboutItems.ClearAll)
                mapObject.Items = null;


            mapObject.Occupied = (byte)areaColorCoordinates.Center.Type;

            if (texture >= 0)
                Center.Texture = (short)texture;
            Center.Altitude = altitude;

            return true;
        }
    }
}