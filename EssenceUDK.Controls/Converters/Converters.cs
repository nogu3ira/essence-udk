using System;
using System.Windows.Data;
using EssenceUDK.Platform;

namespace EssenceUDK.Controls.Converters
{
    public class ConverterImageSourceItemsLandsFromISourface : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var surface = value as ISurface;
            if (surface != null)
            {
                var image = surface.Image.Clone();
                return image;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
 }
