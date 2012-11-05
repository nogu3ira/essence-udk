using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using EssenceUDK.TilesInfo.Components.Base;
using EssenceUDK.TilesInfo.Components.Enums;
using EssenceUDK.TilesInfo.Components.Interface;
using IComponent = EssenceUDK.TilesInfo.Components.Interface.IComponent;

namespace EssenceUDK.TilesInfo.Components
{
    [Serializable()]
    [DataContract]
    public class Tile :  NotificationObject,IEquatable<Tile>, IEquatable<uint>, IComponent, ITile
    {
        private readonly static List<string> PList = new List<string>()
                {
                    "None",
                    "South",
                    "Corner",
                    "East",
                    "Post"
                };

        #region Fields
        protected uint _id = uint.MaxValue;
        protected string Pos;
        private TypeTile _type;
        private string _name;
        #endregion//Fields

        #region Props
        [DataMember]
        public String Name { get { return _name; } set { _name = value; RaisePropertyChanged(()=>Name); } }
        private TileStyle _myStyle = new TileStyle();
        [DataMember]
        public TypeTile Type { get { return _type; } set { _type = value; RaisePropertyChanged(()=>Type); } }

        public virtual List<String> PosList
        {
            get { return PList; }
        } 
        #endregion

        #region ctor

        public Tile()
        {
            Type = TypeTile.None;
            Name = "";
            Pos = PositionTiles.None.ToString();
        }

        public Tile(TileStyle style)
            :this()
        {
            _myStyle = style;
        }
        #endregion

        #region props
        public string PositionString { get { return Pos; } set { Pos = value; RaisePropertyChanged(()=>PositionString);RaisePropertyChanged(()=>Position);} }
        
        [DataMember]
        public virtual int Position
        {
            get { return PosList.IndexOf(Pos); }
            set { Pos = PosList[value]; RaisePropertyChanged(()=>Position); RaisePropertyChanged(()=>PositionString); RaisePropertyChanged(()=>Position);}
        }

        [DataMember]
        public uint Id
        {
            get { return _id; }
            set{ChangeId(value); RaisePropertyChanged(()=>Id);}
        }

        
        #endregion
        
        #region methods

        public void ChangeId(uint id)
        {
            _id = id;
        }
        
        public void SetStyle(TileStyle style)
        {
            _myStyle = style;
        }

        public TileStyle GetStyle()
        {
            return _myStyle;
        }
        
        
        #endregion

        #region Implementation of IEquatable<Tile>

        public bool Equals(Tile other)
        {
            return Id == other.Id;
        }

        #endregion

        #region Implementation of IEquatable<uint>

        public bool Equals(uint other)
        {
            return Id == other;
        }

        #endregion

        public object Value()
        {
            return this;
        }

    }
}
