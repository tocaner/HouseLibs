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


    public static string Serialize2(object data)
    {
      var ser = new JavaScriptSerializer();
      return Json.FormatOutput(ser.Serialize(data));
    }


    public static T Deserialize2<T>(string text)
    {
      var ser = new JavaScriptSerializer();
      return ser.Deserialize<T>(text);
    }


    /// <summary>
    /// Adds indentation and line breaks to output of JavaScriptSerializer
    /// </summary>
    private static string FormatOutput(string jsonString)
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
  }
}
