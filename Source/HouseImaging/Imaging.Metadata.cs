using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using HouseUtils;


namespace HouseImaging.Metadata
{
  public class MetadataPortal
  {
    private Image fSystemImage;
    
     
    public MetadataPortal(ImageInfo imageInfo)
    {
      fSystemImage = imageInfo.SystemImage;
    }


    private PropertyItem _get_property_item(MetadataDefinition defn)
    {
      return defn != null ? fSystemImage.PropertyItems.FirstOrDefault(item => item.Id == defn.Code) : null;
    }


    public static object GetObject(PropertyItem prop)
    {
      object result = null;

      if (prop != null)
      {
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
              result = Encoding.Unicode.GetString(data, 0, data.Length - 2);
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
      else if (value is UInt32)
      {
        bytes = BitConverter.GetBytes((UInt32)value);
        type = MetadataType.IntU32;
      }
      else if (value is Int32)
      {
        bytes = BitConverter.GetBytes((Int32)value);
        type = MetadataType.IntS32;
      }
      else if (value is string)
      {
        // TODO: If Char Array no '\0'
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


    public bool Has(MetadataDefinition defn)
    {
      return _get_property_item(defn) != null;
    }


    public bool Has(string propName)
    {
      MetadataDefinition defn = MetadataLibrary.LookupName(propName);
      return Has(defn);
    }


    public MetadataItem Read(MetadataDefinition defn)
    {
      PropertyItem prop = _get_property_item(defn);
      object value = GetObject(prop);
      return new MetadataItem(defn, value);
    }


    public MetadataItem Read(string propName)
    {
      MetadataDefinition defn = MetadataLibrary.LookupName(propName);
      return Read(defn);
    }


    public void Set(MetadataDefinition defn, object value)
    {
      PropertyItem prop = _get_property_item(defn);

      if (prop == null)
      {
        prop = CreatePropertyItem(defn.Code);
      }

      if (prop != null)
      {
        SetObject(prop, value);
        fSystemImage.SetPropertyItem(prop);
      }
    }


    public void Set(string propName, object value)
    {
      MetadataDefinition defn = MetadataLibrary.LookupName(propName);
      Set(defn, value);
    }


    public void Remove(MetadataDefinition defn)
    {
      PropertyItem prop = _get_property_item(defn);

      if (prop != null)
      {
        // Attempt removing the property only if it exists
        fSystemImage.RemovePropertyItem(prop.Id);
      }
    }


    public void Remove(string propName)
    {
      MetadataDefinition defn = MetadataLibrary.LookupName(propName);
      Remove(defn);
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
        MetadataDefinition defn = MetadataLibrary.LookupCode(prop.Id);

        if (defn == null)
        {
          defn = new MetadataDefinition()
          {
            Directory = "APP1",
            Code = prop.Id,
            Name = "?",
            Description = string.Empty,
            DataType = prop.Type != 0 ? (MetadataType)prop.Type : MetadataType.Undefined
          };
        }

        object value = GetObject(prop);

        result.Add(new MetadataItem(defn, value));
      }

      return result;
    }


    public List<MetadataItem> GetAllDefinedMetadata()
    {
      // Returns all metadata from the picture + other definitions from the library
      List<MetadataItem> result = GetList();

      foreach (MetadataDefinition defn in MetadataLibrary.GetList())
      {
        MetadataItem item = result.FirstOrDefault(c => c.Definition.GetId() == defn.GetId());

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
            string line = string.Format("{0}, {0:X4}, {1}, {2}, {3}, {4}",
              defn.Directory,
              defn.Code,
              defn.Name,
              defn.DataType,
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
        object json = new
        {
          Directory = defn.Directory,
          Section = defn.Section,
          Code = defn.Code,
          Path = defn.Path,
          Name = defn.Name,
          Description = defn.Description,
          DataType = Enum.GetName(typeof(MetadataType), defn.DataType)
        };

        result.Add(defn.GetId(), json);
      }

      File.WriteAllText(path, Json.Serialize(result));
    }
  }


  public class MetadataItem
  {
    public MetadataDefinition Definition { get; }
    public object Value { get; set; }


    public MetadataItem(MetadataDefinition defn, object value = null)
    {
      // TODO: Test code, remove later...
      if (value != null)
      {
        MetadataType check = GetMetadataTypeOf(value);
        if (check != defn.DataType)
        {
          bool ok = false;

          if(check == MetadataType.Undefined)
          {
            if (defn.DataType == MetadataType.Unicode)
            {
              ok = true;
            }
          }

          if (ok == false)
          {
            check = GetMetadataTypeOf(value);
          }
        }
      }

      Definition = defn;

      // Handle unicode data here, because extracting from System Image object returns byte[] object
      if ((value != null) && (defn.DataType == MetadataType.Unicode))
      {
        byte[] data = (byte[])value;
        Value = Encoding.Unicode.GetString(data, 0, data.Length - 2);
      }
      else
      {
        Value = value;
      }
    }


    public static MetadataType GetMetadataTypeOf(object value)
    {
      if (value == null)
      {
        return MetadataType.None;
      }
      else if (value is Int64)
      {
        return MetadataType.FracS32;
      }
      else if (value is UInt64)
      {
        return MetadataType.FracU32;
      }
      else if (value is byte[])
      {
        return MetadataType.Undefined;
      }
      else if ((value is UInt16) || (value is UInt16[]))
      {
        return MetadataType.IntU16;
      }
      else if ((value is UInt32) || (value is UInt32[]))
      {
        return MetadataType.IntU32;
      }
      else if ((value is Int32) || (value is Int32[]))
      {
        return MetadataType.IntS32;
      }
      else if (value is string)
      {
        return MetadataType.String;
      }
      else if (value is byte)
      {
        return MetadataType.Byte;
      }
      else if (value is System.Windows.Media.Imaging.BitmapMetadataBlob)
      {
        return MetadataType.Undefined;
      }
      else
      {
        return MetadataType.None;
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
      else if (Value.GetType().IsPrimitive)
      {
        return Value.ToString();
      }
      else if (Value is System.Windows.Media.Imaging.BitmapMetadataBlob)
      {
        byte[] data = (Value as System.Windows.Media.Imaging.BitmapMetadataBlob).GetBlobValue();
        return "Blob = " + (data.Length > 0 ? StringTools.HexStringFromByteArray(data) : "Empty");
      }
      else
      {
        return Value.GetType().ToString();
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
            Value = Encoding.Unicode.GetBytes(valueAsString + '\0');
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
          {
            Value = byte.Parse(valueAsString);
          }
          break;
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
    None = 0,
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
    public string Path { get; set; }
    public string Directory { get; set; }
    public string Section { get; set; } = string.Empty;
    public int Code { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public MetadataType DataType { get; set; }

    public string FullDir
    {
      get
      {
        if (string.IsNullOrEmpty(Section))
        {
          return this.Directory;
        }
        else
        {
          return this.Directory + "." + this.Section;
        }
      }
    }

    public string FullName
    {
      get
      {
        if (string.IsNullOrEmpty(Section))
        {
          return this.Name;
        }
        else
        {
          return this.Section + "." + this.Name;
        }
      }
    }

    public static string ComposeId(string directory, int code)
    {
      return directory.ToUpper() + "-" + code.ToString("X4");
    }

    public string GetId()
    {
      return ComposeId(Directory, Code);
    }

    public MetadataDefinition Copy()
    {
      return new MetadataDefinition
      {
        Path = this.Path,
        Directory = this.Directory,
        Section = this.Section,
        Code = this.Code,
        Name = this.Name,
        Description = this.Description,
        DataType = this.DataType
      };
    }
  }


  public class MetadataLibrary
  {
    private const string RESOURCENAME = "HouseImaging.TagDefinitions.json";

    private static MetadataLibrary _instance = new MetadataLibrary();

    private Dictionary<string, MetadataDefinition> fIdLookup;

    private Dictionary<string, MetadataDefinition> fNameLookup;


    private void AddIdLookup(MetadataDefinition defn)
    {
      try
      {
        fIdLookup.Add(defn.GetId(), defn);
      }
      catch
      {
        // Already in dictionary
      }
    }


    private void AddNameLookup(MetadataDefinition defn)
    {
      try
      {
        fNameLookup.Add(defn.FullName, defn);
      }
      catch
      {
        // Already in dictionary
      }
    }


    private MetadataLibrary()
    {
      fIdLookup = new Dictionary<string, MetadataDefinition>();
      fNameLookup = new Dictionary<string, MetadataDefinition>();

      try
      {
        //        using (Stream stream = new FileStream("TagDefinitions.json", FileMode.Open, FileAccess.Read))
        using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(RESOURCENAME))
        {
          using (StreamReader reader = new StreamReader(stream))
          {
            string input = reader.ReadToEnd();
            fIdLookup = Json.Deserialize<MetadataDefinition>(input);

            foreach (KeyValuePair<string, MetadataDefinition> kv in fIdLookup)
            {
              AddNameLookup(kv.Value);
            }
          }
        }
      }
      catch (Exception)
      {
        fIdLookup.Clear();
        fNameLookup.Clear();
      }
    }


    public static void AddDefinition(MetadataDefinition defn)
    {
      _instance.AddIdLookup(defn);
      _instance.AddNameLookup(defn);
    }


    public static MetadataDefinition LookupId(string id)
    {
      try
      {
        return _instance.fIdLookup[id].Copy();
      }
      catch
      {
        return null; // Not found in dictionary
      }
    }


    public static MetadataDefinition LookupCode(int code, string directory = "APP1")
    {
      string id = MetadataDefinition.ComposeId(directory, code);
      return LookupId(id);
    }


    public static MetadataDefinition LookupName(string uniqueName)
    {
      try
      {
        return _instance.fNameLookup[uniqueName].Copy();
      }
      catch
      {
        return null; // Not found in dictionary
      }
    }


    public static MetadataDefinition LookupPath(string path)
    {
      MetadataDefinition result = null;

      foreach (KeyValuePair<string, MetadataDefinition> kv in _instance.fIdLookup)
      {
        MetadataDefinition defn = kv.Value;
        if (defn.Path == path)
        {
          result = defn;
        }
      }

      return result;
    }


    public static List<MetadataDefinition> GetList()
    {
      List<MetadataDefinition> result = new List<MetadataDefinition>();

      foreach (KeyValuePair<string, MetadataDefinition> kv in _instance.fIdLookup)
      {
        result.Add(kv.Value);
      }

      return result;
    }
  }
}
