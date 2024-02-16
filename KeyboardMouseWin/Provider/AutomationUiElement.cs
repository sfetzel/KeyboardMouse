using System.Drawing;
using System.Windows.Automation;

namespace KeyboardMouseWin.Provider
{
    public class AutomationUiElement : IUIElement
    {
        public Rectangle BoundingRectangle { get; private set; }
        public System.Windows.Point? ClickPoint { get; private set; }

        public AutomationElement Element { get; set; }

        public AutomationUiElement(AutomationElement element)
        {
            Element = element;
            BoundingRectangle = new Rectangle((int)element.Current.BoundingRectangle.X,
                (int)element.Current.BoundingRectangle.Y,
                (int)element.Current.BoundingRectangle.Width,
                (int)element.Current.BoundingRectangle.Height);

            element.TryGetClickablePoint(out var clickablePoint);
            if (clickablePoint.X != 0 && clickablePoint.Y != 0)
            {
                ClickPoint = clickablePoint;
            }
        }
    }
}
