using System.Windows.Input;

namespace KeyboardMouseWin;

public class Command(Func<object?, Task> onCommandFunc, Func<object?, bool> onCancelFunc) : ICommand
{
    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
        => onCancelFunc.Invoke(parameter);


    public void Execute(object? parameter)
    {
        _ = onCommandFunc.Invoke(parameter);
    }
}
