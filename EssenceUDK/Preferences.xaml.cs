using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EssenceUDK.Resources;

namespace EssenceUDK
{
    /// <summary>
    /// Логика взаимодействия для Preferences.xaml
    /// </summary>
    public partial class Preferences : UserControl
    {
        public Preferences()
        {
            InitializeComponent();

            cbTheme.ItemsSource = ThemeManager.GetThemes();
            cbLocal.ItemsSource = LocalManager.GetLocals();

            if (cbTheme.Items.Count > 0)
                cbTheme.SelectedIndex = 0;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void cbTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0) {
                string theme = e.AddedItems[0].ToString();
                
                // Window Level
                // this.ApplyTheme(theme);

                // Application Level
                Application.Current.ApplyTheme(theme);
            }
        }

        private void cbLocal_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0) {
                string local = e.AddedItems[0].ToString();
                Application.Current.ApplyLocal(local);
            }
        }


    }
}
