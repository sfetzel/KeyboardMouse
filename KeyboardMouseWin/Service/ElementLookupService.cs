using KeyboardMouseWin.Provider;
using KeyboardMouseWin.Utils;
using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KeyboardMouseWin.Service
{
    public class ElementLookupService : IElementLookupService
    {
        private readonly IUIElementProvider uIElementProvider;

        public ElementLookupService(IUIElementProvider uIElementProvider)
        {
            this.uIElementProvider = uIElementProvider;
        }
        CancellationToken cancellationToken = new CancellationToken();
        private int maxdepth = 10;
        public async Task CaptionUiElementsAsync(IEnumerable<IUIElement> startingElements, Action<IEnumerable<IUIElement>>? elementsAddedAction = null, CancellationToken ct = default)
        {
            try
            {
                cancellationToken = ct;
                var updateTask = UpdateElements(new ConcurrentBag<IUIElement>(), startingElements, elementsAddedAction);
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
        private async Task UpdateElements(ConcurrentBag<IUIElement> uIElements, IEnumerable<IUIElement> newElements, Action<IEnumerable<IUIElement>>? elementsAddedAction = null)
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
                    //if (cancellationToken.IsCancellationRequested) return;
                    await UpdateElements(uIElements, subelements, elementsAddedAction);
                }, cancellationToken);
            }
        }
    }
}
