using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HouseUtils;


namespace TestThreads
{
  public enum TestState { Idle, Started, Waiting, CancelRequest, Cancelling }


  public class TestModel : BindingPropertyClass
  {
    private TestState fTestState = TestState.Idle;
    public TestState TestState
    {
      get { return fTestState; }
      set { if (fTestState != value) { fTestState = value; RaisePropertyChanged(null); } }
    }

    private int fCount = 100;
    public int Count
    {
      get { return fCount; }
      set { if (fCount != value) { fCount = value; RaisePropertyChanged("Count"); } }
    }

    private string fStatus = "Idle";
    public string Status
    {
      get { return fStatus; }
      set { if (fStatus != value) { fStatus = value; RaisePropertyChanged("Status"); } }
    }

    public bool IsIdle
    {
      get { return fTestState == TestState.Idle; }
    }

    public bool IsActive
    {
      get { return !IsIdle; }
    }
  }
}
