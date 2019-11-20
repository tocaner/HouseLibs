using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Fingerprint = HouseUtils.Checksum.Fingerprint;
using SizePixels = HouseImaging.SizePixels;
using Orientation = HouseImaging.Orientation;


namespace HouseImaging.Wpf
{
  public class ImageInfo : IDisposable
  {
    private ImageSource fSystemImage = null;
    private Fingerprint fFingerprint = null;
    private MetadataPortal fMetadata = null;


    public ImageInfo(ImageSource image)
    {
      fSystemImage = image;
    }


    public void Dispose()
    {
    }


    public SizePixels SizePixels
    {
      get { return new SizePixels((int)fSystemImage.Width, (int)fSystemImage.Height); }
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


    public ImageSource GetSystemImageSource()
    {
      return fSystemImage;
    }


    public Fingerprint GetFingerprint()
    {
      if (fFingerprint == null)
      {
        byte[] imageData = ImageTools.ExtractPixelBytesFromBitmap(fSystemImage as BitmapSource);
        fFingerprint = new Fingerprint(imageData);
      }

      return fFingerprint;
    }


    public ImageFormatEnum GetImageFormat()
    {
      return ImageFormat.FromDecoder((fSystemImage as BitmapFrame).Decoder);
    }


    public string GetImageFormatDescription()
    {
      return GetImageFormat().GetDescription();
    }


    public string GetPixelFormat()
    {
      return (fSystemImage as BitmapFrame).Format.ToString();
    }


    public int GetPixelFormatBitsPerPixel()
    {
      return (fSystemImage as BitmapFrame).Format.BitsPerPixel;
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


    public Transform TransformFromOrientation(Orientation orientation)
    {
      var transform = new TransformGroup();

      if (orientation.Value > 3)
      {
        transform.Children.Add(new ScaleTransform(-1, 1));
      }

      switch (orientation.Value & 3)
      {
        case 1:
          transform.Children.Add(new RotateTransform(90D));
          break;

        case 2:
          transform.Children.Add(new RotateTransform(180D));
          break;

        case 3:
          transform.Children.Add(new RotateTransform(270D));
          break;

        default: // No rotation
          break;
      }

      return transform;
    }


    public ImageInfo GetTransformedImage(Orientation index)
    {
      Transform transform = TransformFromOrientation(index);
      TransformedBitmap result = new TransformedBitmap((BitmapSource)fSystemImage, transform);
      return new ImageInfo(result);
    }


    public DateTime GetDateTaken()
    {
      DateTime result;

      if (DateTime.TryParseExact((string)Metadata.Read("Origin.DateTime"), "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out result) == false)
      {
        result = DateTime.MinValue;
      }

      return result;
    }


    public Orientation GetOrientation()
    {
      var res = Metadata.Read("Image.Orientation");

      if (res != null)
      {
        return Orientation.FromExif((int)(UInt16)res);
      }
      else
      {
        return new Orientation();
      }
    }


    public Orientation GetThumbnailOrientation()
    {
      var res = Metadata.Read("Tumbnail.Orientation");

      if (res != null)
      {
        return Orientation.FromExif((int)(UInt16)res);
      }
      else
      {
        return new Orientation();
      }
    }


    public ImageInfo ExtractThumbnail()
    {
      ImageSource thumb = (fSystemImage as BitmapFrame).Thumbnail;
      return thumb != null ? new ImageInfo(thumb) : null;
    }


    public ImageInfo GetThumbnail(int maxWidth, int maxHeight)
    {
      //return new ImageInfo(ImageTools.CreateThumbnail(fSystemImage, maxWidth, maxHeight));
      ImageSource thumb = (fSystemImage as BitmapFrame).Thumbnail;

      if (thumb == null)
      {
        thumb = fSystemImage;
      }

      return new ImageInfo(thumb);
    }


    public bool IsEqual(ImageInfo theOther)
    {
      return this.GetFingerprint().IsEqual(theOther.GetFingerprint());
    }
  }
}
