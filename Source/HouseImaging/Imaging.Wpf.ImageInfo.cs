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
    private BitmapSource fSystemImage = null;
    private Fingerprint fFingerprint = null;
    private MetadataPortal fMetadata = null;


    public ImageInfo(ImageSource image)
    {
      fSystemImage = image as BitmapSource;
    }


    public void Dispose()
    {
    }


    public static ImageInfo FromFile(string filename)
    {
      try
      {
        ImageSource image = BitmapFrame.Create(new Uri(filename),
          BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnDemand);
        return new ImageInfo(image);
      }
      catch
      {
        return null;
      }
    }


    public SizePixels SizePixels
    {
      get { return new SizePixels(fSystemImage.PixelWidth, fSystemImage.PixelHeight); }
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
        byte[] imageData = ImageTools.ExtractPixelBytesFromBitmap(fSystemImage);
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
      return fSystemImage.Format.ToString();
    }


    public int GetPixelFormatBitsPerPixel()
    {
      return fSystemImage.Format.BitsPerPixel;
    }


    public bool CanTransformLossless()
    {
      bool result;

      switch (GetImageFormat())
      {
        case ImageFormatEnum.Jpeg:
          {
            SizePixels size = this.SizePixels;
            result = ((size.Height % 16) == 0) && ((size.Width % 16) == 0);
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
      TransformedBitmap result = new TransformedBitmap(fSystemImage, transform);
      return new ImageInfo(result);
    }


    public void SaveImageToFile(string filename, Orientation index, int quality = 80)
    {
      BitmapEncoder encoder = ImageTools.GetEncoder(filename);

      BitmapSource source;

      if (encoder is JpegBitmapEncoder)
      {
        source = fSystemImage;
        (encoder as JpegBitmapEncoder).QualityLevel = quality;
        // TODO: Set Flips and Rotation
      }
      else
      {
        source = GetTransformedImage(index).GetSystemImageSource() as BitmapSource;
      }

      ImageTools.ImageSourceToFile(source, encoder, filename);
    }


    public DateTime GetDateTaken()
    {
      DateTime result;

      string dateString = Metadata.Read("DateTaken").ToString();

      if (DateTime.TryParseExact(dateString, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out result) == false)
      {
        result = DateTime.MinValue;
      }

      return result;
    }


    public Orientation GetOrientation()
    {
      object value = Metadata.Read("Orientation").Value;

      if (value != null)
      {
        return Orientation.FromExif((int)(UInt16)value);
      }
      else
      {
        return new Orientation();
      }
    }


    public Orientation GetThumbnailOrientation()
    {
      object value = Metadata.Read("Thumbnail.Orientation").Value;

      if (value != null)
      {
        return Orientation.FromExif((int)(UInt16)value);
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
