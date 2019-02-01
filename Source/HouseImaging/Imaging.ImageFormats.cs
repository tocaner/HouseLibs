using System;
using System.Collections.Generic;
using System.Linq;
using ImageFormat = System.Drawing.Imaging.ImageFormat;


namespace HouseImaging
{
  public enum ImageFormatEnum { Bmp, Jpeg, Gif, Tiff, Png, Emf, Exif, Icon, MemoryBmp, Wmf, Unknown };


  class ImageFormats
  {
    private class FormatDefinition
    {
      public ImageFormatEnum Type;
      public string Extension;
      public string Description;
      public ImageFormat SystemFormat;

      public FormatDefinition(ImageFormatEnum type, string extension, string description, ImageFormat systemFormat)
      {
        Type = type; Extension = extension; Description = description; SystemFormat = systemFormat;
      }
    }

    private static readonly List<FormatDefinition> _formats = new List<FormatDefinition>
    {
      new FormatDefinition(ImageFormatEnum.Jpeg,       ".JPG",   "Jpeg",           ImageFormat.Jpeg),
      new FormatDefinition(ImageFormatEnum.Bmp,        ".BMP",   "Bitmap",         ImageFormat.Bmp),
      new FormatDefinition(ImageFormatEnum.Png,        ".PNG",   "Png",            ImageFormat.Png),
      new FormatDefinition(ImageFormatEnum.Gif,        ".GIF",   "Gif",            ImageFormat.Gif),
      new FormatDefinition(ImageFormatEnum.Emf,        ".IMAGE", "Emf",            ImageFormat.Emf),
      new FormatDefinition(ImageFormatEnum.Exif,       ".IMAGE", "Exif",           ImageFormat.Exif),
      new FormatDefinition(ImageFormatEnum.Icon,       ".IMAGE", "Icon",           ImageFormat.Icon),
      new FormatDefinition(ImageFormatEnum.MemoryBmp,  ".IMAGE", "Memory Bitmap",  ImageFormat.MemoryBmp),
      new FormatDefinition(ImageFormatEnum.Tiff,       ".IMAGE", "Tiff",           ImageFormat.Tiff),
      new FormatDefinition(ImageFormatEnum.Unknown,    ".IMAGE", "Wmf",            ImageFormat.Wmf),
    };

    public static ImageFormat ExtensionToSystemFormat(string extension)
    {
      string lookup = extension.ToUpper();
      FormatDefinition fd = _formats.FirstOrDefault(x => x.Extension == lookup);
      return fd == null ? null : fd.SystemFormat;
    }

    public static string SystemFormatToExtension(ImageFormat format)
    {
      FormatDefinition fd = _formats.FirstOrDefault(x => x.SystemFormat.Equals(format));
      return fd == null ? ".IMAGE" : fd.Extension;
    }

    public static ImageFormatEnum ExtensionToEnum(string extension)
    {
      string lookup = extension.ToUpper();
      FormatDefinition fd = _formats.FirstOrDefault(x => x.Extension == lookup);
      return fd == null ? ImageFormatEnum.Unknown : fd.Type;
    }

    public static ImageFormatEnum SystemFormatToEnum(ImageFormat format)
    {
      FormatDefinition fd = _formats.FirstOrDefault(x => x.SystemFormat.Equals(format));
      return fd == null ? ImageFormatEnum.Unknown : fd.Type;
    }

    public static string ImageFormatToDescription(ImageFormatEnum type)
    {
      FormatDefinition fd = _formats.FirstOrDefault(x => x.Type == type);
      return fd == null ? "Unknown" : fd.Description;
    }

    public static ImageFormat ImageFormatToSystemFormat(ImageFormatEnum type)
    {
      FormatDefinition fd = _formats.FirstOrDefault(x => x.Type == type);
      return fd == null ? null : fd.SystemFormat;
    }
  }
}
