using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;


namespace HouseUtils
{
  public class FolderInfo
  {
    static public List<string> GetFiles(string path)
    {
      List<string> result = new List<string>();

      foreach (FileInfo file in GetFileList(path))
      {
        result.Add(file.Name);
      }

      return result;
    }


    static public List<string> GetFiles_FullPath(string path)
    {
      List<string> result = new List<string>();

      foreach (FileInfo file in GetFileList(path))
      {
        result.Add(file.FullName);
      }

      return result;
    }


    static private FileInfo[] GetFileList(string path)
    {
      FileInfo[] files;

      if (String.IsNullOrEmpty(path))
      {
        files = new FileInfo[] { };
      }
      else
      {
        DirectoryInfo currentDir = new DirectoryInfo(path + '\\');

        try
        {
          files = currentDir.GetFiles();
        }
        catch
        {
          files = new FileInfo[] { };
        }
      }

      return files;
    }


    static public List<string> GetSubfolders(string path)
    {
      List<string> result = new List<string>();

      if(String.IsNullOrEmpty(path))
      {
        DriveInfo[] drives = DriveInfo.GetDrives();

        foreach(DriveInfo drive in drives)
        {
          result.Add(drive.Name.TrimEnd('\\'));
        }
      }
      else
      {
        DirectoryInfo currentDir = new DirectoryInfo(path + '\\');

        DirectoryInfo[] dirs;

        try
        {
          dirs = currentDir.GetDirectories();
        }
        catch
        {
          dirs = new DirectoryInfo[] { };
        }

        foreach(DirectoryInfo dir in dirs)
        {
          result.Add(dir.Name);
        }
      }

      return result;
    }


    static public bool PathsAreEqual(string path1, string path2)
    {
      // http://stackoverflow.com/questions/1794025/how-to-check-whether-2-directoryinfo-objects-are-pointing-to-the-same-directory
      
      return ( 0 == String.Compare(
              System.IO.Path.GetFullPath(path1).TrimEnd('\\'),
              System.IO.Path.GetFullPath(path2).TrimEnd('\\'),
              StringComparison.InvariantCultureIgnoreCase) );
    }


    static string MakeRelativePath(string origin, string path)
    {
      System.Uri uri1 = new Uri(origin);
      System.Uri uri2 = new Uri(path);
      Uri relativeUri = uri1.MakeRelativeUri(uri2);
      return relativeUri.ToString();
    }


    static public long GetFileSize(string path)
    {
      return new System.IO.FileInfo(path).Length;
    }


    static public string FileSizeToString(long size)
    {
      size = size / 1024;
      return size.ToString("N0") + " KB";
    }


    static public string GetFileSizeAsString(string path)
    {
      return FileSizeToString(GetFileSize(path));
    }


    static public string GetDesktopPath()
    {
      return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    }


    static public void EnsurePath(string path, bool isFile = false)
    {
      if (isFile)
      {
        path = Path.GetDirectoryName(path);
      }

      if (Directory.Exists(path) == false)
      {
        Directory.CreateDirectory(path);
      }
    }
  }
}
