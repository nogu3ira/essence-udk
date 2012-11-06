using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EssenceUDK.MapMaker.Elements.BaseTypes;

namespace EssenceUDK.MapMaker.Elements.Items.Items
{
    public class SingleItem : NotificationObject
    {
        private int _id;
        private int _x;
        private int _y;
        private int _z;
        private string _name;
        private int _hue;


        public int Id { get { return _id; } set { _id = value; RaisePropertyChanged(()=>Id); } }

        public int X { get { return _x; } set { _x = value; RaisePropertyChanged(()=>X); } }

        public int Y { get { return _y; } set { _y = value; RaisePropertyChanged(()=>Y); } }

        public int Z { get { return _z; } set { _z = value; RaisePropertyChanged(()=>Z); } }

        public string Name { get { return _name; } set { _name = value; RaisePropertyChanged(()=>Name); } }

        public int Hue { get { return _hue; } set { _hue = value; RaisePropertyChanged(()=>Hue); } }
    }
}
