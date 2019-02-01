using System;
using System.Threading;
using System.Windows.Interop;
using System.Windows;


namespace HouseUtils.Threading
{
  public class DispatcherUI : HouseDispatcher
  {
    static public System.Windows.Threading.Dispatcher DefaultUI { get; set; } = null;


    private System.Windows.Threading.Dispatcher fUI;


    protected void Init(System.Windows.Threading.Dispatcher ui)
    {
      if (ui != null)
      {
        fUI = ui;
      }
      else if (DefaultUI != null)
      {
        fUI = DefaultUI;
      }
      else if (System.Windows.Application.Current != null)
      {
        fUI = System.Windows.Application.Current.Dispatcher;
      }
      else // This will work only if called from UI thread
      {
        fUI = System.Windows.Threading.Dispatcher.CurrentDispatcher;
      }
    }


    public DispatcherUI(System.Windows.Threading.Dispatcher ui = null)
    {
      Init(ui);
    }


    public override void Post(EventHandler target, object sender, EventArgs args)
    {
      if(target != null)
      {
        if(fUI.CheckAccess())
        {
          target(sender, args);
        }
        else
        {
          fUI.BeginInvoke(target, sender, args);
        }
      }
    }


    public override void Post(HouseDelegate method)
    {
      if(method != null)
      {
        if(fUI.CheckAccess())
        {
          method.Method.Invoke(method.Target, null);
        }
        else
        {
          fUI.BeginInvoke(method);
        }
      }
    }


    public override void Post(Delegate method, params object[] args)
    {
      if (method != null)
      {
        if (fUI.CheckAccess())
        {
          method.Method.Invoke(method.Target, args);
        }
        else
        {
          fUI.BeginInvoke(method, args);
        }
      }
    }
  }
}
