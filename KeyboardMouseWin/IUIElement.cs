using System.Drawing;

namespace KeyboardMouseWin
{
    public interface IUIElement
    {
        public Rectangle BoundingRectangle { get; }
        public System.Windows.Point? ClickPoint { get; }
    }
}
