using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HouseImaging.Metadata;


namespace HouseImaging.Wpf
{
  public class MetadataPortal
  {
    private ImageInfo fImageInfo;
    private BitmapMetadata fNewMetadata;


    public MetadataPortal(ImageInfo imageInfo)
    {
      fImageInfo = imageInfo;
      BitmapMetadata md = imageInfo.GetSystemImageSource().Metadata as BitmapMetadata;
      fNewMetadata = md != null ? md.Clone() : new BitmapMetadata(imageInfo.GetImageFormatDescription());
    }


    private string Query(MetadataDefinition defn)
    {
      string result;

      if (defn != null)
      {
        try
        {
          switch (defn.FullDir.ToUpper())
          {
            case "APP0":
              result = string.Format("/app0/{{ushort={0}}}", defn.Code);
              break;

            case "APP1":
              result = string.Format("/app1/{{ushort=0}}/{{ushort={0}}}", defn.Code);
              break;

            case "APP1.THUMBNAIL":
              result = string.Format("/app1/{{ushort=1}}/{{ushort={0}}}", defn.Code);
              break;

            case "APP1.EXIF":
              result = string.Format("/app1/{{ushort=0}}/{{ushort=34665}}/{{ushort={0}}}", defn.Code);
              break;

            case "APP1.GPS":
              result = string.Format("/app1/{{ushort=0}}/{{ushort=34853}}/{{ushort={0}}}", defn.Code);
              break;

            default:
              result = string.Format("/app1/{{ushort=0}}/{{ushort={0}}}", defn.Code);
              break;
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
          result = string.Empty;
        }
      }
      else
      {
        result = string.Empty;
      }

      return result;
    }


    private MetadataDefinition Lookup(string propName)
    {
      MetadataDefinition result;

      string[] parts = propName.Split('.');

      if ((parts.Length > 1) && (parts[0] == "Thumbnail"))
      {
        result = MetadataLibrary.LookupName(parts.Last());
        result.Section = parts[0];
      }
      else
      {
        result = MetadataLibrary.LookupName(propName);
      }

      return result;
    }


    public bool Has(MetadataDefinition defn)
    {
      string query = Query(defn);
      return fNewMetadata.ContainsQuery(query);
    }


    public bool Has(string propName)
    {
      return Has(Lookup(propName));
    }


    public void Remove(MetadataDefinition defn)
    {
      string query = Query(defn);
      fNewMetadata.RemoveQuery(query);
    }


    public void Remove(string propName)
    {
      Remove(Lookup(propName));
    }


    public MetadataItem Read(MetadataDefinition defn)
    {
      string query = Query(defn);
      object value = fNewMetadata.GetQuery(query);
      return new MetadataItem(defn, value);
    }


    public MetadataItem Read(string propName)
    {
      return Read(Lookup(propName));
    }


    public void Set(MetadataDefinition defn, object value)
    {
      string query = Query(defn);
      fNewMetadata.SetQuery(query, value);
    }


    public void Set(string propName, object value)
    {
      Set(Lookup(propName), value);
    }


    public List<MetadataItem> GetList()
    {
      List<MetadataItem> result = new List<MetadataItem>();
      CaptureMetadata(result, this.fNewMetadata, "");
      return result;
    }


    private void CaptureMetadata(List<MetadataItem> result, ImageMetadata imageMetadata, string query)
    {
      BitmapMetadata bitmapMetadata = imageMetadata as BitmapMetadata;

      if (bitmapMetadata != null)
      {
        foreach (string relativeQuery in bitmapMetadata)
        {
          string fullQuery = query + relativeQuery;
          object resp = bitmapMetadata.GetQuery(relativeQuery);
          BitmapMetadata innerBitmapMetadata = resp as BitmapMetadata;

          if (innerBitmapMetadata == null)
          {
            MetadataDefinition defn = GetMetadataDefinitionFromQuery(fullQuery);
            MetadataItem metadataItem = new MetadataItem(defn, resp);
            result.Add(metadataItem);
          }
          else
          {
            CaptureMetadata(result, innerBitmapMetadata, fullQuery);
          }
        }
      }
    }


    private static string[,] fTable = new string[,]
    {
      // Path                                 // Directory    // Section 
      { "/app1/{ushort=0}/{ushort=34665}/",    "APP1",         "EXIF"      },
      { "/app1/{ushort=0}/{ushort=34853}/",    "APP1",         "GPS"       },
      { "/app1/{ushort=0}/",                   "APP1",         ""          },
      { "/app1/{ushort=1}/",                   "APP1",         "THUMBNAIL" },
      { "/app1/",                              "APP1",         ""          },
      { "/app0/",                              "APP0",         ""          }
    };


    private MetadataDefinition GetMetadataDefinitionFromQuery(string path)
    {
      MetadataDefinition result = MetadataLibrary.LookupPath(path);

      if (result == null)
      {
        string name = "name?";
        string directory = "directory?";
        string section = "";
        int code = -1;

        switch (fImageInfo.GetImageFormat())
        {
          case ImageFormatEnum.Jpeg:
            {
              for (int i = 0; i < fTable.GetLength(0); i++)
              {
                if (path.StartsWith(fTable[i, 0]))
                {
                  name = path.Substring(fTable[i, 0].Length);
                  directory = fTable[i, 1];
                  section = fTable[i, 2];

                  try
                  {
                    string[] p = name.Split(new string[] { "{ushort=", "}" }, System.StringSplitOptions.RemoveEmptyEntries);
                    code = UInt16.Parse(p[0]);

                    // Try finding the definition in dictionary
                    result = MetadataLibrary.LookupCode(code, directory);

                    if (result != null)
                    {
                      result.Section = section;
                    }
                  }
                  catch
                  {
                    code = -1;
                  }
                  break;
                }
              }
            }
            break;

          case ImageFormatEnum.Gif:
          // TODO
          case ImageFormatEnum.Tiff:
          // TODO
          case ImageFormatEnum.Png:
          // TODO
          default:
            break;
        }

        if (result == null)
        {
          result = new MetadataDefinition()
          {
            Code = code,
            Section = section,
            Name = name,
            Directory = directory,
            DataType = 0,
            Description = string.Empty
          };
        }

        result.Path = path;
      }

      return result;
    }
  }
}
