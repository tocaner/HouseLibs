using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HouseImaging;


namespace HouseControls
{
  class SampleListObject
  {
    // Recommended properties, used by the view styles
    public bool IsChecked { get; set; }
    public string Image { get; set; }
    public string Title { get; set; }
  }


  /// <summary>
  /// Interaction logic for ControlListView.xaml
  /// </summary>
  public partial class ControlListView : UserControl
  {
    public ControlListView()
    {
      InitializeComponent();
      this.ShowCheckboxes = false; // Triggers change from true to false in the component
    }

    // First place is the Checkbox column
    // Second place is the Image/Title column
    public string[] ColumnNames
    {
      get { return (string[])GetValue(ColumnNamesProperty); }
      set { SetValue(ColumnNamesProperty, value); }
    }

    private static readonly DependencyProperty ColumnNamesProperty =
        DependencyProperty.Register("ColumnNames", typeof(string[]), typeof(ControlListView),
          new PropertyMetadata(new string[0],  
            (DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
            {
              ControlListView c = sender as ControlListView;
              if (c != null)
              {
                c.RefreshView();
              }
            }));


    public IEnumerable ItemsSource
    {
      get { return (IEnumerable)GetValue(ItemsSourceProperty); }
      set { SetValue(ItemsSourceProperty, value); }
    }

    private static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(ControlListView),
          new PropertyMetadata(null,
            (DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
            {
              ControlListView c = sender as ControlListView;
              if (c != null)
              {
                c.lv.ItemsSource = c.ItemsSource;
              }
            }));


    public string ViewStyle
    {
      get { return (string)GetValue(ViewStyleProperty); }
      set { SetValue(ViewStyleProperty, value); }
    }

    private static readonly DependencyProperty ViewStyleProperty =
        DependencyProperty.Register("ViewStyle", typeof(string), typeof(ControlListView),
          new PropertyMetadata("",
            (DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
            {
              ControlListView c = sender as ControlListView;
              if (c != null)
              {
                c.RefreshView();
              }
            }));


    public bool ShowCheckboxes
    {
      get { return (bool)GetValue(ShowCheckboxesProperty); }
      set { SetValue(ShowCheckboxesProperty, value); }
    }

    private static readonly DependencyProperty ShowCheckboxesProperty =
        DependencyProperty.Register("ShowCheckboxes", typeof(bool), typeof(ControlListView),
          new PropertyMetadata(true,
            (DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
            {
              ControlListView c = sender as ControlListView;
              if (c != null)
              {
                GridView result = (GridView)c.GetGridView();
                result.Columns[0].Width = c.ShowCheckboxes ? Double.NaN : 0;
              }
            }));


    public void RefreshView()
    {
      lv.View = GetView(ViewStyle);
    }


    private ViewBase GetView(string str)
    {
      switch (str)
      {
        case "GridView": return GetGridView();
        case "IconView": return GetIconView();
        case "TileView": return GetTileView();
        default: return GetGridView();
      }
    }


    private ViewBase GetGridView()
    {
      GridView result = lv.FindResource("GridView") as GridView;

      for (int i = 0; i < ColumnNames.Length; i++)
      {
        string columnName = ColumnNames[i];

        if (i < result.Columns.Count)
        {
          // Static columns: Checkbox and Item (Image/Title)
          result.Columns[i].Header = columnName;
        }
        else
        {
          // Rest of the colums: Properties
          result.Columns.Add(new GridViewColumn
          {
            Header = columnName,
            DisplayMemberBinding = new Binding(columnName)
          });
        }
      }

      return result;
    }


    private ViewBase GetIconView()
    {
      PlainView result = lv.FindResource("IconView") as PlainView;
      return result;
    }


    private ViewBase GetTileView()
    {
      PlainView result = lv.FindResource("TileView") as PlainView;
      return result;
    }
  }


  public class ThumbnailGenerator : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      ImageInfo img = ImageInfo.CreateImageInfo((string)value);
      ImageInfo thumb = img != null ? img.GetThumbnail(100, 100) : null;
      return thumb != null ? thumb.GetSystemImageSource() : null;// img != null ? img.GetThumbnail(100, 100) : null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }


  public class ShowHideColumnWidth : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      bool show = (bool)value;
      return show ? Double.NaN : 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
