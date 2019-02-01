using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HouseImaging
{
  class WpfTools
  {
    static public System.Windows.Media.ImageSource BitmapFromWinforms(System.Drawing.Bitmap bitmap)
    {
      System.Windows.Media.ImageSource result;
      IntPtr hBitmap = bitmap.GetHbitmap();

      try
      {
        result = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                       hBitmap,
                       IntPtr.Zero,
                       System.Windows.Int32Rect.Empty,
                       System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
      }
      finally
      {
        NativeLibs.LibGdi32.DeleteObject(hBitmap);
      }

      return result;
    }
  }
}
