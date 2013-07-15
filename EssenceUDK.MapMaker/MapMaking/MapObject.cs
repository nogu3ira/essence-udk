using System.Collections.Generic;

namespace EssenceUDK.MapMaker.MapMaking
{
    public class MapObject
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
}