using Microsoft.Xaml.Behaviors.Core;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace KeyboardMouseWin;

public class WelcomeWindowViewModel
{
    public Settings Settings { get; set; } = Settings.Default;

    public ICommand EditShortCuts { get; set; }
    public ICommand CloseWindow { get; set; }

    public WelcomeWindowViewModel()
    {
        EditShortCuts = new Command(OnEdit, _ => true);
        CloseWindow = new Command(OnClose, _ => true);
    }

    public Task OnEdit(object? param)
    {

        string currentDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        string configFile = Path.Combine(currentDirectory, "KeyboardMouseWin.dll.config");

        Process.Start("notepad", configFile);

        return Task.CompletedTask;
    }
    public Task OnClose(object? param)
    {
        if(param is Window window)
        {
            window.Close();
            Environment.Exit(0);
        }
        return Task.CompletedTask;
    }
}


