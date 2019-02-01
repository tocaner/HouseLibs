using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HouseUtils.Checksum
{
  public class Fingerprint
  {
    public Fingerprint()
    {  }


    public Fingerprint(byte[] data)
    {
      CRC32 crc = new CRC32();
      crc.Add(data);
      Code = BitConverter.GetBytes(crc.Result);
    }


    public byte[] Code { get; set; }


    public override string ToString()
    {
      return StringTools.HexStringFromByteArray(Code);
    }

    public bool IsEqual(Fingerprint other)
    {
      return ArrayCompare(this.Code, other.Code);
    }

    static bool ArrayCompare(byte[] a, byte[] b)
    {
      bool result;

      if ((a == null) || (b == null))
      {
        result = false;
      }
      else if (a.Length != b.Length)
      {
        result = false;
      }
      else
      {
        result = true;

        for (int i = 0; i < a.Length; i++)
        {
          if (a[i] != b[i])
          {
            result = false;
            break;
          }
        }
      }

      return result;
    }
  }
}
