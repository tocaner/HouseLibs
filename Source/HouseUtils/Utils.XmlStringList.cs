using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;


namespace HouseUtils
{
  public class XmlStringList
  {
    private string fFilename;
    private List<string> fItems;


    public XmlStringList(string filename)
    {
      fFilename = filename;
      fItems = new List<string>();
      Load(fItems);
    }


    public string this[int index]
    {
      get
      {
        string result;

        try
        {
          result = fItems[index];
        }
        catch
        {
          result = "";
        }

        return result;
      }
    }


    public int Count
    {
      get { return fItems.Count; }
    }


    public void Add(string value)
    {
      int index = fItems.IndexOf(value);

      if(index < 0)
      {
        fItems.Add(value);
        Save(fItems);
      }
    }


    public void Remove(string value)
    {
      int index = fItems.IndexOf(value);

      if(index >= 0)
      {
        fItems.RemoveAt(index);
        Save(fItems);
      }
    }


    private void Load(List<string> items)
    {
      try
      {
        if(String.IsNullOrEmpty(fFilename) == false)
        {
          if(File.Exists(fFilename))
          {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(fFilename);
            XmlNode xmlRoot = xmlDoc.ChildNodes[1];

            foreach (XmlNode xmlItem in xmlRoot.ChildNodes)
            {
              items.Add(xmlItem.InnerText);
            }
          }
        }
      }
      catch(Exception)
      {
        // do nothing
      }
    }


    private void Save(List<string> items)
    {
      if(String.IsNullOrEmpty(fFilename) == false)
      {
        Directory.CreateDirectory(Path.GetDirectoryName(fFilename));

        XmlDocument xmlDoc = new XmlDocument();

        XmlDeclaration xmlDec = xmlDoc.CreateXmlDeclaration("1.0", null, null);
        xmlDoc.AppendChild(xmlDec);
        XmlNode xmlRoot = xmlDoc.CreateElement("Settings");
        xmlDoc.AppendChild(xmlRoot);

        foreach(string item in items)
        {
          XmlNode xmlItem = xmlDoc.CreateElement("item");
          xmlItem.InnerText = item;
          xmlRoot.AppendChild(xmlItem);
        }

        xmlDoc.Save(fFilename);
      }
    }
  }
}
