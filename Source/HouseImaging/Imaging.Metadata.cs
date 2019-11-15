using System;
using System.Collections.Generic;
using System.Globalization;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using HouseUtils;


namespace HouseImaging
{
  public class MetadataPortal
  {
    private Image fSystemImage;
    
     
    public MetadataPortal(ImageInfo imageInfo)
    {
      fSystemImage = imageInfo.SystemImage;
    }


    public bool Has_ExifOrientation()
    {
      return Has(0x0112);
    }


    public int Read_ExifOrientation()
    {
      return Read_IntU16(0x0112);
    }


    public void Set_ExifOrientation(int value)
    {
      Set_IntU16(0x0112, value);
    }


    public void Remove_ExifOrientation()
    {
      this.Remove(0x0112);
    }


    public int Read_ExifThumbnailOrientation()
    {
      return Read_IntU16(0x5029);
    }


    public void Set_ExifThumbnailOrientation(int value)
    {
      Set_IntU16(0x5029, value);
    }


    public void Remove_ExifThumbnailOrientation()
    {
      this.Remove(0x5029);
    }


    public byte[] Read_ThumbnailBytes()
    {
      return this.Read(0x501B);
    }


    public string Read_UserComment()
    {
      return Read_String(0x9286);
    }


    public string Read_Title()
    {
      return Read_String(0x9C9B);
    }


    public string Read_Comments()
    {
      return Read_String(0x9C9C);
    }


    public string Read_Author()
    {
      return Read_String(0x9C9D);
    }


    public string Read_Tags()
    {
      return Read_String(0x9C9E);
    }


    public string Read_Subject()
    {
      return Read_String(0x9C9F);
    }


    public DateTime Read_DateTaken()
    {
      DateTime result;

      if (DateTime.TryParseExact(Read_String(0x0132), "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out result) == false)
      {
        result = DateTime.MinValue;
      }

      return result;
    }


    public int Read_IntU16(int propId)
    {
      byte[] bytes = this.Read(propId);

      if (bytes != null)
      {
        return bytes[1] << 8 | bytes[0];
      }
      else
      {
        return -1; // No exif found, valid exif values are 1..8
      }
    }


    public void Set_IntU16(int propId, int value)
    {
      byte[] bytes = BitConverter.GetBytes((UInt16)value);
      this.Set(propId, bytes, MetadataType.IntU16);
    }


    public string Read_String(int propId)
    {
      byte[] bytes = this.Read(propId);

      if (bytes != null)
      {
        return MetadataFormat.FormatValue(MetadataType.String, bytes);
      }
      else
      {
        return string.Empty;
      }
    }


    public void Set_String(int propId, string value)
    {
      byte[] bytes = MetadataFormat.EncodeValue(MetadataType.String, value);
      this.Set(propId, bytes, MetadataType.String);
    }


    private PropertyItem _get_property_item(int propId)
    {
      return fSystemImage.PropertyItems.FirstOrDefault(item => item.Id == propId);
    }


    public bool Has(int propId)
    {
      return _get_property_item(propId) != null;
    }


    public byte[] Read(int propId)
    {
      byte[] result = null;

      PropertyItem prop = _get_property_item(propId);

      if (prop != null)
      {
        result = prop.Value;
      }

      return result;
    }


    public void Set(int id, byte[] data, MetadataType dataType = 0)
    {
      PropertyItem prop = _get_property_item(id);

      if (prop == null)
      {
        prop = CreatePropertyItem(id);
      }

      if (prop != null)
      {
        if (dataType == 0)
        {
          // Keep default prop.Type
        }
        else
        {
          prop.Type = (short)(((short)dataType) & 0xFF);
        }

        prop.Value = data;
        prop.Len = data.Length;

        fSystemImage.SetPropertyItem(prop);
      }
    }


    public void Remove(int id)
    {
      PropertyItem prop = _get_property_item(id);

      if (prop != null)
      {
        // Attempt removing the property only if it exists
        fSystemImage.RemovePropertyItem(id);
      }
    }


    private const string RESOURCENAME = "HouseImaging.Dummy.jpg";


    private static PropertyItem CreatePropertyItem(int id)
    {
      Image image = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream(RESOURCENAME));
      PropertyItem prop = image.PropertyItems[0];
      prop.Id = id;
      prop.Type = 0;
      return prop;
    }


    public List<MetadataItem> GetList()
    {
      List<MetadataItem> result = new List<MetadataItem>();

      foreach (PropertyItem prop in fSystemImage.PropertyItems)
      {
        result.Add(new MetadataItem(prop));
      }

      return result;
    }


    public List<MetadataItem> GetAllDefinedMetadata()
    {
      List<MetadataItem> result = GetList();

      foreach (MetadataDefinition defn in MetadataLibrary.GetList())
      {
        MetadataItem item = result.FirstOrDefault(c => c.Definition.Id == defn.Id);

        if (item == null)
        {
          result.Add(new MetadataItem(defn));
        }
      }

      return result;
    }


    public static void ExportMetadataDefinitionsToCsv(string path)
    {
      using (Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
      {
        using (StreamWriter writer = new StreamWriter(stream))
        {
          foreach (MetadataDefinition defn in MetadataLibrary.GetList())
          {
            string line = string.Format("{0:X4}, {0}, {1}, {2}, {3}, {4}",
              defn.Id,
              defn.Category,
              defn.Name,
              "type",
              defn.Description
              );
            writer.WriteLine(line);
          }
        }
      }
    }


    public static void ExportAllKnownMetadataDefinitions(string path)
    {
      Dictionary<string, object> result = new Dictionary<string, object>();

      foreach (MetadataDefinition defn in MetadataLibrary.GetList())
      {
        result.Add("0x" + defn.Id.ToString("X4"), defn);
      }

      File.WriteAllText(path, Json.Serialize(result));
    }
  }


  public class MetadataItem
  {
    public MetadataDefinition Definition;
    public byte[] Data { get; set; }


    public MetadataItem(MetadataDefinition defn)
      : this(null, defn)
    { }


    public MetadataItem(PropertyItem prop = null, MetadataDefinition defn = null)
    {
      if (defn == null)
      {
        if (prop != null)
        {
          defn = MetadataLibrary.Lookup(prop.Id);
        }
      }

      if (defn == null)
      {
        Definition = new MetadataDefinition()
        {
          Id = -1,
          Name = "?",
          Category = "Unknown Tags",
          Description = string.Empty,
          DataType = MetadataType.Undefined
        };
      }
      else
      {
        Definition = defn;
      }

      Data = null;

      if (prop != null)
      {
        // Assume prop.Value.Length == prop.Len
        Data = prop.Value;

        if (Definition.Id == -1)
        {
          Definition.Id = prop.Id;
        }

        if ((Definition.DataType == MetadataType.Undefined) && (prop.Type != 0))
        {
          Definition.DataType = (MetadataType)prop.Type;
        }
      }
    }


    public override string ToString()
    {
      return MetadataFormat.FormatValue(Definition.DataType, Data);
    }


    public void FromString(string valueAsString)
    {
      byte[] data = MetadataFormat.EncodeValue(Definition.DataType, valueAsString);

      if (data != null)
      {
        Data = data;
      }
    }
  }


  public enum MetadataType
  {
    Byte = 1,
    Unicode = 1 | 0x100,
    String = 2,          // Null terminated
    CharArray = 2 | 0x100,  // Fixed length
    IntU16 = 3,
    IntU32 = 4,
    FracU32 = 5,
    Unused6 = 6,
    Undefined = 7,
    Unused8 = 8,
    IntS32 = 9,
    FracS32 = 10
  }


  public class MetadataDefinition
  {
    public int Id { get; set; }
    public string Category { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public MetadataType DataType { get; set; }
  }


  public class MetadataLibrary
  {
    private const string RESOURCENAME = "HouseImaging.TagDefinitions.json";

    private static Dictionary<int, MetadataDefinition> _table = _loadDefinitions();


    public static MetadataDefinition Lookup(int id)
    {
      return _table.ContainsKey(id) ? _table[id] : null;
    }


    public static void AddDefinition(MetadataDefinition defn)
    {
      if (_table.ContainsKey(defn.Id) == false)
      {
        _table.Add(defn.Id, defn);
      }
    }


    public static List<MetadataDefinition> GetList()
    {
      List<MetadataDefinition> result = new List<MetadataDefinition>();

      foreach (KeyValuePair<int, MetadataDefinition> kv in _table)
      {
        result.Add(kv.Value);
      }

      return result;
    }


    private static Dictionary<int, MetadataDefinition> _loadDefinitions()
    {
      Dictionary<int, MetadataDefinition> result = new Dictionary<int, MetadataDefinition>();

      try
      {
//        using (Stream stream = new FileStream("TagDefinitions.json", FileMode.Open, FileAccess.Read))
        using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(RESOURCENAME))
        {
          using (StreamReader reader = new StreamReader(stream))
          {
            string input = reader.ReadToEnd();
            var json = Json.Deserialize<MetadataDefinition>(input);

            foreach (KeyValuePair<string, MetadataDefinition> kv in json)
            {
              MetadataDefinition defn = kv.Value;

              if (String.IsNullOrEmpty(defn.Category))
              {
                defn.Category = "General";
              }

              result.Add(defn.Id, defn);
            }
          }
        }
      }
      catch
      {
        result.Clear();
      }

      return result;
    }
  }


  internal class MetadataFormat
  {
    private const string DOUBLETYPE_FORMAT = "0.0####";


    public static string FormatValue(MetadataType type, byte[] bytes)
    {
      string strRet;

      try
      {
        switch (type)
        {
          case MetadataType.Byte:
            strRet = _formatTagByte(bytes);
            break;
          case MetadataType.String:
            strRet = _formatTagString(bytes);
            break;
          case MetadataType.CharArray:
            strRet = _formatTagCharArray(bytes);
            break;
          case MetadataType.Unicode:
            strRet = _formatTagUnicode(bytes);
            break;
          case MetadataType.IntU16:
            strRet = _formatTagUShort(bytes);
            break;
          case MetadataType.IntU32:
            strRet = _formatTagULong(bytes);
            break;
          case MetadataType.IntS32:
            strRet = _formatTagSLong(bytes);
            break;
          case MetadataType.FracU32:
            strRet = _formatTagRational(bytes);
            break;
          case MetadataType.FracS32:
            strRet = _formatTagSRational(bytes);
            break;
          case MetadataType.Undefined:
          default:
            strRet = _formatTagByte(bytes);
            break;
        }
      }
      catch
      {
        strRet = string.Empty;
      }

      return strRet;
    }


    public static byte[] EncodeValue(MetadataType type, string value)
    {
      byte[] bytes;

      try
      {
        switch (type)
        {
          case MetadataType.Byte:
            bytes = _encodeTagByte(value);
            break;
          case MetadataType.String:
            bytes = _encodeTagString(value);
            break;
          case MetadataType.CharArray:
            bytes = _encodeTagCharArray(value);
            break;
          case MetadataType.Unicode:
            bytes = _encodeTagUnicode(value);
            break;
          case MetadataType.IntU16:
            bytes = _encodeTagUShort(value);
            break;
          case MetadataType.IntU32:
            bytes = _encodeTagULong(value);
            break;
          case MetadataType.IntS32:
            bytes = _encodeTagSLong(value);
            break;
          case MetadataType.FracU32:
            bytes = new byte[0];
            break;
          case MetadataType.FracS32:
            bytes = new byte[0];
            break;
          case MetadataType.Undefined:
          default:
            bytes = _encodeTagByte(value);
            break;
        }
      }
      catch
      {
        bytes = null;
      }

      return bytes;
    }


    private static string _formatTagUnicode(byte[] bytes)
    {
      return Encoding.Unicode.GetString(bytes, 0, bytes.Length - 2);
    }


    private static byte[] _encodeTagUnicode(string value)
    {
      return Encoding.Unicode.GetBytes(value + '\0');
    }


    private static string _formatTagString(byte[] bytes)
    {
      string value2 = StringTools.StringFromByteArray(bytes, bytes.Length -1);
      return new string(value2.Where(c => !char.IsControl(c)).ToArray());
    }


    private static byte[] _encodeTagString(string value)
    {
      return StringTools.ByteArrayFromString(value + '\0');
    }


    private static string _formatTagCharArray(byte[] bytes)
    {
      string value2 = StringTools.StringFromByteArray(bytes, bytes.Length);
      return new string(value2.Where(c => !char.IsControl(c)).ToArray());
    }


    private static byte[] _encodeTagCharArray(string value)
    {
      return StringTools.ByteArrayFromString(value);
    }


    private static string _formatTagByte(byte[] bytes)
    {
      return StringTools.HexStringFromByteArray(bytes);
    }


    private static byte[] _encodeTagByte(string value)
    {
      return StringTools.ByteArrayFromHexString(value);
    }


    private static string _formatTagUShort(byte[] bytes)
    {
      string result = "";

      for (int i = 0; i < bytes.Length; i = i + sizeof(UInt16))
      {
        if (string.IsNullOrEmpty(result) == false)
        {
          result += " ";
        }

        UInt16 val = BitConverter.ToUInt16(bytes, i);
        result += val.ToString();
      }
      return result;
    }


    private static byte[] _encodeTagUShort(string value)
    {
      string[] fields = value.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

      UInt16[] values = new UInt16[fields.Length];

      for (int i = 0; i < fields.Length; i++)
      {
        values[i] = UInt16.Parse(fields[i]);
      }

      byte[] result = new byte[values.Length * sizeof(UInt16)];
      Buffer.BlockCopy(values, 0, result, 0, result.Length);

      return result;
    }


    private static string _formatTagULong(byte[] bytes)
    {
      string result = "";

      for (int i = 0; i < bytes.Length; i = i + sizeof(UInt32))
      {
        if (string.IsNullOrEmpty(result) == false)
        {
          result += " ";
        }

        UInt32 val = BitConverter.ToUInt32(bytes, i);
        result += val.ToString();
      }
      return result;
    }


    private static byte[] _encodeTagULong(string value)
    {
      string[] fields = value.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

      UInt32[] values = new UInt32[fields.Length];

      for (int i = 0; i < fields.Length; i++)
      {
        values[i] = UInt32.Parse(fields[i]);
      }

      byte[] result = new byte[values.Length * sizeof(UInt32)];
      Buffer.BlockCopy(values, 0, result, 0, result.Length);

      return result;
    }


    private static string _formatTagSLong(byte[] bytes)
    {
      string result = "";

      for (int i = 0; i < bytes.Length; i = i + sizeof(Int32))
      {
        if (string.IsNullOrEmpty(result) == false)
        {
          result += " ";
        }

        Int32 val = BitConverter.ToInt32(bytes, i);
        result += val.ToString();
      }
      return result;
    }


    private static byte[] _encodeTagSLong(string value)
    {
      string[] fields = value.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

      Int32[] values = new Int32[fields.Length];

      for (int i = 0; i < fields.Length; i++)
      {
        values[i] = Int32.Parse(fields[i]);
      }

      byte[] result = new byte[values.Length * sizeof(Int32)];
      Buffer.BlockCopy(values, 0, result, 0, result.Length);

      return result;
    }


    private static string _formatTagRational(byte[] bytes, bool decimalFormat = false)
    {
      string result = "";

      for (int i = 0; i < bytes.Length; i = i + 2 * sizeof(UInt32))
      {
        if (string.IsNullOrEmpty(result) == false)
        {
          result += " ";
        }

        System.UInt32 numer = BitConverter.ToUInt32(bytes, i);
        System.UInt32 denom = BitConverter.ToUInt32(bytes, i + sizeof(UInt32));

        if (decimalFormat)
        {
          double dbl = denom == 0 ? 0.0 : (double)numer / (double)denom;
          result += dbl.ToString(DOUBLETYPE_FORMAT);
        }
        else
        {
          result += _fracToString((int)numer, (int)denom);
        }
      }
      return result;
    }


    private static string _formatTagSRational(byte[] bytes, bool decimalFormat = false)
    {
      string result = "";

      for (int i = 0; i < bytes.Length; i = i + 2 * sizeof(Int32))
      {
        if (string.IsNullOrEmpty(result) == false)
        {
          result += " ";
        }

        System.Int32 numer = BitConverter.ToInt32(bytes, i);
        System.Int32 denom = BitConverter.ToInt32(bytes, i + sizeof(Int32));

        if (decimalFormat)
        {
          double dbl = denom == 0 ? 0.0 : (double)numer / (double)denom;
          result += dbl.ToString(DOUBLETYPE_FORMAT);
        }
        else
        {
          result += _fracToString(numer, denom);
        }
      }
      return result;
    }


    private static string _fracToString(int numer, int denom)
    {
      string result;

      if (numer == 0)
      {
        result = "0";
      }
      else if ((denom == 1) || (denom == 0))
      {
        result = numer + "";
      }
      else
      {
        result = numer + "/" + denom;
      }

      return result;
    }
  }
}
