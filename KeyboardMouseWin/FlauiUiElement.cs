using FlaUI.Core.AutomationElements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardMouseWin
{
    internal class FlauiUiElement : IUIElement
    {
        public AutomationElement Element {get; private set;}

        public Rectangle BoundingRectangle => Element.BoundingRectangle;

        public System.Windows.Point? ClickPoint
        {
            get
            {
                if (Element.TryGetClickablePoint(out var clickablePoint) &&
                    !clickablePoint.IsEmpty)
                {
                    return new System.Windows.Point(clickablePoint.X, clickablePoint.Y);
                }
                return null;
            }
        }

        public FlauiUiElement(AutomationElement element)
        {
            Element = element;
        }
    }
}
