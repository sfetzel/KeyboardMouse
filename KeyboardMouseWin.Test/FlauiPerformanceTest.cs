using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using KeyboardMouseWin.TestApp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Window = System.Windows.Window;

namespace KeyboardMouseWin.Test
{
    [TestClass]
    public class FlauiPerformanceTest
    {
        Window window;
        nint windowHandle;

        [TestInitialize]
        public void Initialize()
        {
            window = new TestAppWindow();
            window.Show();
            windowHandle = new WindowInteropHelper(window).Handle;
        }

        [TestCleanup]
        public void Cleanup()
        {
            window.Hide();
        }

        [STATestMethod]
        public void TestTreeWalkerPerformance()
        {
            using var automation = new UIA3Automation();
            var window = automation.FromHandle(windowHandle);
            var walker = automation.TreeWalkerFactory.GetControlViewWalker();
            var stopwatch = Stopwatch.StartNew();
            var firstChild = walker.GetFirstChild(window);
            AutomationElement sibling = firstChild;
            
            while((sibling = walker.GetNextSibling(sibling)) != null)
            {
                Debug.WriteLine(sibling.BoundingRectangle);
            }
            stopwatch.Stop();
            Debug.WriteLine($"Took {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}
