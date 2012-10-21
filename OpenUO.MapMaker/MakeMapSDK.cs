﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Text;
using System.Windows.Media;
using System.Xml.Serialization;
using MiscUtil.Conversion;
using MiscUtil.IO;
using OpenUO.MapMaker.Elements;
using OpenUO.MapMaker.Elements.BaseTypes;
using OpenUO.MapMaker.Elements.ColorArea;
using OpenUO.MapMaker.Elements.ColorArea.ColorArea;
using OpenUO.MapMaker.Elements.Items;
using OpenUO.MapMaker.Elements.Items.ItemCoast;
using OpenUO.MapMaker.Elements.Items.ItemText;
using OpenUO.MapMaker.Elements.Items.ItemsTransition;
using OpenUO.MapMaker.Elements.Textures;
using OpenUO.MapMaker.Elements.Textures.TextureTransition;
using OpenUO.MapMaker.Elements.Textures.TexureCliff;
using OpenUO.MapMaker.MapMaking;
using OpenUO.MapMaker.TextFileReading;
using OpenUO.MapMaker.TextFileReading.Factories.Colors;
using OpenUO.MapMaker.TextFileReading.Factories.Items;
using OpenUO.MapMaker.TextFileReading.Factories.Textures;
using OpenUO.MapMaker.TextFileReading.Factories2.Textures;

namespace OpenUO.MapMaker
{
    public class MakeMapSDK 
    {
        public static Dictionary<int, Color> Colors { get; set; }

        #region props

        #region Datas

        public CollectionAreaColor CollectionColorArea { get; set; }

        public CollectionAreaColor CollectionColorCoast { get; set; }

        public CollectionAreaColorMountains CollectionColorMountains { get; set; }

        public CollectionAreaItems CollectionAreaItems { get; set; }

        public CollectionAreaTransitionItemCoast CollectionAreaItemsCoasts { get; set; }

        public CollectionAreaTransitionItems CollectionAreaTransitionItems { get; set; }

        public CollectionAreaTexture CollectionAreaTexture { get; set; }

        public CollectionAreaTransitionTexture CollectionAreaTransitionTexture { get; set; }

        public CollectionAreaTransitionCliffTexture CollectionAreaTransitionCliffTexture { get; set; }

        #endregion

        #region Utilities

        public IEnumerable<int> TextureIds { get { return CollectionAreaTexture.List.Select(o => o.Index); } }

        public IEnumerable<Color> AreaColorColors { get { return CollectionColorArea.List.Select(o => o.Color); } }

        public IEnumerable<int> AreaColorIndexes { get { return CollectionColorArea.List.Select(o => o.Index); } }

        #endregion

        #region factories

        public List<Factory> Factories { get; set; }

        public FactoryColorArea FactoryColor { get; set; }

        public FactoryCoast FactoryCoast { get; set; }

        public FactoryMountains FactoryMountains { get; set; }

        public FactoryItems FactoryItems { get; set; }

        public FactoryItemCoasts FactoryItemCoasts { get; set; }

        public FactorySmoothItems FactorySmoothItems { get; set; }

        public FactoryTextureArea FactoryTextureArea { get; set; }

        public FactoryTextureSmooth FactoryTextureSmooth { get; set; }

        public FactoryCliff FactoryCliff { get; set; }

        #endregion

        #region FolderLocations

        public String FolderLocation { get; set; }
        
        #endregion

        #region Automatic Height Calculation

        public bool AutomaticMode { get; set; }

        #endregion //Automatic Height Calculation

        #region BitmapLocations

        public string BitmapLocationMap { get; set; }

        public string BitmapLocationMapZ { get; set; }

        #endregion

        #region Bitmap and Maps reader

        public BitmapReader BitMap { get; set; }

        public BitmapReader BitMapZ { get; set; }

        #endregion

        #endregion

        #region Ctor
        public MakeMapSDK(string directory)
        {
            #region initialize props

            CollectionColorArea = new CollectionAreaColor();
            CollectionColorMountains = new CollectionAreaColorMountains();

            CollectionAreaItems = new CollectionAreaItems();
            CollectionAreaItemsCoasts = new CollectionAreaTransitionItemCoast();
            CollectionAreaTransitionItems = new CollectionAreaTransitionItems();


            CollectionAreaTexture = new CollectionAreaTexture();
            CollectionAreaTransitionTexture = new CollectionAreaTransitionTexture();
            CollectionAreaTransitionCliffTexture = new CollectionAreaTransitionCliffTexture();

            #endregion

            #region initialize Factories

            Factories = new List<Factory>();

            FactoryColor = new FactoryColorArea(Path.Combine(directory, "color_area.txt"));

            Factories.Add(FactoryColor);

            FactoryCoast = new FactoryCoast(Path.Combine(directory, "color_coast.txt"));

            Factories.Add(FactoryCoast);

            FactoryMountains = new FactoryMountains(Path.Combine(directory, "color_mntn.txt"));

            Factories.Add(FactoryMountains);

            FactoryItems = new FactoryItems(Path.Combine(directory, "items.txt"));

            Factories.Add(FactoryItems);

            FactoryItemCoasts = new FactoryItemCoasts(Path.Combine(directory, "ite_tex_coast.txt"));

            Factories.Add(FactoryItemCoasts);

            FactorySmoothItems = new FactorySmoothItems(Path.Combine(directory, "items_smooth.txt"));

            Factories.Add(FactorySmoothItems);

            FactoryTextureArea = new FactoryTextureArea(Path.Combine(directory, "texture_area.txt"));

            Factories.Add(FactoryTextureArea);

            FactoryTextureSmooth = new FactoryTextureSmooth(Path.Combine(directory, "texture_smooth.txt"));

            Factories.Add(FactoryTextureSmooth);

            FactoryCliff = new FactoryCliff(Path.Combine(directory, "texture_cliff.txt"));

            Factories.Add(FactoryCliff);

            #endregion

        }
        
        public MakeMapSDK()
        {
            #region initialize props

            CollectionColorArea = new CollectionAreaColor();
            CollectionColorMountains = new CollectionAreaColorMountains();
            CollectionColorCoast = new CollectionAreaColor();

            CollectionAreaItems = new CollectionAreaItems();
            CollectionAreaItemsCoasts = new CollectionAreaTransitionItemCoast();
            CollectionAreaTransitionItems = new CollectionAreaTransitionItems();


            CollectionAreaTexture = new CollectionAreaTexture();
            CollectionAreaTransitionTexture = new CollectionAreaTransitionTexture();
            CollectionAreaTransitionCliffTexture = new CollectionAreaTransitionCliffTexture();

            #endregion
        }
        #endregion
        
        #region factories

        public void InitializeFactories(string directory)
        {
            #region initialize Factories

            FolderLocation = directory;

            Factories = new List<Factory>();

            try
            {
                FactoryColor = new FactoryColorArea(Path.Combine(directory, "color_area.txt"));
            }
            catch(Exception)
            {
                
            }
            if(FactoryColor!=null)
            Factories.Add(FactoryColor);

            try
            {
                FactoryCoast = new FactoryCoast(Path.Combine(directory, "color_coast.txt"));

            }
            catch (Exception)
            {
                
            }
            if(FactoryCoast!=null)
            Factories.Add(FactoryCoast);

            try
            {
                FactoryMountains = new FactoryMountains(Path.Combine(directory, "color_mntn.txt"));

            }
            catch (Exception)
            {
            }
            if (FactoryMountains!=null)
            Factories.Add(FactoryMountains);

            try
            {
                FactoryItems = new FactoryItems(Path.Combine(directory, "items.txt"));
            }
            catch (Exception)
            {
            }
            if (FactoryItems!=null)
            Factories.Add(FactoryItems);

            try
            {
                FactoryItemCoasts = new FactoryItemCoasts(Path.Combine(directory, "ite_tex_coast.txt"));
            }
            catch (Exception)
            {
            }
            if (FactoryItemCoasts!=null)
            Factories.Add(FactoryItemCoasts);

            try
            {
                FactorySmoothItems = new FactorySmoothItems(Path.Combine(directory, "items_smooth.txt"));

            }
            catch (Exception)
            {
            }
            if(FactorySmoothItems!=null)
            Factories.Add(FactorySmoothItems);

            try
            {
                FactoryTextureArea = new FactoryTextureArea(Path.Combine(directory, "texture_area.txt"));
            }
            catch (Exception)
            {
            }
            if (FactoryTextureArea!=null)
            Factories.Add(FactoryTextureArea);

            try
            {
                FactoryTextureSmooth = new FactoryTextureSmooth(Path.Combine(directory, "texture_smooth.txt"));
            }
            catch (Exception)
            {
            }
            if (FactoryTextureSmooth!=null)
            Factories.Add(FactoryTextureSmooth);

            try
            {
                FactoryCliff = new FactoryCliff(Path.Combine(directory, "texture_cliff.txt"));
            }
            catch (Exception)
            {
            }
            if(FactoryCliff!=null)
            Factories.Add(FactoryCliff);

            #endregion
        }

        public void Populate()
        {
            #region Textures

            if (FactoryTextureArea!=null)
            {
                FactoryTextureArea.Read();
                CollectionAreaTexture = FactoryTextureArea.Textures;
            }
            if (FactoryTextureSmooth!=null)
            {
                FactoryTextureSmooth.Read();
                CollectionAreaTransitionTexture = FactoryTextureSmooth.Smooth;
            }

            if (FactoryCliff!=null)
            {
                FactoryCliff.Read();
                CollectionAreaTransitionCliffTexture = FactoryCliff.CollectionAreaCliffs;
            }
            

            #endregion

            #region colorread

            if (FactoryColor != null)
            {
                FactoryColor.Read();
                CollectionColorArea = FactoryColor.Areas;
            }


            if (CollectionColorArea != null) CollectionColorArea.InitializeSeaches();

            if (FactoryCoast != null)
            {
                FactoryCoast.Read();
                CollectionColorCoast = FactoryCoast.Area;
            }

            if (FactoryMountains != null)
            {
                FactoryMountains.Read();
                CollectionColorMountains = FactoryMountains.Mountains;
            }

            #endregion

            #region items

            if (FactoryItems != null)
            {
                FactoryItems.Read();
                CollectionAreaItems = FactoryItems.Items;
            }

            if (FactoryItemCoasts != null)
            {
                FactoryItemCoasts.Read();
                CollectionAreaItemsCoasts = FactoryItemCoasts.CoastsAll;
            }

            if (FactorySmoothItems != null)
            {
                FactorySmoothItems.Read();
                CollectionAreaTransitionItems = FactorySmoothItems.SmoothsAll;
            }

            #endregion

            #region Merging Data
            MergingData();
            #endregion //Merging Data

        }

        private void MergingData()
        {
            #region AreaColors

            if (CollectionColorArea == null) return;

            CollectionColorArea.List.CollectionChanged += EventUpdateList;

            foreach (var area in CollectionColorArea.List)
            {
                area.Type = TypeColor.Land;
                area.PropertyChanged += EventUpdateList;
            }

            CollectionColorArea.InitializeSeaches();

            if (CollectionColorMountains!= null)
                foreach (var mnt in CollectionColorMountains.List)
                {
                    mnt.Type = TypeColor.Moutains;
                    var area = CollectionColorArea.FindByColor(mnt.Color);
                    if (area == null)
                    {
                        mnt.Index = CollectionColorArea.List.Count;
                        CollectionColorArea.List.Add(mnt);
                    }
                    else
                    {
                        int index = area.Index;
                        CollectionColorArea.List.Remove(area);
                        mnt.Name = area.Name;
                        mnt.Index = index;
                        CollectionColorArea.List.Insert(index, mnt);
                        CollectionColorArea.InitializeSeaches();
                    }
                }


            CollectionColorArea.InitializeSeaches();
            if (CollectionColorCoast!=null)
                foreach (var coast in CollectionColorCoast.List)
                {
                    var area = CollectionColorArea.FindByColor(coast.Color);
                    if (area != null)
                    {
                        area.Type = TypeColor.Water;
                        area.Min = coast.Min;
                        area.Max = coast.Max;
                    }
                    else
                    {
                        coast.Index = CollectionColorArea.List.Count;
                        CollectionColorArea.List.Add(coast);
                        coast.Type = TypeColor.Water;
                        CollectionColorArea.InitializeSeaches();
                    }
                }

            #endregion //Colors

            #region Textures
            if (CollectionAreaTransitionTexture!=null)
                foreach (var transition in CollectionAreaTransitionTexture.List)
                {
                    var area = CollectionColorArea.FindByColor(transition.ColorFrom);
                    if (area == null)
                        continue;

                    area.TextureTransitions.Add(transition);
                    var area2 = CollectionColorArea.FindByColor(transition.ColorTo);
                    if (area2 == null) continue;

                    transition.IndexTo = area2.Index;
                }
            if (CollectionAreaTransitionCliffTexture!=null)
                foreach (var cliff in CollectionAreaTransitionCliffTexture.List)
                {
                    var area = CollectionColorArea.FindByColor(cliff.ColorFrom);
                    if (area == null) continue;
                
                    area.TransitionCliffTextures.Add(cliff);

                    var areato = CollectionColorArea.FindByColor(cliff.ColorTo);
                    if (areato == null) continue;
                    cliff.IdTo = areato.Index;
                }
            #endregion //Textures

            #region Items
            if (CollectionAreaItems!=null)
                foreach (var items in CollectionAreaItems.List)
                {
                    var area = CollectionColorArea.FindByColor(items.Color);
                    if (area == null)
                        continue;

                    area.Items = items;
                }
            if (CollectionAreaItemsCoasts!=null)
                foreach (var coast in CollectionAreaItemsCoasts.List)
                {
                    var area = CollectionColorArea.FindByColor(coast.Ground.Color);

                    if (area == null)
                        continue;

                    area.Coasts = coast;
                }
            if (CollectionAreaTransitionItems!=null)
                foreach (var itemtransition in CollectionAreaTransitionItems.List)
                {
                    var area = CollectionColorArea.FindByColor(itemtransition.ColorFrom);

                    if (area == null) continue;

                    area.TransitionItems.Add(itemtransition);
                }

            #endregion

        }

        

        #endregion //Factories

        #region MapMaking

        public void MapMake(string directory, string bitmaplocation, string bitmapZLocation,int x, int y, int index)
        {
            var bitmapMap = new BitmapReader(bitmaplocation, false).BitmapColors;
            var bitmapZ = new BitmapReader(bitmapZLocation, true).BitmapColors;

            var mulmaker = new MapMaking.MapMaker(bitmapMap, bitmapZ, x, y, index)
                               {
                                   CollectionAreaColor = CollectionColorArea,
                                   TextureAreas = CollectionAreaTexture,
                                   AutomaticZMode = AutomaticMode,
                                   MulDirectory = directory
                               };



            mulmaker.Bmp2Map();
        }
            
        #endregion

        #region ACO Making

        public void MakeAco(string file)
        {
            MemoryStream memory = new MemoryStream();
            EndianBinaryWriter bwriterWriter = new EndianBinaryWriter(EndianBitConverter.Big,memory);
            
            const UInt16 separator = 0;
            UInt16 sectionCounter = 0;

            var colorlist = CollectionColorArea.List.Select(c=>c.Color).ToList();
            
            UInt16 numberOfColors = UInt16.Parse(colorlist.Count.ToString());
            sectionCounter++;
            
            using (memory)
            {
                using (bwriterWriter)
                {
                    bwriterWriter.Write(sectionCounter);
                   
                    bwriterWriter.Write(numberOfColors); // write the number of colors

                    foreach (var color in colorlist)
                    {
                        ColorStructureWriter(bwriterWriter, color);
                    }
                    sectionCounter++;

                    bwriterWriter.Write(sectionCounter);

                    bwriterWriter.Write(numberOfColors);

                    var encoding = new UnicodeEncoding(true,true,true);

                    foreach (var color in colorlist)
                    {
                        ColorStructureWriter(bwriterWriter,color);
                        
                        var tmpcol = CollectionColorArea.List.FirstOrDefault(c => c.Color == color);
                        var bytes = (encoding.GetBytes(tmpcol.Name));
                        bwriterWriter.Write((ushort)tmpcol.Name.Length+1);
                        bwriterWriter.Write(bytes);
                        bwriterWriter.Write((ushort)0);
                    }
                    bwriterWriter.Flush();

                    using (FileStream output = new FileStream(file, FileMode.Create))
                    {
                        memory.WriteTo(output);
                    }
                }

                
            }

        }
        
        private void ColorStructureWriter(EndianBinaryWriter writer, Color color)
        {
            writer.Write((ushort)0);

            writer.Write(color.R);
            writer.Write(color.R);

            writer.Write(color.G);
            writer.Write(color.G);

            writer.Write(color.B);
            writer.Write(color.B);

            writer.Write(color.A); //alfa
            writer.Write(color.A);
        }
        
        #endregion

        #region UtilityMethods

        public void AddAreaColor(AreaColor add)
        {
            add.PropertyChanged += EventUpdateList;
            CollectionColorArea.List.Add(add);
        }

        #endregion //Utility Methods

        #region Save/Load functions

        public void SaveBinary(string file)
        {
            var objects = new object[]
                              {
                                  CollectionAreaTexture,
                                  CollectionColorArea,
                                  //CollectionColorCoast,
                                  //CollectionColorMountains,
                                  //CollectionAreaItems,
                                  //CollectionAreaItemsCoasts,
                                  //CollectionAreaTransitionItems,
                                  //CollectionAreaTransitionTexture,
                                  //CollectionAreaTransitionCliffTexture
                              };

            var formatter = new BinaryFormatter();
            var ss = new SurrogateSelector();
            ss.AddSurrogate(typeof(ObservableCollection<string>), new StreamingContext(StreamingContextStates.All), new ObservableCollectionSerializationSurrogate<string>());
            ss.AddSurrogate(typeof(ObservableCollection<int>), new StreamingContext(StreamingContextStates.All), new ObservableCollectionSerializationSurrogate<int>());
            ss.AddSurrogate(typeof(ObservableCollection<AreaColor>), new StreamingContext(StreamingContextStates.All), new ObservableCollectionSerializationSurrogate<AreaColor>());
            ss.AddSurrogate(typeof(ObservableCollection<AreaItems>), new StreamingContext(StreamingContextStates.All), new ObservableCollectionSerializationSurrogate<AreaItems>());
            ss.AddSurrogate(typeof(ObservableCollection<AreaTransitionItemCoast>), new StreamingContext(StreamingContextStates.All), new ObservableCollectionSerializationSurrogate<AreaTransitionItemCoast>());
            ss.AddSurrogate(typeof(ObservableCollection<AreaTransitionItem>), new StreamingContext(StreamingContextStates.All), new ObservableCollectionSerializationSurrogate<AreaTransitionItem>());
            ss.AddSurrogate(typeof(ObservableCollection<SingleItem>), new StreamingContext(StreamingContextStates.All), new ObservableCollectionSerializationSurrogate<SingleItem>());
            ss.AddSurrogate(typeof(ObservableCollection<CollectionItem>), new StreamingContext(StreamingContextStates.All), new ObservableCollectionSerializationSurrogate<CollectionItem>());
            ss.AddSurrogate(typeof(ObservableCollection<AreaTransitionCliffTexture>), new StreamingContext(StreamingContextStates.All), new ObservableCollectionSerializationSurrogate<AreaTransitionCliffTexture>());
            ss.AddSurrogate(typeof(ObservableCollection<AreaTransitionTexture>), new StreamingContext(StreamingContextStates.All), new ObservableCollectionSerializationSurrogate<AreaTransitionTexture>());
            formatter.SurrogateSelector = ss;
            var ms = new MemoryStream();
            using (ms)
            {
                formatter.Serialize(ms, objects);
                using (var stream = new FileStream(file,FileMode.Create))
                {
                    ms.WriteTo(stream);
                }
            }
        }

        public void LoadBinary(string file)
        {

            //var formatter = new BinaryFormatter();
            var ss = new SurrogateSelector();
            
            ss.AddSurrogate(typeof(ObservableCollection<string>), new StreamingContext(StreamingContextStates.All), new ObservableCollectionSerializationSurrogate<string>());
            ss.AddSurrogate(typeof(ObservableCollection<int>), new StreamingContext(StreamingContextStates.All), new ObservableCollectionSerializationSurrogate<int>());
            ss.AddSurrogate(typeof(ObservableCollection<AreaColor>), new StreamingContext(StreamingContextStates.All), new ObservableCollectionSerializationSurrogate<AreaColor>());
            ss.AddSurrogate(typeof(ObservableCollection<AreaItems>), new StreamingContext(StreamingContextStates.All), new ObservableCollectionSerializationSurrogate<AreaItems>());
            ss.AddSurrogate(typeof(ObservableCollection<AreaTransitionItemCoast>), new StreamingContext(StreamingContextStates.All), new ObservableCollectionSerializationSurrogate<AreaTransitionItemCoast>());
            ss.AddSurrogate(typeof(ObservableCollection<AreaTransitionItem>), new StreamingContext(StreamingContextStates.All), new ObservableCollectionSerializationSurrogate<AreaTransitionItem>());
            ss.AddSurrogate(typeof(ObservableCollection<SingleItem>), new StreamingContext(StreamingContextStates.All), new ObservableCollectionSerializationSurrogate<SingleItem>());
            ss.AddSurrogate(typeof(ObservableCollection<CollectionItem>), new StreamingContext(StreamingContextStates.All), new ObservableCollectionSerializationSurrogate<CollectionItem>());
            ss.AddSurrogate(typeof(ObservableCollection<AreaTransitionCliffTexture>), new StreamingContext(StreamingContextStates.All), new ObservableCollectionSerializationSurrogate<AreaTransitionCliffTexture>());
            ss.AddSurrogate(typeof(ObservableCollection<AreaTransitionTexture>), new StreamingContext(StreamingContextStates.All), new ObservableCollectionSerializationSurrogate<AreaTransitionTexture>());
            var formatter = new BinaryFormatter();
            formatter.SurrogateSelector = ss;
            object[] objectfrom;
            using(var strema = new FileStream(file,FileMode.Open))
            {
                objectfrom = (object[])formatter.Deserialize(strema);
            }
            CollectionAreaTransitionTexture = (CollectionAreaTransitionTexture)objectfrom[0];
            CollectionColorArea = (CollectionAreaColor)objectfrom[1];
            //CollectionColorArea = (CollectionAreaColor)objectfrom[0];
            //CollectionColorCoast = (CollectionAreaColor) objectfrom[1];
            //CollectionColorMountains = (CollectionAreaColorMountains)objectfrom[2];

            //CollectionAreaItems = (CollectionAreaItems) objectfrom[3];
            //CollectionAreaItemsCoasts = (CollectionAreaTransitionItemCoast)objectfrom[4];
            //CollectionAreaTransitionItems = (CollectionAreaTransitionItems) objectfrom[5];

            //CollectionAreaTexture = (CollectionAreaTexture) objectfrom[6];
            //CollectionAreaTransitionTexture = (CollectionAreaTransitionTexture) objectfrom[7];
            //CollectionAreaTransitionCliffTexture = (CollectionAreaTransitionCliffTexture) objectfrom[8];

            MergingData();
        }


        public void SaveXML(string file)
        {
            var objects = new NotificationObject[]
                              {
                                  CollectionAreaTexture,
                                  CollectionColorArea
                              };

            var serializer = new SoapFormatter();
            
            var ms = new MemoryStream();
            using (ms)
            {
                serializer.Serialize(ms, objects);
                using (var stream = new FileStream(file, FileMode.Create))
                {
                    ms.WriteTo(stream);
                }
            }
        }

        public void LoadFromXML(string file)
        {
            var serializer = new SoapFormatter();
            NotificationObject[] collections;
            using (var strema = new FileStream(file, FileMode.Open))
            {
                collections = (NotificationObject[])serializer.Deserialize(strema);
            }
            CollectionAreaTexture = (CollectionAreaTexture)collections[0];
            CollectionColorArea = (CollectionAreaColor)collections[1];

            foreach (var area in CollectionColorArea.List)
            {
                area.PropertyChanged += EventUpdateList;
            }
            //MergingData();
        }

        #endregion //Save/Load functions


        private void EventUpdateList(object sender, EventArgs args)
        {
            Colors=new Dictionary<int, Color>();
            foreach (var area in CollectionColorArea.List)
            {
                try
                {
                    Colors.Add(area.Index, area.Color);
                }
                catch (Exception)
                {
                }
                
            }
        }


    }
}
