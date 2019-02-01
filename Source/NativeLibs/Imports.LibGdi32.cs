using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace NativeLibs
{
  public class LibGdi32
  {
    [DllImport("gdi32.dll")]
    public static extern bool DeleteObject(IntPtr hObject);
  }
}
