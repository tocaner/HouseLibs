using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestThreads
{
  /// <summary>
  /// Interaction logic for UserControlTest.xaml
  /// </summary>
  public partial class UserControlTest : UserControl
  {
    private TestCase fTest = null;
    public TestCase Test
    {
      get { return fTest; }
      set { fTest = value; DataContext = fTest.Model; }
    }


    public UserControlTest()
    {
      InitializeComponent();
      this.Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
    }

    private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
    {
      Test.Terminate();
    }

    private void Start_Click(object sender, RoutedEventArgs e)
    {
      Test.Start();
    }


    private void Stop_Click(object sender, RoutedEventArgs e)
    {
      Test.Stop();
    }
  }
}
