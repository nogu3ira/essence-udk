﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
using EssenceUDK.Platform;
using EssenceUDK.Platform.DataTypes;

namespace EssenceUDK.Controls.Common
{
    /// <summary>
    /// Выполните шаги 1a или 1b, а затем 2, чтобы использовать этот пользовательский элемент управления в файле XAML.
    ///
    /// Шаг 1a. Использование пользовательского элемента управления в файле XAML, существующем в текущем проекте.
    /// Добавьте атрибут XmlNamespace в корневой элемент файла разметки, где он 
    /// будет использоваться:
    ///
    ///     xmlns:MyNamespace="clr-namespace:EssenceUDK.Controls.Common"
    ///
    ///
    /// Шаг 1б. Использование пользовательского элемента управления в файле XAML, существующем в другом проекте.
    /// Добавьте атрибут XmlNamespace в корневой элемент файла разметки, где он 
    /// будет использоваться:
    ///
    ///     xmlns:MyNamespace="clr-namespace:EssenceUDK.Controls.Common;assembly=EssenceUDK.Controls.Common"
    ///
    /// Потребуется также добавить ссылку из проекта, в котором находится файл XAML,
    /// на данный проект и пересобрать во избежание ошибок компиляции:
    ///
    ///     Щелкните правой кнопкой мыши нужный проект в обозревателе решений и выберите
    ///     "Добавить ссылку"->"Проекты"->[Поиск и выбор проекта]
    ///
    ///
    /// Шаг 2)
    /// Теперь можно использовать элемент управления в файле XAML.
    ///
    ///     <MyNamespace:TileBaseView/>
    ///
    /// </summary>
    
    public abstract class TileBaseView : ListBox
    {
        #region Control Properties
        
        [Description("Enable and disable multiselect."), Category("EssenceUDK.Controls")]
        public bool MultiSelect {
            get { return (bool)GetValue(MultiSelectProperty); }
            set { SetValue(MultiSelectProperty, value); }
        }
        private static readonly DependencyProperty MultiSelectProperty = DependencyProperty.Register(
                        "MultiSelect", typeof(bool), typeof(TileBaseView), new UIPropertyMetadata());


        [Description("Source Collection<Image> of tiles."), Category("EssenceUDK.Controls")]
        public new IEnumerable<ISurface> ItemsSource {
            get { return (IEnumerable<ISurface>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        #endregion

        protected virtual string _SourcePath { get { return "Image"; } }

        static TileBaseView()
        {
            /*
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TileBaseView), new FrameworkPropertyMetadata(typeof(TileBaseView)));

            Setter setter = new Setter();
            setter.Property = TileBaseView.TemplateProperty;
            ControlTemplate template = new ControlTemplate(typeof(TileBaseView));
            var grid = new FrameworkElementFactory(typeof(Grid));

            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BackgroundProperty, Brushes.Crimson);
            border.SetValue(Border.WidthProperty, 15.0);
            border.SetValue(Border.NameProperty, "mask");
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(15));
            grid.AppendChild(border);

            template.VisualTree = grid;
            setter.Value = template;
            */
        }

        internal TileBaseView() : base()
        { 
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
                   
            //SetValue(BackgroundProperty, Brushes.Chartreuse);
            SetValue(BackgroundProperty, Brushes.Transparent);
            SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
            SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Visible);


            


            var ip = new ItemsPanelTemplate();
            var panel = new FrameworkElementFactory(typeof(WrapPanel));
            ip.VisualTree = panel;
            SetValue(ItemsPanelProperty, ip);


            //var templ = new ControlTemplate(typeof(TileBaseView));
            ControlTemplate template = new ControlTemplate(typeof(TileBaseView));
            var grid = new FrameworkElementFactory(typeof(Grid));

            var border = new FrameworkElementFactory(typeof(Border));
        //    border.SetValue(Border.BorderBrushProperty, Brushes.Crimson);
        //    border.SetValue(Border.BorderThicknessProperty, new Thickness(2.0));
            //border.SetValue(Border.WidthProperty, 2.0);
            //border.SetValue(Border.NameProperty, "mask");
        //    border.SetValue(Border.CornerRadiusProperty, new CornerRadius(5));
            grid.AppendChild(border);

            template.VisualTree = grid;
            //SetValue(TemplateProperty, template);


            var datatemplate = new DataTemplate(typeof(List<BitmapImage>));

            grid = new FrameworkElementFactory(typeof(Grid));
            border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BorderBrushProperty, Brushes.Black);
            border.SetValue(Border.BorderThicknessProperty, new Thickness(2.0));
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(5));
            border.SetValue(Border.MarginProperty, new Thickness(1.0, 1.0, 1.0, 1.0));
            grid.AppendChild(border);

            var image = new FrameworkElementFactory(typeof(Image));
            image.SetValue(Image.StretchProperty, Stretch.Fill);
            var imgsrs = new Binding();

            imgsrs.Path = new PropertyPath(_SourcePath);
            image.SetValue(Image.SourceProperty, imgsrs);
            image.SetValue(Image.WidthProperty, 44.0);
            image.SetValue(Image.HeightProperty, 44.0);
            border.AppendChild(image);
            
            datatemplate.VisualTree = grid;
            SetValue(ItemTemplateProperty, datatemplate);

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                //var uomanager = new UODataManager(new Uri(@"C:\UltimaOnline\client"), UODataType.ClassicAdventuresOnHighSeas, false);
                //SetValue(ItemsSourceProperty, uomanager.GetItemTile());

            }
        }

        /*
         * = Визуальные:
         * - - Размеры элементов
         * - - Рамка (толщина, цвет)
         * - - Растояние между элементами
         * = Поведение:
         * - - Мультивыбор
         * = Отображение:
         * - - Растягивание/обрезание
         * - - Область источника изображения
         */
        //override defa

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
        }
        
    }
}
