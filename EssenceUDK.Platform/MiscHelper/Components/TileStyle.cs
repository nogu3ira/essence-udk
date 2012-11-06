using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using EssenceUDK.TilesInfo.Components.Base;
using EssenceUDK.TilesInfo.Components.Interface;
using EssenceUDK.TilesInfo.Components.Tiles;

namespace EssenceUDK.TilesInfo.Components
{
    [Serializable()]
    [DataContract]
    [XmlInclude(typeof(TileDoor))]
    [XmlInclude(typeof(TileWall))]
    [XmlInclude(typeof(TileMisc))]
    [XmlInclude(typeof(TileRoof))]
    [XmlInclude(typeof(TileFloor))]
    [XmlInclude(typeof(TileStairs))]
    public class TileStyle : NotificationObject, IComponent
    {
        #region fields
        
        private TileCategory _myCategory = new TileCategory();
        
        private ObservableCollection<Tile> _list;
        
        private string _name = "";
        
        private int _index;
       
        #endregion// Fields

        #region Props

        [DataMember]
        public int Id { get { return _index; } set { _index = value; RaisePropertyChanged(()=>Id); } }
        
        [DataMember]
        public string Name { get { return _name; } set { _name = value ?? "";  RaisePropertyChanged(()=>Name);} }
        
        [DataMember]
        public ObservableCollection<Tile> List
        {
            get { return _list; }
            set { _list = value ?? new ObservableCollection<Tile>(); RaisePropertyChanged(()=>List); }
        }

        #endregion // Props

        #region Ctor
        
        public TileStyle()
        {
            _name = "";
            _index = -1;
            List= new ObservableCollection<Tile>();
        }

        public TileStyle(int number)
            :this()
        {
            _index = number;
        }

        #endregion //Ctor

        #region Methods
        
        public Tile FindTile(uint id)
        {

            var tile =
                from t in List
                where t.Id == id
                select t;

            return tile.FirstOrDefault();
        }

        public TileCategory GetCategory()
        {
            return _myCategory;
        }

        public void SetCategory(TileCategory category)
        {
            _myCategory = category;
        }

        public void AddTile(Tile tile)
        {
            List.Add(tile);
            tile.SetStyle(this);
        }

        public IEnumerable<Tile> FindTileByPosition(int position)
        {
            return List.Where(tile => tile.Position == position);
        }
        public IEnumerable<Tile> FindTileByPosition(string position)
        {
            return List.Where(tile => tile.PositionString == position);
        }


        public void RemoveTile(Tile t)
        {
            if (t == null)
                return;
            t.SetStyle(new TileStyle());
            List.Remove(t);
        }
        
        #endregion //Methods

        public object Value()
        {
            return this;
        }
    }
}
