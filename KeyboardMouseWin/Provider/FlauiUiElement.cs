using FlaUI.Core.AutomationElements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardMouseWin
{
    internal class FlauiUiElement : IUIElement, INotifyPropertyChanged
    {
        private AutomationElement element;

        public AutomationElement Element
        {
            get { return element; }
            private set { element = value;
                RaisePropertyChanged(nameof(Element));
            }
        }

        private Rectangle boundingRectangle;

        public Rectangle BoundingRectangle
        {
            get { return boundingRectangle; }
            private set { boundingRectangle = value;
                RaisePropertyChanged(nameof(BoundingRectangle));
            }
        }


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

        private IEnumerable<IUIElement> children;

        public IEnumerable<IUIElement> Children
        {
            get { return children; }
            set { children = value;
                RaisePropertyChanged(nameof(Children));
            }
        }


        public FlauiUiElement(AutomationElement element)
        {
            Element = element;
            BoundingRectangle = Element.BoundingRectangle;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void RaisePropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new(propertyName));
    }
}
