using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using EssenceUDK.MapMaker.Elements.BaseTypes;
using EssenceUDK.MapMaker.Elements.Textures.TextureTransition;

namespace EssenceUDK.MapMaker.Elements.Textures
{
    [Serializable]
    public class CollectionAreaTransitionTexture : NotificationObject, IContainerSet
    {
        private ObservableCollection<AreaTransitionTexture> _list;
        #region Props

        public ObservableCollection<AreaTransitionTexture> List { get { return _list; } set { _list = value; RaisePropertyChanged(()=>List); } }
        
        #endregion
        
        #region Fields
        
        //[NonSerialized] private Dictionary<Color, bool> _dictionaryColorTo;
        //[NonSerialized] private Dictionary<Color, bool> _dictionaryColorFrom;
        [NonSerialized] private Dictionary<int, AreaTransitionTexture> _dictionaryFindIndex;
        private bool init = false;
        #endregion

        #region Ctor

        public CollectionAreaTransitionTexture()
        {
            List = new ObservableCollection<AreaTransitionTexture>();
        }
        #endregion

        #region Search Methods

        //public IEnumerable<AreaTransitionTexture> FindFromByColor(Color color)
        //{
        //    return List.Where(text => text.ColorFrom == color);
        //}

        //public IEnumerable<AreaTransitionTexture> FindToByColor(Color color)
        //{
        //    return List.Where(text => text.ColorTo == color);
        //}

        //public bool ColorFromContains(Color color)
        //{
        //    bool answer;

        //    _dictionaryColorFrom.TryGetValue(color, out answer);
        //    return answer;
        //}

        //public bool Contains(Color color)
        //{
        //    bool answer;

        //    _dictionaryColorFrom.TryGetValue(color, out answer);
        //    if (answer)
        //        return true;

        //    _dictionaryColorTo.TryGetValue(color, out answer);
        //    return answer;

        //}


        public AreaTransitionTexture FindById(int id)
        {
            AreaTransitionTexture result;
            _dictionaryFindIndex.TryGetValue(id, out result);
            return result;
        }
        
        #endregion

        #region IContainerSet
        
        public void InitializeSeaches()
        {
            if (init)
                return;
            //_dictionaryColorTo  = new Dictionary<Color, bool>();
            //_dictionaryColorFrom = new Dictionary<Color, bool>();
            _dictionaryFindIndex = new Dictionary<int, AreaTransitionTexture>();
            foreach (var textureSmooth in List)
            {
                //try
                //{
                //    _dictionaryColorTo.Add(textureSmooth.ColorTo, true);
                //}
                //catch (Exception)
                //{
                //}
                //try
                //{
                //    _dictionaryColorFrom.Add(textureSmooth.ColorFrom,true);
                //}
                //catch (Exception)
                //{
                //}
                try
                {
                    _dictionaryFindIndex.Add(textureSmooth.TextureIdTo,textureSmooth);
                }
                catch (Exception)
                {
                }
            }
            init = true;
        }
        #endregion //IContainerSet

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            Serialize(() => List, info);
        }

        protected CollectionAreaTransitionTexture(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            List = new ObservableCollection<AreaTransitionTexture>(Deserialize(() => List, info));
        }

    }
        
}
