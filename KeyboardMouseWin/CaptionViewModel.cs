﻿using KeyboardMouseWin.Provider;
using KeyboardMouseWin.Utils;
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
using System.Xml.Linq;
using Mouse = FlaUI.Core.Input.Mouse;

namespace KeyboardMouseWin
{
    /// <summary>
    /// ViewModel which handles keyboard input, captions the UI elements and click on ui elements.
    /// </summary>
    public class CaptionViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public int CaptionTimeLimit { get; } = 100;

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
        public IUIElementProvider ElementProvider { get; private set; }

        public Settings Settings { get; set; } = Settings.Default;
        public Dispatcher Dispatcher { get; private set; }

        public CaptionViewModel(CaptionService captionService, IUIElementProvider elementProvider, Dispatcher dispatcher)
        {
            CaptionService = captionService;
            Dispatcher = dispatcher;
            this.ElementProvider = elementProvider;
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
                ClickFirstElement(isControlDown);
            }

        }

        /// <summary>
        /// Sets the size properties Width, Height and LeftPosition, TopPosition
        /// from the window with the specified handle.
        /// </summary>
        /// <param name="handle">The window from which the size is taken.</param>
        private void UpdateSize(nint handle)
        {
            WindowsUtils.GetWindowRect(handle, out var rect);

            LeftPosition = Math.Max(rect.Left, 0);
            TopPosition = Math.Max(rect.Top, 0);
            Width = rect.Right - rect.Left;
            Height = rect.Bottom - rect.Top;
        }

        public void HandleKeyUp(Key key)
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
        public void OnWindowHidden()
        {
            onHiddenAction?.Invoke();
            onHiddenAction = null;
        }

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
            await FilterCaptionedElements(filteredKeys);
            Debug.WriteLine($"Filtered {filteredKeys.Count} elements, {CaptionService.CurrentObjects.Count} remaining");
            ++characterIndex;
            // If only one element is remaining, either click it or show subelements.
            if (CaptionService.CurrentObjects.Count == 1)
            {
                if (isShiftDown || isControlDown)
                {
                    ClickFirstElement(isControlDown);
                    Clear();
                }
                else
                {
                    // get subelements if any.
                    var root = CaptionService.CurrentObjects.First();
                    Clear();
                    // if there are subelements, then display them.
                    await CaptionUiElements(ElementProvider.GetSubElements(root.Value));

                    if (CaptionService.CurrentObjects.Count == 0)
                    {
                        Debug.WriteLine($"No subelements, click on {root.Key}");
                        // undo captioning
                        CaptionService.CurrentObjects = new() { { root.Key, root.Value } };
                        // if there are no subelements of the only remaining element, then click the element.
                        ClickFirstElement(isControlDown);
                        Clear();
                    }
                    else if(CaptionService.CurrentObjects.Count == 1)
                    {
                        // if there is only one child, then click it.
                        ClickFirstElement(isControlDown);
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

        private async Task FilterCaptionedElements(HashSet<string> filteredKeys)
        {
            var elementsToRemove = CaptionedElements.Where(x => filteredKeys.Contains(x.Caption)).ToList();
            await Dispatcher.BeginInvoke(() =>
            {
                foreach (var elementToRemove in elementsToRemove)
                {
                    CaptionedElements.Remove(elementToRemove);
                }
            });
        }

        private void ClickFirstElement(bool isDoubleClick)
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

        public async Task CaptionUiElements() => await CaptionUiElements(ElementProvider.GetElementsOfActiveWindow());
        
        /// <summary>
        /// Gets all subelements until the "CaptionTimeLimit" is exceeded.
        /// </summary>
        /// <param name="startingElements"></param>
        /// <returns></returns>
        public async Task CaptionUiElements(IEnumerable<IUIElement> startingElements)
        {
            var elements = new ConcurrentBag<IUIElement>();
            var executor = new LimitedTimeExecutor(CaptionTimeLimit);
            async void UpdateElements(IEnumerable<IUIElement> newElements)
            {
                if (executor.IsOverLimit)
                {
                    return;
                }
                foreach (var element in newElements)
                {
                    elements.Add(element);
                }
                UpdateCaptionedElements(elements);
                Debug.WriteLine($"Adding objects at {executor.Stopwatch.ElapsedMilliseconds}");
                if (!executor.IsOverLimit)
                {
                    foreach (var element in newElements)
                    {
                        executor.StartNewTask(() => UpdateElements(ElementProvider.GetSubElements(element)));
                    }
                }
            }
            await executor.Run(() => UpdateElements(startingElements), false);
            Debug.WriteLine($"get elements took {executor.Stopwatch.ElapsedMilliseconds}, found {CaptionService.CurrentObjects.Count}");
            characterIndex = 0;

        }

        private void UpdateCaptionedElements(IEnumerable<IUIElement> elements)
        {
            CaptionService.SetObjects(elements);
            CaptionedElements = new ObservableCollection<CaptionedUiElement>(CaptionService.CurrentObjects.
                Select(pair => ToCaptionedElement(pair.Key, pair.Value)));

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
