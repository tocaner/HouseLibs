using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HouseUtils;


namespace TestThreads
{
  class DebugOutput : BindingPropertyClass
  {
    static private DebugOutput fInstance = null;


    static public void Write(string text)
    {
      if (fInstance != null)
      {
        fInstance.Output = text;
      }
    }


    public DebugOutput()
    {
      fInstance = this;
    }


    private string fOutput = "";
    public string Output
    {
      get { return fOutput; }
      set { fOutput += value; RaisePropertyChanged("Output"); }
    }
  }


  class Debug
  {
    private string fId;


    public void Write(string text)
    {
      DebugOutput.Write(fId + ": " + text);
    }

    public void WriteLn(string text)
    {
      Write(text + "\r\n");
    }


    public Debug(string id)
    {
      fId = id;
    }
  }
}
