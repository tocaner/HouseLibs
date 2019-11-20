using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Reflection;
using HouseUtils;
using HouseImaging;


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


    private string Query(int propId)
    {
      return string.Format("/app1/{{ushort=0}}/{{ushort={0}}}", propId);
    }


    public bool Has(int propId)
    {
      string query = Query(propId);
      return fNewMetadata.ContainsQuery(query);
    }


    public void Remove(int propId)
    {
      string query = Query(propId);
      fNewMetadata.RemoveQuery(query);
    }


    public object Read(int propId)
    {
      string query = Query(propId);
      return fNewMetadata.GetQuery(query);
    }


    public void Set(int propId, object value)
    {
      string query = Query(propId);
      fNewMetadata.SetQuery(query, value);
    }


    public bool Has(string propName)
    {
      int id = MetadataLibrary.GetId(propName);
      return Has(id);
    }


    public void Remove(string propName)
    {
      int id = MetadataLibrary.GetId(propName);
      Remove(id);
    }


    public object Read(string propName)
    {
      int id = MetadataLibrary.GetId(propName);
      return Read(id);
    }


    public void Set(string propName, object value)
    {
      int id = MetadataLibrary.GetId(propName);
      Set(id, value);
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

          MetadataDefinition defn = new MetadataDefinition()
          {
            Category = "Category",
            Name = resp.GetType().ToString(),
            Path = fullQuery
          };

          MetadataItem metadataItem = new MetadataItem(defn, resp);
          result.Add(metadataItem);
          BitmapMetadata innerBitmapMetadata = resp as BitmapMetadata;
          if (innerBitmapMetadata != null)
          {
            CaptureMetadata(result, innerBitmapMetadata, fullQuery);
          }
        }
      }
    }


    public List<MetadataItem> GetList()
    {
      List<MetadataItem> result = new List<MetadataItem>();
      CaptureMetadata(result, this.fNewMetadata, "");
      return result;
    }
  }
}
