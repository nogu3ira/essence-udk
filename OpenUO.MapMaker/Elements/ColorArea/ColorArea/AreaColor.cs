using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using OpenUO.MapMaker.Elements.BaseTypes;
using OpenUO.MapMaker.Elements.ColorArea.ColorMountains;
using OpenUO.MapMaker.Elements.Items.ItemCoast;
using OpenUO.MapMaker.Elements.Items.ItemText;
using OpenUO.MapMaker.Elements.Items.ItemsTransition;
using OpenUO.MapMaker.Elements.Textures.TextureArea;
using OpenUO.MapMaker.Elements.Textures.TextureTransition;
using OpenUO.MapMaker.Elements.Textures.TexureCliff;
using System.Linq;

namespace OpenUO.MapMaker.Elements.ColorArea.ColorArea
{
    [Serializable]
    public class AreaColor : NotificationObject
    {
        

        

        #region Declaration

        private bool _automaticMode;
        private int _textureIndex, _index, _min, _max, _indexTextureTop, _indexColorTop;
        private String _name;
        private TypeColor _typeColor;
        private Color _color, _colorMountain;

        private AreaTransitionItemCoast _coast;
        private ObservableCollection<AreaTransitionItem> _transitionItems;
        private AreaItems _items;
        private ObservableCollection<AreaTransitionTexture> _transitionTexture;
        private ObservableCollection<AreaTransitionCliffTexture> _transitionCliff;
        private ObservableCollection<CircleMountain> _list;
        private Dictionary<Color, AreaTransitionTexture> _transitionTextureFinding;
        private Dictionary<Color, AreaTransitionItem> _transitionItemsFinding;
        public bool Initialized { get; private set; }
            #endregion

        #region Props

        #region ColorArea Generical part

        [Description("Index of Textures"), Category("Area Color"), DisplayName("Texture Index")]
        public int TextureIndex { get { return _textureIndex; } set { _textureIndex = value; RaisePropertyChanged(() => TextureIndex); } }

        [Description("Index of Color"), Category("Area Color"), DisplayName("Id")]
        public int Index { get { return _index; } set { _index = value; RaisePropertyChanged(() => Index); } }

        [Description("Mimimum of Randon Height in map making"), Category("Map Making References"), DisplayName("Minimal Height")]
        public int Min { get { return _min; } set { _min = value; RaisePropertyChanged(() => Min); } }

        [Description("Maximum of Randon Height in map making"), Category("Map Making References"), DisplayName("Maximum Height")]
        public int Max { get { return _max; } set { _max = value; RaisePropertyChanged(() => Max); } }

        [Description("Name of the Color Area"), Category("Area Color"), DisplayName("Name")]
        public String Name { get { return _name; } set { _name = value; RaisePropertyChanged(() => Name); } }

        [Description("Color of the Area Color"), Category("Area Color"), DisplayName("Color")]
        public Color Color { get { return _color; } set { _color = value; RaisePropertyChanged(() => Color); } }

        [Description("It describes what Area Color Represents"), Category("Area Color"), DisplayName("Type of Area")]
        public TypeColor Type { get { return _typeColor; } set { _typeColor = value; RaisePropertyChanged(() => Type); } }

        #endregion

        #region Mountain Part

        [Description("It's the List of Circles of automatic grown in Map Making"), Category("Rocks"), DisplayName("Auto Circles")]
        public ObservableCollection<CircleMountain> List { get { return _list; } set { _list = value; RaisePropertyChanged(() => List); } }

        [Description("It refers to the Area Color at the top of the Rock"), Category("Rocks"), DisplayName("Color Top")]
        public Color ColorTopMountain { get { return _colorMountain; } set { _colorMountain = value; RaisePropertyChanged(() => ColorTopMountain); } }

        [Description("It refers to the Area Color at the top of the Rock"), Category("Rocks"), DisplayName("Index Top")]
        public int IndexColorTopMountain
        {
            get { return _indexColorTop; }
            set
            {
                _indexColorTop = value;
                ColorTopMountain = MakeMapSDK.Colors[value];
                RaisePropertyChanged(() => IndexColorTopMountain);
            }
        }

        [Description("It refers to the Texture used at the Top of the Rock"), Category("Rocks"), DisplayName("Index Texture Top")]
        public int IndexTextureTop
        {
            get { return _indexTextureTop; }
            set
            {
                _indexTextureTop = value;
                RaisePropertyChanged(() => _indexTextureTop);
            }
        }

        [Description("It means if the automatic grown of the mountains is set or not"), Category("Rocks"), DisplayName("Automatic Rock Growning")]
        public bool ModeAutomatic { get { return _automaticMode; } set { _automaticMode = value; RaisePropertyChanged(() => ModeAutomatic); } }

        #endregion

        #region Added Collections

        public AreaTransitionItemCoast Coasts { get { return _coast; } set { _coast = value; } }

        public ObservableCollection<AreaTransitionItem> TransitionItems { get { return _transitionItems; } set { _transitionItems = value; } }

        public AreaItems Items { get { return _items; } set { _items = value; } }

        public ObservableCollection<AreaTransitionTexture> TextureTransitions { get { return _transitionTexture; } set { _transitionTexture = value; } }

        public ObservableCollection<AreaTransitionCliffTexture> TransitionCliffTextures { get { return _transitionCliff; } set { _transitionCliff = value; } }

        #endregion

        #endregion //props

        #region Ctor

        public AreaColor()
        {
            Color = Colors.Black;
            TextureIndex = 0;
            Index = 0;
            Min = 0;
            Max = 0;
            Name = "";
            Type = TypeColor.None;
            _items = new AreaItems();
            _transitionTexture = new ObservableCollection<AreaTransitionTexture>();
            _transitionItems = new ObservableCollection<AreaTransitionItem>();
            _coast = new AreaTransitionItemCoast();
            _transitionCliff = new ObservableCollection<AreaTransitionCliffTexture>();
        }

        #endregion//ctor

        //#region Implementation of IComparable

        //public int CompareTo(AreaColor other)
        //{
        //    if (other.TextureIndex == TextureIndex)
        //        return 0;
        //    else
        //    {
        //        return -1;
        //    }
        //}

        //#endregion //Implementation of IComparable

        //#region Implementation of IEquatable

        //public override bool Equals(object obj)
        //{
        //    if (ReferenceEquals(null, obj)) return false;
        //    if (ReferenceEquals(this, obj)) return true;
        //    if (obj.GetType() != this.GetType()) return false;
        //    return Equals((AreaColor)obj);
        //}



        //public bool Equals(AreaColor other)
        //{
        //    if (ReferenceEquals(null, other)) return false;
        //    if (ReferenceEquals(this, other)) return true;
        //    return _automaticMode.Equals(other._automaticMode) && _textureIndex == other._textureIndex && _index == other._index && _min == other._min && _max == other._max && _indexTextureTop == other._indexTextureTop && _indexColorTop == other._indexColorTop && string.Equals(_name, other._name) && _typeColor.Equals(other._typeColor) && _color.Equals(other._color) && _colorMountain.Equals(other._colorMountain) && Equals(_coast, other._coast) && Equals(_transitionItems, other._transitionItems) && Equals(_items, other._items) && Equals(_transitionTexture, other._transitionTexture) && Equals(_transitionCliff, other._transitionCliff) && Equals(_list, other._list) && Equals(_transitionTextureFinding, other._transitionTextureFinding) && Equals(_transitionItemsFinding, other._transitionItemsFinding) && Initialized.Equals(other.Initialized);
        //}


        //public override int GetHashCode()
        //{
        //    unchecked
        //    {
        //        int hashCode = _automaticMode.GetHashCode();
        //        hashCode = (hashCode * 397) ^ _textureIndex;
        //        hashCode = (hashCode * 397) ^ _index;
        //        hashCode = (hashCode * 397) ^ _min;
        //        hashCode = (hashCode * 397) ^ _max;
        //        hashCode = (hashCode * 397) ^ _indexTextureTop;
        //        hashCode = (hashCode * 397) ^ _indexColorTop;
        //        hashCode = (hashCode * 397) ^ (_name != null ? _name.GetHashCode() : 0);
        //        hashCode = (hashCode * 397) ^ _typeColor.GetHashCode();
        //        hashCode = (hashCode * 397) ^ _color.GetHashCode();
        //        hashCode = (hashCode * 397) ^ _colorMountain.GetHashCode();
        //        hashCode = (hashCode * 397) ^ (_coast != null ? _coast.GetHashCode() : 0);
        //        hashCode = (hashCode * 397) ^ (_transitionItems != null ? _transitionItems.GetHashCode() : 0);
        //        hashCode = (hashCode * 397) ^ (_items != null ? _items.GetHashCode() : 0);
        //        hashCode = (hashCode * 397) ^ (_transitionTexture != null ? _transitionTexture.GetHashCode() : 0);
        //        hashCode = (hashCode * 397) ^ (_transitionCliff != null ? _transitionCliff.GetHashCode() : 0);
        //        hashCode = (hashCode * 397) ^ (_list != null ? _list.GetHashCode() : 0);
        //        hashCode = (hashCode * 397) ^ Initialized.GetHashCode();
        //        return hashCode;
        //    }
        //}

        //#endregion //Implementation of IEquatable

        #region Override

        

        //public override string ToString()
        //{
        //    return Name;
        //}

        //public override string this[string columnName]
        //{
        //    get
        //    {
        //        string result = null;

        //        switch (columnName)
        //        {
        //            #region General Part
        //            case "Index":
        //                {
        //                    if (MakeMapSDK.Colors.Keys.Contains(_index))
        //                    {
        //                        result = "Index MUST be a not used Index";
        //                    }
        //                }
        //                break;
        //            case "Min":
        //                {
        //                    if (_min < -128 || _min > 127)
        //                    {
        //                        result = "Min MUST be between -128 and 127";
        //                    }
        //                }
        //                break;
        //            case "Max":
        //                {
        //                    if (_max < -128 || _max > 127)
        //                    {
        //                        result = "Max MUST be between -128 and 127";
        //                    }
        //                }
        //                break;
        //            case "Color":
        //                {
        //                    if (MakeMapSDK.Colors.ContainsValue(_color))
        //                    {
        //                        result = "Color MUST be an unused one";
        //                    }
        //                }
        //                break;

        //            #endregion //General Part

        //            #region Mountain Part

        //            case "ColorTopMountain":
        //                {
        //                    if (!MakeMapSDK.Colors.ContainsValue(_colorMountain))
        //                    {
        //                        result = "Color of the mountain MUST be an used one";
        //                    }
        //                }
        //                break;
        //            case "IndexTextureTop":
        //                {
        //                    if (!MakeMapSDK.Colors.ContainsKey(_indexTextureTop))
        //                    {
        //                        result = "Color of the mountain MUST be an used one";
        //                    }
        //                }
        //                break;

        //            #endregion //Mountain Part
        //        }
        //        return result;
        //    }
        //}

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            Serialize(() => Index, info);
            Serialize(() => Min, info);
            Serialize(() => Max, info);
            //Serialize(() => Type, info);
            info.AddValue("Type", (int)_typeColor);
            Serialize(() => TextureIndex, info);
            Serialize(() => Name, info);
            Serialize(() => Color, info);
            Serialize(() => List, info);
            Serialize(() => ColorTopMountain, info);
            Serialize(() => IndexColorTopMountain, info);
            Serialize(() => IndexTextureTop, info);
            Serialize(() => ModeAutomatic, info);
            Serialize(() => Coasts, info);
            Serialize(() => TransitionItems, info);
            Serialize(() => Items, info);
            Serialize(() => TextureTransitions, info);
            Serialize(() => TransitionCliffTextures, info);
        }


        #endregion //Override

        #region ISerializable Ctor

        protected AreaColor(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            Index = Deserialize(() => Index, info);
            Min = Deserialize(() => Min, info);
            Max = Deserialize(() => Max, info);
            //Type = Deserialize(() => Type, info);
            _typeColor = (TypeColor)info.GetInt32("Type");
            TextureIndex = Deserialize(() => TextureIndex, info);
            Name = Deserialize(() => Name, info);
            Color = Deserialize(() => Color, info);
            List = new ObservableCollection<CircleMountain>(Deserialize(() => List, info));
            ColorTopMountain = Deserialize(() => ColorTopMountain, info);
            _indexColorTop = Deserialize(() => IndexColorTopMountain, info);
            IndexTextureTop = Deserialize(() => IndexTextureTop, info);
            ModeAutomatic = Deserialize(() => ModeAutomatic, info);
            Coasts = Deserialize(() => Coasts, info);
            TransitionItems = new ObservableCollection<AreaTransitionItem>(Deserialize(() => TransitionItems, info));
            Items = Deserialize(() => Items, info);
            TextureTransitions = new ObservableCollection<AreaTransitionTexture>(Deserialize(() => TextureTransitions, info));
            TransitionCliffTextures = new ObservableCollection<AreaTransitionCliffTexture>(Deserialize(() => TransitionCliffTextures, info));
        }

        #endregion //ISerializable Ctor

        public void InizializeSearches()
        {
            Initialized = true;
            _transitionTextureFinding = new Dictionary<Color, AreaTransitionTexture>();
            _transitionItemsFinding = new Dictionary<Color, AreaTransitionItem>();
            foreach (var areaTransitionTexture in TextureTransitions)
            {
                try
                {
                    _transitionTextureFinding.Add(areaTransitionTexture.ColorTo, areaTransitionTexture);

                }
                catch (Exception)
                {
                }
            }
            foreach (var areaTransitionItem in TransitionItems)
            {
                try
                {
                    _transitionItemsFinding.Add(areaTransitionItem.ColorTo, areaTransitionItem);
                }
                catch (Exception)
                {
                }
            }

        }

        public AreaTransitionTexture FindTransitionTexture(Color color)
        {
            AreaTransitionTexture found;
            _transitionTextureFinding.TryGetValue(color, out found);
            return found;
        }

        public AreaTransitionItem FindTransationItemByColor(Color color)
        {
            AreaTransitionItem transitionItem;
            _transitionItemsFinding.TryGetValue(color, out transitionItem);
            return transitionItem;
        }

       

    }

    public enum TypeColor
    {
        None,
        Water,
        Moutains,
        Land,
        Cliff,
        Special,
        WaterCoast
    }
}
