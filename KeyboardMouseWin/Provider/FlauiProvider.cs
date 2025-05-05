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


        public (IUIElement, Task) GetActiveWindowElement(CancellationToken token, Action<IEnumerable<IUIElement>>? onChildrenAdded)
        {
            var window = automation.FromHandle(foregroundWindowHandle ?? WindowsUtils.GetForegroundWindow());
            var windowElement = new FlauiUiElement(window);
            
            return (windowElement, AddDescendants(windowElement, token, onChildrenAdded));
        }

        public async Task AddDescendants(IUIElement rootElement, CancellationToken token, Action<IEnumerable<IUIElement>>? onChildrenAdded)
        {
            if (rootElement is FlauiUiElement flauiElement)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                if (token.IsCancellationRequested)
                {
                    return;
                }
                var rawChildren = flauiElement.Element.FindAllChildren();
                var children = rawChildren.Select(element => new FlauiUiElement(element)).ToArray();
                onChildrenAdded?.Invoke(children);

                foreach (var child in children)
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    await AddDescendants(child, token, onChildrenAdded);
                }

                stopwatch.Stop();
                Debug.WriteLine($"Took {stopwatch.ElapsedMilliseconds} ms to find {rawChildren.Length} descendants");
            }
        }
    }
}
