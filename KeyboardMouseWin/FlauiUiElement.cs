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
        private AutomationElement element;

        public Rectangle BoundingRectangle => element.BoundingRectangle;

        public System.Windows.Point? ClickPoint
        {
            get
            {
                if (element.TryGetClickablePoint(out var clickablePoint) &&
                    !clickablePoint.IsEmpty)
                {
                    return new System.Windows.Point(clickablePoint.X, clickablePoint.Y);
                }
                return null;
            }
        }

        public FlauiUiElement(AutomationElement element)
        {
            this.element = element;
        }
    }
}
