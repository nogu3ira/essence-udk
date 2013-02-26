﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using EssenceUDK.MapMaker.Elements.BaseTypes;
using EssenceUDK.MapMaker.Elements.Textures.TextureArea;

namespace EssenceUDK.MapMaker.Elements.Textures
{
    [Serializable]
    public class CollectionAreaTexture : NotificationObject, IContainerSet
    {
        private ObservableCollection<AreaTextures> _list;
        public ObservableCollection<AreaTextures> List { get { return _list; } set { _list = value; RaisePropertyChanged(()=>List); } }
        
        [XmlIgnore]
        [NonSerialized] public Dictionary<int, AreaTextures> _fast; 
        public CollectionAreaTexture()
        {
            List = new ObservableCollection<AreaTextures>();
        }

        #region Search Methods

        public AreaTextures FindByIndex(int id )
        {
            AreaTextures text = null;
            if(_fast!=null)
            _fast.TryGetValue(id,out text);
            return text;
        }
        
        #endregion

        public void InitializeSeaches()
        {
            _fast = new Dictionary<int, AreaTextures>();
            foreach (AreaTextures texturese in List)
            {
                _fast.Add(texturese.Index, texturese);
            }
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            Serialize(() => List, info);
        }

        protected CollectionAreaTexture(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            List = new ObservableCollection<AreaTextures>(Deserialize(() => List, info));
        }
    }
}
