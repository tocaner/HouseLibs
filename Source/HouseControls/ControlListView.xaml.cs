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


namespace HouseControls
{
  class SampleListObject
  {
    // Recommended properties, used by the view styles
    public bool IsChecked { get; set; }
    public ImageSource Thumbnail { get; set; }
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

    #endregion DependencyProperties


    public void AddViewStyle(string name, ViewBase view)
    {
      this.Resources.Add(name, view);
    }


    private void RefreshView()
    {
      lv.View = GetViewStyle(ViewStyle);
    }


    private ViewBase GetViewStyle(string str)
    {
      ViewBase result;

      try
      {
        result = FindResource(str) as ViewBase;

        if (result is GridView)
        {
          InitGridView(result as GridView);
        }
      }
      catch
      {
        result = null;
      }

      return result;
    }


    private void InitGridView(GridView result)
    {
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
    }
  }
}
