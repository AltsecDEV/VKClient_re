using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VKMessenger.Framework.Convertors
{
  public class BoolToVisibilityInveterterConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if ((bool) value)
        return (object) Visibility.Collapsed;
      return (object) Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
