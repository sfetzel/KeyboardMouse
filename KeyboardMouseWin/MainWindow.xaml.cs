using SharpHook;
using SharpHook.Native;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
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
        public new void Activate() => base.Activate();

        private (double scalingX, double scalingY) GetDpiScaling()
        {
            double scalingX = 1;
            double scalingY = 1;
            PresentationSource presentationsource = PresentationSource.FromVisual(this);

            if (presentationsource != null) // make sure it's connected
            {
                scalingX = presentationsource.CompositionTarget.TransformToDevice.M11;
                scalingY = presentationsource.CompositionTarget.TransformToDevice.M22;
            }
            return (scalingX, scalingY);
        }
    }
}