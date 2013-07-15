using EssenceUDK.MapMaker.Elements.ColorArea.ColorArea;

namespace EssenceUDK.MapMaker.MapMaking
{
    public class AreaColorCoordinates
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

        public AreaColor Center
        {
            get { return _center; }
        }

        public AreaColor North
        {
            get { return _north; }
        }

        public AreaColor South
        {
            get { return _south; }
        }

        public AreaColor East
        {
            get { return _east; }
        }

        public AreaColor West
        {
            get { return _west; }
        }

        public AreaColor NorthEast
        {
            get { return _northEast; }
        }

        public AreaColor NorthWest
        {
            get { return _northWest; }
        }

        public AreaColor SouthEast
        {
            get { return _southEast; }
        }

        public AreaColor SouthWest
        {
            get { return _southWest; }
        }

        public AreaColor[] List { get; set; }


        public AreaColorCoordinates(Coordinates coordinates, AreaColor[] map)
        {
            List = new AreaColor[9];

            List[(int) Directions.Center] = map[coordinates.Center];
            _center = List[(int) Directions.Center];

            List[(int) Directions.East] = map[coordinates.East];
            _east = List[(int) Directions.East];

            List[(int) Directions.North] = map[coordinates.North];
            _north = List[(int) Directions.North];

            List[(int) Directions.NorthEast] = map[coordinates.NorthEast];
            _northEast = List[(int) Directions.NorthEast];

            List[(int) Directions.NorthWest] = map[coordinates.NorthWest];
            _northWest = List[(int) Directions.NorthWest];

            List[(int) Directions.South] = map[coordinates.South];
            _south = List[(int) Directions.South];

            List[(int) Directions.SouthEast] = map[coordinates.SouthEast];
            _southEast = List[(int) Directions.SouthEast];

            List[(int) Directions.SouthWest] = map[coordinates.SouthWest];
            _southWest = List[(int) Directions.SouthWest];

            List[(int) Directions.West] = map[coordinates.West];
            _west = List[(int) Directions.West];


        }

        public bool IsEastLine(TypeColor type)
        {
            if (West.Type != type
                || NorthWest.Type != type
                || SouthWest.Type != type
                || North.Type != type
                || South.Type != type
                )
                return false;

            return East.Type != type
                   || NorthEast.Type != type
                   || SouthEast.Type != type;
        }

        public bool IsWestLine(TypeColor type)
        {
            if (
                NorthEast.Type != type
                || North.Type != type
                || East.Type != type
                || SouthEast.Type != type
                || South.Type != type
                )
                return false;

            return West.Type != type || SouthWest.Type != type || NorthWest.Type != type;
        }

        public bool IsNorthLine(TypeColor type)
        {
            if (South.Type != type
                || SouthWest.Type != type
                || SouthEast.Type != type
                || East.Type != type
                || West.Type != type)
                return false;

            return North.Type != type
                   || NorthEast.Type != type
                   || NorthWest.Type != type;
        }

        public bool IsSouthLine(TypeColor type)
        {
            if (North.Type != type
                || NorthWest.Type != type
                || NorthEast.Type != type
                || East.Type != type
                || West.Type != type)
                return false;

            return South.Type != type || SouthEast.Type != type ||
                   SouthWest.Type != type;
        }

        public bool IsSouthWestEdge(TypeColor type)
        {
            if (North.Type != type || NorthEast.Type != type || East.Type != type)
                return false;

            if (South.Type != type || SouthEast.Type != type || SouthWest.Type != type)
                if (West.Type != type || NorthWest.Type != type)
                    return true;

            return false;
        }

        public bool IsNortEastEdge(TypeColor type)
        {
            if (South.Type != type || SouthWest.Type != type || West.Type != type)
                return false;

            if (East.Type != type || SouthEast.Type != type)
                if (North.Type != type || NorthWest.Type != type || NorthEast.Type != type)
                    return true;

            return false;
        }

        public bool IsSouthEastEdge(TypeColor type)
        {
            if (North.Type != type || NorthWest.Type != type || West.Type != type)
                return false;

            if (South.Type != type || SouthWest.Type != type)
                if (East.Type != type || NorthEast.Type != type || SouthEast.Type != type)
                {
                    return true;
                }
            return false;
        }

        public bool IsNorthWestEdge(TypeColor type)
        {
            if (South.Type != type || SouthEast.Type != type || East.Type != type)
                return false;

            if (North.Type != type || NorthEast.Type != type || NorthWest.Type != type)
                if (West.Type != type || SouthWest.Type != type)
                    return true;

            return false;
        }
    }
}