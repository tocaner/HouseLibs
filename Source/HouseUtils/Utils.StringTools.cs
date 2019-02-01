using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HouseUtils
{
  public class StringTools
  {
    // String    = "Hello"
    // ByteArray = 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x00
    // HexString = "48656C6C6F00"

    static public byte[] ByteArrayFromString(string str)
    {
      return System.Text.ASCIIEncoding.ASCII.GetBytes(str);
    }


    static public string StringFromByteArray(byte[] bytes)
    {
      return System.Text.ASCIIEncoding.ASCII.GetString(bytes);
    }


    static public string StringFromByteArray(byte[] value, int length)
    {
      return System.Text.ASCIIEncoding.ASCII.GetString(value, 0, length);
    }


    public static bool IsHexDigit(char c)
    {
      if ('0' <= c && c <= '9') return true;
      if ('a' <= c && c <= 'f') return true;
      if ('A' <= c && c <= 'F') return true;
      return false;
    }


    public static string CleanHexString(string hexStr)
    {
      StringBuilder sb = new StringBuilder();

      for (int i = 0; i < hexStr.Length; i++)
      {
        if (IsHexDigit(hexStr[i]))
        {
          sb.Append(hexStr[i]);
        }
      }

      string result = sb.ToString();

      if ((result.Length % 2) != 0)
      {
        result = '0' + result;
      }

      return result.ToUpper();
    }


    public static string HexStringFromByte(byte aByte)
    {
      return aByte.ToString("X2");
    }


    public static byte ByteFromHexString(string hexStr)
    {
      return Convert.ToByte(hexStr, 16);
    }


    static public string HexStringFromByteArray(byte[] byteArray)
    {
      return BitConverter.ToString(byteArray);
    }


    public static byte[] ByteArrayFromHexString(string hexStr)
    {
      hexStr = CleanHexString(hexStr);

      byte[] byteArray = new byte[hexStr.Length / 2];

      for (int i = 0; i < byteArray.Length; i++)
      {
        string s = hexStr.Substring(2 * i, 2);
        byteArray[i] = ByteFromHexString(s);
      }

      return byteArray;
    }


    static public int ParseInt(string str, int defaultValue)
    {
      int result;

      try
      {
        result = int.Parse(str);
      }
      catch
      {
        result = defaultValue;
      }

      return result;
    }
  }
}
