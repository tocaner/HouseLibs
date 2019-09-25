using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HouseUtils;
using HouseControls;


namespace HouseControls
{
  public class ListItemMovedEventArgs : EventArgs
  {
    public int SourceIndex { get; set; }
    public int TargetIndex { get; set; }
  }


  public class DataObjectDroppedEventArgs : EventArgs
  {
    public object DataObject { get; set; }
  }


  public class DragDropAssist
  {
    public bool Enabled { get; set; } = true;


    private ListView fListView = null;
    private Point? fpMouseDownPos = null;
    private InsertionMark fInsertionMark = null;


    public event EventHandler ListItemMoved;

    private void Raise_ListItemMoved(int sourceIndex, int targetIndex)
    {
      ListItemMoved?.Invoke(this, new ListItemMovedEventArgs { SourceIndex = sourceIndex, TargetIndex = targetIndex });
    }


    public event EventHandler DataObjectDropped;

    private void Raise_DataObjectDropped(object dataObject)
    {
      DataObjectDropped?.Invoke(this, new DataObjectDroppedEventArgs { DataObject = dataObject });
    }


    public DragDropAssist(ListView listView)
    {
      fListView = listView;
      fInsertionMark = new InsertionMark(fListView);
      fListView.PreviewMouseLeftButtonDown += ListView_PreviewMouseLeftButtonDown;
      fListView.PreviewMouseLeftButtonUp += ListView_PreviewMouseLeftButtonUp;
      fListView.PreviewMouseMove += ListView_PreviewMouseMove;
      fListView.DragOver += ListView_DragOver;
      fListView.Drop += ListView_Drop;
      fListView.QueryContinueDrag += ListView_QueryContinueDrag;
      fListView.AllowDrop = true;
    }


    private void ListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      fpMouseDownPos = e.GetPosition(fListView);
    }


    private void ListView_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      // Comes here only if left button is up without dragging started
      fpMouseDownPos = null;
    }


    private void ListView_PreviewMouseMove(object sender, MouseEventArgs e)
    {
      if ((e.LeftButton == MouseButtonState.Pressed) && (fpMouseDownPos.HasValue))
      {
        Point pt = e.GetPosition(fListView);

        if ((fpMouseDownPos.Value - pt).Length > 10)
        {
          ListViewItem dragStartItem = WpfListViewAssist.GetListViewItemAtPoint(fListView, fpMouseDownPos.Value);

          if (dragStartItem != null)
          {
            DragDropEffects effects = DragDropEffects.Move;

            if (fListView.SelectedItems.Count > 1)
            {
              effects = DragDropEffects.Copy;
            }

            DragDrop.DoDragDrop(fListView, dragStartItem, effects);
          }

          fInsertionMark.Hide();
          fpMouseDownPos = null; // Make sure this code is executed once per Mouse Down
          e.Handled = true;
        }
      }
    }


    private void ListView_DragOver(object sender, System.Windows.DragEventArgs e)
    {
      int sourceIndex = -1;

      if (e.Data.GetDataPresent(typeof(ListViewItem)))
      {
        ListViewItem draggedItem = (ListViewItem)e.Data.GetData(typeof(ListViewItem));

        if (draggedItem != null)
        {
          sourceIndex = fListView.Items.IndexOf(draggedItem.Content);
        }
      }
      else if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
      {
        // Importing files
        if (!Enabled)
        {
          e.Effects = DragDropEffects.None;
        }
      }
      else
      {
        e.Effects = DragDropEffects.None;
      }

      if (e.Effects == DragDropEffects.None)
      {
        fInsertionMark.Hide();
      }
      else
      {
        int targetIndex = -1;

        Point pt = e.GetPosition(fListView);
        ListViewItem item = WpfListViewAssist.GetListViewItemAtPoint(fListView, pt);

        if (item != null)
        {
          targetIndex = fListView.Items.IndexOf(item.Content);
        }

        if (targetIndex < 0)
        {
          fInsertionMark.Hide();
        }
        else
        {
          int scrollIndex = targetIndex;

          if (pt.Y < item.ActualHeight)
          {
            if (scrollIndex > 0)
            {
              scrollIndex--;
            }
          }
          else if (pt.Y > (fListView.ActualHeight - item.ActualHeight))
          {
            if (scrollIndex < (fListView.Items.Count - 1))
            {
              scrollIndex++;
            }
          }
          else
          {
            // No Scrolling
          }

          if (scrollIndex != targetIndex)
          {
            fInsertionMark.Hide();
            fListView.ScrollIntoView(fListView.Items[scrollIndex]);
            System.Threading.Thread.Sleep(70);   //slow down the scrolling a bit
          }
          else if (sourceIndex < 0)
          {
            fInsertionMark.Show(item, true);
          }
          else if (targetIndex == sourceIndex)
          {
            fInsertionMark.Hide();
          }
          else
          {
            fInsertionMark.Show(item, targetIndex < sourceIndex);
          }
        }
      }

      e.Handled = true;
    }


    private void ListView_Drop(object sender, System.Windows.DragEventArgs e)
    {
      // Check for dragged ListViewPage ----
      int targetIndex = fInsertionMark.Index;

      if (targetIndex >= 0)
      {
        if (e.Data.GetDataPresent(typeof(ListViewItem)))
        {
          ListViewItem draggedItem = (ListViewItem)e.Data.GetData(typeof(ListViewItem));

          if (draggedItem != null)
          {
            int sourceIndex = fListView.Items.IndexOf(draggedItem.Content);
            Raise_ListItemMoved(sourceIndex, targetIndex);
          }
        }
      }

      // Check for dragged files from another application -----
      if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
      {
        object dataObject = e.Data.GetData(DataFormats.FileDrop, false);
        if (dataObject != null)
        {
          Raise_DataObjectDropped(dataObject); // TODO: At target index
        }
      }

      e.Handled = true;
    }


    private void ListView_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
    {
      if (e.EscapePressed)
      {
        e.Action = DragAction.Cancel;
        // In case the drap operation was initiated from another application and we did not call DoDrapDrop...
        fInsertionMark.Hide();
      }
    }
  }
}
