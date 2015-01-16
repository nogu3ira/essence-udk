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
﻿using PixelFormat = EssenceUDK.Platform.DataTypes.PixelFormat;
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
            tabControl1.SelectedIndex = 6;

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

        private static bool defaultflat = true;
        private int cmdlast = defaultflat ? 10 : 20;
        private UODataManager UOManager;
        private ISurface surf = null;

        private void Render(int cmd)
        {
            var flt = cmd < 20;
            //if (flt)
            //    cmd -= 10;
            //else
            //    cmd -= 20;
            cmdlast = flt ? 10 : 20;

            switch (cmd) {
                case 17: goto case 28;
                case 13: goto case 22;
                case 19: goto case 26;
                case 11: goto case 24;
                case 18: goto case 29;
                case 12: goto case 21;
                case 14: goto case 27;
                case 16: goto case 23;
                case 27: nudX.Value -= 1; break;
                case 23: nudX.Value += 1; break;
                case 29: nudY.Value -= 1; break;
                case 21: nudY.Value += 1; break;
                case 28: nudX.Value -= 1; nudY.Value -= 1; break;
                case 22: nudX.Value += 1; nudY.Value += 1; break;
                case 24: nudX.Value -= 1; nudY.Value += 1; break;
                case 26: nudX.Value += 1; nudY.Value -= 1; break;
                default: break;
            }

            var map = (byte)nudM.Value;
            var range = (byte)nudR.Value;
            var tcx = (ushort)nudX.Value;
            var tcy = (ushort)nudY.Value;
            var minz = (sbyte)nudMinZ.Value;
            var maxz = (sbyte)nudMaxZ.Value;
            var alt = (sbyte)(map == 1 ? -45 : 0);
            

            //if (surf == null)
                //surf = UOManager.CreateSurface((ushort)2560, (ushort)1600, PixelFormat.Bpp16X1R5G5B5);
            #if DEBUG
                surf = UOManager.CreateSurface((ushort)1200, (ushort)1200, PixelFormat.Bpp16X1R5G5B5);
            #else
                surf = UOManager.CreateSurface((ushort)2560, (ushort)1600, PixelFormat.Bpp16X1R5G5B5);
            #endif

            

            if (flt)
                UOManager.FacetRender.DrawFlatMap(map, alt, ref surf, range, tcx, tcy, minz, maxz);
            else
                UOManager.FacetRender.DrawObliqueMap(map, alt, ref surf, range, tcx, tcy, minz, maxz);

            //var bid = UOManager.GetMapFacet(map).GetBlockId((uint)nudX.Value, (uint)nudY.Value);
            //UOManager.FacetRender.DrawBlock(ref surf, map, bid);
            imgFacet.Source = surf.GetSurface().Image;
        }

        private void btnRender_Click(object sender, RoutedEventArgs e)
        {
            var tag = Convert.ToInt32((sender as Button).Tag);
            Render(tag);
        }

        private void keyRender_MoveU(object sender, ExecutedRoutedEventArgs e)
        {
            Render(cmdlast + 8);
        }

        private void keyRender_MoveD(object sender, ExecutedRoutedEventArgs e)
        {
            Render(cmdlast + 2);
        }

        private void keyRender_MoveL(object sender, ExecutedRoutedEventArgs e)
        {
            Render(cmdlast + 4);
        }

        private void keyRender_MoveR(object sender, ExecutedRoutedEventArgs e)
        {
            Render(cmdlast + 6);
        }

        private void keyRender_MoveUL(object sender, ExecutedRoutedEventArgs e)
        {
            Render(cmdlast + 7);
        }

        private void keyRender_MoveUR(object sender, ExecutedRoutedEventArgs e)
        {
            Render(cmdlast + 9);
        }

        private void keyRender_MoveDL(object sender, ExecutedRoutedEventArgs e)
        {
            Render(cmdlast + 1);
        }

        private void keyRender_MoveDR(object sender, ExecutedRoutedEventArgs e)
        {
            Render(cmdlast + 3);
        }

        private void keyRender_MoveO(object sender, ExecutedRoutedEventArgs e)
        {
            Render(cmdlast + 0);
        }

        private void AddHotKeys(ExecutedRoutedEventHandler handler, Key key, ModifierKeys mod = ModifierKeys.None)
        {
            try {
                RoutedCommand firstSettings = new RoutedCommand();
                firstSettings.InputGestures.Add(new KeyGesture(key, mod));
                CommandBindings.Add(new CommandBinding(firstSettings, handler));
                //private void My_first_event_handler(object sender, ExecutedRoutedEventArgs e) 
                //private void My_second_event_handler(object sender, RoutedEventArgs e)
            }
            catch (Exception err)
            {
                //handle exception error
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var datauri = new Uri(@"C:\UltimaOnline\distro\__deploy\__output\uoassets\_override");
            var dataopt = new UODataOptions();
            dataopt.majorFacet[0] = FacetDesc.Dangeons;
            dataopt.majorFacet[1] = FacetDesc.Assidiya;
            var _manager = new UODataManager(datauri, UODataType.ClassicAdventuresOnHighSeas, UOLang.Russian, dataopt, true);
            UOManager = _manager;

            AddHotKeys(keyRender_MoveU, Key.Up);
            AddHotKeys(keyRender_MoveL, Key.Left);
            AddHotKeys(keyRender_MoveR, Key.Right);
            AddHotKeys(keyRender_MoveD, Key.Down);

            AddHotKeys(keyRender_MoveU, Key.NumPad8);
            AddHotKeys(keyRender_MoveL, Key.NumPad4);
            AddHotKeys(keyRender_MoveR, Key.NumPad6);
            AddHotKeys(keyRender_MoveD, Key.NumPad2);

            AddHotKeys(keyRender_MoveUL, Key.NumPad7);
            AddHotKeys(keyRender_MoveUR, Key.NumPad9);
            AddHotKeys(keyRender_MoveDL, Key.NumPad1);
            AddHotKeys(keyRender_MoveDR, Key.NumPad3);

            AddHotKeys(keyRender_MoveUL, Key.Home);
            AddHotKeys(keyRender_MoveUR, Key.PageUp);
            AddHotKeys(keyRender_MoveDL, Key.End);
            AddHotKeys(keyRender_MoveDR, Key.PageDown);


            nudMinZ.Minimum = nudMaxZ.Minimum = nudMinZ.Value = - 128;
            nudMinZ.Maximum = nudMaxZ.Maximum = nudMaxZ.Value = + 127;

            nudM.Minimum = 0;
            nudM.Maximum = 5;
            nudR.Minimum = 0;
            nudR.Maximum = 255;

            nudX.Minimum = 0;
            nudX.Maximum = 12288;
            nudY.Minimum = 0;
            nudY.Maximum = 8192;

            nudM.Value = 1;
            nudR.Value = 30;
            nudX.Value = 7320;//913 * 8 + 3;
            nudY.Value = 3364;//411 * 8 + 3;
            this.Width = 1278;
            this.Height = 938;
        }

        
    }

}
