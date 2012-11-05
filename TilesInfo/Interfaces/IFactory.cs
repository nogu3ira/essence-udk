using System.Collections.ObjectModel;
using EssenceUDK.TilesInfo.Components;

namespace EssenceUDK.TilesInfo.Interfaces
{
    public interface IFactory
    {
        ObservableCollection<TileCategory> Categories { get; }
        void Populate();
        
    }
}
