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
        public IEnumerable<IUIElement> GetElementsOfActiveWindow()
        {
            using (var automation = new UIA3Automation())
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var window = automation.FromHandle(WindowsUtils.GetForegroundWindow());
                var descendants = window.FindAllChildren().AsParallel().Select(
                    element => new FlauiUiElement(element));
                stopwatch.Stop();
                //Debug.WriteLine($"Took {stopwatch.ElapsedMilliseconds} ms to find all descendants");
                foreach(var element in descendants)
                {
                    yield return element;
                }
            }
        }

        public IEnumerable<IUIElement> GetSubElements(IUIElement rootElement)
        {
            if (rootElement is FlauiUiElement flauiElement)
            {
                using (var automation = new UIA3Automation())
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    var descendants = flauiElement.Element.FindAllChildren().AsParallel().Select(
                        element => new FlauiUiElement(element));
                    stopwatch.Stop();
                    //Debug.WriteLine($"Took {stopwatch.ElapsedMilliseconds} ms to find all descendants");
                    foreach (var element in descendants)
                    {
                        yield return element;
                    }
                }
            }
        }
    }
}
