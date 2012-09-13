using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System;
using System.Windows.Input;
using EssenceUDK.Platform.DataTypes;

namespace EssenceUDK.Platform
{
    public sealed class ModelItemData : UOBaseViewModel, IItemTile, IItemData
    {
        #region Declarations
        private readonly IItemTile _data;
        #endregion

        #region Properties

        public uint TileId { get { return _data.TileId; } set { _data.TileId = value; RaisePropertyChanged(() => TileId); } }

        public ISurface Surface { get { return _data.Surface; } set { _data.Surface = value; RaisePropertyChanged(() => Surface); } }

        public string Name { get { return _data.Name; } set { _data.Name = value; RaisePropertyChanged(() => Name); } }

        public TileFlag Flags { get { return _data.Flags; } set { _data.Flags = value; RaisePropertyChanged(() => Flags); } }

        public byte Height { get { return _data.Height; } set { _data.Height = value; RaisePropertyChanged(() => Height); } }

        public byte Quality { get { return _data.Quality; } set { _data.Quality = value; RaisePropertyChanged(() => Quality); } }

        public byte Quantity { get { return _data.Quantity; } set { _data.Quantity = value; RaisePropertyChanged(() => Quantity); } }

        public ushort Animation { get { return _data.Animation; } set { _data.Animation = value; RaisePropertyChanged(() => Animation); } }

        public byte StackingOff { get { return _data.StackingOff; } set { _data.StackingOff = value; RaisePropertyChanged(() => StackingOff); } }
        
        #endregion

        #region Ctor
        public ModelItemData(IItemTile data)
        {
            _data = data;
        }
        #endregion
    }

    #region Model Helpers
    
    public abstract class UOBaseViewModel : NotificationUOObject
    {
    }

    public abstract class DelegateUOCommand : ICommand
    {
        private readonly Action _command;
        private readonly Func<bool> _canExecute;
        public event EventHandler CanExecuteChanged;

        public DelegateUOCommand(Action command, Func<bool> canExecute = null)
        {
            if (command == null)
                throw new ArgumentNullException();
            _canExecute = canExecute;
            _command = command;
        }

        public void Execute(object parameter)
        {
            _command();
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
                return true;
            return _canExecute();
        }

        public void RasieCanExecuteChanged()
        {
            CanExecuteChanged(this, EventArgs.Empty);
        }
    }

    public abstract class NotificationUOObject : INotifyPropertyChanged
    {
        protected void RaisePropertyChanged<T>(Expression<Func<T>> action)
        {
            var propertyName = GetPropertyName(action);
            this.RaisePropertyChanged(propertyName);
        }

        private static string GetPropertyName<T>(Expression<Func<T>> action)
        {
            var expression = (MemberExpression)action.Body;
            var propertyName = expression.Member.Name;
            return propertyName;
        }

        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    #endregion
}