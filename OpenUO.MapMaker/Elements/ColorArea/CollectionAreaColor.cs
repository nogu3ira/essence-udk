﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using System.Text;
using System.Xml.Serialization;
using OpenUO.MapMaker.Elements.BaseTypes;
using OpenUO.MapMaker.Elements.ColorArea.ColorArea;

namespace OpenUO.MapMaker.Elements.ColorArea
{
    [Serializable]
    [XmlInclude(typeof(AreaColorCoast))]
    public sealed class CollectionAreaColor : NotificationObject, IContainerSet
    {
        private ObservableCollection<AreaColor> _list;
        #region Ctor

        public CollectionAreaColor()
        {
            _list = new ObservableCollection<AreaColor>();
            _findfastcolor = null;
            _findfastid = null;
            _colordic = null;
        }

        #endregion//Ctor

        #region Props

        public ObservableCollection<AreaColor> List { get { return _list; } set { _list = value; RaisePropertyChanged(null); } }
        
        #endregion //Props
        
        #region Fields
        
        [NonSerialized] private Dictionary<Color, AreaColor> _findfastcolor;
        [NonSerialized] private Dictionary<int, AreaColor> _findfastid;
        [NonSerialized] private Dictionary<Color, bool> _colordic;
        
        #endregion//Fields

        #region SearchMethods
        
        public AreaColor FindByColor(Color color)
        {
            AreaColor a;
            _findfastcolor.TryGetValue(color, out a);
            return a;
        }
         
        public AreaColor FindByIndex(int index)
        {
            AreaColor a;
            _findfastid.TryGetValue(index,out a);
            return a;
        }

        public void InitializeSeaches()
        {
            _findfastid = new Dictionary<int, AreaColor>();
            _colordic = new Dictionary<Color, bool>();
            _findfastcolor = new Dictionary<Color, AreaColor>();
            
            foreach (var area in List)
            {
                try
                {
                    _colordic.Add(area.Color, true);
                    _findfastcolor.Add(area.Color, area);
                    _findfastid.Add(area.Index, area);

                }
                catch (Exception)
                {
                }
                
            }
        }

        #endregion //SearchMethods

        
        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            Serialize(() => List, info);
        }

        protected CollectionAreaColor(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            List = new ObservableCollection<AreaColor>(Deserialize(() => List, info));
        }
    }
}