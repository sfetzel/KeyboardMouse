using SharpHook;
using System.Reactive;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;

namespace KeyboardMouseWin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CaptionService CaptionService = new();

        private int characterIndex = 0;
        private IUIElementProvider elementProvider = new AutomationElementProvider();

        private EventSimulator simulator;

        public MainWindow()
        {
            var hook = new TaskPoolGlobalHook();
            simulator = new EventSimulator();

            hook.KeyPressed += Hook_KeyPressed;
            hook.RunAsync();
            InitializeComponent();
            Hide();
        }

        async void Hook_KeyPressed(object? sender, KeyboardHookEventArgs e)
        {
            Debug.WriteLine($"key pressed: {e.Data.KeyCode}");
            var pressedKey = char.ToUpper((char)e.Data.RawCode);
            if (e.Data.KeyCode == SharpHook.Native.KeyCode.VcEscape)
            {
                // Clear everything on the screen.
                await Dispatcher.InvokeAsync(() =>
                {
                    Canvas.Children.Clear();
                    CaptionElements.Clear();
                });

                if (CaptionService.CurrentObjects.Count == 0)
                {
                    await CaptionUiElements();
                }
                else
                {
                    CaptionService.CurrentObjects.Clear();
                    await Dispatcher.InvokeAsync(Hide);
                }
            }
            else if (CaptionService.CurrentObjects.Count > 0 && 'A' <= pressedKey && pressedKey <= 'Z')
            {
                var filteredElements = CaptionService.GetFilteredOut(pressedKey, characterIndex).ToList();
                e.SuppressEvent = true;
                await Dispatcher.InvokeAsync(() =>
                {
                    // remove all filtered elements
                    foreach (var element in filteredElements)
                    {
                        CaptionElements[element.Key].ForEach(captionElement => Canvas.Children.Remove(captionElement));
                    }
                });
                // remove all filtered elements
                foreach (var element in filteredElements)
                {
                    CaptionService.CurrentObjects.Remove(element.Key);
                }

                if (CaptionService.CurrentObjects.Count == 1)
                {
                    await ClickFirstElement();
                }
                ++characterIndex;
            }
        }

        private async Task ClickFirstElement()
        {
            var uiElement = CaptionService.CurrentObjects.First().Value;
            if (uiElement.ClickPoint.HasValue)
            {
                Dispatcher.Invoke(() =>
                {
                    // hide the window such that keyboard event go again to the active window and
                    // the click actually works.
                    Hide();

                    simulator.SimulateMousePress((short)uiElement.ClickPoint.Value.X, (short)uiElement.ClickPoint.Value.Y, SharpHook.Native.MouseButton.Button1, 1);
                    simulator.SimulateMouseRelease((short)uiElement.ClickPoint.Value.X, (short)uiElement.ClickPoint.Value.Y, SharpHook.Native.MouseButton.Button1, 1);
                });
            }

            CaptionService.CurrentObjects.Clear();
            await Dispatcher.InvokeAsync(() => CaptionElements.First().Value.ForEach(captionElement => Canvas.Children.Clear()));
        }

        private async Task CaptionUiElements()
        {
            var watch = new Stopwatch();
            var elements = new List<IUIElement>();
            await foreach (var element in elementProvider.GetElementsOfActiveWindow())
            {
                elements.Add(element);
            }

            characterIndex = 0;

            CaptionService.AddObjects(elements);
            foreach ((var key, var element) in CaptionService.CurrentObjects)
            {
                await Dispatcher.InvokeAsync(() => CaptionElement(key, element));
            }

            // show the window to prevent any keyboard events going to the active
            // window.
            await Dispatcher.InvokeAsync(Show);
            await Dispatcher.InvokeAsync(Activate);
        }

        private Dictionary<string, List<FrameworkElement>> CaptionElements = new();

        private void CaptionElement(string captionText, IUIElement element)
        {
            var rectangle = new Rectangle();
            rectangle.StrokeThickness = 1;
            rectangle.Stroke = Brushes.Red;
            var factor = 1;
            Canvas.SetLeft(rectangle, 10 + element.BoundingRectangle.Left * factor);
            Canvas.SetTop(rectangle, 5 + element.BoundingRectangle.Top * factor);
            rectangle.Width = element.BoundingRectangle.Width * factor;
            rectangle.Height = element.BoundingRectangle.Height * factor;
            Canvas.Children.Add(rectangle);

            var caption = new TextBlock();
            caption.Text = captionText;
            caption.Effect = new DropShadowEffect() { ShadowDepth = 4, Color = Colors.Black, BlurRadius = 4 };
            caption.Foreground = Brushes.White;
            caption.FontWeight = FontWeights.Bold;
            caption.FontSize = 14;

            //caption.Background = Brushes.White;
            Canvas.SetLeft(caption, 10 + element.BoundingRectangle.Left * factor);
            Canvas.SetTop(caption, 15 + element.BoundingRectangle.Top * factor);
            Canvas.Children.Add(caption);
            CaptionElements.Add(captionText, new() { caption, rectangle });
        }

    }
}