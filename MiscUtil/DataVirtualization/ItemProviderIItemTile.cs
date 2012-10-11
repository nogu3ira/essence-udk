using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using EssenceUDK.Platform;
using EssenceUDK.Platform.DataTypes;

namespace MapMakerApplication.Utilities.DataVirtualization
{
    public class ItemProviderModelItemData : IItemsProvider<ModelItemData>
    {
        #region Declarations

        private IList<ModelItemData> _list;
        private int _count;
        
        #endregion // Declarations

        #region Ctor

        public ItemProviderModelItemData(IList<ModelItemData> list)
        {
            _list = list;
            _count = list.Count;
        }

        public ItemProviderModelItemData(IList<ModelItemData> list, int count)
        {
            _list = list;
            _count = _list.Count;
        } 

        #endregion

        #region IItemsProvider

        public int FetchCount()
        {
            return _count;
        }

        public IList<ModelItemData> FetchRange(int startIndex, int count)
        {
            IList<ModelItemData> items = new List<ModelItemData>();
            for (int i = 0; startIndex + i < _list.Count && i < count; i++)
            {
                items.Add(_list[startIndex + i]);
            }

            return items;
        } 

        #endregion
    }


    public class ItemProviderModelLandData : IItemsProvider<ModelLandData>
    {
        #region Declarations

        private readonly IList<ModelLandData> _list;
        private ObservableCollection<ModelLandData> _collection;  
        
        #endregion

        #region Ctor

        public ItemProviderModelLandData(IList<ModelLandData> list)
        {
            _list = list;
        }

        public ItemProviderModelLandData(IList<ModelLandData> list, int count)
        {
            _list = list;
        } 
        #endregion

        #region IItemsProvider
        public int FetchCount()
        {
            //return _list.Count;
            return _list.Count;
        }

        public IList<ModelLandData> FetchRange(int startIndex, int count)
        {
            var collection = new ObservableCollection<ModelLandData>();
            for (int i = startIndex; i < startIndex+count && startIndex+count < _list.Count; i++)
            {
                collection.Add(_list[i]);
            }
            return collection;
        } 
        #endregion
    }
}
