using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
    ///     <MyNamespace:CustomControl1/>
    ///
    /// </summary>
    public class NumericUpDown : Control, INotifyPropertyChanged
    {
        private TextBox      _TextBox;
        private RepeatButton _UpButton;
        private RepeatButton _DownButton;
        public readonly static DependencyProperty MaximumProperty;
        public readonly static DependencyProperty MinimumProperty;
        public readonly static DependencyProperty ValueProperty;
        public readonly static DependencyProperty StepProperty;   

        static NumericUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(typeof(NumericUpDown)));
            MaximumProperty = DependencyProperty.Register("Maximum",    typeof(int), typeof(NumericUpDown), new UIPropertyMetadata(255));
            MinimumProperty = DependencyProperty.Register("Minimum",    typeof(int), typeof(NumericUpDown), new UIPropertyMetadata(0));
            StepProperty    = DependencyProperty.Register("StepValue",  typeof(int), typeof(NumericUpDown), new FrameworkPropertyMetadata(1));
            ValueProperty   = DependencyProperty.Register("Value",      typeof(int), typeof(NumericUpDown), new FrameworkPropertyMetadata(0));
        }

        public delegate void ValueChangeHandler(object sender);
        public event ValueChangeHandler OnValueChanged;



        #region DpAccessior
        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }
        public int Minimum
        {
            get { return (int)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }
        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetCurrentValue(ValueProperty, value); 
                if (PropertyChanged != null) {
                    PropertyChanged(this, new PropertyChangedEventArgs("Text"));
                    if (OnValueChanged != null)
                        OnValueChanged(this);
                }
            }
        }
        public int StepValue
        {
            get { return (int)GetValue(StepProperty); }
            set { SetValue(StepProperty, value); }
        }
        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            //_UpButton = Template.FindName("_UpButton", this) as RepeatButton;
            //_DownButton = Template.FindName("_DownButton", this) as RepeatButton;
            //_UpButton.Click += _UpButton_Click;
            //_DownButton.Click += _DownButton_Click;
        }

        void _DownButton_Click(object sender, RoutedEventArgs e)
        {
            if (Value > Minimum)
            {
                Value -= StepValue;
                if (Value < Minimum)
                    Value = Minimum;
            }
        }

        void _UpButton_Click(object sender, RoutedEventArgs e)
        {
            if (Value < Maximum)
            {
                Value += StepValue;
                if (Value > Maximum)
                    Value = Maximum;
            }
        }

        public String Text
        {
            get { return Value.ToString(); }
            set { int val; 
                  if (Int32.TryParse(value, NumberStyles.Number, null, out val)) {
                      Value = val;
                  } else {
                      Text = Text;
                  }
                  if (PropertyChanged != null)
                      PropertyChanged(this, new PropertyChangedEventArgs("Text"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ApllyTemplate();
        }

        private void ApllyTemplate()
        {
            var grid = new FrameworkElementFactory(typeof(Grid));
            
            var column1 = new FrameworkElementFactory(typeof(ColumnDefinition));
            //column1.SetValue(ColumnDefinition.WidthProperty, GridLength.Auto);
            column1.SetValue(ColumnDefinition.WidthProperty, new GridLength(90D, GridUnitType.Star));
            grid.AppendChild(column1);
            var column2 = new FrameworkElementFactory(typeof(ColumnDefinition));
            column2.SetValue(ColumnDefinition.WidthProperty, new GridLength(13D));   
            grid.AppendChild(column2);

            var row1 = new FrameworkElementFactory(typeof(ColumnDefinition));
            row1.SetValue(RowDefinition.HeightProperty, new GridLength(13D));
            grid.AppendChild(row1);
            var row2 = new FrameworkElementFactory(typeof(ColumnDefinition));
            row2.SetValue(RowDefinition.HeightProperty, new GridLength(13D));
            grid.AppendChild(row2);
            
            var textbox = new FrameworkElementFactory(typeof(TextBox));
            textbox.Name = "_TextBox";
            textbox.SetValue(Grid.RowProperty, 0);
            textbox.SetValue(Grid.ColumnProperty, 0);
            textbox.SetValue(Grid.RowSpanProperty, 2);
            textbox.SetValue(Grid.ColumnSpanProperty, 1);
            textbox.SetValue(TextBox.TextAlignmentProperty, TextAlignment.Right);
            textbox.SetValue(RepeatButton.ForegroundProperty, GetValue(Control.ForegroundProperty));
            Binding binding = new Binding("Text");
            //binding.Path = new PropertyPath(Text);
            binding.Source = this;
            textbox.SetBinding(TextBox.TextProperty, binding);
            grid.AppendChild(textbox);
            
            var btnup = new FrameworkElementFactory(typeof(RepeatButton));
            btnup.Name = "_UpButton";
            btnup.SetValue(Grid.ColumnProperty, 1);
            btnup.SetValue(Grid.RowProperty, 0);
            btnup.SetValue(RepeatButton.HorizontalContentAlignmentProperty, HorizontalAlignment.Center);
            btnup.SetValue(RepeatButton.VerticalContentAlignmentProperty, VerticalAlignment.Bottom);
            btnup.SetValue(RepeatButton.HeightProperty, 13D);
            btnup.SetValue(RepeatButton.VerticalAlignmentProperty, VerticalAlignment.Top);
            btnup.SetValue(RepeatButton.FontFamilyProperty, new FontFamily("Marlett"));
            btnup.SetValue(RepeatButton.FontSizeProperty, 9D);
            btnup.SetValue(RepeatButton.FontWeightProperty, FontWeights.UltraBlack);
            btnup.SetValue(RepeatButton.ForegroundProperty, GetValue(Control.ForegroundProperty));
            btnup.SetValue(RepeatButton.ContentProperty, "5");
            btnup.AddHandler(RepeatButton.ClickEvent, new RoutedEventHandler(_UpButton_Click));
            grid.AppendChild(btnup);

            var btndown = new FrameworkElementFactory(typeof(RepeatButton));
            btndown.Name = "_DownButton";
            btndown.SetValue(Grid.ColumnProperty, 1);
            btndown.SetValue(Grid.RowProperty, 1);
            btndown.SetValue(RepeatButton.HorizontalContentAlignmentProperty, HorizontalAlignment.Center);
            btndown.SetValue(RepeatButton.VerticalContentAlignmentProperty, VerticalAlignment.Top);
            btndown.SetValue(RepeatButton.HeightProperty, 13D);
            btndown.SetValue(RepeatButton.VerticalAlignmentProperty, VerticalAlignment.Bottom);
            btndown.SetValue(RepeatButton.FontFamilyProperty, new FontFamily("Marlett"));
            btndown.SetValue(RepeatButton.FontSizeProperty, 9D);
            btnup.SetValue(RepeatButton.FontWeightProperty, FontWeights.UltraBlack);
            btndown.SetValue(RepeatButton.ForegroundProperty, GetValue(Control.ForegroundProperty));
            btndown.SetValue(RepeatButton.ContentProperty, "6");
            btndown.AddHandler(RepeatButton.ClickEvent, new RoutedEventHandler(_DownButton_Click));
            grid.AppendChild(btndown);
            
            ControlTemplate template = new ControlTemplate(typeof(NumericUpDown));
            template.VisualTree = grid;
            SetValue(Control.TemplateProperty, template);
        }
    }
}
