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
        public IEnumerable<IUIElement> GetElementsOfActiveWindow();

        public IEnumerable<IUIElement> GetSubElements(IUIElement element);
    }
}
