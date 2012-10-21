using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

namespace OpenUO.MapMaker.Elements.BaseTypes.ComplexTypes
{
    [Serializable]
    public class CollectionItem : NotificationObject
    {
        private ObservableCollection<int> _list;
        public ObservableCollection<int> List { get { return _list; } set { _list = value; RaisePropertyChanged(()=>List); } } 
        public CollectionItem()
        {
            List = new ObservableCollection<int>();
        }

        public void Add(int element)
        {
            List.Add(element);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Serialize(()=>List,info);
        }

        protected CollectionItem(SerializationInfo info,StreamingContext context)
        {
           List= new ObservableCollection<int>(Deserialize(() => List, info));
        }
        
    }
}
