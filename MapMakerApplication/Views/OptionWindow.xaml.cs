using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MapMakerApplication.Messages;

namespace MapMakerApplication
{
	/// <summary>
	/// Interaction logic for OptionWindow.xaml
	/// </summary>
	public partial class OptionWindow : Window
	{
		public OptionWindow()
		{
			this.InitializeComponent();
            AppMessages.DialogRequest.Register(this, MessageDialogRequestHandler);
            AppMessages.OptionAnswer.Register(this, MessageOptionHandler);
            
			// Insert code required on object creation below this point.
		}
        
        private void MessageOptionHandler(OptionMessage message)
        {
            Close();
        }


        private void MessageDialogRequestHandler(MessageDialogRequest request)
        {
            var folderdialog = new System.Windows.Forms.FolderBrowserDialog();
            switch (request.Content)
            {
                case "OpenOptionFolder":
                    {
                        var result = folderdialog.ShowDialog();
                        if (result == System.Windows.Forms.DialogResult.OK)
                        {
                            AppMessages.DialogAnwer.Send(new MessageDialogResult(folderdialog.SelectedPath) { Type = DialogType.OpenOptionFolder });
                        }
                    }
                    break;
               
            }
        }
	}
}