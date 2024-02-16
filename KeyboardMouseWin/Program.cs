using SharpHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KeyboardMouseWin
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var hook = new TaskPoolGlobalHook();
            hook.RunAsync();
            var window = new MainWindow();
            var application = new Application();
            window.RegisterHook(hook);
            Task.Run(() => window.Dispatcher.Invoke(() => window.Hide()));
            application.Run(window);
        }
    }
}
