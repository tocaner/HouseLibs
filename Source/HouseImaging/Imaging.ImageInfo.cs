using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Fingerprint = HouseUtils.Checksum.Fingerprint;
using Image = System.Drawing.Image;
using ImageFormat = System.Drawing.Imaging.ImageFormat;
using System.Windows.Media;


namespace HouseImaging
{
  public class ImageInfo : IDisposable
  {
    private Image fSystemImage = null;
    private Fingerprint fFingerprint = null; 


    public ImageInfo(Image image)
    {
      fSystemImage = image;
    }


    public void Dispose()
    {
      if (fSystemImage != null)
      {
        fSystemImage.Dispose();
        fSystemImage = null;
      }
    }


    public static ImageInfo CreateImageInfo(byte[] byteArray)
    {
      try
      {
        Image image = ImageTools.ImageFromByteArray(byteArray);
        return new ImageInfo(image);
      }
      catch
      {
        return null;
      }
    }


    public static ImageInfo CreateImageInfo(string filename)
    {
      try
      {
        Image image = ImageTools.LoadImageFromFile(filename);
        return new ImageInfo(image);
      }
      catch
      {
        return null;
      }
    }


    public static bool IsImageFile(string filename)
    {
      string ext = Path.GetExtension(filename);
      return ImageFormats.ExtensionToEnum(ext) != ImageFormatEnum.Unknown;
    }


    public SizePixels SizePixels
    {
      get { return new SizePixels(SystemImage.Width, SystemImage.Height); }
    }


    public Image SystemImage
    {
      get { return fSystemImage; }
    }


    public ImageSource GetSystemImageSource()
    {
      return WpfTools.BitmapFromWinforms((System.Drawing.Bitmap)SystemImage);
    }


    public static ImageSource ReadImageSourceFromFile(string path)
    {
      using (ImageInfo image = ImageInfo.CreateImageInfo(path))
      {
        return image.GetSystemImageSource();
      }
    }


    public Fingerprint GetFingerprint()
    {
      if (fFingerprint == null)
      {
        byte[] imageData = ImageTools.GetPixelBytesFromImage(fSystemImage);
        fFingerprint = new Fingerprint(imageData);
      }

      return fFingerprint;
    }


    public string GetImageExtension()
    {
      return ImageFormats.SystemFormatToExtension(fSystemImage.RawFormat);
    }


    public ImageFormatEnum GetImageFormat()
    {
      return ImageFormats.SystemFormatToEnum(fSystemImage.RawFormat);
    }


    public string GetImageFormatDescription()
    {
      return ImageFormats.ImageFormatToDescription(GetImageFormat());
    }


    public string GetPixelFormat()
    {
      return fSystemImage.PixelFormat.ToString();
    }


    public int GetPixelFormatBitsPerPixel()
    {
      return Image.GetPixelFormatSize(fSystemImage.PixelFormat);
    }


    public bool CanTransformLossless()
    {
      bool result;

      switch (GetImageFormat())
      {
        case ImageFormatEnum.Jpeg:
          {
            result = ((fSystemImage.Height % 16) == 0) && ((fSystemImage.Width % 16) == 0);
          }
          break;

        case ImageFormatEnum.Bmp:
        case ImageFormatEnum.MemoryBmp:
          {
            // Not compressed, so should be able to rotate lossless
            result = true;
          }
          break;

        default:
          {
            // Other compression formats
            result = false;
          }
          break;
      }

      return result;
    }


    public void SaveImageToFile(string filename, int compressionFactor = 80)
    {
      string extension = Path.GetExtension(filename);
      ImageFormat destFormat = ImageFormats.ExtensionToSystemFormat(extension);
      if (destFormat == null)
      {
        destFormat = fSystemImage.RawFormat;
        extension = ImageFormats.SystemFormatToExtension(destFormat).ToLower();
        filename = Path.ChangeExtension(filename, extension);
      }
      ImageTools.SaveImageToFile(fSystemImage, filename, destFormat, compressionFactor);
    }


    private void UpdateSystemImage(Image newImage)
    {
      if (fSystemImage != newImage)
      {
        fSystemImage.Dispose(); // Free resources for the existing image and create new one from byte buffer
        fSystemImage = newImage;
        fFingerprint = null;
      }
    }


    public void Convert(ImageFormatEnum destFormat, int compressionFactor)
    {
      ImageFormat systemFormat = ImageFormats.ImageFormatToSystemFormat(destFormat);
      Image newImage = ImageTools.ConvertImage(fSystemImage, systemFormat, compressionFactor);
      UpdateSystemImage(newImage);
    }


    public void Transform(ImageTransformer index)
    {
      if (index.IsTransformed)
      {
        Image newImage = ImageTools.TransformImage(fSystemImage, index.Value);
        UpdateSystemImage(newImage);
      }
    }


    public ImageInfo GetTransformedImage(ImageTransformer index)
    {
      // TODO: Simplify this by removing internal redundant steps
      ImageInfo result = this.Clone();
      result.Transform(index);
      return result;
    }


    public ImageTransformer GetTransformerFromExif()
    {
      return ImageTransformer.CreateFromExif(this.ReadImageMetadata_ExifOrientation());
    }


    public ImageTransformer GetThumbnailTransformerFromExif()
    {
      return ImageTransformer.CreateFromExif(this.ReadImageMetadata_ExifThumbnailOrientation());
    }


    public ImageInfo CreateThumbnail(int maxWidth, int maxHeight)
    {
      return new ImageInfo(ImageTools.CreateThumbnail(fSystemImage, maxWidth, maxHeight));
    }


    public ImageInfo ExtractEmbeddedThumbnail()
    {
      byte[] data = this.ReadImageMetadata_ThumbnailBytes();

      if ((data != null) && (data.Length > 0))
      {
        return CreateImageInfo(data);
      }
      else
      {
        return null;
      }
    }


    public ImageInfo GetThumbnail(int maxWidth, int maxHeight)
    {
      ImageInfo result = ExtractEmbeddedThumbnail();

      if (result == null)
      {
        result = CreateThumbnail(maxWidth, maxHeight);
      }

      return result;
    }


    public ImageInfo Clone()
    {
      return new ImageInfo(ImageTools.ImageClone(fSystemImage));
    }


    public bool IsEqual(ImageInfo theOther)
    {
      return this.GetFingerprint().IsEqual(theOther.GetFingerprint());
    }


    public List<MetadataItem> ReadMetadata()
    {
      return MetadataPortal.GetImageMetadataList(fSystemImage);
    }


    public int ReadImageMetadata_ExifOrientation()
    {
      byte[] bytes = this.ReadImageMetadata(0x0112);

      if (bytes != null)
      {
        return bytes[1] << 8 | bytes[0];
      }
      else
      {
        return -1; // No exif found, valid exif values are 1..8
      }
    }


    public void SetImageMetadata_ExifOrientation(int value)
    {
      byte[] bytes = BitConverter.GetBytes((UInt16)value);
      this.SetImageMetadata(0x0112, bytes, MetadataType.IntU16);
    }


    public void RemoveImageMetadata_ExifOrientation()
    {
      MetadataPortal.RemoveImageMetadata(SystemImage, 0x0112);
    }


    public int ReadImageMetadata_ExifThumbnailOrientation()
    {
      byte[] bytes = this.ReadImageMetadata(0x5029);

      if (bytes != null)
      {
        return bytes[1] << 8 | bytes[0];
      }
      else
      {
        return -1; // No exif found, valid exif values are 1..8
      }
    }


    public void SetImageMetadata_ExifThumbnailOrientation(int value)
    {
      byte[] bytes = BitConverter.GetBytes((UInt16)value);
      this.SetImageMetadata(0x5029, bytes, MetadataType.IntU16);
    }


    public void RemoveImageMetadata_ExifThumbnailOrientation()
    {
      MetadataPortal.RemoveImageMetadata(SystemImage, 0x5029);
    }


    public byte[] ReadImageMetadata_ThumbnailBytes()
    {
      return this.ReadImageMetadata(0x501B);
    }


    public string ReadImageMetadata_UserComment()
    {
      byte[] bytes = this.ReadImageMetadata(0x9286);

      if (bytes != null)
      {
        return MetadataFormat.FormatValue(MetadataType.String, bytes);
      }
      else
      {
        return string.Empty;
      }
    }


    public void SetImageMetadata_UserComment(string value)
    {
      byte[] bytes = MetadataFormat.EncodeValue(MetadataType.String, value);
      this.SetImageMetadata(0x9286, bytes, MetadataType.String);
    }


    public DateTime ReadImageMetadata_DateTaken()
    {
      DateTime result;

      if (DateTime.TryParseExact(ReadImageMetadata_String(0x0132), "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out result) == false)
      {
        result = DateTime.MinValue;
      }

      return result;
    }


    public string ReadImageMetadata_String(int propId)
    {
      byte[] bytes = this.ReadImageMetadata(propId);

      if (bytes != null)
      {
        return MetadataFormat.FormatValue(MetadataType.String, bytes);
      }
      else
      {
        return string.Empty;
      }
    }


    public void SetImageMetadata_String(int propId, string value)
    {
      byte[] bytes = MetadataFormat.EncodeValue(MetadataType.String, value);
      this.SetImageMetadata(propId, bytes, MetadataType.String);
    }


    public byte[] ReadImageMetadata(int propId)
    {
      return MetadataPortal.ReadImageMetadata(SystemImage, propId);
    }


    public void SetImageMetadata(int propId, byte[] data, MetadataType dataType)
    {
      MetadataPortal.SetImageMetadata(SystemImage, propId, data, dataType);
    }
  }
}
