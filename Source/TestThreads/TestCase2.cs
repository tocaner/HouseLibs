using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HouseUtils.Threading;


namespace TestThreads
{
  class TestCase2 : TestCase
  {
    private Debug fDebug;
    private HouseDispatcher fUI;
    private HouseDispatcher fThread;
    private bool fCancel;


    public delegate void CountCallback(int count);
    public delegate void DoneCallback(bool done);


    public TestCase2()
    {
      fDebug = new Debug("T2");
      fUI = new DispatcherUI();
      fThread = new DispatcherThread("TestCase2");
      fCancel = false;
    }


    public override void Terminate()
    {
      fThread.Stop();
    }


    public override void Start()
    {
      Model.TestState = TestState.Started;
      fCancel = false;
      fDebug.WriteLn("Started");

      for (int i = 0; i < Model.Count; i++)
      {
        Execute(i, (seq) =>
        {
          Model.Status = seq + "";

          if (seq == (Model.Count - 1))
          {
            Model.Status = "Idle";
            Model.TestState = TestState.Idle;
            fDebug.WriteLn("Stopped");
          }
        });
      }

      fDebug.WriteLn("Waiting for finish");
    }


    public override void Stop()
    {
      Model.TestState = TestState.CancelRequest;
      fCancel = true;
      fDebug.WriteLn("Stop request");
    }


    private void Execute(int sequence, CountCallback callback)
    {
      fThread.Post(new HouseDelegate(() =>
      {
        if (fCancel == false)
        {
          LongOper();
        }

        // Three alternate ways to call
        fUI.Post(callback, sequence);

        fUI.Post(new HouseDelegate(() =>
        {
          callback(sequence);
        }));

        fUI.Post(() =>
        {
          callback(sequence);
        });
      }));
    }


    private void LongOper()
    {
      double a = 0;
      long count = 10000000;// 500000000;

      while (count > 0)
      {
        count--;

        a = Math.Sqrt(count);
      }
    }
  }
}
