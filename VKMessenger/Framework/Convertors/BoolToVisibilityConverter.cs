using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VKMessenger.Framework.Convertors
{
  public class BoolToVisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if ((bool) value)
        return (object) Visibility.Visible;
      return (object) Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}