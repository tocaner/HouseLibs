using System;
using System.Collections.Generic;
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
    private MetadataPortal fMetadata = null;


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


    public static ImageInfo FromByteArray(byte[] byteArray)
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


    public static ImageInfo FromFile(string filename)
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


    public MetadataPortal Metadata
    {
      get
      {
        if (fMetadata == null)
        {
          fMetadata = new MetadataPortal(this);
        }

        return fMetadata;
      }
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
      using (ImageInfo image = ImageInfo.FromFile(path))
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
        fMetadata = null;
      }
    }


    public void Convert(ImageFormatEnum destFormat, int compressionFactor)
    {
      ImageFormat systemFormat = ImageFormats.ImageFormatToSystemFormat(destFormat);
      Image newImage = ImageTools.ConvertImage(fSystemImage, systemFormat, compressionFactor);
      UpdateSystemImage(newImage);
    }


    public void Transform(Orientation index)
    {
      if (index.IsTransformed)
      {
        Image newImage = ImageTools.TransformImage(fSystemImage, index.Value);
        UpdateSystemImage(newImage);
      }
    }


    public ImageInfo GetTransformedImage(Orientation index)
    {
      // TODO: Simplify this by removing internal redundant steps
      ImageInfo result = this.Clone();
      result.Transform(index);
      return result;
    }


    public Orientation GetOrientation()
    {
      return Orientation.FromExif(Metadata.Read_ExifOrientation());
    }


    public Orientation GetThumbnailOrientation()
    {
      return Orientation.FromExif(Metadata.Read_ExifThumbnailOrientation());
    }


    public ImageInfo CreateThumbnail(int maxWidth, int maxHeight)
    {
      return new ImageInfo(ImageTools.CreateThumbnail(fSystemImage, maxWidth, maxHeight));
    }


    public ImageInfo ExtractEmbeddedThumbnail()
    {
      byte[] data = Metadata.Read_ThumbnailBytes();

      if ((data != null) && (data.Length > 0))
      {
        return FromByteArray(data);
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
  }
}
