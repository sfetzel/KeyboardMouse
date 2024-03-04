using KeyboardMouseWin.Provider;
using KeyboardMouseWin.Service;
using KeyboardMouseWin.TestApp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace KeyboardMouseWin.Test
{
    [TestClass]
    public class ElementLookupServiceIntegrationTest
    {
        IUIElementProvider? provider;
        TestAppWindow? window;
        IElementLookupService? service;
        int timeLimit = 2000;

        [TestInitialize]   
        public void Initialize()
        {
            window = new TestAppWindow();
            window.Show();
            var handle = new WindowInteropHelper(window).Handle;
            provider = new FlauiProvider(handle);
            service = new ElementLookupService(provider);
            //service = new LegacyElementLookupService(provider, () => new Utils.LimitedTimeExecutor(timeLimit));
        }

        [TestCleanup]
        public void Cleanup()
        {
            window?.Hide();
        }

        [STATestMethod]
        public async Task TestPerformance()
        {
            ArgumentNullException.ThrowIfNull(service);
            ArgumentNullException.ThrowIfNull(provider);
            IEnumerable<IUIElement>? cachedElements = null;
            var tokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeLimit));
            var stopwatch = Stopwatch.StartNew();
            await service.CaptionUiElementsAsync(provider.GetElementsOfActiveWindow(), (elements) => cachedElements = elements, tokenSource.Token);
            stopwatch.Stop();
            // Actually there are 100 elements.
            Assert.IsNotNull(cachedElements);
            var expectedCount = TestViewModel.ElementCount;
            var actualCount = cachedElements.Count();
            Assert.IsTrue(actualCount > expectedCount, $"Expected {expectedCount}, having {actualCount}");
        }
    }
}
