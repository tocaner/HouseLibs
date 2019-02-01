using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HouseUtils
{
  public class ArrayTools
  {
    public static byte[] DuplicateByteArray(byte[] source)
    {
      byte[] dest = new byte[source.Length];
      Array.Copy(source, dest, dest.Length);
      return dest;
    }


    public static bool IsByteArraySame(byte[] arr1, byte[] arr2)
    {
      bool result = true;

      if ((arr1 == null) || (arr2 == null))
      {
        if ((arr1 != null) || (arr2 != null))
        {
          result = false;
        }
      }
      else
      {
        if (arr1.Length != arr2.Length)
        {
          result = false;
        }
        else
        {
          for (int i = 0; i < arr1.Length; i++)
          {
            if (arr1[i] != arr2[i])
            {
              result = false;
              break;
            }
          }
        }
      }

      return result;
    }
  }
}
