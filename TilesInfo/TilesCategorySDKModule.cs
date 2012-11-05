﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using EssenceUDK.Platform.DataTypes;
using EssenceUDK.Platform;
using EssenceUDK.TilesInfo.Components;
using EssenceUDK.TilesInfo.Components.Base;
using EssenceUDK.TilesInfo.Components.Enums;
using EssenceUDK.TilesInfo.Components.MultiStruct;
using EssenceUDK.TilesInfo.Components.Tiles;
using EssenceUDK.TilesInfo.Factories;

namespace EssenceUDK.TilesInfo
{
    public class TilesCategorySDKModule : NotificationObject
    {
        #region private fields

        private UODataManager _data;
        
        private bool _checkText;

        private ObservableCollection<Tile> _listTmpTile;

        private ObservableCollection<TileStyle> _listTmpStyle;

        private MultiCollection _multiCollection;

        private TxtFile _textFactory;

        #endregion

        #region static fields

        private static readonly char[] Separator = new[]{'-'};
        
        private static readonly char[] Separator2 = new[] {' '};
        
        #endregion //static Fields

        #region static fields
        
        public static UODataManager Install;
        
        public static ObservableCollection<ObservableCollection<TileCategory>> Categories;
        
        #endregion //static fields

        #region public fields
        
        public IList<Factory> Factories; 
        
        public static SuppInfo Supp;
        
        public Walls Walls;
        
        public Misc Misc;
        
        public Roofs Roofs;
        
        public Floors Floors;


        #endregion //public fields

        #region Props

        public ObservableCollection<ObservableCollection<TileCategory>> Cats { get { return Categories; } set { Categories = value; RaisePropertyChanged(()=>Cats); } } 

        public ObservableCollection<TileCategory> WallsCat { get { return Categories[0]; } set { Categories[0] = value; RaisePropertyChanged(()=>WallsCat);} }
        
        public ObservableCollection<TileCategory> MiscCat { get { return Categories[1]; } set { Categories[1] = value; RaisePropertyChanged(()=>MiscCat);} }
        
        public ObservableCollection<TileCategory> RoofCat { get { return Categories[2]; } set { Categories[2] = value; RaisePropertyChanged(()=>RoofCat);} }
        
        public ObservableCollection<TileCategory> FloorsCat { get { return Categories[3]; } set { Categories[3] = value; RaisePropertyChanged(()=>FloorsCat);} }
        
        public ObservableCollection<TileCategory> Token { get { return Categories[4]; } set { Categories[4] = value; RaisePropertyChanged(()=>Token);} }

        public Boolean CheckFromTxt { get { return _checkText; } set { _checkText = value; RaisePropertyChanged(()=>CheckFromTxt); } }

        public ObservableCollection<Tile> ListTmpTile { get { return _listTmpTile; } set { _listTmpTile = value; RaisePropertyChanged(()=>ListTmpTile); } }

        public ObservableCollection<TileStyle> ListTmpStyle { get { return _listTmpStyle; } set { _listTmpStyle = value;RaisePropertyChanged(()=>ListTmpStyle); } }

        public MultiCollection Multi { get { return _multiCollection; } set { _multiCollection = value; RaisePropertyChanged(()=>Multi); } }

        #endregion //props

        #region ctor

        public TilesCategorySDKModule(UODataManager install)
        {
            
            Install = install;
            Factories = new List<Factory>();
            Supp = new SuppInfo(install);
            Supp.Populate();
            Walls = new Walls(install);
            Misc = new Misc(install);
            Roofs = new Roofs(install);
            Floors = new Floors(install);
            Factories.Add(Walls);
            Factories.Add(Misc);
            Factories.Add(Roofs);
            Factories.Add(Floors);
            Categories = new ObservableCollection<ObservableCollection<TileCategory>>();
            CheckFromTxt = true;
            _listTmpStyle = new ObservableCollection<TileStyle>();
            _listTmpTile = new ObservableCollection<Tile>();


        }
        #endregion

        #region Methods

        #region Populate 

        public void Populate()
        {
            if(CheckFromTxt)
            {
                foreach (var factory in Factories)
                {

                    factory.Populate();
                }
            }
            foreach (var fact in Factories)
            {
                Categories.Add(fact.Categories);
            }


            var walls = _data.GetItemTile().Where(itemData => itemData.Flags.HasFlag(TileFlag.Wall) && itemData.Height == 20 && !itemData.Name.Contains("arc")&&!itemData.Flags.HasFlag(TileFlag.Window)&& !itemData.Flags.HasFlag(TileFlag.Door)).ToList();
            var windows = _data.GetItemTile().Where(itemData => itemData.Flags.HasFlag(TileFlag.Wall) && itemData.Height == 20 && itemData.Flags.HasFlag(TileFlag.Window)).ToList();
            var halfWalls = _data.GetItemTile().Where(itemData => itemData.Flags.HasFlag(TileFlag.Wall) && itemData.Height == 10 && !itemData.Flags.HasFlag(TileFlag.Window)).ToList();
            var quarterWalls = _data.GetItemTile().Where(itemData => itemData.Flags.HasFlag(TileFlag.Wall) && itemData.Height == 5 && !itemData.Flags.HasFlag(TileFlag.Window)).ToList();
            var archs = _data.GetItemTile().Where(itemData => itemData.Flags.HasFlag(TileFlag.Wall) && itemData.Name.Contains("arc")).ToList();
            var roof = _data.GetItemTile().Where(itemData => itemData.Flags.HasFlag(TileFlag.Roof)).ToList();
            var floors = _data.GetItemTile().Where(itemData => itemData.Flags.HasFlag(TileFlag.Surface)).ToList();

            var wallCategory = new TileCategory(){Name = "wall",TypeTile = TypeTile.Wall};
            var windowCategory = new TileCategory() {Name = "window", TypeTile = TypeTile.Wall};
            var halfCategory = new TileCategory() {Name = "half",TypeTile = TypeTile.Wall};
            var quarterCategory = new TileCategory() {Name = "quarter",TypeTile = TypeTile.Wall};
            var arcsCategory = new TileCategory() { Name = "arch" ,TypeTile = TypeTile.Wall};
            var roofCategory = new TileCategory() {Name = "roof",TypeTile = TypeTile.Roofs};
            var floorcategory = new TileCategory() {Name = "floor", TypeTile = TypeTile.Floor};

            var lists = Misc.Categories.Union(Walls.Categories).Union(Roofs.Categories).Union(Floors.Categories).ToList();
            
            if(!CheckFromTxt)
                lists.Clear();

            var listcat = new ObservableCollection<TileCategory>();

            FullEmptyCategoriesTxTChecked(lists, walls, wallCategory);
            FullEmptyCategoriesTxTChecked(lists, quarterWalls, quarterCategory);
            FullEmptyCategoriesTxTChecked(lists, halfWalls, halfCategory);
            FullEmptyCategoriesTxTChecked(lists, windows, windowCategory);
            FullEmptyCategoriesTxTChecked(lists, archs, arcsCategory);
            FullEmptyCategoriesTxTChecked(lists,roof,roofCategory);
            FullEmptyCategoriesTxTChecked(lists,floors,floorcategory);

            foreach (var s in wallCategory.List)
            {
                var category = new TileCategory() {Name = s.Name};
                category.AddStyle(s);
                
                var half = Selector(halfCategory, s);
                var quarter = Selector(quarterCategory, s);
                var window = Selector(windowCategory, s);
                var arch = Selector(arcsCategory, s);
                var r = Selector(roofCategory, s);
                
                if(half!= null)
                    category.AddStyle(half);
                if(quarter != null)
                    category.AddStyle(quarter);
                if(window!=null)
                    category.AddStyle(window);
                if(arch!=null)
                    category.AddStyle(arch);
                if(r!=null)
                    category.AddStyle(r);
               
                listcat.Add(category);
                
            }

            listcat.Add(RemoveDuplicates(listcat,windowCategory));
            listcat.Add(RemoveDuplicates(listcat, roofCategory));
            listcat.Add(RemoveDuplicates(listcat, halfCategory));
            listcat.Add(RemoveDuplicates(listcat, quarterCategory));
            listcat.Add(RemoveDuplicates(listcat, arcsCategory));
            
            Categories.Add(listcat);
        }

        private void FullEmptyCategoriesTxTChecked(IEnumerable<TileCategory> list, IEnumerable<ModelItemData> datalist, TileCategory tileCategory)
        {
            var style = new TileStyle();
            foreach (var itemData in datalist)
            {
                Tile tile;
                uint number = itemData.TileId;

                var tiles = from cat in list
                            let t = cat.FindTile(number)
                            where t != null
                            select t;
                tile = tiles.FirstOrDefault();
                if (tile == null)
                {
                    string name = string.Format("{0}-{1}", tileCategory.Name,
                                                    itemData.Name.Replace(tileCategory.Name, "").Split(Separator2,
                                                                                                       StringSplitOptions
                                                                                                           .
                                                                                                           RemoveEmptyEntries)
                                                        .FirstOrDefault());

                    if (string.IsNullOrEmpty(style.Name) || style.Name != name)
                    {
                        if (style.List.Count > 0 && tileCategory.FindStyleByName(style.Name) == null)
                        {
                            tileCategory.AddStyle(style);
                        }


                        var st2 = tileCategory.FindStyleByName(name);
                        style = st2 ?? new TileStyle { Name = name };
                    }
                    switch (tileCategory.TypeTile)
                    {
                        case TypeTile.Wall:
                            {
                                style.AddTile(new TileWall() { Id = number, Name = itemData.Name });
                                break;
                            }
                        case TypeTile.Roofs:
                            {
                                style.AddTile(new TileRoof() { Id = number, Name = itemData.Name });
                                break;
                            }
                        case TypeTile.Floor:
                            {
                                style.AddTile(new TileFloor() { Id = number, Name = itemData.Name });
                                break;
                            }
                        default:
                            {
                                style.AddTile(new Tile { Id = number, Name = itemData.Name });
                                break;
                            }
                    }

                }
            }
            tileCategory.AddStyle(style);
        }

        private TileStyle Selector(TileCategory tileCategory, TileStyle s)
        {
            var style = from sh in tileCategory.List
                        where sh.Name.Split(Separator, StringSplitOptions.RemoveEmptyEntries).Last() == s.Name.Split(Separator, StringSplitOptions.RemoveEmptyEntries).Last()
                        select sh;
            return style.FirstOrDefault();
        }

        private IEnumerable<TileStyle> StylesSelector(IEnumerable<TileCategory> listcat, TileCategory category)
        {
            return (from cat in listcat
                    from s in category.List
                    where cat.FindStyleByName(s.Name) != null
                    select s).ToList();
        }

        private TileCategory RemoveDuplicates(IEnumerable<TileCategory> listcat, TileCategory cat)
        {
            var styles = StylesSelector(listcat, cat);

            foreach (TileStyle tileStyle in styles)
            {
                cat.List.Remove(tileStyle);
            }
            return cat;
        }

        #endregion //populate

        #region Persistance

        public void Save(string where)
        {
            using (var mem = new MemoryStream())
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(mem, Categories);
                using (var file = new FileStream(where, FileMode.Create))
                {
                    mem.WriteTo(file);
                }
            }
        }

        public void SaveXml(string where)
        {
            using (var file = new FileStream(where, FileMode.Create))
            {
                using (var mem = new MemoryStream())
                {
                    var serializer = new XmlSerializer(typeof(ObservableCollection<ObservableCollection<TileCategory>>));
                    using (var xmltextwriter = new XmlTextWriter(mem, Encoding.UTF8))
                    {
                        xmltextwriter.Formatting = Formatting.Indented;
                        serializer.Serialize(xmltextwriter, Categories);
                        mem.WriteTo(file);
                    }
                }
            }
        }

        public void LoadXml(string from)
        {
            using (var file = new FileStream(from, FileMode.Open))
            {
                var serializer = new XmlSerializer(typeof(ObservableCollection<ObservableCollection<TileCategory>>));
                using (var xmlTextReader = new XmlTextReader(file))
                {
                    Categories.Clear();
                    Categories = (ObservableCollection<ObservableCollection<TileCategory>>)serializer.Deserialize(xmlTextReader);
                }
            }
        }

        public void Load(string from)
        {
            var binaryformatter = new BinaryFormatter();

            using (var file = new FileStream(from, FileMode.Open))
            {
                Categories.Clear();
                Categories = (ObservableCollection<ObservableCollection<TileCategory>>)binaryformatter.Deserialize(file);
            }
        }

        public void SaveCategories(string where, ObservableCollection<TileCategory> categories)
        {
            using (var mem = new MemoryStream())
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(mem, categories);
                using (var file = new FileStream(where, FileMode.Create))
                {
                    mem.WriteTo(file);
                }
            }
        }

        public ObservableCollection<TileCategory> LoadCategories(string from)
        {
            ObservableCollection<TileCategory> categories;
            using (var file = new FileStream(from, FileMode.Open))
            {
                var binaryformatter = new BinaryFormatter();

                categories = (ObservableCollection<TileCategory>)binaryformatter.Deserialize(file);
            }
            return categories;
        }

        public void SaveXmlCategory(string where, ObservableCollection<TileCategory> categories)
        {
            using (var mem = new MemoryStream())
            {
                var serializer = new XmlSerializer(typeof(ObservableCollection<TileCategory>));

                using (var xmlwriter = new XmlTextWriter(mem, Encoding.UTF8))
                {
                    xmlwriter.Formatting = Formatting.Indented;
                    serializer.Serialize(xmlwriter, categories);
                    using (var file = new FileStream(where, FileMode.Create))
                    {
                        mem.WriteTo(file);
                    }
                }
            }
        }

        public ObservableCollection<TileCategory> LoadXmlCategories(string from)
        {
            ObservableCollection<TileCategory> categories;
            using (var file = new FileStream(from, FileMode.Open))
            {
                var xmlReader = new XmlTextReader(file);
                using (xmlReader)
                {
                    var serializer = new XmlSerializer(typeof(ObservableCollection<TileCategory>));
                    try
                    {
                        categories = (ObservableCollection<TileCategory>)serializer.Deserialize(xmlReader);
                    }
                    catch (Exception e)
                    {
                        return null;
                    }
                }
            }
            return categories;
        }
        #endregion //persistence

        #region Txt file handling
        
        public void TakeFromTXTFile(string locationFile)
        {
            _textFactory = new TxtFile(locationFile,_data);
            _textFactory.Populate();
            Multi = _textFactory._multi;
        }


        #endregion //Txt file handling

        #endregion
    }
}
