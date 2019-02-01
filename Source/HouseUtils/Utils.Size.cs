using System;
using System.Collections.Generic;
using System.Text;


namespace HouseUtils
{
  public class Size2D
  {
    public double Width { get; set; }
    public double Height { get; set; }

    public Size2D()
    { }

    public Size2D(double width, double height)
    {
      Width = width;
      Height = height;
    }

    public Size2D Copy()
    {
      return new Size2D(Width, Height);
    }
  }


  public class Bounds
  {
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }

    public Bounds()
    { }

    public Bounds(double x, double y, double width, double height)
    {
      X = x;
      Y = y;
      Width = width;
      Height = height;
    }

    public Bounds(double x, double y, Size2D size)
      : this(x, y, size.Width, size.Height)
    { }
  }
}
