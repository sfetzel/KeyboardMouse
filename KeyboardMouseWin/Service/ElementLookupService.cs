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
        public async Task CaptionUiElementsAsync(IEnumerable<IUIElement> startingElements, Action<IEnumerable<IUIElement>>? elementsAddedAction = null, CancellationToken ct = default)
        {
            try
            {
                var updateTask = UpdateElementsAsync(new ConcurrentBag<IUIElement>(), startingElements, ct, elementsAddedAction);
                var timeoutTask = Task.Delay(Timeout.Infinite, ct);

                var completedTask = await Task.WhenAny(updateTask, timeoutTask);

                if (completedTask == updateTask)
                {
                    // The updateTask completed first
                    // Add your logic here
                }
                else if (completedTask == timeoutTask)
                {
                    // The timeoutTask completed first
                    // Add your logic here
                }
            }
            catch (TaskCanceledException ex)
            {
                // catch Cancellation token exception from Task.Run() and return
            }

        }
        private async Task UpdateElementsAsync(ConcurrentBag<IUIElement> uIElements, IEnumerable<IUIElement> newElements, CancellationToken cancellationToken, Action<IEnumerable<IUIElement>>? elementsAddedAction = null)
        {
            if (cancellationToken.IsCancellationRequested) return;
            for (int i = 0; i < newElements.Count(); i++)
            {
                var element = newElements.ElementAt(i);
                uIElements.Add(element);
            }

            if (cancellationToken.IsCancellationRequested) return;
            elementsAddedAction?.Invoke(uIElements);
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
