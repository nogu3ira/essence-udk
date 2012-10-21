using System.Windows;
using GalaSoft.MvvmLight.Threading;

namespace MapMakerApplication
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ApplicationController ApplicationController { get; set; }
        public App()
        {
            DispatcherHelper.Initialize();

            ApplicationController = new ApplicationController();
        }
    }
}
