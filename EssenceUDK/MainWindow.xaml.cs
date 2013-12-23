﻿using System;
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
﻿using EssenceUDK.Platform.DataTypes;
using EssenceUDK.Platform.UtilHelpers;
﻿using EssenceUDK.Resources;
﻿using UOLang = EssenceUDK.Platform.UtilHelpers.Language;

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
            Application.Current.ApplyTheme("ExpressionDark"); 
            // ThemeManager.GetThemes()[0];

            // test - lets switch to our tab at startup
            tabControl1.SelectedIndex = 2;

            // Lets try to find clients at system
            var clients = ClientInfo.GetInSystem();
            if (clients.Length != 0) { // we must remember that we have no warinties that client is cirtanly valid

                // now we need create base uo data storage class, it base class we will use to do any work with uo data, so generally we need to store it as static.
                // but just now for testing we dont do it. (Remember shilw we will write controls in EsseceUDK.Add-ins we need to get manager at EsseceUDK assembly)
                var manager = new UODataManager(new Uri(clients[0].DirectoryPath), UODataType.ClassicAdventuresOnHighSeas, UOLang.Russian, new UODataOptions(), true);// false);
                userControlTileMerger.UODataManager = manager;

                // ok, we get manager just now let get tiles and set them as sourse to our list. Yeh, it's really simple)
                var items = manager.GetItemTile(TileFlag.None, true);//TileFlag.Wall); // lets get all walls to look throw
                //foreach (var item in items)
                //    item.Surface.GetSurface().GetHammingDistanceForAvrHash(null);
                tileItemView1.ItemsSource = items;
                

                // just now we use same souce for binding to differen controls. So we represent different data viewer for same data.
                var lands = manager.GetLandTile(TileFlag.None).Where(t => t.EntryId < 1000); // just now we get first 1000 valid lands for testing (we dont take care what is this)
                tileLandView1.ItemsSource = lands;
                tileTexmView1.ItemsSource = lands;

                //manager.GetLandTile(0x0001).Surface = manager.CreateSurface(@"E:\______________________\3d\++\ss\L0x002A.bmp");
                //manager.GetLandTile(0x0002).Surface = manager.CreateSurface(@"E:\______________________\3d\++\ss\L0x0089.bmp");
                //manager.GetLandTile(0x0003).Surface = manager.CreateSurface(@"E:\______________________\3d\++\ss\L0x321D.bmp");
                //manager.GetLandTile(0x0004).Surface = manager.CreateSurface(@"E:\______________________\3d\++\ss\L0x3472.bmp");
                //manager.GetLandTile(0x0005).Surface = manager.CreateSurface(@"E:\______________________\3d\++\ss\L0x346E.bmp");
                //manager.GetLandTile(0x0006).Surface = manager.CreateSurface(@"E:\______________________\3d\++\ss\L0x3475.bmp");

                //manager.GetLandTile(0x0001).Texture = manager.CreateSurface(@"E:\______________________\3d\++\ss\T0x002A.bmp");
                //manager.GetLandTile(0x0002).Texture = manager.CreateSurface(@"E:\______________________\3d\++\ss\T0x0089.bmp");
                //manager.GetLandTile(0x0003).Texture = manager.CreateSurface(@"E:\______________________\3d\++\ss\T0x321D.bmp");
                //manager.GetLandTile(0x0004).Texture = manager.CreateSurface(@"E:\______________________\3d\++\ss\T0x3472.bmp");
                //manager.GetLandTile(0x0005).Texture = manager.CreateSurface(@"E:\______________________\3d\++\ss\T0x346E.bmp");
                //manager.GetLandTile(0x0006).Texture = manager.CreateSurface(@"E:\______________________\3d\++\ss\T0x3475.bmp");

                //manager.GetItemTile(0x0001).Surface = manager.CreateSurface(@"E:\______________________\3d\++\ss\I0xF6C2.bmp");
                //manager.GetItemTile(0x0002).Surface = manager.CreateSurface(@"E:\______________________\3d\++\ss\I0xF6FC.bmp");
                //manager.GetItemTile(0x0003).Surface = manager.CreateSurface(@"E:\______________________\3d\++\ss\I0xF6BA.bmp");
                //manager.GetItemTile(0x0004).Surface = manager.CreateSurface(@"E:\______________________\3d\++\ss\I0x3BB4.bmp");
                //manager.GetItemTile(0x0005).Surface = manager.CreateSurface(@"E:\______________________\3d\++\ss\I0xF6A2.bmp");
                //manager.GetItemTile(0x0006).Surface = manager.CreateSurface(@"E:\______________________\3d\++\ss\I0x248B.bmp");

                // PS xaml is good, but lets devide all properties of controls in two types: visual-style and visual-logic.
                // The first one is part of theme or control design. The second are user customizable or controll changeble,
                // for example - sizes of tiles in tileItemView1 (we just add some Properties to it later). The idea is that if
                // we decide ti rewrite control in future to own we can easily change it without any problems.

            } else {
                // it's seems we cant find clients so we just throw Exception
                throw new Exception("No one \"Ultima Online\" client was founded.");
            }

            
        }
    }

}
