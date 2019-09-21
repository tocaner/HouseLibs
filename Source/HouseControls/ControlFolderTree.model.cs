using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HouseUtils;


namespace HouseControls
{
  class ModelFolderTree : BindingPropertyClass
  {
    public ObservableCollection<TreeViewFolderItem> RootItems { get; set; } = new ObservableCollection<TreeViewFolderItem>();

    public ModelFolderTree()
    {
      this.RootItems.Add(new TreeViewComputerItem());
    }
  }


  public class TreeViewFolderItem
  {
    public TreeViewFolderItem(string title, TreeViewFolderItem parent)
    {
      Title = title;
      Parent = parent;
    }


    public string Title { get; set; }


    public TreeViewFolderItem Parent { get; }


    private ObservableCollection<TreeViewFolderItem> fItems = null;

    public ObservableCollection<TreeViewFolderItem> Items
    {
      get
      {
        if (fItems == null)
        {
          fItems = new ObservableCollection<TreeViewFolderItem>();

          string path = GetPath();

          foreach (string name in FolderInfo.GetSubfolders(path))
          {
            fItems.Add(CreateChild(name));
          }
        }

        return fItems;
      }
    }


    public string GetPath()
    {
      string result = "";

      TreeViewFolderItem node = this;

      while (node != null)
      {
        result = node.Title + "\\" + result;
        node = node.Parent;
      }

      try
      {
        result = result.Substring(result.IndexOf('\\') + 1);
      }
      catch
      {
        result = "";
      }

      return result;
    }


    virtual protected TreeViewFolderItem CreateChild(string name)
    {
      return new TreeViewFolderItem(name, this);
    }
  }


  public class TreeViewComputerItem : TreeViewFolderItem
  {
    public TreeViewComputerItem()
      : base("Computer", null)
    { }

    override protected TreeViewFolderItem CreateChild(string name)
    {
      return new TreeViewDriveItem(name, this);
    }
  }


  public class TreeViewDriveItem : TreeViewFolderItem
  {
    public TreeViewDriveItem(string title, TreeViewFolderItem parent)
      : base(title, parent)
    { }
  }
}
