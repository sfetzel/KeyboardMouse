using SharpHook;
using SharpHook.Native;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Mouse = FlaUI.Core.Input.Mouse;

namespace KeyboardMouseWin
{
    /// <summary>
    /// ViewModel which handles keyboard input, captions the UI elements and click on ui elements.
    /// </summary>
    internal class CaptionViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private ObservableCollection<CaptionedUiElement> captionedElements = new();
        /// <summary>
        /// Gets the captioned elements.
        /// </summary>
        public ObservableCollection<CaptionedUiElement> CaptionedElements
        {
            get => captionedElements;
            private set
            {
                captionedElements = value;
                NotifyPropertyChanged(nameof(CaptionedElements));
            }
        }

        private void NotifyPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private bool isActive;
        /// <summary>
        /// Indicates whether the caption window is active (shown in foreground). True for active
        /// and false for inactive (hidden).
        /// </summary>
        public bool IsActive
        {
            get => isActive;
            set
            {
                isActive = value;
                NotifyPropertyChanged(nameof(IsActive));
            }
        }

        private int leftPosition;

        public int LeftPosition
        {
            get => leftPosition;
            set
            {
                leftPosition = value;
                NotifyPropertyChanged(nameof(LeftPosition));
            }
        }

        private int topPosition;

        public int TopPosition
        {
            get => topPosition;
            set
            {
                topPosition = value;
                NotifyPropertyChanged(nameof(TopPosition));
            }
        }

        private double width;

        public double Width
        {
            get => width;
            set
            {
                width = value;
                NotifyPropertyChanged(nameof(Width));
            }
        }

        private double height;

        public double Height
        {
            get => height;
            set
            {
                height = value;
                NotifyPropertyChanged(nameof(Height));
            }
        }




        private int characterIndex { get; set; } = 0;
        private readonly EventSimulator simulator = new();

        public CaptionService CaptionService { get; set; }
        private IUIElementProvider elementProvider = new FlauiProvider();

        public Settings Settings { get; set; } = Settings.Default;
        public Dispatcher Dispatcher { get; private set; }
        public CaptionViewModel(CaptionService captionService, Dispatcher dispatcher)
        {
            CaptionService = captionService;
            Dispatcher = dispatcher;
        }

        public HashSet<Key> DownKeys { get; set; } = new();

        private bool isShiftDown => DownKeys.Contains(Key.LeftShift);

        private bool isControlDown => DownKeys.Contains(Key.LeftCtrl);


        private Action? onHiddenAction = null;

        public async Task HandleKeyDown(Key key)
        {
            Debug.WriteLine($"key pressed: {key}");
            var converter = new KeyConverter();

            var keyString = converter.ConvertToString(key);
            char? letter = null;
            if (keyString != null && keyString.Length == 1)
            {
                letter = keyString.ToUpperInvariant()[0];
            }

            DownKeys.Add(key);

            if (KeyCombination.FromString(Settings.ClearKeyCombiantion).IsPressed(DownKeys))
            {
                // Stop current action if any present.
                Clear();
                IsActive = false;
            }
            else if (KeyCombination.FromString(Settings.CaptionKeyCombination).IsPressed(DownKeys))
            {
                var handle = WindowsUtils.GetForegroundWindow();
                UpdateSize(handle);
                // Apply captioning of UI elements.
                Clear();

                await CaptionUiElements();

                Debug.WriteLine($"Window position: L{LeftPosition}, T:{TopPosition}, W:{Width}, H:{Height}");
                // show the window to prevent any keyboard events going to the active
                // window.
                IsActive = true;
                // Update size a second time, otherwise width/height might be incorrect. Reason unknown.
                UpdateSize(handle);
            }
            else if (CaptionService.CurrentObjects.Count > 0 && letter != null &&
                'A' <= letter && letter <= 'Z')
            {
                var filteredElements = CaptionService.GetFilteredOut(letter.Value, characterIndex).ToList();
                await ApplyFilter(filteredElements.Select(x => x.Key));
            }
            else if (key == Key.Enter)
            {
                Clear();
                await ClickFirstElement(isControlDown);
            }

        }

        private void UpdateSize(nint handle)
        {
            WindowsUtils.GetWindowRect(handle, out var rect);

            LeftPosition = Math.Max(rect.Left, 0);
            TopPosition = Math.Max(rect.Top, 0);
            Width = rect.Right - rect.Left;
            Height = rect.Bottom - rect.Top;
        }

        public async Task HandleKeyUp(Key key)
        {
            Debug.WriteLine($"key up: {key}");
            DownKeys.Remove(key);
        }

        public void Clear()
        {
            characterIndex = 0;
            CaptionService.CurrentObjects.Clear();
            // Clear everything on the screen.
            CaptionedElements = [];
        }

        /// <summary>
        /// Executes any actions which can only execute when the window is hidden.
        /// </summary>
        public void OnWindowHidden() => onHiddenAction?.Invoke();

        /// <summary>
        /// Handles filtering of captioned UI elements.
        /// </summary>
        /// <param name="filteredElementKeys">The keys of all UI elements which should be removed.</param>
        /// <returns></returns>
        private async Task ApplyFilter(IEnumerable<string> filteredElementKeys)
        {
            var filteredKeys = filteredElementKeys.ToHashSet();
            // remove all filtered elements from UI and caption service.
            CaptionService.Remove(filteredElementKeys);
            var elementsToRemove = CaptionedElements.Where(x => filteredKeys.Contains(x.Caption)).ToList();
            await Dispatcher.BeginInvoke(() =>
            {
                foreach (var elementToRemove in elementsToRemove)
                {
                    CaptionedElements.Remove(elementToRemove);
                }
            });

            ++characterIndex;
            // If only one element is remaining, either click it or show subelements.
            if (CaptionService.CurrentObjects.Count == 1)
            {
                if (isShiftDown || isControlDown)
                {
                    await ClickFirstElement(isControlDown);
                    Clear();
                }
                else
                {
                    // get subelements if any.
                    var root = CaptionService.CurrentObjects.First();
                    var newObjects = GetListWithSubElements(elementProvider.GetSubElements(root.Value).ToList(), 50);

                    // if there are subelements, then display them.
                    if (newObjects.Count != 0)
                    {
                        Clear();
                        CaptionService.SetObjects(newObjects);
                        CaptionedElements = new ObservableCollection<CaptionedUiElement>(CaptionService.CurrentObjects.Select(pair => ToCaptionedElement(pair.Key, pair.Value)));
                    }
                    else
                    {
                        // if there are no subelements of the only remaining element, then click the element.
                        await ClickFirstElement(isControlDown);
                        Clear();
                    }
                }
            }
            else if (CaptionService.CurrentObjects.Count == 0)
            {
                Clear();
                IsActive = false;
            }
        }

        private async Task ClickFirstElement(bool isDoubleClick)
        {
            if (CaptionService.CurrentObjects.Count > 0)
            {
                var uiElementPair = CaptionService.CurrentObjects.First();
                var uiElement = uiElementPair.Value;

                Point? clickPoint;
                if (Settings.Default.ClickOnCenterOfBoundingRectangle)
                {
                    // use center of bounding rectangle instead of click point.
                    // The click point is not always correct.
                    clickPoint = new Point(uiElement.BoundingRectangle.X + uiElement.BoundingRectangle.Width / 2,
                                    uiElement.BoundingRectangle.Y + uiElement.BoundingRectangle.Height / 2);
                }
                else
                {
                    clickPoint = uiElement.ClickPoint;
                }
                if (clickPoint.HasValue)
                {
                    // hide the window such that keyboard event goes again to the active window and
                    // the click actually works.
                    IsActive = false;
                    Debug.WriteLine($"Click on {uiElementPair.Key}, isDoubleClick: {isDoubleClick}");
                    onHiddenAction = () => ClickOnPoint(isDoubleClick, clickPoint.Value);
                }
            }
        }

        private void ClickOnPoint(bool isDoubleClick, Point clickPoint)
        {
            var currentPosition = Mouse.Position;
            (var scalingX, var scalingY) = GetDpiScaling();
            Debug.WriteLine($"Using scaling: x: {scalingX}, y:{scalingY}");
            var x = (short)(clickPoint.X / scalingX);
            var y = (short)(clickPoint.Y / scalingY);
            var clickCount = isDoubleClick ? 2 : 1;
            Debug.WriteLine($"Click on x: {x}, y: {y}");
            for (int i = 0; i < clickCount; ++i)
            {
                simulator.SimulateMousePress(x, y, SharpHook.Native.MouseButton.Button1, (ushort)clickCount);
                simulator.SimulateMouseRelease(x, y, SharpHook.Native.MouseButton.Button1, (ushort)clickCount);
                Thread.Sleep(5);
            }
            Mouse.Position = currentPosition;
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
            catch (Exception exception) when (exception is AggregateException || exception is TaskCanceledException) { }
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

            CaptionService.SetObjects(elements);
            CaptionedElements = new ObservableCollection<CaptionedUiElement>(CaptionService.CurrentObjects.
                Select(pair => ToCaptionedElement(pair.Key, pair.Value)));
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

                if (!anyElement)
                {
                    elements.Add(element);
                };
                //Debug.WriteLine($"get sub elements took {stopwatch.ElapsedMilliseconds}");
            });
            return elements.ToList();
        }

        private CaptionedUiElement ToCaptionedElement(string caption, IUIElement element)
        {
            var captionedElement = new CaptionedUiElement(caption, element);
            captionedElement.BoundingRectangle = new System.Drawing.Rectangle(
                captionedElement.BoundingRectangle.X - LeftPosition,
                captionedElement.BoundingRectangle.Y - TopPosition,
                captionedElement.BoundingRectangle.Width,
                captionedElement.BoundingRectangle.Height);
            return captionedElement;
        }

        private (double scalingX, double scalingY) GetDpiScaling()
        {
            double scalingX = 1;
            double scalingY = 1;

            PresentationSource presentationsource = null; //PresentationSource.FromVisual(this);

            if (presentationsource != null) // make sure it's connected
            {
                scalingX = presentationsource.CompositionTarget.TransformToDevice.M11;
                scalingY = presentationsource.CompositionTarget.TransformToDevice.M22;
            }
            return (scalingX, scalingY);
        }
    }
}
