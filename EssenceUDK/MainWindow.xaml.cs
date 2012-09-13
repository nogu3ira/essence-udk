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

            // test
            tabControl1.SelectedIndex = 2;

            /*
            ImageSource img;
            
            var uomanager = new UODataManager(new Uri(@"C:\UltimaOnline\client"), UODataType.ClassicAdventuresOnHighSeas, false);
            foreach (var tile in uomanager.GetItemTile())
            {
                img = tile.Surface.Image;
                // Do evets
                if (Application.Current != null)
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));

            }*/
                

            //SetValue(ItemsSourceProperty, uomanager.GetItemTile());
        }


        public static List<BitmapImage> LoadImages()
		{
			List<BitmapImage> robotImages = new List<BitmapImage>();
			DirectoryInfo robotImageDir = new DirectoryInfo( @"C:\UltimaOnline\34\45\CustomItemsPanel\CustomItemsPanel\Robots" ); //new DirectoryInfo( @"..\..\Robots" );
			
            foreach( FileInfo robotImageFile in robotImageDir.GetFiles( "*.jpg" ) ) {
				Uri uri = new Uri( robotImageFile.FullName );
				robotImages.Add( new BitmapImage( uri ) );
			}
			return robotImages;
		}
    }

    public static class RobotImageLoader
    {
        public static List<BitmapImage> LoadImages()
        {
            List<BitmapImage> robotImages = new List<BitmapImage>();
            DirectoryInfo robotImageDir = new DirectoryInfo(@"C:\UltimaOnline\34\45\CustomItemsPanel\CustomItemsPanel\Robots"); //new DirectoryInfo( @"..\..\Robots" );

            foreach (FileInfo robotImageFile in robotImageDir.GetFiles("*.jpg"))
            //DirectoryInfo robotImageDir = new DirectoryInfo(@"C:\UltimaOnline\_build\uoFiddler\Extracted\Equip");

            //if (DesignerProperties.GetIsInDesignMode(this))
            //foreach( FileInfo robotImageFile in robotImageDir.GetFiles( "*.bmp" ) )
            {
                Uri uri = new Uri(robotImageFile.FullName);
                robotImages.Add(new BitmapImage(uri));
            }
            return robotImages;
        }
    }
}
