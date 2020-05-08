using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Threading.Tasks;


namespace HouseUtils
{
  public class Json
  {
    public static string Serialize(Dictionary<string, object> data)
    {
      var ser = new JavaScriptSerializer();
      return Json.FormatOutput(ser.Serialize(data));
    }


    public static string Serialize2(object data)
    {
      var ser = new JavaScriptSerializer();
      return Json.FormatOutput(ser.Serialize(data));
    }


    public static Dictionary<string, T> Deserialize<T>(string text)
    {
      var ser = new JavaScriptSerializer();
      return ser.Deserialize<Dictionary<string, T>>(text);
    }


    public static Dictionary<string, object> Deserialize(string text)
    {
      var ser = new JavaScriptSerializer();
      return ser.Deserialize<dynamic>(text);
    }


    public static T Deserialize2<T>(string text)
    {
      var ser = new JavaScriptSerializer();
      return ser.Deserialize<T>(text);
    }


    /// <summary>
    /// Adds indentation and line breaks to output of JavaScriptSerializer
    /// </summary>
    public static string FormatOutput(string jsonString)
    {
      var stringBuilder = new StringBuilder();

      bool escaping = false;
      bool inQuotes = false;
      int indentation = 0;

      foreach (char character in jsonString)
      {
        if (escaping)
        {
          escaping = false;
          stringBuilder.Append(character);
        }
        else
        {
          if (character == '\\')
          {
            escaping = true;
            stringBuilder.Append(character);
          }
          else if (character == '\"')
          {
            inQuotes = !inQuotes;
            stringBuilder.Append(character);
          }
          else if (!inQuotes)
          {
            if (character == ',')
            {
              stringBuilder.Append(character);
              stringBuilder.Append("\r\n");
              stringBuilder.Append('\t', indentation);
            }
            else if (character == '[' || character == '{')
            {
              stringBuilder.Append(character);
              stringBuilder.Append("\r\n");
              stringBuilder.Append('\t', ++indentation);
            }
            else if (character == ']' || character == '}')
            {
              stringBuilder.Append("\r\n");
              stringBuilder.Append('\t', --indentation);
              stringBuilder.Append(character);
            }
            else if (character == ':')
            {
              stringBuilder.Append(character);
              stringBuilder.Append('\t');
            }
            else
            {
              stringBuilder.Append(character);
            }
          }
          else
          {
            stringBuilder.Append(character);
          }
        }
      }

      return stringBuilder.ToString();
    }


    /// <summary>
    /// Replaces all dashes with underscores, in keys (and subkeys) of the dictionary 
    /// </summary>
    public static Dictionary<string, object> FixKeys(Dictionary<string, object> dic)
    {
      Dictionary<string, object> result = new Dictionary<string, object>();

      foreach (KeyValuePair<string, object> kv in dic)
      {
        string newKey = kv.Key.Replace("-", "_");
        object newValue;

        if (kv.Value is object[])
        {
          object[] newArray = new object[(kv.Value as object[]).Length];

          for(int i = 0; i < (kv.Value as object[]).Length; i++)
          {
            object item = (kv.Value as object[])[i];

            if (item is Dictionary<string, object>)
            {
              newArray[i] = FixKeys(item as Dictionary<string, object>);
            }
            else
            {
              newArray[i] = item;
            }
          }

          newValue = newArray;
        }
        else if (kv.Value is Dictionary<string, object>)
        {
          newValue = FixKeys(kv.Value as Dictionary<string, object>);
        }
        else
        {
          newValue = kv.Value;
        }

        if (result.ContainsKey(newKey))
        {
          result.Add(newKey + "_2", newValue);
        }
        else
        {
          result.Add(newKey, newValue);
        }
      }

      return result;
    }


    public static SortedDictionary<string, object> SortDictionary(Dictionary<string, object> dic)
    {
      List<KeyValuePair<string, object>> list = new List<KeyValuePair<string, object>>();

      foreach (KeyValuePair<string, object> kv in dic)
      {
        list.Add(kv);
      }

      foreach (KeyValuePair<string, object> kv in list)
      {
        if (kv.Value is Dictionary<string, object>)
        {
          dic[kv.Key] = SortDictionary(kv.Value as Dictionary<string, object>);
        }
        else if (kv.Value is object[])
        {
          object[] objs = kv.Value as object[];

          for (int i = 0; i < objs.Length; i++)
          {
            if (objs[i] is Dictionary<string, object>)
            {
              objs[i] = SortDictionary(objs[i] as Dictionary<string, object>);
            }
          }
        }
        else
        { }
      }

      return new SortedDictionary<string, object>(dic);
    }


    public static Dictionary<string, object> ToDictionary(object data)
    {
      var ser = new JavaScriptSerializer();
      string json = ser.Serialize(data);
      return ser.Deserialize<dynamic>(json);
    }


    public static string SortSerialize(object data)
    {
      var b = SortDictionary(ToDictionary(data));
      return Json.Serialize2(b);
    }


    public static string SortSerialize(Dictionary<string, object> dic)
    {
      var b = SortDictionary(dic);
      return Json.Serialize2(b);
    }


    public static string SortJson(string jsonString)
    {
      var dic = Json.Deserialize(jsonString);
      return SortSerialize(dic);
    }


    public static string FixAndSortJson(string jsonString)
    {
      var a = Json.Deserialize(jsonString);
      var b = FixKeys(a);
      return SortSerialize(b);
    }
  }
}
