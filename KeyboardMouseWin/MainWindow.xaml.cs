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

namespace KeyboardMouseWin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<string, AutomationElement> captionedElements = new();

        private CaptionService CaptionService = new();

        private int characterIndex = 0;

        public MainWindow()
        {
            var hook = new TaskPoolGlobalHook();
            var simulator = new EventSimulator();
            async void Hook_KeyPressed(object? sender, KeyboardHookEventArgs e)
            {
                Debug.WriteLine($"key pressed: {e.Data.KeyCode}");
                var key = char.ToUpper((char)e.Data.RawCode);
                if (e.Data.KeyCode == SharpHook.Native.KeyCode.VcEscape)
                {
                    var elements = new List<AutomationElement>();
                    await foreach (var element in EnumerateElements(AutomationElement.RootElement, 0))
                    {
                        elements.Add(element);
                    }
                    Dispatcher.Invoke(() =>
                    {
                        captionedElements.Clear();
                        Canvas.Children.Clear();
                        CaptionElements.Clear();

                        if (CaptionService.CurrentObjects.Count == 0)
                        {
                            characterIndex = 0;

                            CaptionService.AddObjects(elements.Select(x => new CaptionedElement() { UiElement = x }));
                            foreach ((var key, var element) in CaptionService.CurrentObjects)
                            {
                                CaptionElement(key, element);
                            }

                            // show the window to prevent any keyboard events going to the active
                            // window.
                            Show();
                            Activate();
                        }
                        else
                        {
                            CaptionService.CurrentObjects.Clear();
                            Dispatcher.InvokeAsync(Hide);
                        }
                    });
                }
                else if (CaptionService.CurrentObjects.Count > 0 && 'A' <= key && key <= 'Z')
                {
                    var filteredElements = CaptionService.GetFilteredOut(key, characterIndex).ToList();
                    e.SuppressEvent = true;
                    Dispatcher.InvokeAsync(() =>
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
                        var uiElement = CaptionService.CurrentObjects.First().Value.UiElement;
                        //if (uiElement.TryGetCurrentPattern(InvokePattern.Pattern, out var invokePattern))
                        //{
                        //    (invokePattern as InvokePattern)?.Invoke();
                        //}
                        //if (uiElement.TryGetCurrentPattern(SelectionPattern.Pattern, out var selectionPattern))
                        //{
                        //    (selectionPattern as SelectionPattern)?.Current.
                        //}
                        uiElement.TryGetClickablePoint(out var clickablePoint);
                        if (clickablePoint.X != 0 && clickablePoint.Y != 0)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                // hide the window such that keyboard event go again to the active window and
                                // the click actually works.
                                Hide();

                                simulator.SimulateMousePress((short)clickablePoint.X, (short)clickablePoint.Y, SharpHook.Native.MouseButton.Button1, 1);
                                simulator.SimulateMouseRelease((short)clickablePoint.X, (short)clickablePoint.Y, SharpHook.Native.MouseButton.Button1, 1);
                            });
                        }

                        CaptionService.CurrentObjects.Clear();
                        Dispatcher.InvokeAsync(() => CaptionElements.First().Value.ForEach(captionElement => Canvas.Children.Clear()));

                        

                    }
                    ++characterIndex;
                }
            }
            hook.KeyPressed += Hook_KeyPressed;
            hook.RunAsync();
            InitializeComponent();
            Hide();
        }

        private Dictionary<string, List<FrameworkElement>> CaptionElements = new();

        private void CaptionElement(string captionText, CaptionedElement element)
        {
            var child = element.UiElement.Current;
            if (child.BoundingRectangle.Left != double.PositiveInfinity &&
                child.BoundingRectangle.Top != double.PositiveInfinity &&
                child.BoundingRectangle.Width != double.PositiveInfinity &&
                    child.BoundingRectangle.Height != double.PositiveInfinity)
            {

                var rectangle = new Rectangle();
                rectangle.StrokeThickness = 1;
                rectangle.Stroke = Brushes.Red;
                var factor = 1;
                Canvas.SetLeft(rectangle, 10 + child.BoundingRectangle.Left * factor);
                Canvas.SetTop(rectangle, 5 + child.BoundingRectangle.Top * factor);
                rectangle.Width = child.BoundingRectangle.Width * factor;
                rectangle.Height = child.BoundingRectangle.Height * factor;
                Canvas.Children.Add(rectangle);

                var caption = new TextBlock();
                caption.Text = captionText;
                caption.Effect = new DropShadowEffect() { ShadowDepth = 4, Color = Colors.Black, BlurRadius = 4 };
                caption.Foreground = Brushes.White;
                caption.FontWeight = FontWeights.Bold;
                caption.FontSize = 14;

                //caption.Background = Brushes.White;
                Canvas.SetLeft(caption, 10 + child.BoundingRectangle.Left * factor);
                Canvas.SetTop(caption, 15 + child.BoundingRectangle.Top * factor);
                Canvas.Children.Add(caption);
                CaptionElements.Add(captionText, new() { caption, rectangle });
            }
        }


        async IAsyncEnumerable<AutomationElement> EnumerateElements(AutomationElement element, int depth = 0)
        {
            if (depth > 3)
            {
                yield break;
            }
            var result = element.FindAll(TreeScope.Children, new System.Windows.Automation.PropertyCondition(AutomationElement.IsOffscreenProperty, false));

            foreach (AutomationElement child in result)
            {
                bool isControlOffscreen1;
                object isOffscreenNoDefault =
                    child.GetCurrentPropertyValue(AutomationElement.IsOffscreenProperty, false);
                if (isOffscreenNoDefault != AutomationElement.NotSupported)
                {
                    isControlOffscreen1 = (bool)isOffscreenNoDefault;
                    if (!isControlOffscreen1 && child.Current.IsControlElement && child.Current.BoundingRectangle.Left != double.PositiveInfinity &&
                            child.Current.BoundingRectangle.Top != double.PositiveInfinity &&
                            child.Current.BoundingRectangle.Width != double.PositiveInfinity &&
                                child.Current.BoundingRectangle.Height != double.PositiveInfinity)
                    {
                        yield return child;
                        await foreach (var childElement in EnumerateElements(child, depth + 1))
                        {
                            yield return childElement;
                        }
                    }
                }
            }
        }
    }
}