﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using System.Linq;
using EssenceUDK.MapMaker;
using EssenceUDK.MapMaker.Elements;
using EssenceUDK.MapMaker.Elements.BaseTypes.ComplexTypes;
using EssenceUDK.MapMaker.Elements.ColorArea;
using EssenceUDK.MapMaker.Elements.ColorArea.ColorArea;
using EssenceUDK.MapMaker.Elements.ColorArea.ColorMountains;
using EssenceUDK.MapMaker.Elements.Items;
using EssenceUDK.MapMaker.Elements.Items.Items;
using EssenceUDK.MapMaker.Elements.Items.ItemsTransition;
using EssenceUDK.MapMaker.Elements.Textures;
using EssenceUDK.MapMaker.Elements.Textures.TextureArea;
using EssenceUDK.MapMaker.Elements.Textures.TextureTransition;
using EssenceUDK.MapMaker.Elements.Textures.TexureCliff;
using EssenceUDK.MapMaker.MapMaking;
using EssenceUDK.Platform.DataTypes;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MapMakerApplication.Messages;
using Color = System.Windows.Media.Color;
using CollectionItem = EssenceUDK.MapMaker.Elements.Items.ItemText.CollectionItem;

namespace MapMakerApplication.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm/getstarted
    /// </para>
    /// </summary>
    public class SdkViewModel : ViewModelBase
    {

        #region Declarations

        #region General
        readonly MapSdk _makeMapSDK;
        private object _selectedAreaColor;
        private int tmp;
        private int _collectionAreaSelectedIndex;
        private TransationEditorViewModel _transation;

       #endregion

        #region Area Textures

        private object _selectedAreaTexture;
        private object _selectedAreaTextureTile;
        private object _selectedAreaTextureTileInt;

        #endregion //Area Textures

        #region CircleMountain

        private object _selectedGrownCircle;

        private object _selectedCoastSmoothCircle;

        private int _indexGrownCircle;

        #endregion //Circle Mountain

        #region Area Items

        private CollectionItem _selectedAreaItem;
        private object _selectedAreaItemTile;
        private object _selectedAreaItemTileInt;

        #endregion //Area Items

        #region Coasts

        private int _selectedCoastComboboxTypeLineIndex;
        private int _selectedCoastComboboxDirectionIndex;
        private object _selectedCoastTile;
        private object _selectedCoastTileInt;
        private int _selectedCoastType;

        #endregion

        #region EventHandling

        private Visibility _visibility = Visibility.Hidden;

        private string _textProgres;

        private int _proressBarValue;

        private bool _busy;

        #endregion //EventHandling

        #region Cliffs

        private object _selectedCliff;

        private object _selectedTextureForCliff;

        private object _selectedTextureInListCliff;

        #endregion //cliffs

        #region ContexMenu Copy&Paste

        private AreaColor _copiedAreaColor;

        private AreaTextures _copiedTexture;

        #endregion //ContexMenu Copy&Paste

        #endregion //Declarations

        #region Properties

        #region Inherited Props
        public MapSdk MakeMapSdk
        {
            get { return _makeMapSDK; }
        }

        #region Automatic Mode

        public bool AutomaticMode { get { return _makeMapSDK.AutomaticMode; } set { _makeMapSDK.AutomaticMode = value; RaisePropertyChanged(() => AutomaticMode); } }

        #endregion //Automatic Mode

        #region BitmapLocationMap
        public String BitmapLocationMap
        {
            get { return _makeMapSDK.BitmapLocationMap; }
            set
            {
                _makeMapSDK.BitmapLocationMap = value;
                RaisePropertyChanged("BitmapLocationMap");
            }
        }
        #endregion

        #region BitmapLocationMapZ
        public String BitmapLocationMapZ
        {
            get { return _makeMapSDK.BitmapLocationMapZ; }
            set
            {
                _makeMapSDK.BitmapLocationMapZ = value;
                RaisePropertyChanged("BitmapLocationMapZ");
            }
        }
        #endregion

        #region Collections

        #region CollectionAreaItems
        public CollectionAreaItems CollectionAreaItems
        {
            get { return _makeMapSDK.CollectionAreaItems; }
            set
            {
                _makeMapSDK.CollectionAreaItems = value;
                RaisePropertyChanged("CollectionAreaItems");
            }
        }
        #endregion

        #region CollectionAreaTransitionItemCoast

        public CollectionAreaTransitionItemCoast CollectionAreaItemsCoasts
        {
            get { return _makeMapSDK.CollectionAreaItemsCoasts; }
            set
            {
                _makeMapSDK.CollectionAreaItemsCoasts = value;
                RaisePropertyChanged("CollectionAreaItemsCoasts");
            }
        }
        #endregion

        #region CollectionAreaTexture

        public CollectionAreaTexture CollectionAreaTexture
        {
            get { return _makeMapSDK.CollectionAreaTexture; }
            set
            {
                _makeMapSDK.CollectionAreaTexture = value;
                RaisePropertyChanged("CollectionAreaTexture");
            }
        }

        #endregion

        #region CollectionAreaTransitionCliffTexture
        public CollectionAreaTransitionCliffTexture CollectionAreaTransitionCliffTexture
        {
            get { return _makeMapSDK.CollectionAreaTransitionCliffTexture; }
            set
            {
                _makeMapSDK.CollectionAreaTransitionCliffTexture = value;
                RaisePropertyChanged("CollectionAreaTransitionCliffTexture");
            }
        }
        #endregion

        #region CollectionAreaTransitionItems
        public CollectionAreaTransitionItems CollectionAreaTransitionItems
        {
            get { return _makeMapSDK.CollectionAreaTransitionItems; }
            set
            {
                _makeMapSDK.CollectionAreaTransitionItems = value;
                RaisePropertyChanged("CollectionAreaTransitionItems");
            }
        }
        #endregion

        #region CollectionAreaTransitionTexture
        public CollectionAreaTransitionTexture CollectionAreaTransitionTexture
        {
            get { return _makeMapSDK.CollectionAreaTransitionTexture; }
            set
            {
                _makeMapSDK.CollectionAreaTransitionTexture = value;
                RaisePropertyChanged("CollectionAreaTransitionTexture");
            }
        }
        #endregion

        #region CollectionAreaColor

        public CollectionAreaColor CollectionColorArea
        {
            get { return _makeMapSDK.CollectionColorArea; }
            set
            {
                _makeMapSDK.CollectionColorArea = value;
                RaisePropertyChanged("CollectionColorArea");
            }
        }
        public IEnumerable<Type> CollectionColorAreaTypes { get { return new[] { typeof(AreaColor) }; } }
        #endregion

        #region CollectionAreaColor
        public CollectionAreaColor CollectionColorCoast
        {
            get { return _makeMapSDK.CollectionColorCoast; }
            set
            {
                _makeMapSDK.CollectionColorCoast = value;
                RaisePropertyChanged("CollectionColorCoast");
            }
        }
        #endregion

        #region CollectionAreaColorMountains
        public CollectionAreaColorMountains CollectionColorMountains
        {
            get { return _makeMapSDK.CollectionColorMountains; }
            set
            {
                _makeMapSDK.CollectionColorMountains = value;
                RaisePropertyChanged("CollectionColorMountains");
            }
        }
        #endregion

        #endregion //Collections

        #region FolderLocation
        public String FolderLocation
        {
            get { return _makeMapSDK.FolderLocation; }
            set
            {
                _makeMapSDK.FolderLocation = value;
                RaisePropertyChanged("FolderLocation");
            }
        }
        #endregion

        #region TextureIds
        public IEnumerable<int> TextureIds { get { return _makeMapSDK.TextureIds; } }

        public IEnumerable<String> TextureNames { get
        {
            var list = new List<string>();
            foreach (var texture in CollectionAreaTexture.List)
            {
                list.Add(texture.Name);
            }
            return list;
        } } 
        #endregion

        #region AreaColorIndexes
        public IEnumerable<int> AreaColorIndexes { get { return _makeMapSDK.AreaColorIndexes; } }
        #endregion

        #region AreaColorColors
        public IEnumerable<Color> AreaColorColors { get { return _makeMapSDK.AreaColorColors; } }
        #endregion
        #endregion //Inherited Props

        #region AreaColor Props

        public object CollectionAreaSelectedItem
        {
            get { return _selectedAreaColor; }
            set
            {
                _selectedAreaColor = value;
                SelectedAreaTexture = CollectionAreaColorSelected != null ? CollectionAreaTexture.FindByIndex(CollectionAreaColorSelected.TextureIndex) : null;
                RaisePropertyChanged(null);
            }
        }

        public object SelectedAreaTexture { get { return _selectedAreaTexture; } set { _selectedAreaTexture = value; RaisePropertyChanged(() => SelectedAreaTexture); RaisePropertyChanged(()=>SelectedTextures); } }
        public AreaTextures SelectedTextures { get { return SelectedAreaTexture as AreaTextures; } }

        public AreaColor CollectionAreaColorSelected
        {
            get { return _selectedAreaColor as AreaColor; }
        }

        public int CollectionAreaSelectedIndex
        {
            get { return _collectionAreaSelectedIndex; }
            set
            {
                _collectionAreaSelectedIndex = value;
                RaisePropertyChanged(() => CollectionAreaSelectedIndex);
            }
        }

        #endregion //Area Color Props

        #region Grown/ Smoothing

        public object SelectedGrownCircle { get { return _selectedGrownCircle; } set { _selectedGrownCircle = value; RaisePropertyChanged(() => SelectedGrownCircle); } }

        public object SelectedSmoothCoast { get { return _selectedCoastSmoothCircle; } set { _selectedCoastSmoothCircle = value; RaisePropertyChanged(() => SelectedSmoothCoast); } }


        public int IndexGrownCircle { get { return _indexGrownCircle; } set { _indexGrownCircle = value; RaisePropertyChanged(()=>IndexGrownCircle); } }

        public int IndexSmoothCircle { get { return _indexGrownCircle; } set { _indexGrownCircle = value; RaisePropertyChanged(() => IndexGrownCircle); } }

        #endregion  //Grown/ Smoothing

        #region ContexMenu Copy&Paste

        public AreaColor AreaColorCopied { get { return _copiedAreaColor; } set { _copiedAreaColor = value; RaisePropertyChanged(()=>AreaColorCopied); } }

        public AreaTextures AreaTextureCopied { get { return _copiedTexture; } set { _copiedTexture = value; RaisePropertyChanged(()=>AreaTextureCopied); } }

        #endregion //ContexMenu Copy&Paste

        #region Area Texture

        public object SelectedAreaTextureTile { get { return _selectedAreaTextureTile; } set { _selectedAreaTextureTile = value; RaisePropertyChanged(() => SelectedAreaTextureTile); } }

        public object SelectedAreaTextureTileInt { get { return _selectedAreaTextureTileInt; } set { _selectedAreaTextureTileInt = value; RaisePropertyChanged(() => SelectedAreaTextureTileInt); } }


        #endregion //Area Texture

        #region Transations

        public TransationEditorViewModel TransationEditorViewModel { get { return _transation; } set { _transation = value; RaisePropertyChanged(() => TransationEditorViewModel); } }

        #endregion

        #region Area Item

        public object SelectedAreaItem
        {
            get { return _selectedAreaItem; }
            set
            {
                _selectedAreaItem = (CollectionItem)value;
                RaisePropertyChanged(() => SelectedAreaItem);
            }
        }

        public object SelectedAreaItemTile { get { return _selectedAreaItemTile; } set { _selectedAreaItemTile = value; RaisePropertyChanged(() => SelectedAreaItemTile); } }

        public object SelectedAreaItemTileInt { get { return _selectedAreaItemTileInt; } set { _selectedAreaItemTileInt = value; RaisePropertyChanged(() => SelectedAreaItemTileInt); } }

        #endregion

        #region Coasts

        public int SelectedComboboxCoastDirectionIndex
        {
            get { return _selectedCoastComboboxDirectionIndex; }
            set
            {
                _selectedCoastComboboxDirectionIndex = value;
                RaisePropertyChanged(() => SelectedComboboxCoastDirectionIndex);
                RaisePropertyChanged(() => SelectedWater);
                RaisePropertyChanged(() => SelectedGround);
            }
        }

        public int SelectedCoastComboboxTypeLineIndex
        {
            get { return _selectedCoastComboboxTypeLineIndex; }
            set
            {
                _selectedCoastComboboxTypeLineIndex = value;
                RaisePropertyChanged(() => SelectedCoastComboboxTypeLineIndex);
                RaisePropertyChanged(() => SelectedWater);
                RaisePropertyChanged(() => SelectedGround);
            }
        }

        public ObservableCollection<int> SelectedGround
        {
            get
            {
                var color = CollectionAreaColorSelected as AreaColor;
                if (color == null) return null;
                if (color.Coasts == null)
                    return null;
                var coasts = color.Coasts;
                if (coasts.Ground == null) return null;
                var ground = coasts.Ground;
                if (_selectedCoastComboboxDirectionIndex < 0 || SelectedCoastComboboxTypeLineIndex < 0) return null;
                return ground.Lines[SelectedCoastComboboxTypeLineIndex].List[_selectedCoastComboboxDirectionIndex].List;
            }
        }

        public ObservableCollection<int> SelectedWater
        {
            get
            {
                var color = _selectedAreaColor as AreaColor;
                if (color == null) return null;
                if (color.Coasts == null)
                    return null;
                var coasts = color.Coasts;
                if (coasts.Coast == null) return null;
                var water = coasts.Coast;
                if (_selectedCoastComboboxDirectionIndex < 0 || SelectedCoastComboboxTypeLineIndex < 0) return null;
                return water.Lines[SelectedCoastComboboxTypeLineIndex].List[_selectedCoastComboboxDirectionIndex].List;
            }
        }

        public int SelectedCoastType { get { return _selectedCoastType; } set { _selectedCoastType = value; RaisePropertyChanged(() => SelectedCoastType); } }

        public object SelectedCoastTileInt { get { return _selectedCoastTileInt; } set { _selectedCoastTileInt = value; RaisePropertyChanged(() => SelectedCoastTileInt); } }

        public object SelectedCoastTile { get { return _selectedCoastTile; } set { _selectedCoastTile = value; RaisePropertyChanged(() => SelectedCoastTile); } }

        #endregion

        #region Cliffs

        public object SelectedCliff { get { return _selectedCliff; } set { _selectedCliff = value; RaisePropertyChanged(() => SelectedCliff); RaisePropertyChanged(() => CliffList); } }

        public ObservableCollection<int> CliffList { get { return _selectedCliff != null ? ((AreaTransitionCliffTexture)SelectedCliff).List : null; } }

        public object SelectedTextureForCliff { get { return _selectedTextureForCliff; } set { _selectedTextureForCliff = value; RaisePropertyChanged(() => SelectedTextureForCliff); } }

        public object SelectedTextureInCliffList { get { return _selectedTextureInListCliff; } set { _selectedTextureInListCliff = value; RaisePropertyChanged(() => SelectedTextureInCliffList); } }

        #endregion //Cliff

        #region EventHandling

        public bool Busy { get { return _busy; } set { _busy = value; RaisePropertyChanged(() => Busy); } }

        public string TextProgress { get { return _textProgres; } set { _textProgres = value; RaisePropertyChanged(() => TextProgress); } }

        public int ProgressBarValue { get { return _proressBarValue; } set { _proressBarValue = value; RaisePropertyChanged(() => ProgressBarValue); } }

        public Visibility Visibility { get { return _visibility; } set { _visibility = value; RaisePropertyChanged(() => Visibility); } }

        #endregion //EventHandling

        #endregion //Properties

        #region Command Properties


        #region ContexMenu Copy&Paste

        public ICommand CommandCopyColor { get; private set; }

        public ICommand CommandCopyTexture { get; private set; }

        #endregion // ContexMenu Copy&Paste

        #region Color Area Collection Commands
        public ICommand CommandCollectionAreaColorRemove { get; private set; }

        public ICommand CommandCollectionAreaColorAdd { get; private set; }

        public ICommand CommandCollectionAreaColorMoveDown { get; private set; }

        public ICommand CommandCollectionAreaColorMoveUp { get; private set; }
        #endregion //Color Area Collection Commands

        #region Grown/Smooth Circle Commands

        public ICommand CommmandRemoveGrownCircle { get; private set; }

        public ICommand CommandAddGrownCircle { get; private set; }

        public ICommand CommandMoveUpGrownCircle { get; private set; }

        public ICommand CommandMoveDownGrownCircle { get; private set; }

        public ICommand CommmandRemoveSmoothCircle { get; private set; }

        public ICommand CommandAddSmoothCircle { get; private set; }

        public ICommand CommandMoveUpSmoothCircle { get; private set; }

        public ICommand CommandMoveDownSmoothCircle { get; private set; }

        #endregion // Grown/Smooth Circle Commands

        #region ContexMenu Copy&Paste 

        public ICommand CommandPasteCoast { get; private set; }
        public ICommand CommandPasteWaterCoast { get; private set; }
        public ICommand CommandPasteWaterCliff { get; private set; }
        public ICommand CommandPasteCoastSpecialOptions { get; private set; }
        public ICommand CommandPasteCliffs { get; private set; }

        public ICommand CommandPasteTextures { get; private set; }
        public ICommand CommandPasteTextureTransitions { get; private set; }
        public ICommand CommandPasteItemTransitions { get; private set; }
        public ICommand CommandPasteTextureId { get; private set; }



        #endregion //ContexMenu Copy&Paste

        #region Texture Area commands

        public ICommand CommandTextureAdd { get; private set; }

        public ICommand CommandTextureRemove { get; private set; }

        public ICommand CommandTextureTileAdd { get; private set; }

        public ICommand CommandTextureTileRemove { get; private set; }

        #endregion //Texture Area commands

        #region Area Items

        public ICommand CommandAreaItemTileAdd { get; private set; }

        public ICommand CommandAreaItemTileRemove { get; private set; }

        public ICommand CommandAreaItemCollectionAdd { get; private set; }

        public ICommand CommandAreaItemCollectionRemove { get; private set; }

        #endregion //Area Items

        #region Coasts

        public ICommand CommandCoastRemoveTile { get; private set; }

        public ICommand CommandCoastAddTile { get; private set; }

        public ICommand CommandCoastSetAsDefault { get; private set; }

        #endregion //Coasts

        #region File

        public ICommand CommandFileOpen { get; private set; }

        public ICommand CommandSave { get; private set; }

        public ICommand CommandSaveAco { get; private set; }

        public ICommand CommandOpenScriptFolder { get; private set; }

        #endregion

        #region Coasts

        #endregion //Coasts

        #region CliffCommands

        public ICommand CommandAddCliff { get; private set; }

        public ICommand CommandDeleteCliff { get; private set; }

        public ICommand CommandAddCliffTexture { get; private set; }

        public ICommand CommandRemoveCliffTexture { get; private set; }

        #endregion //Cliff Commands

        #region Export to CentrED+ Commands

        public ICommand CommandExportTransations { get; private set; }

        #endregion //Export to Centred Commands

        public ICommand CommandOpenOptionWindow { get; private set; }

        #endregion //Command Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the SdkView class.
        /// </summary>
        public SdkViewModel(MapSdk makeMapSdk)
            : this()
        {
            _makeMapSDK = makeMapSdk;

        }

        public SdkViewModel()
        {
            if (IsInDesignMode)
            {
                //_makeMapSDK = new MakeMapSDK(@"C:\Users\Xen\Desktop\scripts-08-06-10");
                //_makeMapSDK.Populate();
            }
            else
            {
                if (_makeMapSDK == null)
                    _makeMapSDK = new MapSdk();
            }
            _transation = new TransationEditorViewModel();

            #region Commands


            #region CollectionAreaColor
            #region Commands

            CommandCollectionAreaColorRemove = new RelayCommand(() => CollectionAreaColorCommandRemoveExecuted(), () => CollectionAreaColorIsSelected());

            CommandCollectionAreaColorAdd = new RelayCommand(() => CollectionAreaColorCommandAddExecuted());

            CommandCollectionAreaColorMoveDown = new RelayCommand(() => CollectionAreaMoveCommandExecuted(-1), () => CollectionAreaColorCanMoveDown());

            CommandCollectionAreaColorMoveUp = new RelayCommand(() => CollectionAreaMoveCommandExecuted(+1), () => CollectionAreaColorCanMoveUp());

            #endregion
            #endregion

            #region Grown/Smooth Circle Commands

            CommmandRemoveGrownCircle = new RelayCommand(() => CollectionAreaColorSelected.List.Remove((CircleMountain)SelectedGrownCircle), CommandCanRemoveGrownCircle);

            CommandAddGrownCircle = new RelayCommand(CommandAddGrownCircleExecuted, CommandCanAddGrownCircle);
            
            
            CommandMoveDownGrownCircle=
                new RelayCommand(()=>
                                                     {
                                                         var selected = SelectedGrownCircle;
                                                         CollectionAreaColorSelected.List.Remove((CircleMountain)selected);
                                                         
                                                         CollectionAreaColorSelected.List.Insert(IndexGrownCircle + 1, (CircleMountain)selected);
                                                     },CommandCanMoveDownGrownCircle);

            CommandMoveUpGrownCircle =
                new RelayCommand(CommandGrownCircleMoveUpExecuted, CommandCanMoveUpGrownCircle);



            CommmandRemoveSmoothCircle = new RelayCommand(() => CollectionAreaColorSelected.CoastSmoothCircles.Remove((CircleMountain)SelectedSmoothCoast), CommandCanRemoveSmoothCircle);

            CommandAddSmoothCircle = new RelayCommand(CommandAddSmoothCircleExecuted, CommandCanAddSmoothCircle);


            CommandMoveDownSmoothCircle =
                new RelayCommand(() =>
                {
                    var selected = SelectedSmoothCoast;
                    CollectionAreaColorSelected.CoastSmoothCircles.Remove((CircleMountain)selected);

                    CollectionAreaColorSelected.CoastSmoothCircles.Insert(IndexSmoothCircle + 1, (CircleMountain)selected);
                }, CommandCanMoveDownSmoothCircle);

            CommandMoveUpSmoothCircle =
                new RelayCommand(CommandSmoothCircleMoveUpExecuted, CommandCanMoveUpSmoothCircle);



            #endregion //Grown/Smooth Circle Commands

            #region ContexMenu Copy&Paste

            CommandCopyColor = new RelayCommand(()=>
                                                    {
                                                        AreaColorCopied = CollectionAreaColorSelected;

                                                    },()=> CollectionAreaColorSelected != null);

            CommandCopyTexture = new RelayCommand(() =>
            {
                AreaTextureCopied = (AreaTextures)SelectedAreaTexture;

            }, () => SelectedAreaTexture as AreaTextures != null);

            #endregion //ContexMenu Copy&Paste

            #region Collection Area Textures
            CommandTextureAdd = new RelayCommand(CommandAreaTextureAddExecuted);

            CommandTextureRemove = new RelayCommand(CommandAreaTextureRemoveExecuted, CommandAreaTextureRemoveCan);

            CommandTextureTileAdd = new RelayCommand(CommandAreaTextureTileAddExecuted, CommandAreaTextureTileAddCan);

            CommandTextureTileRemove = new RelayCommand(CommandAreaTextureTileRemove, CommandAreaTextureTileRemoveCan);

            #endregion //Collection Area Textures

            #region Collection Area item

            CommandAreaItemCollectionAdd = new RelayCommand(CommandAreaItemCollectionAddExecuted, CommandAreaItemCollectionAddCan);

            CommandAreaItemCollectionRemove = new RelayCommand(CommandAreaItemCollectionRemoveExecuted, CommandAreaItemCollectionRemoveCan);

            CommandAreaItemTileAdd = new RelayCommand(CommandAreaItemTileAddExecuted, CommandAreaItemTileAddCan);

            CommandAreaItemTileRemove = new RelayCommand(CommandAreaItemTileRemoveExecuted, CommandAreaItemTileRemoveCan);

            #endregion //Collection Area Item

            #region Coasts

            CommandCoastRemoveTile = new RelayCommand(CommandCoastRemoveTileExecuted, CommandCoastRemoveTileCan);

            CommandCoastAddTile = new RelayCommand(CommandCoastAddTileExecuted, CommandCoastAddTileCan);

            CommandCoastSetAsDefault = new RelayCommand(CommandCoastSetAsDefaultExecuted, CommandCoastSetAsDefaultCan);
            #endregion //Coasts

            #region ContexMenu Copy&Paste
            CommandPasteCoast = new RelayCommand(CommandPasteCoastExecuted, CommandCanPasteCoast);
            CommandPasteWaterCoast = new RelayCommand(CommandPasteWaterCoastExecuted, CommandCanPasteCoast);
            CommandPasteCoastSpecialOptions = new RelayCommand(CommandPasteCoastSpecialOptionsExecuted, CommandCanPasteCoast);
            CommandPasteWaterCliff = new RelayCommand(CommandPasteWaterCliffExecuted,CommandCanPasteCoast);
            CommandPasteCliffs = new RelayCommand(() =>
                                                      {
                                                          CollectionAreaColorSelected.TransitionCliffTextures =
                                                              new ObservableCollection<AreaTransitionCliffTexture>(
                                                                  AreaColorCopied.TransitionCliffTextures);

                                                      },
                                                  () => CollectionAreaColorSelected != null && AreaColorCopied != null);


            CommandPasteTextureTransitions = new RelayCommand(() => 
                                                 {
                                                     SelectedTextures.AreaTransitionTexture.List = new ObservableCollection<AreaTransitionTexture>(AreaTextureCopied.AreaTransitionTexture.List.ToList());
                                                     
                                                 },()=>SelectedAreaTexture != null && AreaTextureCopied!=null);
            CommandPasteItemTransitions = new RelayCommand(() =>
            {
                SelectedTextures.CollectionAreaItems.List = new ObservableCollection<AreaTransitionItem>(AreaTextureCopied.CollectionAreaItems.List);

            }, () => SelectedAreaTexture != null && AreaTextureCopied != null);


            CommandPasteTextureId = new RelayCommand(() => CollectionAreaColorSelected.TextureIndex = AreaTextureCopied.Index, () => CollectionAreaColorSelected != null && AreaTextureCopied!=null);

            CommandPasteTextures=new RelayCommand(()=>
                                                      {
                                                          SelectedTextures.List=new ObservableCollection<int>(AreaTextureCopied.List);
                                                      },()=>SelectedTextures!=null && _copiedTexture != null);
            #endregion //ContexMenu Copy&Paste
            #endregion //Commands

            #region Save Commands
            CommandSaveAco =
                new RelayCommand
                    (
                        () => AppMessages.DialogRequest.Send(new MessageDialogRequest("ACO")),
                        () => CollectionColorArea.List.Count > 0
                    );
            CommandOpenScriptFolder =
                new RelayCommand(
                () => AppMessages.DialogRequest.Send(new MessageDialogRequest("FOLDER"))
                );

            CommandSave = new RelayCommand(() => AppMessages.DialogRequest.Send(new MessageDialogRequest("SAVE")));

            CommandFileOpen = new RelayCommand(() => AppMessages.DialogRequest.Send(new MessageDialogRequest("LOAD")));
            #endregion //Save Commands

            #region Cliffs Commands
            CommandAddCliff = new RelayCommand(() => CollectionAreaColorSelected.TransitionCliffTextures.Add(new AreaTransitionCliffTexture()), () => CollectionAreaColorSelected!=null);

            CommandDeleteCliff = new RelayCommand(() =>
            {
                try
                {
                    var cliff = SelectedCliff as AreaTransitionCliffTexture;
                    CollectionAreaColorSelected.TransitionCliffTextures.Remove(cliff);
                }
                catch (Exception e)
                {
                    AppMessages.DialogRequest.Send(new MessageDialogRequest(e.Message));
                }

            }, () => SelectedCliff != null);

            CommandAddCliffTexture = new RelayCommand(() =>
            {
                try
                {
                    var tile = SelectedTextureForCliff as IEntryTile;
                    var collection =
                        SelectedCliff as AreaTransitionCliffTexture;

                    if (tile != null)
                        if (collection != null)
                            collection.List.Add((int)tile.TileId);
                }
                catch (Exception e)
                {
                    AppMessages.DialogRequest.Send(new MessageDialogRequest(e.Message));
                }
            
            }, () => SelectedTextureForCliff != null &&
                                                                 SelectedCliff != null && CollectionAreaColorSelected != null);

            CommandRemoveCliffTexture = new RelayCommand(() =>
            {
                try
                {
                    var number = (int)SelectedTextureInCliffList;
                    var collection =
                      SelectedCliff as AreaTransitionCliffTexture;
                    if (collection != null) collection.List.Remove(number);
                }
                catch (Exception e)
                {
                    AppMessages.DialogRequest.Send(new MessageDialogRequest(e.Message));
                }
                
               
            }, () => SelectedCliff != null && SelectedTextureInCliffList != null && CollectionAreaColorSelected != null);

            #endregion //Cliff Commands


            CommandExportTransations =
                new RelayCommand(
                    () => AppMessages.DialogRequest.Send(
                        new MessageDialogRequest("OpenFileXmlExport")),
                    () => _selectedAreaColor != null
                    );

            CommandOpenOptionWindow = new RelayCommand(() => AppMessages.DialogRequest.Send(new MessageDialogRequest("OpenOptionWindow")));

            AppMessages.OptionAnswer.Register(this, HandlerOptionResults);
            AppMessages.DialogAnwer.Register(this, HandlerDialogResults);
            AppMessages.MapGeneratorMessage.Register(this, HandlerGenerateMap);

        }

        #endregion //Constructors

        #region Command Methods

        #region AreaColorCollection Commands

        private void CollectionAreaColorCommandRemoveExecuted()
        {
            try
            {
                var area = CollectionAreaSelectedItem as AreaColor;

                if (area == null)
                    return;

                CollectionColorArea.List.Remove(area);
            }
            catch (Exception e)
            {
                AppMessages.DialogRequest.Send(new MessageDialogRequest(e.Message));
            }
           

        }

        private void CollectionAreaColorCommandAddExecuted()
        {
            _makeMapSDK.AddAreaColor(new AreaColor());
        }

        private bool CollectionAreaColorIsSelected()
        {
            return CollectionAreaSelectedItem is AreaColor;
        }

        private bool CollectionAreaColorCanMoveUp()
        {
            var firstCondition = CollectionAreaColorIsSelected();
            var secondCondition = CollectionAreaSelectedIndex >= 0;
            return firstCondition && secondCondition;
        }

        private bool CollectionAreaColorCanMoveDown()
        {
            var firstCondition = CollectionAreaColorIsSelected();
            var secondCondition = CollectionAreaSelectedIndex < CollectionColorArea.List.Count && CollectionAreaSelectedIndex > 0;
            if (secondCondition == true)
            {
                tmp = CollectionAreaSelectedIndex;
            }
            return firstCondition && secondCondition;
        }

        private void CollectionAreaMoveCommandExecuted(int increase)
        {
            try
            {
                var area = (AreaColor)CollectionAreaSelectedItem;
                CollectionColorArea.List.Remove(area);
                CollectionColorArea.List.Insert(tmp + increase, area);
                RaisePropertyChanged("CollectionColorArea");

            }
            catch (Exception e)
            {
                AppMessages.DialogRequest.Send(new MessageDialogRequest(e.Message));
            }
           

        }

        #endregion

        #region Grown / Smooth Circles

        private bool CommandCanAddGrownCircle()
        {
            return CollectionAreaColorSelected != null;
        }

        private void CommandGrownCircleMoveUpExecuted()
        {
            var selected = SelectedGrownCircle;
            var tmpIndex = IndexGrownCircle;
            CollectionAreaColorSelected.List.Remove((CircleMountain)SelectedGrownCircle);
            CollectionAreaColorSelected.List.Insert(tmpIndex - 1, (CircleMountain)selected);
        }

        private void CommandAddGrownCircleExecuted()
        {
            if (!CommandCanAddGrownCircle())
                return;

            CollectionAreaColorSelected.List.Add(new CircleMountain(){From=0, To=0});
        }

        private bool CommandCanRemoveGrownCircle()
        {
            return CommandCanAddGrownCircle() && SelectedGrownCircle as CircleMountain != null;
        }

        private bool CommandCanMoveDownGrownCircle()
        {
            return CommandCanRemoveGrownCircle() && IndexGrownCircle < CollectionAreaColorSelected.List.Count-1 && CollectionAreaColorSelected.List.Count>1;
        }

        private bool CommandCanMoveUpGrownCircle()
        {
            return CommandCanRemoveGrownCircle() && IndexGrownCircle > 0;
        }


        private bool CommandCanAddSmoothCircle()
        {
            return CollectionAreaColorSelected != null;
        }

        private void CommandSmoothCircleMoveUpExecuted()
        {
            var selected = SelectedSmoothCoast;
            var tmpIndex = IndexSmoothCircle;
            CollectionAreaColorSelected.CoastSmoothCircles.Remove((CircleMountain)SelectedSmoothCoast);
            CollectionAreaColorSelected.CoastSmoothCircles.Insert(tmpIndex - 1, (CircleMountain)selected);
        }

        private void CommandAddSmoothCircleExecuted()
        {
            if (!CommandCanAddSmoothCircle())
                return;

            CollectionAreaColorSelected.CoastSmoothCircles.Add(new CircleMountain() { From = 0, To = 0 });
        }

        private bool CommandCanRemoveSmoothCircle()
        {
            return CommandCanAddSmoothCircle() && SelectedSmoothCoast as CircleMountain != null;
        }

        private bool CommandCanMoveDownSmoothCircle()
        {
            return CommandCanRemoveSmoothCircle() && IndexGrownCircle < CollectionAreaColorSelected.CoastSmoothCircles.Count - 1 && CollectionAreaColorSelected.CoastSmoothCircles.Count > 1;
        }

        private bool CommandCanMoveUpSmoothCircle()
        {
            return CommandCanRemoveSmoothCircle() && IndexSmoothCircle > 0;
        }

        #endregion //Grown / Smooth Circles

        #region Area Texture Commands

        private void CommandAreaTextureAddExecuted()
        {
            try
            {
                CollectionAreaTexture.List.Add(new AreaTextures());
                RaisePropertyChanged(null);
            }
            catch (Exception e)
            {
                AppMessages.DialogRequest.Send(new MessageDialogRequest(e.Message));
            }
           
        }

        private void CommandAreaTextureRemoveExecuted()
        {
            try
            {
                var selected = SelectedAreaTexture as AreaTextures;

                CollectionAreaTexture.List.Remove(selected);
                RaisePropertyChanged(null);
            }
            catch (Exception e)
            {
                AppMessages.DialogRequest.Send(new MessageDialogRequest(e.Message));
            }
         
        }

        private bool CommandAreaTextureRemoveCan()
        {
            return SelectedAreaTexture is AreaTextures;
        }

        private void CommandAreaTextureTileAddExecuted()
        {
            try
            {
                var selected = SelectedAreaTexture as AreaTextures;
                var tile = SelectedAreaTextureTile as IEntryTile;
                selected.List.Add((int)tile.TileId);
            }
            catch (Exception e)
            {
                AppMessages.DialogRequest.Send(new MessageDialogRequest(e.Message));
            }
            
        }

        private bool CommandAreaTextureTileAddCan()
        {
            var tile = SelectedAreaTextureTile as IEntryTile;
            var selected = SelectedAreaTexture as AreaTextures;
            var condition1 = tile != null;
            var condition2 = selected != null;

            return (condition1 && condition2 && !selected.List.Contains((int)tile.TileId));

        }

        private void CommandAreaTextureTileRemove()
        {
            try
            {
                var selected = SelectedAreaTexture as AreaTextures;
                selected.List.Remove((int)SelectedAreaTextureTileInt);
            }
            catch (Exception e)
            {
                AppMessages.DialogRequest.Send(new MessageDialogRequest(e.Message));
            }
          
        }

        private bool CommandAreaTextureTileRemoveCan()
        {
            return SelectedAreaTexture is AreaTextures &&
                   SelectedAreaTextureTileInt is int;
        }

        #endregion //Area Texture Commands

        #region Item Commands
        
        private void CommandAreaItemCollectionAddExecuted()
        {
            try
            {
                var area = CollectionAreaSelectedItem as AreaColor;
                area.Items.List.Add(new CollectionItem());
            }
            catch (Exception e)
            {
                AppMessages.DialogRequest.Send(new MessageDialogRequest(e.Message));
            }
            
    
        }

        private bool CommandAreaItemCollectionAddCan()
        {
            return CollectionAreaSelectedItem != null;
        }


        private void CommandAreaItemCollectionRemoveExecuted()
        {
            try
            {
                var area = CollectionAreaSelectedItem as AreaColor;
                var selected = SelectedAreaItem as CollectionItem;

                area.Items.List.Remove(selected);

            }
            catch (Exception e)
            {
                AppMessages.DialogRequest.Send(new MessageDialogRequest(e.Message));
            }
     
        }

        private bool CommandAreaItemCollectionRemoveCan()
        {
            var area = CollectionAreaSelectedItem as AreaColor != null;
            var selected = SelectedAreaItem as CollectionItem != null;

            return area && selected;
        }

        private void CommandAreaItemTileAddExecuted()
        {
            try
            {
                var selectedtile = SelectedAreaItemTile as IEntryTile;
                var selectedCollection = SelectedAreaItem as CollectionItem;

                selectedCollection.List.Add(new SingleItem() { Id = (int)selectedtile.TileId });

            }
            catch (Exception e)
            {
                AppMessages.DialogRequest.Send(new MessageDialogRequest(e.Message));
            }
           
        }

        private bool CommandAreaItemTileAddCan()
        {
            var selectedtile = SelectedAreaItemTile as IEntryTile != null;
            var selectedCollection = SelectedAreaItem as CollectionItem != null;
            return selectedtile && selectedCollection;

        }


        private void CommandAreaItemTileRemoveExecuted()
        {
            try
            {
                var selected = _selectedAreaItemTileInt as SingleItem;
                var selectedCollection = SelectedAreaItem as CollectionItem;
                selectedCollection.List.Remove(selected);
            }
            catch (Exception e)
            {
                AppMessages.DialogRequest.Send(new MessageDialogRequest(e.Message));
            }
         

        }

        private bool CommandAreaItemTileRemoveCan()
        {
            var selected = _selectedAreaItemTileInt as SingleItem != null;
            var selectedCollection = SelectedAreaItem as CollectionItem != null;
            return selected && selectedCollection;
        }

        #endregion //Item Commands

        #region Coasts Commands

        private void CommandCoastRemoveTileExecuted()
        {
            try
            {
                switch (SelectedCoastType)
                {
                    case 0:
                        {

                            SelectedWater.Remove((int)SelectedCoastTileInt);
                        }
                        break;

                    case 1:
                        {
                            SelectedGround.Remove((int)SelectedCoastTileInt);
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                AppMessages.DialogRequest.Send(new MessageDialogRequest(e.Message));
            }
           
        }

        private bool CommandCoastRemoveTileCan()
        {

            switch (SelectedCoastType)
            {
                case 0:
                    {
                        return SelectedCoastTileInt is int && SelectedWater != null;
                    }

                case 1:
                    {
                        return SelectedCoastTileInt is int && SelectedGround != null;
                    }

                default:
                    {
                        return false;
                    }
            }

        }

        private void CommandCoastAddTileExecuted()
        {
            try
            {
                switch (SelectedCoastType)
                {
                    case 0:
                        {
                            SelectedWater.Add((int)((IEntryTile)SelectedCoastTile).TileId);
                        }
                        break;

                    case 1:
                        {
                            SelectedGround.Add((int)((IEntryTile)SelectedCoastTile).TileId);
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                AppMessages.DialogRequest.Send(new MessageDialogRequest(e.Message));
            }
            
        }

        private bool CommandCoastAddTileCan()
        {


            switch (SelectedCoastType)
            {
                case 0:
                    {

                        return SelectedCoastTile is IEntryTile && SelectedWater != null && !SelectedWater.Contains((int)((IEntryTile)SelectedCoastTile).TileId);
                    }

                case 1:
                    {
                        return SelectedCoastTile is IEntryTile && SelectedGround != null && !SelectedGround.Contains((int)((IEntryTile)SelectedCoastTile).TileId);
                    }
                default:
                    return false;
            }
        }

        private void CommandCoastSetAsDefaultExecuted()
        {
            try
            {
                var area = CollectionAreaColorSelected;
                area.Coasts.Coast.Texture = ((int)((IEntryTile)SelectedCoastTile).TileId);
            }
            catch (Exception e)
            {
                AppMessages.DialogRequest.Send(new MessageDialogRequest(e.Message));
            }

        }

        private bool CommandCoastSetAsDefaultCan()
        {
            return SelectedCoastTile != null && CollectionAreaColorSelected != null
                   && SelectedCoastType == 0;
        }

        #endregion //Coasts Commands

        #region ContexMenu Copy&Paste

        private void CommandPasteCoastExecuted()
        {
            CollectionAreaColorSelected.Coasts.Coast.Lines = new ObservableCollection<CollectionLine>(AreaColorCopied.Coasts.Coast.Lines.ToList());
            CollectionAreaColorSelected.Coasts.Ground.Lines= new ObservableCollection<CollectionLine>(AreaColorCopied.Coasts.Ground.Lines.ToList());
            CollectionAreaColorSelected.CoastSmoothCircles = new ObservableCollection<CircleMountain>(AreaColorCopied.CoastSmoothCircles.ToList());
            CollectionAreaColorSelected.ItemsAltitude = AreaColorCopied.ItemsAltitude;
            CollectionAreaColorSelected.CoastAltitude = AreaColorCopied.CoastAltitude;
        }

        private bool CommandCanPasteCoast()
        {
            return CollectionAreaColorSelected != null && AreaColorCopied != null;
        }

        private void CommandPasteWaterCoastExecuted()
        {
            CollectionAreaColorSelected.Coasts.Coast.Lines = new ObservableCollection<CollectionLine>(AreaColorCopied.Coasts.Coast.Lines.ToList());
        }

        private void CommandPasteWaterCliffExecuted()
        {
            CollectionAreaColorSelected.Coasts.Ground.Lines = new ObservableCollection<CollectionLine>(AreaColorCopied.Coasts.Ground.Lines.ToList());
        }

        private void CommandPasteCoastSpecialOptionsExecuted()
        {
            CollectionAreaColorSelected.CoastSmoothCircles = new ObservableCollection<CircleMountain>(AreaColorCopied.CoastSmoothCircles.ToList());
            CollectionAreaColorSelected.ItemsAltitude = AreaColorCopied.ItemsAltitude;
            CollectionAreaColorSelected.CoastAltitude = AreaColorCopied.CoastAltitude;
        }
        #endregion //ContexMenu Copy&Paste

        #endregion //Command Methods

        #region Message Handling

        private void HandlerDialogResults(MessageDialogResult result)
        {
            if (result == null) return;
            try
            {
                switch (result.Type)
                {
                    case DialogType.SaveAco:
                        {
                            _makeMapSDK.MakeAco(result.Content);
                        }
                        break;
                    case DialogType.SaveFile:
                        {
                            _makeMapSDK.SaveXML(result.Content);
                        }
                        break;
                    case DialogType.OpenFile:
                        {
                            _makeMapSDK.LoadFromXML(result.Content);
                            RaisePropertyChanged(null);
                        }
                        break;
                    case DialogType.OpenFolder:
                        {
                            _makeMapSDK.InitializeFactories(result.Content);
                            _makeMapSDK.Populate();
                            RaisePropertyChanged(null);
                        }
                        break;
                    case DialogType.SaveBrushFile:
                        {
                            ExportToCentredPlus(result.Content);
                            ExportCentredPlusGroups(result.Content);
                        }
                        break;

                }
            }
            catch (Exception e)
            {
                
                AppMessages.DialogRequest.Send(new MessageDialogRequest(e.Message));
            }
            
        }

        private void HandlerOptionResults(OptionMessage result)
        {
            if (result.Success)
                RaisePropertyChanged(null);
        }

        private void HandlerGenerateMap(MapMakeMessage message)
        {
            Busy = true;
            ProgressBarValue = 0;
            TextProgress = "";
            var index = message.Index;
            var xy = Globals.Dimentions[index];
            var indexes = Globals.Indexes[index];
            _makeMapSDK.EventMakingMapEnd += MakingMapEnd;

            _makeMapSDK.EventMapMakingProgress += EventHandlerProgressMapCreation;
                var task = new Thread(() => Start(xy, indexes));
                task.Start();
            Visibility = Visibility.Visible;


        }

        private void Start(int[] xy, int indexes)
        {
            try
            {
                _makeMapSDK.MapMake(ApplicationController.OutputFolder, BitmapLocationMap, BitmapLocationMapZ, xy[0], xy[1],
                              indexes);
            }
            catch (Exception e)
            {
                // Are we not on the main UI thread?
                if (!Application.Current.Dispatcher.CheckAccess())
                {
                    // Unhandled exceptions on worker threads will halt the application. We want to
                    // use our global exception handler(s), so dispatch or "forward" to the UI thread.
                    Application.Current.Dispatcher.Invoke(
                        DispatcherPriority.Normal,
                        new Action<Exception>(WorkerThreadException), e);
                    MakingMapEnd(this,EventArgs.Empty);
                }
                else
                {   
                 
                    throw;  // Already on UI thread; just rethrow the exception to global handlers
                }
            }
          
        }
        private static void WorkerThreadException(Exception ex)
        {
            throw ex;
        }


        private void MakingMapEnd(object sender, EventArgs eventArgs)
        {
            Busy = false;
            Visibility = Visibility.Hidden;
        }


        private void EventHandlerProgressMapCreation(object sender, EventArgs args)
        {
            var arg = args as ProgressEventArgs;
            if (args != null)
            {
                TextProgress = arg.PayLoad;
                ProgressBarValue = arg.Progress;
            }

        }


        #region Export to CentrED+

        private void ExportToCentredPlus(string directoryname)
        {
            try
            {
                var filename = directoryname + "/TilesBrush.xml";

                var xml = new XmlDocument();
                xml.Load(filename);
                CollectionAreaTexture.InitializeSeaches();
                var node = xml.SelectSingleNode("./TilesBrush");

                foreach (var areaColorColor in CollectionColorArea.List)
                {
                    ParseColorToXml(xml, areaColorColor, node, CollectionAreaTexture);
                }
                foreach (var area in CollectionColorArea.List)
                {
                    ReParseColorToXml(xml, area, node, CollectionAreaTexture);
                }

                xml.Save(filename);
            }
            catch (Exception e)
            {
                AppMessages.DialogRequest.Send(new MessageDialogRequest(e.Message));
            }
            

        }

        private static void ParseColorToXml(XmlDocument xml, AreaColor area, XmlNode root, CollectionAreaTexture collectionAreaTexture)
        {

            var texture = collectionAreaTexture.FindByIndex(area.TextureIndex);
            if (texture == null) return;


            var thisNode = xml.CreateNode(XmlNodeType.Element, "Brush", xml.NamespaceURI);
            var Attribute = xml.CreateAttribute("Id");
            Attribute.Value = String.Format("{0:0000}", area.Index);
            thisNode.Attributes.Append(Attribute);

            Attribute = xml.CreateAttribute("Name");
            Attribute.Value = area.Name;
            thisNode.Attributes.Append(Attribute);


            foreach (var VARIABLE in texture.List)
            {
                var landNode = xml.CreateNode(XmlNodeType.Element, "Land", xml.NamespaceURI);
                Attribute = xml.CreateAttribute("ID");
                Attribute.Value = "0x" + VARIABLE.ToString("X4");
                landNode.Attributes.Append(Attribute);
                thisNode.AppendChild(landNode);
            }

            foreach (var transition in area.TextureTransitions)
            {
                var edgenode = xml.CreateNode(XmlNodeType.Element, "Edge", xml.NamespaceURI);
                Attribute = xml.CreateAttribute("To");
                Attribute.Value = String.Format("{0:0000}", transition.IndexTo);
                edgenode.Attributes.Append(Attribute);

                InsertEdgesToXml(xml, "DR", edgenode, transition.BorderSouthEast.List);
                InsertEdgesToXml(xml, "DL", edgenode, transition.BorderSouthWest.List);
                InsertEdgesToXml(xml, "UL", edgenode, transition.BorderNorthWest.List);
                InsertEdgesToXml(xml, "UR", edgenode, transition.BorderNorthEast.List);

                InsertEdgesToXml(xml, "LL", edgenode, transition.LineWest.List);
                InsertEdgesToXml(xml, "UU", edgenode, transition.LineNorth.List);

                thisNode.AppendChild(edgenode);
            }


            root.AppendChild(thisNode);
        }

        private void ReParseColorToXml(XmlDocument xml, AreaColor area, XmlNode root, CollectionAreaTexture collectionAreaTexture)
        {

            var texture = collectionAreaTexture.FindByIndex(area.TextureIndex);
            if (texture == null) return;

            foreach (var transition in area.TextureTransitions)
            {
                var xpathquery = "//Brush[@Id=" + "'" + String.Format("{0:0000}", transition.IndexTo) + "'" + "]";
                var node = xml.SelectSingleNode(xpathquery);
                if (node == null) continue;
                var edgenode = xml.CreateNode(XmlNodeType.Element, "Edge", xml.NamespaceURI);
                var Attribute = xml.CreateAttribute("To");
                Attribute.Value = String.Format("{0:0000}", area.Index);
                edgenode.Attributes.Append(Attribute);
                
                InsertEdgesToXml(xml, "DR", edgenode, transition.EdgeNorthWest.List);
                InsertEdgesToXml(xml, "DL", edgenode, transition.EdgeNorthEast.List);
                InsertEdgesToXml(xml, "UL", edgenode, transition.EdgeSouthEast.List);
                InsertEdgesToXml(xml, "UR", edgenode, transition.EdgeSouthWest.List);

                InsertEdgesToXml(xml, "LL", edgenode, transition.LineEast.List);
                InsertEdgesToXml(xml, "UU", edgenode, transition.LineSouth.List);

                node.AppendChild(edgenode);
            }


        }

        private static void InsertEdgesToXml(XmlDocument xml, string type, XmlNode edge, IEnumerable<int> list)
        {
            foreach (var id in list)
            {

                var node = xml.CreateNode(XmlNodeType.Element, "Land", xml.NamespaceURI);
                var Attribute = xml.CreateAttribute("Type");
                Attribute.Value = type;
                node.Attributes.Append(Attribute);
                Attribute = xml.CreateAttribute("ID");
                Attribute.Value = "0x" + id.ToString("X4");
                node.Attributes.Append(Attribute);


                edge.AppendChild(node);
            }
        }

        private void ExportCentredPlusGroups(string directoryName)
        {
            var filename = directoryName + "/TilesGroup.xml";

            var xml = new XmlDocument();
            xml.Load(filename);

            var root = xml.SelectSingleNode("/TilesGroup");
            var node = xml.CreateNode(XmlNodeType.Element, "Group", xml.NamespaceURI);
            root.AppendChild(node);

            var attribute = xml.CreateAttribute("Name");
            attribute.Value = "Brushes";
            node.Attributes.Append(attribute);

            attribute = xml.CreateAttribute("bold");
            attribute.Value = "True";
            node.Attributes.Append(attribute);

            attribute = xml.CreateAttribute("Nodes");
            attribute.Value = (-1).ToString();
            node.Attributes.Append(attribute);

            var parent = node;

            foreach (var area in CollectionColorArea.List)
            {
                node = xml.CreateNode(XmlNodeType.Element, "Group", xml.NamespaceURI);
                attribute = xml.CreateAttribute("color");
                attribute.Value = area.Color.ToString();
                node.Attributes.Append(attribute);
                attribute = xml.CreateAttribute("Name");
                attribute.Value = area.Name;
                node.Attributes.Append(attribute);
                attribute = xml.CreateAttribute("ital");
                attribute.Value = "True";
                node.Attributes.Append(attribute);
                var innernode = xml.CreateNode(XmlNodeType.Element, "Brush", xml.NamespaceURI);
                attribute = xml.CreateAttribute("ID");
                attribute.Value = String.Format("{0:0000}", area.Index);
                innernode.Attributes.Append(attribute);
                node.AppendChild(innernode);

                parent.AppendChild(node);
            }

            xml.Save(filename);
        }

        #endregion //Export to CentrED+


        #endregion

    }
}