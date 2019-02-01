using System;
using System.ComponentModel;
using System.Windows.Input;


namespace HouseUtils
{
  public class WpfTools
  {
  }


  public class BindingPropertyClass : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    public void RaisePropertyChanged(string name)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
  }


  public class CommandHandler : ICommand
  {
    public CommandHandler(Action action)
    {
      _action = action;
    }


    private Action _action;


    private bool _isEnabled = true;
    public bool IsEnabled
    {
      get { return _isEnabled; }
      set { if (_isEnabled != value) { _isEnabled = value; CanExecuteChanged?.Invoke(this, EventArgs.Empty); } }
    }


    public bool CanExecute(object parameter)
    {
      return _isEnabled;
    }


    public event EventHandler CanExecuteChanged;


    public void Execute(object parameter)
    {
      _action();
    }
  }
}
