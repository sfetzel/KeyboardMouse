using KeyboardMouseWin.Provider;
using System.Collections.Concurrent;

namespace KeyboardMouseWin.Service
{
    public class ElementLookupService : IElementLookupService
    {
        private readonly IUIElementProvider uIElementProvider;

        public ElementLookupService(IUIElementProvider uIElementProvider)
        {
            this.uIElementProvider = uIElementProvider;
        }
        public async Task<IEnumerable<IUIElement>> CaptionUiElementsAsync(IEnumerable<IUIElement> startingElements, Action<IEnumerable<IUIElement>>? elementsAddedAction = null, CancellationToken ct = default)
        {
            var elements = new ConcurrentBag<IUIElement>();
            try
            {
                var updateTask = Task.Run(() => UpdateElementsAsync(elements, startingElements, ct, elementsAddedAction), ct);
                var timeoutTask = Task.Delay(Timeout.Infinite, ct);

                _ = await Task.WhenAny(updateTask, timeoutTask);
            }
            catch (TaskCanceledException)
            {
                // catch Cancellation token exception from Task.Run() and return
            }
            return elements;
        }
        private async Task UpdateElementsAsync(ConcurrentBag<IUIElement> uIElements, IEnumerable<IUIElement> newElements, CancellationToken cancellationToken, Action<IEnumerable<IUIElement>>? elementsAddedAction = null)
        {
            if (cancellationToken.IsCancellationRequested) return;
            for (int i = 0; i < newElements.Count(); i++)
            {
                var element = newElements.ElementAt(i);
                uIElements.Add(element);
            }

            elementsAddedAction?.Invoke(uIElements);
            // Cancel after new elements have been added (otherwise elements belonging to one
            // layer may be missing).
            if (cancellationToken.IsCancellationRequested) return;
            for (int i = 0; i < newElements.Count(); i++)
            {
                var element = newElements.ElementAt(i);
                await Task.Run(async () =>
                {
                    var subelements = uIElementProvider.GetSubElements(element);
                    if (cancellationToken.IsCancellationRequested) return;
                    await UpdateElementsAsync(uIElements, subelements, cancellationToken, elementsAddedAction);
                }, cancellationToken);
            }
        }
    }
}
