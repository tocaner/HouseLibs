﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;


namespace HouseUtils
{
  public class XmlTable
  {
    private Dictionary<string, string> fItems;
    private string fFilename;


    public XmlTable(string filename)
    {
      fItems = new Dictionary<string, string>();
      fFilename = filename;
      Load(fFilename);
    }


    public void Load(string filename)
    {
      try
      {
        if (String.IsNullOrEmpty(filename) == false)
        {
          if (File.Exists(filename))
          {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filename);
            XmlNode xmlRoot = xmlDoc.ChildNodes[1];

            foreach (XmlNode xmlItem in xmlRoot.ChildNodes)
            {
              fItems.Add(xmlItem.Name, xmlItem.InnerText);
            }
          }
        }
      }
      catch(Exception)
      {
        // do nothing
      }
    }


    public void Save(string filename)
    {
      if(String.IsNullOrEmpty(filename) == false)
      {
        Directory.CreateDirectory(Path.GetDirectoryName(filename));

        XmlDocument xmlDoc = new XmlDocument();

        XmlDeclaration xmlDec = xmlDoc.CreateXmlDeclaration("1.0", null, null);
        xmlDoc.AppendChild(xmlDec);
        XmlNode xmlRoot = xmlDoc.CreateElement("Settings");
        xmlDoc.AppendChild(xmlRoot);

        foreach(string key in fItems.Keys)
        {
          XmlNode xmlItem = xmlDoc.CreateElement(key);
          xmlItem.InnerText = fItems[key];
          xmlRoot.AppendChild(xmlItem);
        }

        xmlDoc.Save(filename);
      }
    }


    public string Get(string key, string defaultValue)
    {
      string result;

      try
      {
        result = fItems[key];
      }
      catch
      {
        result = defaultValue;
      }

      return result;
    }


    public void Set(string key, string value)
    {
      try
      {
        fItems[key] = value;
      }
      catch
      {
        fItems.Add(key, value);
      }

      Save(fFilename);
    }


    public void SetInteger(string key, int value)
    {
      Set(key, value.ToString());
    }


    public int GetInteger(string key, int defaultValue)
    {
      int result;

      try
      {
        result = Int32.Parse(Get(key, ""));
      }
      catch
      {
        result = defaultValue;
      }
      
      return result;
    }


    public void SetBool(string key, bool value)
    {
      SetInteger(key, value ? 1 : 0);
    }


    public bool GetBool(string key, bool defaultValue)
    {
      int value = GetInteger(key, -1);

      bool result;

      if(value == 0)
      {
        result = false;
      }
      else if(value == 1)
      {
        result = true;
      }
      else
      {
        result = defaultValue;
      }

      return result;
    }


    public void SetDouble(string key, double value)
    {
      Set(key, value.ToString());
    }


    public double GetDouble(string key, double defaultValue)
    {
      double result;

      try
      {
        result = Double.Parse(Get(key, ""));
      }
      catch
      {
        result = defaultValue;
      }

      return result;
    }
  }
}
