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
        public static int CharacterCount { get; } = 26;
        public Dictionary<string, CaptionedElement> CurrentObjects { get; set; } = new();
        public int CharCount { get; set; }

        public void AddObjects(IEnumerable<CaptionedElement> elements)
        {
            CurrentObjects.Clear();
            // The count of characters we need for captioning is 26^k > #elements.
            CharCount = (int)Math.Ceiling(Math.Log10(elements.Count()) / Math.Log10(CharacterCount));
            var iterator = elements.GetEnumerator();
            var charIndices = new int[CharCount];

            while (iterator.MoveNext())
            {
                var element = iterator.Current;
                var caption = String.Join(string.Empty, charIndices.Select(x => (char)(x + 65)));
                CurrentObjects[caption] = element;

                charIndices[CharCount - 1] += 1;
                var i = CharCount - 1;
                while (charIndices[i] >= CharacterCount)
                {
                    charIndices[i] = 0;
                    --i;
                    charIndices[i] += 1;
                }
            }
        }

        public IEnumerable<KeyValuePair<string, CaptionedElement>> GetFilteredOut(char c, int index)
        {
            if (index >= CharCount)
            {
                return Enumerable.Empty<KeyValuePair<string, CaptionedElement>>();
            }
            return CurrentObjects.Where(pair => pair.Key[index] != c);
        }

    }
}
