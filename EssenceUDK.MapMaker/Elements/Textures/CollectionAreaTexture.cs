using System;
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
        private ObservableCollection<TextureArea.AreaTextures> _list;
        public ObservableCollection<TextureArea.AreaTextures> List { get { return _list; } set { _list = value; RaisePropertyChanged(()=>List); } }
        
        [XmlIgnore]
        [NonSerialized] private Dictionary<int, TextureArea.AreaTextures> _fast; 
        public CollectionAreaTexture()
        {
            List = new ObservableCollection<TextureArea.AreaTextures>();
        }

        #region Search Methods

        public TextureArea.AreaTextures FindByIndex(int id )
        {
            TextureArea.AreaTextures text;
            _fast.TryGetValue(id,out text);
            return text;
        }
        
        #endregion

        public void InitializeSeaches()
        {
            _fast = new Dictionary<int, TextureArea.AreaTextures>();
            foreach (TextureArea.AreaTextures texturese in List)
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
