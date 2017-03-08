using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace SonicStreamer.Common
{
    public sealed class SelectionModeToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is ListViewSelectionMode && (ListViewSelectionMode) value == ListViewSelectionMode.Multiple;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (value is bool && (bool) value) ? ListViewSelectionMode.Multiple : ListViewSelectionMode.None;
        }
    }
}