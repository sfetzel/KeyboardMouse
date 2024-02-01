using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UiElement = System.Windows.Automation.AutomationElement;

namespace KeyboardMouseWin
{
    public class CaptionedElement
    {
        public UiElement UiElement { get; set; }

        public FrameworkElement Caption { get; set; }
    }
}
