using KeyboardMouseWin.Provider;
using KeyboardMouseWin.Utils;
using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KeyboardMouseWin.Service
{
    public class LegacyElementLookupService : IElementLookupService
    {
        private readonly IUIElementProvider uIElementProvider;
        private readonly Func<LimitedTimeExecutor> limitedTimeExecutorBuilder;

        public LegacyElementLookupService(IUIElementProvider uIElementProvider, Func<LimitedTimeExecutor> limitedTimeExecutorBuilder)
        {
            this.uIElementProvider = uIElementProvider;
            this.limitedTimeExecutorBuilder = limitedTimeExecutorBuilder;
        }
        public async Task<IEnumerable<IUIElement>> CaptionUiElementsAsync(IEnumerable<IUIElement> startingElements, Action<IEnumerable<IUIElement>>? elementsAddedAction = null, CancellationToken ct = default)
        {
            var elements = new ConcurrentBag<IUIElement>();
            var executor = limitedTimeExecutorBuilder();
            async void UpdateElements(IEnumerable<IUIElement> newElements)
            {
                foreach (var element in newElements)
                {
                    elements.Add(element);
                }
                elementsAddedAction?.Invoke(elements);
                Debug.WriteLine($"Adding objects at {executor.Stopwatch.ElapsedMilliseconds}");
                if (!executor.IsOverLimit)
                {
                    foreach (var element in newElements)
                    {
                        executor.StartNewTask(() => UpdateElements(uIElementProvider.GetSubElements(element)));
                    }
                }
            }
            await executor.Run(() => UpdateElements(startingElements), true);

            return elements;
        }
    }
}
