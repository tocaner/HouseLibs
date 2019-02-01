using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TestThreads
{
  public abstract class SimpleConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return Convert(value);
    }

    protected abstract object Convert(object value);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }


  public class BoolToVisibleCollapsed : SimpleConverter
  {
    protected override object Convert(object value)
    {
      return (bool)value ? Visibility.Visible : Visibility.Collapsed;
    }
  }


  public class BoolInverter : SimpleConverter
  {
    protected override object Convert(object value)
    {
      return (bool)value ? false : true;
    }
  }
}
