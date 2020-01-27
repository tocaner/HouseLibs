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
  public class ImageTools
  {
    public static BitmapEncoder GetEncoder(ImageFormatEnum format)
    {
      switch (format)
      {
        case ImageFormatEnum.Jpeg: return new JpegBitmapEncoder();
        case ImageFormatEnum.Bmp: return new BmpBitmapEncoder();
        case ImageFormatEnum.Gif: return new GifBitmapEncoder();
        case ImageFormatEnum.Png: return new PngBitmapEncoder();
        case ImageFormatEnum.Tiff: return new TiffBitmapEncoder();
        case ImageFormatEnum.Icon: return new PngBitmapEncoder(); // There is no IconBitmapEncoder
        case ImageFormatEnum.Wmp: return new WmpBitmapEncoder();
        default: return null;
      }
    }


    public static BitmapEncoder GetEncoder(string filePath)
    {
      return GetEncoder(ImageFormat.FromPath(filePath));
    }


    public static void ImageSourceToFile(BitmapSource bitmapSource, string filePath, ImageFormatEnum format = ImageFormatEnum.Unknown)
    {
      ImageFormatEnum extFormat = ImageFormat.FromPath(filePath);

      if (format == ImageFormatEnum.Unknown)
      {
        // If format is not specified, then use file extension to determine the target format
        format = extFormat; 
      }

      if (format == ImageFormatEnum.Unknown)
      {
        // If format could not be resolved, use Png as default format
        format = ImageFormatEnum.Png;
      }

      if (extFormat != format)
      {
        // If specified format mismatches the file extension, then replace the file extension
        filePath = Path.ChangeExtension(filePath, format.GetExtension()); 
      }
      
      BitmapEncoder encoder = ImageTools.GetEncoder(format);
      ImageSourceToFile(bitmapSource, encoder, filePath);
    }


    public static void ImageSourceToFile(BitmapSource bitmapSource, BitmapEncoder encoder, string filePath)
    {
      encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

      using (var fileStream = new FileStream(filePath, FileMode.Create))
      {
        encoder.Save(fileStream);
      }
    }


    public static BitmapSource ByteArrayToBitmapSource(byte[] imageData)
    {
      BitmapSource result;

      using (MemoryStream memoryStream = new MemoryStream(imageData))
      {
        BitmapFrame bitmapFrame = BitmapFrame.Create(memoryStream,
          BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
        result = bitmapFrame;
      }

      return result;
    }


    public static byte[] BitmapSourceToByteArray(BitmapSource bitmapSource, BitmapEncoder encoder)
    {
      byte[] bytes = null;

      encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

      using (var stream = new MemoryStream())
      {
        encoder.Save(stream);
        bytes = stream.ToArray();
      }

      return bytes;
    }


    public static byte[] ExtractPixelBytesFromBitmap(BitmapSource bitmapSource)
    {
      // From: https://social.msdn.microsoft.com/Forums/vstudio/en-US/9bf9dea5-e21e-4361-a0a6-be331efde835/how-do-you-calculate-the-image-stride-for-a-bitmap?forum=csharpgeneral
      int bytesPerPixel = (bitmapSource.Format.BitsPerPixel + 7) / 8;
      int stride = 4 * ((bitmapSource.PixelWidth * bytesPerPixel + 3) / 4);

      byte[] pixels = new byte[bitmapSource.PixelHeight * stride];
      bitmapSource.CopyPixels(pixels, stride, 0);
      return pixels;
    }
  }
}
