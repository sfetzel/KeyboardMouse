using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using KeyboardMouseWin.Provider;
using KeyboardMouseWin.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardMouseWin
{
    public class FlauiProvider : IUIElementProvider
    {
        private UIA3Automation automation = new();

        private nint? foregroundWindowHandle;

        public FlauiProvider(nint? foregroundWindowHandle = null)
        {
            this.foregroundWindowHandle = foregroundWindowHandle;
        }


        public IEnumerable<IUIElement> GetElementsOfActiveWindow()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var window = automation.FromHandle(foregroundWindowHandle ?? WindowsUtils.GetForegroundWindow());
            var children = window.FindAllChildren();
            var descendants = children.Select(element => new FlauiUiElement(element));
            stopwatch.Stop();
            Debug.WriteLine($"Took {stopwatch.ElapsedMilliseconds} ms to find {children.Length} descendants");
            return descendants;
        }

        public IEnumerable<IUIElement> GetSubElements(IUIElement rootElement)
        {
            if (rootElement is FlauiUiElement flauiElement)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var children = flauiElement.Element.FindAllChildren();
                var descendants = children.Select(element => new FlauiUiElement(element));
                stopwatch.Stop();
                Debug.WriteLine($"Took {stopwatch.ElapsedMilliseconds} ms to find {children.Length} descendants");
                return descendants;
            }
            return [];
        }
    }
}
