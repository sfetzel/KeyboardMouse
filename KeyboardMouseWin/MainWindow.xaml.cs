using SharpHook;
using SharpHook.Native;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;
using Mouse = FlaUI.Core.Input.Mouse;

namespace KeyboardMouseWin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Hide();
        }

        /// <summary>
        /// Brings the window to front. Needs to hide base implementation because
        /// return type must be void for XAML behaviors.
        /// </summary>
        public new void Activate()
        {
            try
            {
                // Workaround to avoid problems in debug mode.
                var dispatcherType = typeof(Dispatcher);
                var countField = dispatcherType.GetField("_disableProcessingCount", BindingFlags.Instance | BindingFlags.NonPublic);
                var count = (int)countField.GetValue(Dispatcher.CurrentDispatcher);
                var suspended = count > 0;
                if (!suspended)
                {
                    base.Activate();
                }
            }
            catch (InvalidOperationException)
            {
            }
        }
    }
}