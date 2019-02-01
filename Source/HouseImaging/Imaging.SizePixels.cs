using System;
using System.Collections.Generic;
using System.Text;


namespace HouseImaging
{
  public class SizePixels
  {
    public int Width { get; private set; }

    public int Height { get; private set; }

    public SizePixels(int width, int height)
    {
      Width = width;
      Height = height;
    }

    public SizePixels Flip()
    {
      return new SizePixels(Height, Width);
    }
  }
}
