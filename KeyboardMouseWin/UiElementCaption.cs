using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace KeyboardMouseWin
{
    public class UiElementCaption
    {
        public TextBlock TextCaption { get; set; }
        public Rectangle RectangleCaption { get; set; }

        public UiElementCaption(TextBlock textCaption, Rectangle rectangleCaption)        {
            TextCaption = textCaption;
            RectangleCaption = rectangleCaption;
        }

    }
}
