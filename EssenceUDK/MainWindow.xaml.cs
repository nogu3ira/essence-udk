using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using EssenceUDK.Platform;

namespace EssenceUDK
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // test - lets switch to our tab at startup
            tabControl1.SelectedIndex = 2;

            // now we need create base uo data storage class, it base class we will use to do any work with uo data, so generally we need to store it as static.
            // but just now for testing we dont do it. (Remember shilw we will write controls in EsseceUDK.Add-ins we need to get manager at EsseceUDK assembly)
            //var manager = new UODataManager(new Uri(@"C:\Ultima\Clients\SA"), UODataType.ClassicAdventuresOnHighSeas, false);
            
            // ok, we get manager just now let get tiles and set them as sourse to our list. Yeh, it's really simple)
            //tileItemView1.ItemsSource = manager.GetItemTile(); // just now we just get first 1000 items for testing

            // PS xaml is good, but lets devide all properties of controls in two types: visual-style and visual-logic.
            // The first one is part of theme or control design. The second are user customizable or controll changeble,
            // for example - sizes of tiles in tileItemView1 (we just add some Properties to it later). The idea is that if
            // we decide ti rewrite control in future to own we can easily change it without any problems.
        }
    }

}
