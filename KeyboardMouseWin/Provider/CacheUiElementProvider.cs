using KeyboardMouseWin.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardMouseWin.Provider
{
    internal class CacheUiElementProvider : IUIElementProvider
    {
        private IUIElementProvider baseProvider;

        private Dictionary<nint, IUIElement> cachedResults = new();
        private Dictionary<nint, List<IUIElement>> cachedChildren = new();
        Thread workerThread;
        private CancellationTokenSource tokenSource = new();

        public CacheUiElementProvider(IUIElementProvider baseProvider)
        {
            this.baseProvider = baseProvider;
            workerThread = new(WorkingMethod);
            workerThread.Start();
        }

        private async void WorkingMethod()
        {
            while (true)
            {
                var handle = WindowsUtils.GetForegroundWindow();
                cachedChildren[handle] = new();
                var (element, task) = baseProvider.GetActiveWindowElement(tokenSource.Token, (newChildren) => cachedChildren[handle].AddRange(newChildren));
                cachedResults[handle] = element;
                await task;
            }
        }

        public (IUIElement, Task) GetActiveWindowElement(CancellationToken token, Action<IEnumerable<IUIElement>>? onChildrenAdded)
        {
            var handle = WindowsUtils.GetForegroundWindow();
            onChildrenAdded?.Invoke(cachedChildren[handle]);
            return (cachedResults[handle], Task.FromResult(true));
        }

        public async Task AddDescendants(IUIElement element, CancellationToken token, Action<IEnumerable<IUIElement>>? onChildrenAdded)
         => await Task.FromResult(true); //await baseProvider.AddDescendants(element, token, onChildrenAdded); 
    }
}
