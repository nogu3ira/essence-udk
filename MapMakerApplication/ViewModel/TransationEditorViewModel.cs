using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using EssenceUDK.Platform.DataTypes;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using OpenUO.MapMaker.Elements.Interfaces;
using OpenUO.MapMaker.Elements.Items.ItemsTransition;
using OpenUO.MapMaker.Elements.Textures.TextureTransition;

namespace MapMakerApplication.ViewModel
{
    public class TransationEditorViewModel : ViewModelBase
    {
        #region Declarations

        private int _comboboxTransitionLineTypeSelectedIndex;
        private int _comboboxTransitionDirectionSelectedIndex;
        private ITransition _selectedTransition;
        private int _selectedKindOfTransition;
        private IEntryTile _selectedTile;
        private int _selectedTileInt;

        #endregion //Declarations

        #region Properties

        public int ComboBoxLineTypeSelectedIndex
        {
            get { return _comboboxTransitionLineTypeSelectedIndex; }
            set
            {
                _comboboxTransitionLineTypeSelectedIndex = value;
                RaisePropertyChanged(() => ComboBoxLineTypeSelectedIndex);
                RaisePropertyChanged(() => SelectedLineTransition);
            }
        }

        public ObservableCollection<int> SelectedLineTransition
        {
            get
            {
                if (SelectedTransition == null) return null;
                if (ComboboxDirectionSelectedIndex < 0) return null;
                if (ComboBoxLineTypeSelectedIndex < 0) return null;
                return SelectedTransition.Lines[ComboBoxLineTypeSelectedIndex].List[ComboboxDirectionSelectedIndex].List;
            }
        }

        public ITransition SelectedTransition
        {
            get { return _selectedTransition; }
            set
            {
                _selectedTransition = value;
                RaisePropertyChanged(() => SelectedTransition);
                RaisePropertyChanged(() => SelectedLineTransition);
            }
        }

        public int ComboboxDirectionSelectedIndex
        {
            get
            { return _comboboxTransitionDirectionSelectedIndex; }
            set
            {
                _comboboxTransitionDirectionSelectedIndex = value;
                RaisePropertyChanged(() => ComboboxDirectionSelectedIndex);
                RaisePropertyChanged(() => SelectedLineTransition);
            }
        }

        public int SelectedKindOfTransition
        {
            get { return _selectedKindOfTransition; }
            set
            {
                _selectedKindOfTransition = value;
                RaisePropertyChanged(() => SelectedKindOfTransition);
            }
        }

        public object SelectedTile
        {
            get { return _selectedTile; }
            set
            {
                _selectedTile = (IEntryTile)value;
                RaisePropertyChanged(() => SelectedTile);
            }
        }

        public object SelectedTileInt
        {
            get { return _selectedTileInt; }
            set
            {
                if (value == null)
                    _selectedTileInt = -1;
                else
                {
                    _selectedTileInt = (int)value;
                }
                RaisePropertyChanged(() => SelectedTileInt);
            }
        }

        #endregion

        #region Ctor
        public TransationEditorViewModel()
        {

            #region Commands
            TransitionAdd = new RelayCommand(TransitionAddExecuted);
            TransitionRemove = new RelayCommand(TransitionRemoveExecuted, TransitionCanExecuteRemove);

            TileAdd = new RelayCommand(TileAddExecuted, TileAddCanExecute);
            TileRemove = new RelayCommand(TileRemoveExecuted, TileRemoveCanExecute);
            #endregion //Commands
        }
        #endregion

        #region Commands Properties

        public ICommand TransitionRemove { get; private set; }

        public ICommand TransitionAdd { get; private set; }

        public ICommand TileRemove { get; private set; }

        public ICommand TileAdd { get; private set; }

        #endregion //Commands Properties

        #region Commands Methods

        #region Transition

        private void TransitionRemoveExecuted()
        {
            ViewModelLocator._sdk.CollectionAreaColorSelected.TextureTransitions.Remove(SelectedTransition as AreaTransitionTexture);
            ViewModelLocator._sdk.CollectionAreaColorSelected.TransitionItems.Remove(SelectedTransition as AreaTransitionItem);
        }

        private Boolean TransitionCanExecuteRemove()
        {
            return SelectedTransition != null;

        }

        private void TransitionAddExecuted()
        {
            if (SelectedKindOfTransition == 0)
            {
                ViewModelLocator._sdk.CollectionAreaColorSelected.TextureTransitions.Add(new AreaTransitionTexture());
            }
            if (SelectedKindOfTransition == 1)
            {
                ViewModelLocator._sdk.CollectionAreaColorSelected.TransitionItems.Add(new AreaTransitionItem());
            }

        }

        #endregion //Transition

        #region Tiles

        private void TileAddExecuted()
        {
            SelectedLineTransition.Add((int)_selectedTile.TileId);
        }
        private Boolean TileAddCanExecute()
        {
            return _selectedTile != null &&
                SelectedLineTransition != null &&
                !SelectedLineTransition.Contains((int)_selectedTile.TileId);
        }

        private Boolean TileRemoveCanExecute()
        {
            return
                _selectedTileInt >= 0 &&
                SelectedLineTransition != null;
        }

        private void TileRemoveExecuted()
        {
            SelectedLineTransition.Remove(_selectedTileInt);
            ComboboxDirectionSelectedIndex = ComboboxDirectionSelectedIndex;
        }
        #endregion //Tiles

        #endregion // commands Methods





    }
}
