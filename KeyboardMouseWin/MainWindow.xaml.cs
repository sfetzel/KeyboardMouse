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
using System.Windows.Forms;
using System.Collections.Concurrent;
using System.Collections;
using SharpHook.Native;

namespace KeyboardMouseWin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CaptionService CaptionService = new();

        private int characterIndex = 0;
        private IUIElementProvider elementProvider = new FlauiProvider();
        private List<KeyCode> downKeys = new();
        private EventSimulator simulator;

        private bool isShiftDown => downKeys.Contains(KeyCode.VcLeftShift);

        public MainWindow()
        {
            var hook = new TaskPoolGlobalHook();
            simulator = new EventSimulator();

            hook.KeyPressed += Hook_KeyPressed;
            hook.KeyReleased += Hook_KeyReleased;
            hook.RunAsync();
            InitializeComponent();
            Hide();
        }

        private void Hook_KeyReleased(object? sender, KeyboardHookEventArgs e)
        {
            downKeys.Remove(e.Data.KeyCode);
        }

        async void Hook_KeyPressed(object? sender, KeyboardHookEventArgs e)
        {
            Debug.WriteLine($"key pressed: {e.Data.KeyCode}");
            var pressedKey = char.ToUpper((char)e.Data.RawCode);
            downKeys.Add(e.Data.KeyCode);
            if (e.Data.KeyCode == KeyCode.VcEscape)
            {
                if (CaptionService.CurrentObjects.Count > 0)
                {
                    ClearScreen();
                    CaptionService.CurrentObjects.Clear();
                    await Dispatcher.InvokeAsync(Hide);
                }
            }
            if (downKeys.Contains(KeyCode.VcLeftControl) && downKeys.Contains(KeyCode.VcLeftAlt)
                && downKeys.Contains(KeyCode.VcW))
            {
                ClearScreen();

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
                        if (CaptionElements.TryGetValue(element.Key, out var value))
                        {
                            value.ForEach(captionElement => Canvas.Children.Remove(captionElement));
                        }
                    }
                });
                // remove all filtered elements
                foreach (var element in filteredElements)
                {
                    CaptionService.CurrentObjects.Remove(element.Key);
                }

                ++characterIndex;
                if (CaptionService.CurrentObjects.Count == 1)
                {
                    if (isShiftDown)
                    {
                        await ClickFirstElement();
                    }
                    else
                    {
                        var root = CaptionService.CurrentObjects.First();
                        var newObjects = GetSubElements(root.Value);

                        if (newObjects.Any()) // if there are subelements, then display them.
                        {
                            CaptionService.CurrentObjects.Clear();
                            characterIndex = 0;
                            ClearScreen();
                            var newKeys = CaptionService.AddObjects(newObjects);
                            foreach (var obj in newKeys)
                            {
                                await Dispatcher.InvokeAsync(() => CaptionElement(obj, CaptionService.CurrentObjects[obj]));
                            }
                        }
                        else
                        {
                            await ClickFirstElement();
                        }
                    }
                }
                else if (CaptionService.CurrentObjects.Count == 0)
                {
                    ClearScreen();
                    await Dispatcher.InvokeAsync(Hide);
                }
            }
            else if (e.Data.KeyCode == SharpHook.Native.KeyCode.VcEnter)
            {
                ClearScreen();
                await ClickFirstElement();
            }
        }

        private void ClearScreen()
        {
            // Clear everything on the screen.
            Dispatcher.Invoke(() =>
            {
                Canvas.Children.Clear();
                CaptionElements.Clear();
            });
        }

        private async Task ClickFirstElement()
        {
            if (CaptionService.CurrentObjects.Count > 0)
            {
                var uiElement = CaptionService.CurrentObjects.First().Value;
                if (uiElement.ClickPoint.HasValue)
                {
                    Dispatcher.Invoke(() =>
                    {
                        // hide the window such that keyboard event go again to the active window and
                        // the click actually works.
                        Hide();
                        (var scalingX, var scalingY) = GetDpiScaling();
                        var x = (short)(uiElement.ClickPoint.Value.X / scalingX);
                        var y = (short)(uiElement.ClickPoint.Value.Y / scalingY);
                        simulator.SimulateMousePress(x, y, SharpHook.Native.MouseButton.Button1, 1);
                        simulator.SimulateMouseRelease(x, y, SharpHook.Native.MouseButton.Button1, 1);
                    });
                }
                await Dispatcher.InvokeAsync(() =>
                {
                    if (CaptionElements.Count > 0)
                    {
                        CaptionElements.First().Value.ForEach(captionElement => Canvas.Children.Clear());
                    }
                });

            }

            CaptionService.CurrentObjects.Clear();
        }

        private async Task ProcessElement(ConcurrentBag<IUIElement> elements, IUIElement element, CancellationToken token)
        {
            var subElements = elementProvider.GetSubElements(element);

            foreach (var item in subElements)
            {
                elements.Add(item);
            }
            if (token.IsCancellationRequested)
            {
                return;
            }
            foreach (var item in subElements)
            {
                await ProcessElement(elements, item, token);
            }

        }

        private IEnumerable<IUIElement> GetSubElements(IUIElement element)
        {
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            ConcurrentBag<IUIElement> results = new();
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                var task = Task.Run(async () =>
                {
                    await ProcessElement(results, element, token);
                }, token);
                tokenSource.CancelAfter(20);
                Thread.Sleep(20);
            }
            catch (AggregateException)
            {

            }
            catch (TaskCanceledException)
            {

            }
            stopwatch.Stop();

            return results;
        }

        private async Task CaptionUiElements()
        {
            var stopwatch = new Stopwatch();
            var windowChildren = elementProvider.GetElementsOfActiveWindow().ToList();
            stopwatch.Start();
            var elements = new List<IUIElement>();
            Parallel.ForEach(windowChildren, (element) =>
            {
                elements.Add(element);
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                elements.AddRange(GetSubElements(element));
                stopwatch.Stop();
                Debug.WriteLine($"get sub elements took {stopwatch.ElapsedMilliseconds}");
            });

            stopwatch.Stop();
            Debug.WriteLine($"get elements took {stopwatch.ElapsedMilliseconds}");
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

            var x = 10 + (element.BoundingRectangle.Left) * factor;
            var y = 15 + (element.BoundingRectangle.Top) * factor;
            if (element.BoundingRectangle.Width > 30)
            {
                x += 10;
            }
            if (element.BoundingRectangle.Height > 30)
            {
                y += 10;
            }
            Canvas.SetLeft(caption, x);
            Canvas.SetTop(caption, y);
            Canvas.Children.Add(caption);

            CaptionElements.Add(captionText, new() { caption, rectangle });
        }

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