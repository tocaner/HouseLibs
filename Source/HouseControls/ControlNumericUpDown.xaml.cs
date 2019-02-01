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
  /// Interaction logic for ControlNumericUpDown.xaml
  /// </summary>
  public partial class ControlNumericUpDown : UserControl, INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    public void RaisePropertyChanged(string name)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }


    public ControlNumericUpDown()
    {
      InitializeComponent();
    }


    private void Root_Loaded(object sender, RoutedEventArgs e)
    {
      // This makes ValueText get initialized
      Value = Value;
    }


    private static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register("Maximum", typeof(decimal), typeof(ControlNumericUpDown), new PropertyMetadata(100M));

    private static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register("Minimum", typeof(decimal), typeof(ControlNumericUpDown), new PropertyMetadata(0M));

    private static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(decimal), typeof(ControlNumericUpDown), new PropertyMetadata(0M));

    private static readonly DependencyProperty StepProperty =
        DependencyProperty.Register("Step", typeof(decimal), typeof(ControlNumericUpDown), new PropertyMetadata(1M));

    private static readonly DependencyProperty DecimalPlacesProperty =
        DependencyProperty.Register("DecimalPlaces", typeof(int), typeof(ControlNumericUpDown), new PropertyMetadata(0));

    private static readonly DependencyProperty TextAlignmentProperty =
        DependencyProperty.Register("TextAlignment", typeof(TextAlignment), typeof(ControlNumericUpDown), new PropertyMetadata(TextAlignment.Right));


    public decimal Maximum
    {
      get { return (decimal)GetValue(MaximumProperty); }
      set { SetValue(MaximumProperty, value); }
    }

    public decimal Minimum
    {
      get { return (decimal)GetValue(MinimumProperty); }
      set { SetValue(MinimumProperty, value); }
    }

    public decimal Value
    {
      get { return (decimal)GetValue(ValueProperty); }
      set
      {
        decimal y = (decimal)Math.Pow(10, DecimalPlaces);
        decimal cval = Math.Truncate(value * y) / y;

        if (cval < Minimum)
        {
          cval = Minimum;
        }

        if (cval > Maximum)
        {
          cval = Maximum;
        }

        SetValue(ValueProperty, cval);
        ValueText = Value.ToString("N" + DecimalPlaces);
      }
    }

    public decimal Step
    {
      get { return (decimal)GetValue(StepProperty); }
      set { SetValue(StepProperty, value); }
    }

    public int DecimalPlaces
    {
      get { return (int)GetValue(DecimalPlacesProperty); }
      set { SetValue(DecimalPlacesProperty, value); }
    }

    public TextAlignment TextAlignment
    {
      get { return (TextAlignment)GetValue(TextAlignmentProperty); }
      set { SetValue(TextAlignmentProperty, value); }
    }

    public string ValueText
    {
      get { return TextBoxCtrl.Text; }
      set { TextBoxCtrl.Text = value; }
    }

    private void UpdateInput()
    {
      decimal result;
      decimal.TryParse(ValueText, out result);
      Value = result;
    }

    private void UpIncr()
    {
      UpdateInput();
      Value += Step;
      TextBoxCtrl.Focus();
      TextBoxCtrl.SelectAll();
    }

    private void DownIncr()
    {
      UpdateInput();
      Value -= Step;
      TextBoxCtrl.Focus();
      TextBoxCtrl.SelectAll();
    }

    private void Up_Click(object sender, RoutedEventArgs e)
    {
      UpIncr();
    }

    private void Down_Click(object sender, RoutedEventArgs e)
    {
      DownIncr();
    }

    private void TextBoxCtrl_LostFocus(object sender, RoutedEventArgs e)
    {
      UpdateInput();
    }
  }
}
