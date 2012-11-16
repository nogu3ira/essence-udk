using System.Windows;
using GalaSoft.MvvmLight.Threading;
using MapMakerApplication.Messages;

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

        private void ApplicationDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            AppMessages.DialogRequest.Send(new MessageDialogRequest(e.Exception.InnerException.InnerException.Message));
            e.Handled = true;
        }
    }
}
