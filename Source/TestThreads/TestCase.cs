using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestThreads
{
  public abstract class TestCase
  {
    public TestModel Model { get; set; } = new TestModel();

    public abstract void Terminate();

    public abstract void Start();

    public abstract void Stop();
  }
}
