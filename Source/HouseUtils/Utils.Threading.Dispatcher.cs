using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;


namespace HouseUtils.Threading
{
  public delegate void HouseDelegate();

#if REMOVE_IHouseDispatcher

  public interface IHouseDispatcher
  {
    HouseDispatcher Dispatcher { get; }
  }

#endif

  public abstract class HouseDispatcher
  {
    public abstract void Post(EventHandler target, object sender, EventArgs args);
    public abstract void Post(HouseDelegate method);
    public abstract void Post(Delegate method, params object[] args);


    public virtual void Stop()
    { }


    public virtual void ClearTasks()
    { }


    public virtual bool HasPendingTasks()
    {
      return true;
    }

#if REMOVE_IHouseDispatcher

    static public void Invoke(EventHandler handler, object sender, EventArgs args)
    {
      if(handler != null)
      {
        if(handler.Target is IHouseDispatcher)
        {
          (handler.Target as IHouseDispatcher).Dispatcher.Post(handler, sender, args);
        }
        else
        {
          handler(sender, args);
        }
      }
    }

    static public void Invoke(HouseDelegate method)
    {
      if(method != null)
      {
        if(method.Target is IHouseDispatcher)
        {
          (method.Target as IHouseDispatcher).Dispatcher.Post(method);
        }
        else
        {
          method.Method.Invoke(method.Target, null);
        }
      }
    }

    static public void Invoke(Delegate method, params object[] args)
    {
      if (method != null)
      {
        if (method.Target is IHouseDispatcher)
        {
          (method.Target as IHouseDispatcher).Dispatcher.Post(method, args);
        }
        else
        {
          method.Method.Invoke(method.Target, args);
        }
      }
    }
#endif

  }


  public abstract class HouseTask
  {
    public abstract void Invoke();
  }


  public class EventTask : HouseTask
  {
    private event EventHandler fTarget;
    private object fSender;
    private EventArgs fArgs;

    public EventTask(EventHandler target, object sender, EventArgs args)
    {
      fTarget = target;
      fSender = sender;
      fArgs = args;
    }

    public override void Invoke()
    {
      if(fTarget != null)
      {
        fTarget(fSender, fArgs);
      }
    }
  }


  public class MethodTask : HouseTask
  {
    private Delegate fMethod;
    private object[] fArgs;

    public MethodTask(Delegate method, params object[] args)
    {
      fMethod = method;
      fArgs = args;
    }

    public override void Invoke()
    {
      if(fMethod != null)
      {
        fMethod.Method.Invoke(fMethod.Target, fArgs);
      }
    }
  }


  public class DispatcherThread : HouseDispatcher
  {
    private string fId;
    private Queue<HouseTask> fQueue;
    private Thread fThread;
    private AutoResetEvent fSignal;
    private bool fRun;

    public DispatcherThread(string id)
    {
      fId = id;
      fQueue = new Queue<HouseTask>();
      fThread = null;
      fSignal = null;
      fRun = false;
      this.Start();
    }

    private void Start()
    {
      fThread = new Thread(new ThreadStart(Run));
      fThread.Name = fId;
      fThread.IsBackground = true;
      fThread.SetApartmentState(ApartmentState.STA);
      fSignal = new AutoResetEvent(false);
      fRun = true;
      fThread.Start();
    }

    public override void Stop()
    {
      if(fRun)
      {
        fRun = false;
        fSignal.Set();

        fThread.Join();
        fThread = null;
      }
    }

    public bool IsActive()
    {
      return ((fThread != null) && (fRun == true));
    }

    public bool IsStopped()
    {
      return (fThread == null);
    }

    public override bool HasPendingTasks()
    {
      bool result = false;

      lock (fQueue)
      {
        result = (fQueue.Count > 0);
      }

      return result;
    }

    private void Run()
    {
      while(fRun)
      {
        if(this.ProcessOne() == false)
        {
          // Consume all events before going to sleep
          fSignal.WaitOne(10000); // Wait for a signal for a maximum of 10 second
        }
      }
    }

    public override void Post(EventHandler target, object sender, EventArgs args)
    {
      AddTask(new EventTask(target, sender, args));
    }

    public override void Post(HouseDelegate method)
    {
      AddTask(new MethodTask(method, null));
    }

    public override void Post(Delegate method, params object[] args)
    {
      AddTask(new MethodTask(method, args));
    }

    private void AddTask(HouseTask task)
    {
      lock(fQueue)
      {
        fQueue.Enqueue(task);
      }

      fSignal.Set();
    }

    public override void ClearTasks()
    {
      lock (fQueue)
      {
        fQueue.Clear();
      }

      fSignal.Set();
    }


    protected bool ProcessOne()
    {
      bool result = false;

      HouseTask e = null;

      lock(fQueue)
      {
        if(fQueue.Count > 0)
        {
          e = fQueue.Dequeue();
        }
      }

      if(e != null)
      {
        e.Invoke();
        result = true;
      }

      return result;
    }
  }
}


