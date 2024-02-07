using SharpHook;
using SharpHook.Native;
using System.Collections.Concurrent;
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
        private CaptionService CaptionService = new();

        private int characterIndex = 0;
        private IUIElementProvider elementProvider = new FlauiProvider();
        private HashSet<KeyCode> downKeys = new();
        private EventSimulator simulator;

        private bool isShiftDown => downKeys.Contains(KeyCode.VcLeftShift);

        private bool isControlDown => downKeys.Contains(KeyCode.VcLeftControl);

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
            Debug.WriteLine($"Key released: {e.Data.KeyCode}");
        }

        async void Hook_KeyPressed(object? sender, KeyboardHookEventArgs e)
        {
            Debug.WriteLine($"key pressed: {e.Data.KeyCode}");
            var pressedKey = char.ToUpper((char)e.Data.RawCode);
            downKeys.Add(e.Data.KeyCode);
            if (e.Data.KeyCode == KeyCode.VcEscape)
            {
                ClearScreen();
                CaptionService.CurrentObjects.Clear();
                await Dispatcher.InvokeAsync(Hide);
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
                    if (isShiftDown || isControlDown)
                    {
                        await ClickFirstElement(isControlDown);
                    }
                    else
                    {
                        var root = CaptionService.CurrentObjects.First();
                        var newObjects = GetListWithSubElements(elementProvider.GetSubElements(root.Value).ToList(), 50);

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
                            await ClickFirstElement(isControlDown);
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
                await ClickFirstElement(isControlDown);
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

        private async Task ClickFirstElement(bool isDoubleClick)
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
                        var currentPosition = Mouse.Position;
                        (var scalingX, var scalingY) = GetDpiScaling();
                        var x = (short)(uiElement.ClickPoint.Value.X / scalingX);
                        var y = (short)(uiElement.ClickPoint.Value.Y / scalingY);
                        var clickCount = isDoubleClick ? 2 : 1;
                        for (int i = 0; i < clickCount; ++i)
                        {
                            simulator.SimulateMousePress(x, y, SharpHook.Native.MouseButton.Button1, (ushort)clickCount);
                            simulator.SimulateMouseRelease(x, y, SharpHook.Native.MouseButton.Button1, (ushort)clickCount);
                            Thread.Sleep(5);
                        }
                        Mouse.Position = currentPosition;
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

        private IEnumerable<IUIElement> GetSubElements(IUIElement element, int timeout = 20)
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
                tokenSource.CancelAfter(timeout);
                Thread.Sleep(timeout);
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
            List<IUIElement> elements = GetListWithSubElements(windowChildren);

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

        private List<IUIElement> GetListWithSubElements(List<IUIElement> elementChildren, int timeout = 20)
        {
            var elements = new ConcurrentBag<IUIElement>();
            Parallel.ForEach(elementChildren, (element) =>
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var subElements = GetSubElements(element, timeout);
                var anyElement = false;
                foreach (var subElement in subElements)
                {
                    elements.Add(subElement);
                    anyElement = true;
                }
                stopwatch.Stop();

                if(!anyElement)
                {
                    elements.Add(element);
                };
                Debug.WriteLine($"get sub elements took {stopwatch.ElapsedMilliseconds}");
            });
            return elements.ToList();
        }

        private Dictionary<string, List<FrameworkElement>> CaptionElements = new();

        private void CaptionElement(string captionText, IUIElement element)
        {
            if (element != null)
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