using System;
using System.Collections.Generic;
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


    private PropertyItem _get_property_item(int propId)
    {
      return fSystemImage.PropertyItems.FirstOrDefault(item => item.Id == propId);
    }


    public static object GetObject(PropertyItem prop)
    {
      object result = null;
      MetadataType dataType = (MetadataType)prop.Type;
      byte[] data = prop.Value;

      switch (dataType)
      {
        case MetadataType.String:
          {
            result = StringTools.StringFromByteArray(data, data.Length - 1);
          }
          break;
        case MetadataType.CharArray:
          {
            result = StringTools.StringFromByteArray(data, data.Length);
          }
          break;
        case MetadataType.Unicode:
          {
            result = Encoding.Unicode.GetString(data, 0, data.Length - 2); // TODO
          }
          break;
        case MetadataType.IntU16:
          {
            UInt16[] array = new UInt16[data.Length / sizeof(UInt16)];
            for (int i = 0; i < array.Length; i++)
            {
              array[i] = BitConverter.ToUInt16(data, i * sizeof(UInt16));
            }
            result = array.Length == 1 ? (object)array[0] : (object)array;
          }
          break;
        case MetadataType.IntU32:
          {
            UInt32[] array = new UInt32[data.Length / sizeof(UInt32)];
            for (int i = 0; i < array.Length; i++)
            {
              array[i] = BitConverter.ToUInt32(data, i * sizeof(UInt32));
            }
            result = array.Length == 1 ? (object)array[0] : (object)array;
          }
          break;
        case MetadataType.IntS32:
          {
            Int32[] array = new Int32[data.Length / sizeof(Int32)];
            for (int i = 0; i < array.Length; i++)
            {
              array[i] = BitConverter.ToInt32(data, i * sizeof(Int32));
            }
            result = array.Length == 1 ? (object)array[0] : (object)array;
          }
          break;
        case MetadataType.FracU32:
          {
            UInt64[] array = new UInt64[data.Length / (2 * sizeof(UInt32))];
            for (int i = 0; i < array.Length; i++)
            {
              int pos = i * 2 * sizeof(UInt32);
              UInt32 numer = BitConverter.ToUInt32(data, pos);
              UInt32 denom = BitConverter.ToUInt32(data, pos + sizeof(UInt32));
              array[i] = ((((UInt64)denom) << 32) | numer);
            }
            result = array.Length == 1 ? (object)array[0] : (object)array;
          }
          break;
        case MetadataType.FracS32:
          {
            Int64[] array = new Int64[data.Length / (2 * sizeof(Int32))];
            for (int i = 0; i < array.Length; i++)
            {
              int pos = i * 2 * sizeof(Int32);
              Int32 numer = BitConverter.ToInt32(data, pos);
              Int32 denom = BitConverter.ToInt32(data, pos + sizeof(Int32));
              array[i] = ((((Int64)(UInt32)denom) << 32) | (UInt32)numer);
            }
            result = array.Length == 1 ? (object)array[0] : (object)array;
          }
          break;
        case MetadataType.Byte:
        case MetadataType.Undefined:
        default:
          {
            result = data != null ? ArrayTools.DuplicateByteArray(data) : null;
          }
          break;
      }

      return result;
    }


    private static void SetObject(PropertyItem prop, object value)
    {
      byte[] bytes;
      MetadataType type;

      if (value is UInt16)
      {
        bytes = BitConverter.GetBytes((UInt16)value);
        type = MetadataType.IntU16;
      }
      else if (value is string)
      {
        // ToDo: If Char Array no '\0'
        bytes = StringTools.ByteArrayFromString((value as string) + '\0');
        type = MetadataType.String;
      }
      else
      {
        bytes = new byte[0];
        type = MetadataType.Undefined;
      }

      prop.Type = (short)type;
      prop.Value = bytes;
      prop.Len = bytes.Length;
    }


    public bool Has(int propId)
    {
      return _get_property_item(propId) != null;
    }


    public object Read(int propId)
    {
      object result = null;

      PropertyItem prop = _get_property_item(propId);

      if (prop != null)
      {
        result = GetObject(prop);
      }

      return result;
    }


    public void Set(int propId, object value)
    {
      PropertyItem prop = _get_property_item(propId);

      if (prop == null)
      {
        prop = CreatePropertyItem(propId);
      }

      if (prop != null)
      {
        SetObject(prop, value);
        fSystemImage.SetPropertyItem(prop);
      }
    }


    public void Set(int propId, byte[] data, MetadataType dataType = 0)
    {
      PropertyItem prop = _get_property_item(propId);

      if (prop == null)
      {
        prop = CreatePropertyItem(propId);
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


    public void Remove(int propId)
    {
      PropertyItem prop = _get_property_item(propId);

      if (prop != null)
      {
        // Attempt removing the property only if it exists
        fSystemImage.RemovePropertyItem(propId);
      }
    }


    public bool Has(string propName)
    {
      int propId = MetadataLibrary.GetId(propName);
      return Has(propId);
    }


    public object Read(string propName)
    {
      int propId = MetadataLibrary.GetId(propName);
      return Read(propId);
    }


    public void Set(string propName, object value)
    {
      int propId = MetadataLibrary.GetId(propName);
      Set(propId, value);
    }


    public void Set(string propName, byte[] data, MetadataType dataType = 0)
    {
      int propId = MetadataLibrary.GetId(propName);
      Set(propId, data, dataType);
    }


    public void Remove(string propName)
    {
      int propId = MetadataLibrary.GetId(propName);
      Remove(propId);
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
    public object Value { get; set; }


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

      Value = null;

      if (prop != null)
      {
        // Assume prop.Value.Length == prop.Len
        Value = MetadataPortal.GetObject(prop);

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


    public override string ToString()
    {
      if (Value == null)
      {
        return "?";
      }
      else if (Value is Int64)
      {
        int numer = (Int32)((Int64)Value & 0xFFFFFFFF);
        int denom = (Int32)(((Int64)Value >> 32) & 0xFFFFFFFF);
        return _fracToString(numer, denom);
      }
      else if (Value is UInt64)
      {
        int numer = (Int32)((UInt64)Value & 0xFFFFFFFF);
        int denom = (Int32)(((UInt64)Value >> 32) & 0xFFFFFFFF);
        return _fracToString(numer, denom);
      }
      else if (Value is byte[])
      {
        return StringTools.HexStringFromByteArray((byte[])Value);
      }
      else if (Value is Array)
      {
        string result = "";
        string delimiter = "";

        foreach (var item in Value as Array)
        {
          result += delimiter + item.ToString();
          delimiter = ", ";
        }

        return result;
      }
      else if (Value is string)
      {
        return new string((Value as string).Where(c => !char.IsControl(c)).ToArray());
      }
      else
      {
        return Value.ToString();
      }
    }


    public void FromString(string valueAsString)
    {
      switch (Definition.DataType)
      {
        case MetadataType.String:
          {
            Value = valueAsString;
          }
          break;
        case MetadataType.CharArray:
          {
            Value = valueAsString;
          }
          break;
        case MetadataType.Unicode:
          {
            Value = new byte[0]; // TODO
            // bytes = Encoding.Unicode.GetBytes(valueAsString + '\0');
          }
          break;
        case MetadataType.IntU16:
          {
            string[] fields = valueAsString.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            UInt16[] values = new UInt16[fields.Length];

            for (int i = 0; i < fields.Length; i++)
            {
              values[i] = UInt16.Parse(fields[i]);
            }

            Value = values.Length == 1 ? (object)values[0] : (object)values;
          }
          break;
        case MetadataType.IntU32:
          {
            string[] fields = valueAsString.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            UInt32[] values = new UInt32[fields.Length];

            for (int i = 0; i < fields.Length; i++)
            {
              values[i] = UInt32.Parse(fields[i]);
            }

            Value = values.Length == 1 ? (object)values[0] : (object)values;
          }
          break;
        case MetadataType.IntS32:
          {
            string[] fields = valueAsString.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            Int32[] values = new Int32[fields.Length];

            for (int i = 0; i < fields.Length; i++)
            {
              values[i] = Int32.Parse(fields[i]);
            }

            Value = values.Length == 1 ? (object)values[0] : (object)values;
          }
          break;
        case MetadataType.FracU32:
          {
            Value = new byte[0]; // TODO
          }
          break;
        case MetadataType.FracS32:
          {
            Value = new byte[0]; // TODO
          }
          break;
        case MetadataType.Byte:
        case MetadataType.Undefined:
        default:
          {
            Value = StringTools.ByteArrayFromHexString(valueAsString);
          }
          break;
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

    private static MetadataLibrary _instance = new MetadataLibrary();

    private Dictionary<int, MetadataDefinition> fIdDefLookup;

    private Dictionary<string, int> fNameIdLookup;


    public static void AddDefinition(MetadataDefinition defn)
    {
      if (_instance.fIdDefLookup.ContainsKey(defn.Id) == false)
      {
        // TODO: Test
        _instance.fIdDefLookup.Add(defn.Id, defn);
        _instance.fNameIdLookup.Add(defn.Category + "." + defn.Name, defn.Id);
      }
    }


    private MetadataLibrary()
    {
      fIdDefLookup = new Dictionary<int, MetadataDefinition>();
      fNameIdLookup = new Dictionary<string, int>();

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

              fIdDefLookup.Add(defn.Id, defn);
              fNameIdLookup.Add(defn.Category + "." + defn.Name, defn.Id);
            }
          }
        }
      }
      catch (Exception ex)
      {
        fIdDefLookup.Clear();
        fNameIdLookup.Clear();
      }
    }


    public static MetadataDefinition Lookup(int id)
    {
      return _instance.fIdDefLookup.ContainsKey(id) ? _instance.fIdDefLookup[id] : null;
    }


    public static int GetId(string name)
    {
      return _instance.fNameIdLookup.ContainsKey(name) ? _instance.fNameIdLookup[name] : -1;
    }


    public static MetadataDefinition Lookup(string name)
    {
      return Lookup(GetId(name));
    }


    public static List<MetadataDefinition> GetList()
    {
      List<MetadataDefinition> result = new List<MetadataDefinition>();

      foreach (KeyValuePair<int, MetadataDefinition> kv in _instance.fIdDefLookup)
      {
        result.Add(kv.Value);
      }

      return result;
    }
  }
}
