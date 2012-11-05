using System;
using System.Collections.ObjectModel;
using EssenceUDK.MapMaker.Elements.BaseTypes;

namespace EssenceUDK.MapMaker.Elements.Textures.TextureArea
{
    [Serializable]
    public class AreaTextures : NotificationObject 
    {
        private int _index;
        private string _name;
        private ObservableCollection<int> _list;

        #region Props
        public int Index { get { return _index; } set { _index = value; RaisePropertyChanged(()=>Index); } }
        public ObservableCollection<int> List { get { return _list; } set { _list = value; RaisePropertyChanged(()=>List); } }
        public String Name { get { return _name; } set { _name = value; RaisePropertyChanged(() => Name); } }

        #endregion //Props

        #region Ctor
        public AreaTextures()
        {
            Index = 0;
            List = new ObservableCollection<int>();
            Name = "";
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            Serialize(()=>List,info);
            Serialize(()=>Name,info);
            Serialize(()=>Index,info);
        }

        protected AreaTextures(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            List = new ObservableCollection<int>(Deserialize(()=>List,info));
            Name = Deserialize(() => Name, info);
            Index = Deserialize(() => Index, info);
        }
        #endregion ctor
    }
}
