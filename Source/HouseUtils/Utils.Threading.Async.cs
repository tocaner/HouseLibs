using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;


namespace HouseUtils.Threading
{
  public class HouseAsync : DispatcherThread
  {
    public HouseAsync(string name) : base(name)
    { }


    public void Await(HouseDelegate method)
    {
      var frame = new DispatcherFrame();

      this.Post(() =>
      {
        method();
        frame.Continue = false;
      });

      Dispatcher.PushFrame(frame); // start the secondary dispatcher, pausing this code
    }


    static public void BeginAsync(HouseDelegate method)
    {
      System.Windows.Application.Current.Dispatcher.BeginInvoke(method);
    }
  }
}
