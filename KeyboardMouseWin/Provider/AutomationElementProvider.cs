using KeyboardMouseWin.Provider;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace KeyboardMouseWin
{
    public class AutomationElementProvider : IUIElementProvider
    {
        public int MaxDepth { get; set; } = 2;

        public IEnumerable<AutomationElement> EnumerateElements(AutomationElement element, int depth)
        {
            if (depth <= MaxDepth)
            {

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var result = element.FindAll(TreeScope.Children, System.Windows.Automation.Condition.TrueCondition);
                stopwatch.Stop();
                Debug.WriteLine($"time for depth {depth}: {stopwatch.ElapsedMilliseconds}");

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
                            foreach (var childElement in EnumerateElements(child, depth + 1))
                            {
                                yield return childElement;
                            }
                        }
                    }
                }
            }
        }

        public IEnumerable<IUIElement> GetElementsOfActiveWindow()
        {
            var root = AutomationElement.FromHandle(WindowsUtils.GetForegroundWindow());
            foreach (var element in EnumerateElements(root, 0))
            {
                AutomationUiElement? uiElement = null;
                try
                {
                    uiElement = new AutomationUiElement(element);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                if (uiElement != null)
                {
                    yield return uiElement;
                }
            }
        }

        public IEnumerable<IUIElement> GetSubElements(IUIElement rootElement)
        {
            if (rootElement is AutomationUiElement automationElement)
            {
                foreach (var element in EnumerateElements(automationElement.Element, 0))
                {
                    AutomationUiElement? uiElement = null;
                    try
                    {
                        uiElement = new AutomationUiElement(element);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                    if (uiElement != null)
                    {
                        yield return uiElement;
                    }
                }
            }
        }
    }
}
