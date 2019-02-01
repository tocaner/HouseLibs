using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;


namespace HouseUtils
{
  public class WpfListViewAssist
  {
    static public ListViewItem GetListViewItemAtPoint(ListView listView, Point pt)
    {
      HitTestResult hitTest = VisualTreeHelper.HitTest(listView, pt);

      DependencyObject depObj = hitTest.VisualHit as DependencyObject;

      if (depObj != null)
      {
        // go up the visual hierarchy until we find the list view item the click came from  
        // the click might have been on the grid or column headers so we need to cater for this  
        DependencyObject current = depObj;
        while ((current != null) && (current != listView))
        {
          ListViewItem ListViewItem = current as ListViewItem;
          if (ListViewItem != null)
          {
            return ListViewItem;
          }
          current = VisualTreeHelper.GetParent(current);
        }
      }

      return null;
    }
  }


  public class InsertionMark
  {
    private ListView fListView;
    private InsertionMarkAdorner fAdorner;
    private ListViewItem fItem;

    public InsertionMark(ListView listView)
    {
      fListView = listView;
      fAdorner = null;
      fItem = null;
    }

    public int Index
    {
      get { return fItem == null ? -1 :  fListView.Items.IndexOf(fItem.Content); }
    }

    public void Show(ListViewItem item, bool before)
    {
      Hide();

      if (item != null)
      {
        fItem = item;
        fAdorner = new InsertionMarkAdorner(fItem, before);
      }
    }

    public void Hide()
    {
      if (fAdorner != null)
      {
        fAdorner.Close(fItem);
        fAdorner = null;
        fItem = null;
      }
    }

    public bool IsVisible
    {
      get { return (fAdorner != null); }
    }
  }


  class InsertionMarkAdorner : Adorner
  {
    private bool _before;

    public InsertionMarkAdorner(ListViewItem item, bool before) :
      base(item)
    {
      _before = before;
      AdornerLayer.GetAdornerLayer(item).Add(this);
    }

    public void Close(ListViewItem item)
    {
      AdornerLayer.GetAdornerLayer(item).Remove(this);
    }

    protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
    {
      if (_before)
      {
        drawingContext.DrawRectangle(System.Windows.Media.Brushes.Gray, null, new System.Windows.Rect(0, 0, ActualWidth, 3));
      }
      else
      {
        drawingContext.DrawRectangle(System.Windows.Media.Brushes.Gray, null, new System.Windows.Rect(0, ActualHeight - 3, ActualWidth, 3));
      }
    }
  }
}
