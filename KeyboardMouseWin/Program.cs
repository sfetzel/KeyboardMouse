using KeyboardMouseWin.Utils;
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
            var service = new CaptionService();
            var window = new MainWindow();
            var viewModel = new CaptionViewModel(service, new FlauiProvider(), window.Dispatcher);
            hook.KeyPressed += async (_, e) => await viewModel.HandleKeyDown(SharpHookConverter.ToKey(e.Data.KeyCode), e);
            hook.KeyReleased += (_, e) => viewModel.HandleKeyUp(SharpHookConverter.ToKey(e.Data.KeyCode));
            window.DataContext = viewModel;
            var application = new Application();
            Task.Run(() => window.Dispatcher.Invoke(() => window.Hide()));
            application.Run(window);
        }
    }
}
