using System;
using System.Collections.Generic;
using System.ComponentModel;
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


namespace HouseControls
{
  /// <summary>
  /// Interaction logic for ControlImagePanZoom.xaml
  /// </summary>
  public partial class ControlImagePanZoom : UserControl
  {
    private ScaleTransform _Zoom;
    private Point _ScrollOffsetsOnMouseDown;
    private Point _PositionOnMouseDown;
    private double _FittedScale = 1.0;


    public event EventHandler LayoutChanged;


    public double Scale
    {
      get { return _Zoom.ScaleX; } // ScaleX and ScaleY are same
      private set { _Zoom.ScaleX = _Zoom.ScaleY = value; LayoutChanged?.Invoke(this, null); }
    }


    public double PanX
    {
      get { return MyScrollViewer.HorizontalOffset; }
      private set { MyScrollViewer.ScrollToHorizontalOffset(value); LayoutChanged?.Invoke(this, null); }
    }


    public double PanY
    {
      get { return MyScrollViewer.VerticalOffset; }
      private set { MyScrollViewer.ScrollToVerticalOffset(value); LayoutChanged?.Invoke(this, null); }
    }


    public ControlImagePanZoom()
    {
      InitializeComponent();
      _Zoom = new ScaleTransform();

      // Render Transform will Zoom and Pan but will not update Scroll bars.
      // Layout Transform updates Scroll bars correctly when Zoomed but will not Pan.
      // Use Layout Transform, together ScrollToHorizontalOffset, ScrollToVerticalOffset
      // functions to adjust the Pan
      TransformGroup group = new TransformGroup();
      group.Children.Add(_Zoom);
      MyImage.LayoutTransform = group;
      MyImage.DataContext = this;
      MyImage.TargetUpdated += MyImage_TargetUpdated;
   }


    private void MyImage_TargetUpdated(object sender, DataTransferEventArgs e)
    {
      PanZoomFit();
    }


    public void PanZoomFit()
    {
      if (MyImage.Source != null)
      {
        double scaleX = this.ActualWidth / MyImage.Source.Width;
        double scaleY = this.ActualHeight / MyImage.Source.Height;
        this.Scale = scaleX < scaleY ? scaleX : scaleY;
        this.PanX = 0;
        this.PanY = 0;
        _FittedScale = this.Scale;
      }
    }


    public void PanZoomReset()
    {
      this.Scale = 1.0;
      this.PanX = 0;
      this.PanY = 0;
    }


    public static readonly DependencyProperty TestTextProperty =
        DependencyProperty.Register("TestText", typeof(string), typeof(ControlImagePanZoom), new PropertyMetadata(""));


    public static readonly DependencyProperty DisplayedImageProperty =
        DependencyProperty.Register("DisplayedImage", typeof(ImageSource), typeof(ControlImagePanZoom), new PropertyMetadata(null));


    public string TestText
    {
      get { return (string)GetValue(TestTextProperty); }
      set { SetValue(TestTextProperty, value); }
    }


    public ImageSource DisplayedImage
    {
      get { return (ImageSource)GetValue(DisplayedImageProperty); }
      set { SetValue(DisplayedImageProperty, value); }
    }


    private void MyImage_MouseWheel(object sender, MouseWheelEventArgs e)
    {
      double step = this.Scale * 0.1;
      double zoom = e.Delta > 0 ? step : -step;

      if ((zoom > 0) || ((this.Scale + zoom) >= (0.8 * _FittedScale)))
      {
        Point relative = e.GetPosition(MyImage); // Using the position in MyImage

        this.Scale += zoom;
        this.PanX += relative.X * zoom;
        this.PanY += relative.Y * zoom;

        e.Handled = true;
      }
    }


    private void MyImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      _PositionOnMouseDown = e.GetPosition(MyScrollViewer); // NOT using the position in MyImage
      _ScrollOffsetsOnMouseDown = new Point(MyScrollViewer.HorizontalOffset, MyScrollViewer.VerticalOffset);
      this.Cursor = Cursors.Hand;
      MyImage.CaptureMouse();
    }


    private void MyImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      MyImage.ReleaseMouseCapture();
      this.Cursor = Cursors.Arrow;
    }


    private void MyImage_MouseMove(object sender, MouseEventArgs e)
    {
      if (MyImage.IsMouseCaptured)
      {
        Vector v = _PositionOnMouseDown - e.GetPosition(MyScrollViewer); // NOT using the position in MyImage
        this.PanX = (_ScrollOffsetsOnMouseDown.X + v.X);
        this.PanY = (_ScrollOffsetsOnMouseDown.Y + v.Y);
      }
    }


    private void MyImage_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
      PanZoomReset();
    }
  }
}
