using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HouseUtils.Threading;


namespace TestThreads
{
  class TestCase1 : TestCase
  {
    private Debug fDebug;
    private HouseAsync fAsync;
    private bool fCancel;


    public delegate void CountCallback(int count);
    public delegate void DoneCallback(bool done);


    public TestCase1()
    {
      fDebug = new Debug("T1");
      fAsync = new HouseAsync("TestCase1");
      fCancel = false;
    }


    public override void Terminate()
    {
      fAsync.Stop();
    }


    public override void Start()
    {
      fCancel = false;
      Model.TestState = TestState.Started;
      fDebug.WriteLn("Started");

      for (int i = 0; (i < Model.Count) && (fCancel == false); i++)
      {
        fDebug.WriteLn("Init " + i);

        Execute(i, (seq) =>
        {
          Model.Status = seq + "";

          if (seq == (Model.Count - 1))
          {
            fDebug.WriteLn("Last One");
          }
        });

        fDebug.WriteLn("Check Next");
      }

      fDebug.WriteLn("Exiting");
      Model.Status = "Idle";
      Model.TestState = TestState.Idle;
    }


    public override void Stop()
    {
      Model.TestState = TestState.CancelRequest;
      fCancel = true;
      fDebug.WriteLn("Stop request");
    }


    private void Execute(int sequence, CountCallback callback)
    {
      fAsync.Await(LongOper);
      fDebug.WriteLn("Executed " + sequence);
      callback(sequence);
    }


    private void LongOper()
    {
      double a = 0;
      long count = 100000000;// 500000000;

      while (count > 0)
      {
        count--;

        a = Math.Sqrt(count);
      }
    }
  }
}
