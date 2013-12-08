using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Runtime.CompilerServices;
using EssenceUDK.Controls.Common;
using EssenceUDK.Controls.Ultima;
using EssenceUDK.Platform;
using EssenceUDK.Platform.DataTypes;
using EssenceUDK.Platform.UtilHelpers;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using UOLang = EssenceUDK.Platform.UtilHelpers.Language;
using UserControl = System.Windows.Controls.UserControl;

namespace EssenceUDK.Add_ins.Client
{
    /// <summary>
    /// Логика взаимодействия для TileMerger.xaml
    /// </summary>
    public partial class TileMerger : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null) { 
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private UODataManager _UODataManager = null;
        public  UODataManager UODataManager { get { return _UODataManager; } set
        {
            if (_UODataManager == value)
                return;
            _UODataManager = value;
            var items = _UODataManager.GetItemTile(TileFlag.None, true);//TileFlag.Wall); // lets get all walls to look throw
            tileItemView1.ItemsSource = items;
            tileItemView1.UpdateLayout();

            DirectoryPath = value.Location.AbsolutePath;
            tbDirectory_KeyDown(null, null);
        } }

        private IEnumerable<ModelItemData> items = null;

        public TileMerger()
        {
            InitializeComponent();          
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            // Бред, но иначе чекбоксы по умолчанию не отображаются правильно
            foreach (var rb in this.FindVisualChildren<System.Windows.Controls.RadioButton>().Where(r => r.IsChecked.Value)) {
                rb.IsChecked = false;
                rb.IsChecked = true;
            }

            //DirectoryPath = @"C:\UltimaOnline\tools\Fiddler+\Extracted\test";
            tbDirectory_KeyDown(null, null);
        }

        // -------------

        private void Comparison()
        {
            var surf = (imgSelectedFile.Tag as IImageSurface);
            if (surf == null)
                return;
            var dif = nudHamming.Value;
            if ((int)tileItemView1.Tag == 0)
                tileItemView1.ItemsSource = _UODataManager.GetItemTile(TileFlag.None, true).Where(t => t.Surface.GetSurface().GetHammingDistanceForAvrHash(surf) <= dif);
            else if ((int)tileItemView1.Tag == 1)
                tileItemView1.ItemsSource = _UODataManager.GetLandTile(TileFlag.None, true).Where(t => t.Surface != null && t.Surface.GetSurface().GetHammingDistanceForAvrHash(surf) <= dif);
            else if ((int)tileItemView1.Tag == 2)
                tileItemView1.ItemsSource = _UODataManager.GetLandTile(TileFlag.None, true).Where(t => t.Texture != null && t.Texture.GetSurface().GetHammingDistanceForAvrHash(surf) <= dif);
        }

        private  int _HammingVal = 0;
        internal int HammingVal { get { return _HammingVal; } set
        {
            // BUG: Shit - it don't work when property is changed in GUI...
            _HammingVal = value; 
            Comparison();
        } }

        // -------------

        internal IEnumerable<FileEntry> FileEntries = null;
        private void lvFileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var file = e.AddedItems[e.AddedItems.Count - 1] as FileEntry;
            var surf = UODataManager.CreateSurface(System.IO.Path.Combine(_DirectoryPath, file.FileName + file.FileExts)).GetSurface();
            imgSelectedFile.Source = surf.Image;
            imgSelectedFile.Tag = surf;

            Comparison();
        }

        private void rbTileType_Checked(object sender, RoutedEventArgs e)
        {
            var rb = sender as System.Windows.Controls.RadioButton;
            if (rb == null || _UODataManager == null) return;
            int tag = -1;
            if (rb.Tag is int) tag = (int)rb.Tag;
            if (rb.Tag is string) tag = int.Parse((string)rb.Tag);

            tileItemView1.Texture = tag == 2;
            tileItemView1.Tag = tag;
            tileItemView1.UpdateLayout();

            Comparison();
        }

        // -------------

        private  string _DirectoryPath { get; set; }
        internal string DirectoryPath { get { return _DirectoryPath; } set
        {
            _DirectoryPath = value;
            tbDirectory.Text = value;
            //NotifyPropertyChanged("DirectoryPath");
        } }

        private void tbDirectory_KeyDown(object sender, KeyEventArgs e)
        {
            if (e == null || e.Key == Key.Return) {
                _DirectoryPath = tbDirectory.Text;
                var searchPattern = new Regex(@"\.(bmp|png|tif|tiff|gif)$", RegexOptions.IgnoreCase);
                FileEntries = Directory.GetFiles(_DirectoryPath).Where(f => searchPattern.IsMatch(f)).Select(f => new FileEntry(f));
                lvFileList.ItemsSource = FileEntries;
            }
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.SelectedPath = DirectoryPath;
            dialog.ShowNewFolderButton = false;
            if (dialog.ShowDialog() == DialogResult.OK) {
                DirectoryPath = dialog.SelectedPath;
                tbDirectory_KeyDown(null, null);
            }
        }

        // -------------

        

        
    }

    internal class FileEntry
    { 
        public string FileName { get; set; }
        public string FileExts { get; set; }
        public ImageSource FileIcon { get; set; }

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        internal FileEntry(string path)
        {
            FileName = System.IO.Path.GetFileNameWithoutExtension(path);
            FileExts = System.IO.Path.GetExtension(path);

            IntPtr ptr = Icon.ExtractAssociatedIcon(path).ToBitmap().GetHbitmap();
            FileIcon =  System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        ptr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(ptr);
        }
    }
}
