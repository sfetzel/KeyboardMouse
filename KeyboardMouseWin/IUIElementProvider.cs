using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace KeyboardMouseWin
{
    public interface IUIElementProvider
    {
        public IAsyncEnumerable<IUIElement> GetElementsOfActiveWindow();

        public IAsyncEnumerable<IUIElement> GetSubElements(IUIElement element);
    }
}
