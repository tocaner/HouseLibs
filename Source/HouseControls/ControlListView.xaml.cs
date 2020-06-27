using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
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


namespace HouseControls
{
  public enum OverlayEnum { None, Check, Error };


  class SampleListObject
  {
    // Recommended properties, used by the view styles
    public string Name { get; set; }
    public bool IsChecked { get; set; }
    public ImageSource Thumbnail { get; set; }
    public int OverlayIndex { get; set; }
  }


  public class ItemEventArgs : EventArgs
  {
    public object Item { get; set; }
  }

  
  /// <summary>
  /// Interaction logic for ControlListView.xaml
  /// </summary>
  public partial class ControlListView : UserControl
  {
    private DragDropAssist _dragDrop = null;
    public DragDropAssist DragDropAssist
    {
      get
      {
        if (_dragDrop == null)
        {
          _dragDrop = new DragDropAssist(lv);
        }
        return _dragDrop;
      }
    }


    public ControlListView()
    {
      InitializeComponent();
      this.ShowCheckboxes = true; // Triggers change from true to false in the component

      // Initialize overlays
      ImageSource[] overlays = new ImageSource[Enum.GetValues(typeof(OverlayEnum)).Length];
      foreach (var e in Enum.GetValues(typeof(OverlayEnum)))
      {
        overlays[(int)e] = (ImageSource)TryFindResource(e.ToString());
      }
      this.InitOverlays(overlays);
    }


    public void InitOverlays(ImageSource[] overlays)
    {
      OverlayConverter.Overlays = overlays;
    }


    public List<object> GetCheckedItems()
    {
      List<object> result = new List<object>();

      foreach (var item in ItemsSource)
      {
        PropertyInfo property = item.GetType().GetProperty("IsChecked");

        if(property != null)
        {
          if ((bool)(property.GetValue(item)) == true)
          {
            result.Add(item);
          }
        }
      }

      return result;
    }


    #region DependencyProperties

    // ColumnNames ------------------------------------------------------------
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


    // ItemsSource ------------------------------------------------------------
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


    // ViewStyle ------------------------------------------------------------
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


    // ShowCheckboxes ------------------------------------------------------------
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
                GridView result = (GridView)c.GetViewStyle("GridView");
                result.Columns[0].Width = c.ShowCheckboxes ? Double.NaN : 0;
              }
            }));


    // MultiSelect ---------------------------------------------------------------
    public bool MultiSelect
    {
      get { return (bool)GetValue(MultiSelectProperty); }
      set { SetValue(MultiSelectProperty, value); }
    }

    private static readonly DependencyProperty MultiSelectProperty =
        DependencyProperty.Register("MultiSelect", typeof(bool), typeof(ControlListView),
          new PropertyMetadata(true,
            (DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
            {
              ControlListView c = sender as ControlListView;
              if (c != null)
              {
                c.lv.SelectionMode = c.MultiSelect ? SelectionMode.Extended : SelectionMode.Single;
              }
            }));

    #endregion DependencyProperties


    #region Events

    public event EventHandler SelectionChanged;

    private void Raise_SelectionChanged(object item)
    {
      SelectionChanged?.Invoke(this, new ItemEventArgs { Item = item });
    }


    public event EventHandler ItemInvoked;

    private void Raise_ItemInvoked(object item)
    {
      ItemInvoked?.Invoke(this, new ItemEventArgs { Item = item });
    }

    #endregion Events


#if NOT_USED

    public void AddViewStyle(string name, ViewBase view)
    {
      this.Resources.Add(name, view);
    }

#endif


    private void RefreshView()
    {
      lv.View = GetViewStyle(ViewStyle);
    }


    private ViewBase GetViewStyle(string str)
    {
      ViewBase result = TryFindResource(str) as ViewBase;

      if (str == "ListView")
      {
        InitListView(result as GridView);
      }
      else if (str == "GridView")
      {
        InitGridView(result as GridView);
      }
      else
      { }

      return result;
    }


    private void InitListView(GridView listView)
    {
      listView.Columns.Clear();

      for (int i = 0; i < ColumnNames.Length; i++)
      {
        string columnName = ColumnNames[i];
        
        listView.Columns.Add(new GridViewColumn
        {
          Header = columnName,
          DisplayMemberBinding = new Binding(columnName)
        });
      }
    }


    private void InitGridView(GridView gridView)
    {
      // Static columns
      GridViewColumn col0 = gridView.Columns[0];
      GridViewColumn col1 = gridView.Columns[1];

      gridView.Columns.Clear();
      gridView.Columns.Add(col0);
      gridView.Columns.Add(col1);

      for (int i = 0; i < ColumnNames.Length; i++)
      {
        string columnName = ColumnNames[i];

        if (i == 0)
        {
          // Checkbox column, no change
          col0.Header = columnName;
        }
        else if (i == 1)
        {
          // Icon + name column
          col1.Header = columnName;
        }
        else
        {
          // Rest of the columns: Properties
          gridView.Columns.Add(new GridViewColumn
          {
            Header = columnName,
            DisplayMemberBinding = new Binding(columnName)
          });
        }
      }
    }


    public object CurrentItem { get; private set; }


    private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (lv.SelectedItems.Count > 0)
      {
        CurrentItem = lv.SelectedItems[0];
      }
      else
      {
        CurrentItem = null;
      }

      Raise_SelectionChanged(CurrentItem);
    }


    private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      Raise_ItemInvoked(CurrentItem);
    }
  }


  public class OverlayConverter : IValueConverter
  {
    static public ImageSource[] Overlays;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return Overlays[(int)value];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
