using System;
using System.Collections.Generic;
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

