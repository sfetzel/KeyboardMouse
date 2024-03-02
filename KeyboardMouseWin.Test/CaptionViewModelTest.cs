﻿using KeyboardMouseWin.Provider;
using KeyboardMouseWin.Service;
using System.Diagnostics;
using System.Drawing;

namespace KeyboardMouseWin.Test
{
    [TestClass]
    public class CaptionViewModelTest
    {
        public class MockUiElement : IUIElement
        {
            public Rectangle BoundingRectangle { get; set; }

            public System.Windows.Point? ClickPoint { get; set; }
        }

        public class MockElementProvider : IUIElementProvider
        {
            public static int elementCount = 8;
            public IEnumerable<IUIElement> GetElementsOfActiveWindow()
            {
                for (var i = 0; i < elementCount; ++i)
                {
                    yield return new MockUiElement();
                }
            }

            private int waitingTimeMs = 10;

            public IEnumerable<IUIElement> GetSubElements(IUIElement element)
            {
                Thread.Sleep(waitingTimeMs);
                for (var i = 0; i < elementCount; ++i)
                {
                    yield return new MockUiElement();
                }
                waitingTimeMs = 5000;
            }
        }

        /// <summary>
        /// Tests that the method CaptionUiElements takes no longer than the
        /// time limit to complete.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task TestCaptionUiElements_ShouldNotExceedTimeLimit()
        {
            var provider = new MockElementProvider();
            var viewModel = new CaptionViewModel(new CaptionService(), provider, System.Windows.Threading.Dispatcher.CurrentDispatcher);

            await viewModel.CaptionUiElements();
            viewModel.Clear();
            var watch = new Stopwatch();
            watch.Start();
            await viewModel.CaptionUiElements();
            watch.Stop();
            Assert.IsTrue(watch.ElapsedMilliseconds < viewModel.CaptionTimeLimit + 30);
            // 8 elements from GetElementsOfActiveWindow() and for each we again have 8 elements from
            // GetSubElements(), therefore the expected count of elements is 8*8 = 64.
            Assert.AreEqual(Math.Pow(MockElementProvider.elementCount, 2), viewModel.CaptionService.CurrentObjects.Count);
        }
        [TestMethod]
        public async Task CaptionUiElements_ShouldReturnAfterSpecifiedCancellationTokenTimeout()
        {
            // Arrange
            var elementProviderMock = new MockElementProvider();
            var elementLookupService = new ElementLookupService(elementProviderMock);
            var expectedMillisecondsTimeout = 2000;
            var MilliseoncdsTimeoutTolerance = 200;
            var totalAllowedMilliseconds = expectedMillisecondsTimeout + MilliseoncdsTimeoutTolerance;
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(expectedMillisecondsTimeout));
            var trackedItems = new List<IUIElement>();

            // Act
            var watch = new Stopwatch();
            watch.Start();
            await elementLookupService.CaptionUiElementsAsync(elementProviderMock.GetElementsOfActiveWindow(), (a) => trackedItems.AddRange(a), ct: cancellationTokenSource.Token); ;
            watch.Stop();

            // Assert
            Assert.IsTrue(watch.ElapsedMilliseconds < totalAllowedMilliseconds, $"Method execution took {watch.ElapsedMilliseconds}ms but only {totalAllowedMilliseconds}ms than allowed");
            Assert.IsTrue(trackedItems.Count > 0);
            
        }
    }
}
