using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HouseImaging
{
  public class Orientation
  {
    private int fRotation;
    private bool fIsMirrored;


    public Orientation(int value = 0)
    {
      Value = value;
    }


    public int Value
    {
      get { return 7 & (fRotation + (fIsMirrored ? 4 : 0)); }

      private set
      {
        fIsMirrored = (value & 4) != 0;
        fRotation = value & 3;
      }
    }


    public bool IsTransformed
    {
      get { return (Value != 0); }
    }


    public bool IsFlipped
    {
      get { return ((fRotation & 1) == 1); }
    }


    public void RotateClockwise()
    {
      fRotation = (fRotation + 1) % 4;
    }


    public void RotateCounterClockwise()
    {
      fRotation = (fRotation + 3) % 4;
    }


    public void MirrorHorizontally()
    {
      fIsMirrored = !fIsMirrored;

      if (IsFlipped == true)
      {
        fRotation = (fRotation + 2) % 4;
      }
    }


    public void MirrorVertically()
    {
      fIsMirrored = !fIsMirrored;

      if (IsFlipped == false)
      {
        fRotation = (fRotation + 2) % 4;
      }
    }


    private static int[] fExifTable = new int[8]
    {
      1, 6, 3, 8, 2, 7, 4, 5
    };


    public static int ValueFromExif(int exif)
    {
      int result = Array.IndexOf(fExifTable, exif);
      return result >= 0 ? result : 0;
    }


    public static int ValueToExif(int value)
    {
      return fExifTable[value];
    }


    public int ValueToExif()
    {
      return ValueToExif(this.Value);
    }


    private static int[,] fAdditionTable = new int[8, 8]
    {
      { 0, 1, 2, 3, 4, 5, 6, 7 },
      { 1, 2, 3, 0, 7, 4, 5, 6 },
      { 2, 3, 0, 1, 6, 7, 4, 5 },
      { 3, 0, 1, 2, 5, 6, 7, 4 },
      { 4, 5, 6, 7, 0, 1, 2, 3 },
      { 5, 6, 7, 4, 3, 0, 1, 2 },
      { 6, 7, 4, 5, 2, 3, 0, 1 },
      { 7, 4, 5, 6, 1, 2, 3, 0 }
    };


    public void Add(Orientation other)
    {
      this.Value = fAdditionTable[this.Value, other.Value];
    }


    private static int[] fReverseTable = new int[8]
    {
      0, 3, 2, 1, 4, 5, 6, 7
    };


    public void Reverse(Orientation other)
    {
      this.Value = fReverseTable[other.Value];
    }


    public Orientation GetReversal()
    {
      Orientation result = new Orientation();
      result.Reverse(this);
      return result;
    }


    public static Orientation FromExif(int exif)
    {
      return new Orientation(Orientation.ValueFromExif(exif));
    }


    public override string ToString()
    {
      return string.Format("{0} ({1} exif)", this.Value, this.ValueToExif());
    }
  }
}
