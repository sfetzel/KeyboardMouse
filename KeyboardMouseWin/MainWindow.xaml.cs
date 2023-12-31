using SharpHook;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KeyboardMouseWin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            var hook = new TaskPoolGlobalHook();

            void Hook_KeyPressed(object? sender, KeyboardHookEventArgs e)
            {
                Debug.WriteLine($"key pressed: {e.Data.KeyCode}");
                if (e.Data.KeyCode == SharpHook.Native.KeyCode.VcEscape)
                {
                    Dispatcher.Invoke(() =>
                    {
                        Canvas.Children.Clear();
                        EnumerateElements(AutomationElement.RootElement, 0);
                    });
                }
            }
            hook.KeyPressed += Hook_KeyPressed;
            hook.RunAsync();
            InitializeComponent();
        }

        void EnumerateElements(AutomationElement element, int depth = 0)
        {
            if (depth > 3)
            {
                return;
            }
            var result = element.FindAll(TreeScope.Children, System.Windows.Automation.Condition.TrueCondition);

            foreach (AutomationElement child in result)
            {
                bool isControlOffscreen1;
                object isOffscreenNoDefault =
                    child.GetCurrentPropertyValue(AutomationElement.IsOffscreenProperty, true);
                if (isOffscreenNoDefault == AutomationElement.NotSupported)
                {
                }
                else
                {
                    isControlOffscreen1 = (bool)isOffscreenNoDefault;
                    
                    if (!isControlOffscreen1)
                    {
                        if (child.Current.IsControlElement)
                        {
                            if (child.Current.BoundingRectangle.Left != double.PositiveInfinity &&
                                child.Current.BoundingRectangle.Top != double.PositiveInfinity &&
                                child.Current.BoundingRectangle.Width != double.PositiveInfinity &&
                                    child.Current.BoundingRectangle.Height != double.PositiveInfinity)
                            {

                                var rectangle = new Rectangle();
                                rectangle.StrokeThickness = 1;
                                rectangle.Stroke = Brushes.Red;
                                var factor = 1;
                                Canvas.SetLeft(rectangle, 10 + child.Current.BoundingRectangle.Left * factor);
                                Canvas.SetTop(rectangle, 5 + child.Current.BoundingRectangle.Top * factor);
                                rectangle.Width = child.Current.BoundingRectangle.Width * factor;
                                rectangle.Height = child.Current.BoundingRectangle.Height * factor;
                                Canvas.Children.Add(rectangle);

                                var caption = new TextBlock();
                                caption.Text = "AB";
                                caption.Foreground = Brushes.Blue;
                                
                                //caption.Background = Brushes.White;
                                Canvas.SetLeft(caption, 10 + child.Current.BoundingRectangle.Left * factor);
                                Canvas.SetTop(caption, 15 + child.Current.BoundingRectangle.Top * factor);
                                Canvas.Children.Add(caption);

                            }
                        }
                        EnumerateElements(child, depth + 1);
                    }
                }
            }
        }
    }
}