using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.IO;


namespace HouseUtils
{
  public class AppInfo
  {
    static public string GetApplicationName()
    {
      return Application.ProductName;
    }


    static public string GetApplicationVersion()
    {
      return Application.ProductVersion;
    }


    static public string GetApplicationV3()
    {
      string[] fields = Application.ProductVersion.Split('.');
      return string.Format("{0}.{1}.{2}", fields[0], fields[1], fields[2]);
    }


    static public string GetApplicationGuid()
    {
      Assembly asm = Assembly.GetExecutingAssembly();
      object[] attribs = (asm.GetCustomAttributes(typeof(GuidAttribute), true));
      return ((GuidAttribute)attribs[0]).Value.ToString();
    }


    static public string GetUserAppDataFolder()
    {
      return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    }


    static public string GetCommonAppDataFolder()
    {
      return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
    }


    static public string GetFullPathToUserApplicationData(string relativePath)
    {
      return Path.Combine(AppInfo.GetUserAppDataFolder(), AppInfo.GetApplicationName(), relativePath);
    }


    static public string GetFullPathToCommonApplicationData(string relativePath)
    {
      return Path.Combine(AppInfo.GetCommonAppDataFolder(), AppInfo.GetApplicationName(), relativePath);
    }
  }
}
