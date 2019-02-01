using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;


namespace HouseUtils.Threading
{
  public class DispatcherUI_WinForms : HouseDispatcher
  {
    static private System.Windows.Forms.Control fUI;

    static public void SetUI(System.Windows.Forms.Control ui)
    {
      fUI = ui;
    }

    public override void Post(EventHandler target, object sender, EventArgs args)
    {
      if(target != null)
      {
        if(fUI.InvokeRequired)
        {
          fUI.BeginInvoke(target, sender, args);
        }
        else
        {
          target(sender, args);
        }
      }
    }

    public override void Post(HouseDelegate method)
    {
      if(method != null)
      {
        if(fUI.InvokeRequired)
        {
          fUI.BeginInvoke(method);
        }
        else
        {
          method.Method.Invoke(method.Target, null);
        }
      }
    }

    public override void Post(Delegate method, params object[] args)
    {
      if (method != null)
      {
        if (fUI.InvokeRequired)
        {
          fUI.BeginInvoke(method, args);
        }
        else
        {
          method.Method.Invoke(method.Target, args);
        }
      }
    }
  }
}
