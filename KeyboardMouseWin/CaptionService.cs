using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace KeyboardMouseWin
{
    /// <summary>
    /// Manages captions.
    /// </summary>
    public class CaptionService
    {
        public Dictionary<string, IUIElement> CurrentObjects { get; set; } = new();

        public void SetObjects(IEnumerable<IUIElement> elements, string prefix = "")
        {
            CurrentObjects = CaptionHelper.CreateCaptions(CurrentObjects.Values.Concat(elements));
        }

        public IEnumerable<KeyValuePair<string, IUIElement>> GetFilteredOut(char c, int index)
        {
            return CurrentObjects.Where(pair => pair.Key.Length > index && pair.Key[index] != c);
        }

        /// <summary>
        /// Remove all elements with the specified keys.
        /// </summary>
        /// <param name="keys"></param>
        public void Remove(IEnumerable<string> keys)
        {
            foreach (var key in keys)
            {
                CurrentObjects.Remove(key);
            }
        }
    }
}
