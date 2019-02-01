using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace NativeLibs
{
  public class LibDefines
  {
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
      public int left;
      public int top;
      public int right;
      public int bottom;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
      public int X;
      public int Y;
    }


    [StructLayout(LayoutKind.Sequential)]
    public class SCROLLINFO
    {
      public int cbSize = Marshal.SizeOf(typeof(SCROLLINFO));
      public int fMask;
      public int nMin;
      public int nMax;
      public int nPage;
      public int nPos;
      public int nTrackPos;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct WINMSG
    {
      public IntPtr hwnd;
      public int message;
      public IntPtr wParam;
      public IntPtr lParam;
      public int time;
      public int x;
      public int y;
    }
  }
}
