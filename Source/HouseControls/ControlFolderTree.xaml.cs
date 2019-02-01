using System;
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
  /// <summary>
  /// Interaction logic for ControlFolderTree.xaml
  /// </summary>
  public partial class ControlFolderTree : UserControl
  {
    ModelFolderTree fModel;


    public ControlFolderTree()
    {
      InitializeComponent();
      fModel = new ModelFolderTree();
      DataContext = fModel;
    }


    public event EventHandler FolderSelected;


    private void Raise_FolderSelected(TreeViewFolderItem folderItem)
    {
      FolderSelected?.Invoke(this, new FolderSelectedEventArgs(folderItem));
    }


    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
      if (e != null)
      {
        if (e.NewValue is TreeViewFolderItem)
        {
          Raise_FolderSelected(e.NewValue as TreeViewFolderItem);
        }
      }
    }
  }


  public class FolderSelectedEventArgs : EventArgs
  {
    public string FolderName { get; }

    public string FolderPath { get; }

    public FolderSelectedEventArgs(TreeViewFolderItem folderItem)
    {
      FolderName = folderItem.Title;
      FolderPath = folderItem.GetPath();
    }
  }
}
