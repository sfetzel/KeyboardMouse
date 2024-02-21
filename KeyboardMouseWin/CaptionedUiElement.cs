﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardMouseWin
{
    public class CaptionedUiElement
    {
        public string Caption { get; set; }

        public IUIElement UiElement { get; set; }

        public CaptionedUiElement(string caption, IUIElement uiElement)
        {
            Caption = caption;
            UiElement = uiElement;
        }
    }
}
