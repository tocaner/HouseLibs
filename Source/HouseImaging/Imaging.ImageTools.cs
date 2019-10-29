using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;


namespace HouseImaging
{
  public class ImageTools
  {
    private static readonly ImageConverter fImageConverter = new ImageConverter();


    public static byte[] ByteArrayFromImage(Image image)
    {
      byte[] result = (byte[])fImageConverter.ConvertTo(image, typeof(byte[]));
      return result;
    }


    public static Image ImageFromByteArray(byte[] byteArray)
    {
      Image result = (Image)fImageConverter.ConvertFrom(byteArray);
      return result;
    }


    public static Image ImageClone(Image source)
    {
      byte[] byteArray = ByteArrayFromImage(source);
      return ImageFromByteArray(byteArray);
    }


    public static byte[] GetPixelBytesFromBitmap(Bitmap bitmap)
    {
      BitmapData bitmapData = bitmap.LockBits(
          new Rectangle(0, 0, bitmap.Width, bitmap.Height),
          ImageLockMode.ReadOnly,
          bitmap.PixelFormat);

      var length = bitmapData.Stride * bitmapData.Height;
      byte[] result = new byte[length];
      Marshal.Copy(bitmapData.Scan0, result, 0, length);
      bitmap.UnlockBits(bitmapData);

      return result;
    }


    public static byte[] GetPixelBytesFromImage(Image image)
    {
      return GetPixelBytesFromBitmap(new Bitmap(image));
    }


    public static byte[] LoadImageByteArrayFromFile(string fileName)
    {
      return File.ReadAllBytes(fileName);
    }


    public static byte[] LoadImageByteArrayFromStream(Stream stream)
    {
      byte[] result = new byte[stream.Length];
      stream.Read(result, 0, result.Length);
      return result;
    }


    public static void SaveImageByteArrayToFile(string fileName, byte[] byteArray)
    {
      File.WriteAllBytes(fileName, byteArray);
    }


    public static void SaveImageByteArrayToStream(Stream stream, byte[] byteArray)
    {
      stream.Write(byteArray, 0, byteArray.Length);
    }


    public static Image LoadImageFromFile(string fileName)
    {
      byte[] byteArray = LoadImageByteArrayFromFile(fileName);
      return ImageFromByteArray(byteArray);
    }


    public static Image LoadImageFromStream(Stream stream)
    {
      byte[] byteArray = LoadImageByteArrayFromStream(stream);
      return ImageFromByteArray(byteArray);
    }


    public static void SaveImageToFile(Image image, string fileName, ImageFormat format = null, int compressionFactor = 80)
    {
      byte[] byteArray = EncodeImage(image, format, compressionFactor);
      SaveImageByteArrayToFile(fileName, byteArray);
    }


    public static void SaveImageToStream(Image image, Stream stream, ImageFormat format = null, int compressionFactor = 80)
    {
      byte[] byteArray = EncodeImage(image, format, compressionFactor);
      SaveImageByteArrayToStream(stream, byteArray);
    }


    public static Image ConvertImage(Image image, ImageFormat format, int compressionFactor)
    {
      byte[] byteArray = EncodeImage(image, format, compressionFactor);
      return ImageFromByteArray(byteArray);
    }


    private static byte[] EncodeImageParams(Image image, ImageFormat format, EncoderParameters encoderParams)
    {
      System.Diagnostics.Debug.Assert(format != null);

      byte[] result = null;

      ImageCodecInfo codecInfo = GetEncoderInfo(format);

      if (codecInfo != null)
      {
        using (MemoryStream stream = new MemoryStream())
        {
          image.Save(stream, codecInfo, encoderParams);
          result = stream.ToArray();
        }
      }
      else
      {
        // Default image native data to byte array without encoding 
        result = ImageTools.ByteArrayFromImage(image);
      }

      return result;
    }


    // From SaveImageToFile, JpegCompress
    public static byte[] EncodeImage(Image image, ImageFormat destFormat, int quality = 80)
    {
      byte[] imageBytes;

      if ((destFormat == null) || (image.RawFormat.Equals(destFormat)))
      {
        imageBytes = ImageTools.ByteArrayFromImage(image);
      }
      else
      {
        EncoderParameters encoderParams = null;

        if (destFormat.Equals(ImageFormat.Jpeg))
        {
          // quality is used by Jpeg and probably ignored by other formats
          encoderParams = new EncoderParameters(1);
          encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
        }

        imageBytes = ImageTools.EncodeImageParams(image, destFormat, encoderParams);
      }

      return imageBytes;
    }


    private static ImageCodecInfo GetEncoderInfo(ImageFormat format)
    {
      ImageCodecInfo result = null;
      ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();

      for(int j = 0; (result == null) && (j < encoders.Length); j++)
      {
        if(encoders[j].FormatID == format.Guid)
        {
          result = encoders[j];
        }
      }
      return result;
    }


    public static Image CreateThumbnail(Image image, int maxWidth, int maxHeight)
    {
      Rectangle area = new Rectangle(0, 0, maxWidth, maxHeight);
      Rectangle fit = FitToArea(image.Width, image.Height, area);
      return image.GetThumbnailImage(fit.Width, fit.Height, () => { return false; }, IntPtr.Zero);
    }


    public static Rectangle FitToArea(int imageWidth, int imageHeight, Rectangle area)
    {
      double image_aspect_ratio = imageWidth / (double)imageHeight;
      double area_aspect_ratio = area.Width / (double)area.Height;

      int fittedWidth;
      int fittedHeight;

      if (image_aspect_ratio > area_aspect_ratio) // same as page.IsLandscape
      {
        // means our image has the width as the maximum dimension
        fittedWidth = area.Width;
        fittedHeight = (int)(area.Width / image_aspect_ratio);
      }
      else
      {
        // means our image has the height as the maximum dimension
        fittedWidth = (int)(area.Height * image_aspect_ratio);
        fittedHeight = area.Height;
      }

      Rectangle result = new Rectangle();

      result.Width = fittedWidth;
      result.Height = fittedHeight;
      result.X = area.X + ((area.Width - fittedWidth) / 2);
      result.Y = area.Y + ((area.Height - fittedHeight) / 2);

      return result;
    }


    readonly static RotateFlipType[] rf_table =
    { 
      RotateFlipType.RotateNoneFlipNone,
      RotateFlipType.Rotate90FlipNone,
      RotateFlipType.Rotate180FlipNone,
      RotateFlipType.Rotate270FlipNone,
      RotateFlipType.RotateNoneFlipX,
      RotateFlipType.Rotate90FlipY,
      RotateFlipType.Rotate180FlipX,
      RotateFlipType.Rotate270FlipY,
    };


    public static Image TransformImage(Image image, int transformationIndex)
    {
      Image result;

      if (image.RawFormat.Equals(ImageFormat.Jpeg))
      {
        EncoderValue rotationCode;
        EncoderValue flipCode;

        switch (transformationIndex & 3)
        {
          case 1:  rotationCode = EncoderValue.TransformRotate90; break;
          case 2:  rotationCode = EncoderValue.TransformRotate180; break;
          case 3:  rotationCode = EncoderValue.TransformRotate270; break;
          default: rotationCode = 0; break;
        }

        switch (transformationIndex)
        {
          case 4: flipCode = EncoderValue.TransformFlipHorizontal; break;
          case 5: flipCode = EncoderValue.TransformFlipVertical; break;
          case 6: flipCode = EncoderValue.TransformFlipHorizontal; break;
          case 7: flipCode = EncoderValue.TransformFlipVertical; break;
          default: flipCode = 0; break;
        }

        if ((rotationCode > 0) && (flipCode > 0))
        {
          using (Image firstStep = TransformJpeg(image, rotationCode))
          {
            result = TransformJpeg(firstStep, flipCode);
          }
        }
        else if (rotationCode > 0)
        {
          result = TransformJpeg(image, rotationCode);
        }
        else if (flipCode > 0)
        {
          result = TransformJpeg(image, flipCode);
        }
        else
        {
          result = image;
        }
      }
      else
      {
        image.RotateFlip(rf_table[transformationIndex]);
        result = image;
      }
      return result;
    }


    private static Image TransformJpeg(Image image, EncoderValue transformationCode)
    {
      EncoderParameters encoderParams = new EncoderParameters(1);
      encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Transformation, (long)transformationCode);
      byte[] imageBytes = ImageTools.EncodeImageParams(image, ImageFormat.Jpeg, encoderParams);
      return ImageFromByteArray(imageBytes);
    }


#if CLEAN_UP
    
    public static string GetImageFormatDescription(Image image)
    {
      string[] result = _getImageRawFormatDescription(image.RawFormat);
      return result[0];
    }


    public static string GetImageExtension(Image image)
    {
      string[] result = _getImageRawFormatDescription(image.RawFormat);
      return result[1];
    }


    private static string[] _getImageRawFormatDescription(System.Drawing.Imaging.ImageFormat rawFormat)
    {
      string[] result = new string[2];

      if (rawFormat.Equals(System.Drawing.Imaging.ImageFormat.Jpeg))
      {
        result[0] = "Jpeg";
        result[1] = ".jpg";
      }
      else if (rawFormat.Equals(System.Drawing.Imaging.ImageFormat.Bmp))
      {
        result[0] = "Bmp";
        result[1] = ".bmp";
      }
      else if (rawFormat.Equals(System.Drawing.Imaging.ImageFormat.Png))
      {
        result[0] = "Png";
        result[1] = ".png";
      }
      else if (rawFormat.Equals(System.Drawing.Imaging.ImageFormat.Gif))
      {
        result[0] = "Gif";
        result[1] = ".gif";
      }
      else if (rawFormat.Equals(System.Drawing.Imaging.ImageFormat.Emf))
      {
        result[0] = "Emf";
        result[1] = ".image";
      }
      else if (rawFormat.Equals(System.Drawing.Imaging.ImageFormat.Exif))
      {
        result[0] = "Exif";
        result[1] = ".image";
      }
      else if (rawFormat.Equals(System.Drawing.Imaging.ImageFormat.Icon))
      {
        result[0] = "Icon";
        result[1] = ".image";
      }
      else if (rawFormat.Equals(System.Drawing.Imaging.ImageFormat.MemoryBmp))
      {
        result[0] = "MemoryBmp";
        result[1] = ".image";
      }
      else if (rawFormat.Equals(System.Drawing.Imaging.ImageFormat.Tiff))
      {
        result[0] = "Tiff";
        result[1] = ".image";
      }
      else if (rawFormat.Equals(System.Drawing.Imaging.ImageFormat.Wmf))
      {
        result[0] = "Wmf";
        result[1] = ".image";
      }
      else
      {
        result[0] = "Unknown";
        result[1] = ".image";
      }

      return result;
    }

#endif

  }
}
