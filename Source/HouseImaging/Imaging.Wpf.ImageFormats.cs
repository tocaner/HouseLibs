using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace HouseImaging.Wpf
{
  public enum ImageFormatEnum { Bmp, Jpeg, Gif, Tiff, Png, Icon, Wmp, Unknown };


  static class ImageFormat
  {
    public static ImageFormatEnum FromExtension(string ext)
    {
      switch (ext.ToLower())
      {
        case ".jpg": case ".jpeg": return ImageFormatEnum.Jpeg;
        case ".bmp": return ImageFormatEnum.Bmp;
        case ".gif": return ImageFormatEnum.Gif;
        case ".tif": case ".tiff":  return ImageFormatEnum.Tiff;
        case ".png": return ImageFormatEnum.Png;
        case ".ico": case ".icon": return ImageFormatEnum.Icon;
        case ".wmp": return ImageFormatEnum.Wmp;
        default: return ImageFormatEnum.Unknown;
      }
    }


    public static ImageFormatEnum FromPath(string filePath)
    {
      return FromExtension(Path.GetExtension(filePath));
    }


    public static string GetExtension(this ImageFormatEnum format)
    {
      switch (format)
      {
        case ImageFormatEnum.Jpeg: return ".jpg";
        case ImageFormatEnum.Bmp: return ".bmp";
        case ImageFormatEnum.Gif: return ".gif";
        case ImageFormatEnum.Tiff: return ".tif";
        case ImageFormatEnum.Png: return ".png";
        case ImageFormatEnum.Icon: return ".ico";
        case ImageFormatEnum.Wmp: return ".wmp";
        default: return string.Empty;
      }
    }


    public static string GetDescription(this ImageFormatEnum format)
    {
      return format.ToString();
    }


    public static ImageFormatEnum FromDecoder(BitmapDecoder decoder)
    {
      if (decoder is JpegBitmapDecoder)
      {
        return ImageFormatEnum.Jpeg;
      }
      else if (decoder is BmpBitmapDecoder)
      {
        return ImageFormatEnum.Bmp;
      }
      else if (decoder is GifBitmapDecoder)
      {
        return ImageFormatEnum.Gif;
      }
      else if (decoder is PngBitmapDecoder)
      {
        return ImageFormatEnum.Png;
      }
      else if (decoder is TiffBitmapDecoder)
      {
        return ImageFormatEnum.Tiff;
      }
      else if (decoder is IconBitmapDecoder)
      {
        return ImageFormatEnum.Icon;
      }
      else if (decoder is WmpBitmapDecoder)
      {
        return ImageFormatEnum.Wmp;
      }
      else
      {
        return ImageFormatEnum.Unknown;
      }
    }
  }
}

