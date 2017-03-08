using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace SonicStreamer.Common
{
    public sealed class BooleanToColorConverter : IValueConverter
    {
        private readonly Color _orange = Color.FromArgb(255, 255, 165, 0);
        private readonly Color _white = Color.FromArgb(255, 255, 255, 255);

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value is bool && (bool) value) ? _orange : _white;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var brush = value as SolidColorBrush;
            return brush != null && brush.Color == _white;
        }
    }
}