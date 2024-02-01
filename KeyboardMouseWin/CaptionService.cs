﻿using System;
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
        public Dictionary<string, IUIElement> CurrentObjects { get; set; } = new();

        public IEnumerable<string> AddObjects(IEnumerable<IUIElement> elements, string prefix = "")
        {
            var addedKeys = new List<string>();
            var elementCount = elements.Count();
            if (elementCount > 0)
            {
                // The count of characters we need for captioning is 26^k > #elements.
                var charCount = elementCount > 1 ? (int)Math.Ceiling(Math.Log10(elementCount) / Math.Log10(CharacterCount)) : 1;
                var iterator = elements.GetEnumerator();
                var charIndices = new int[charCount];

                while (iterator.MoveNext())
                {
                    var element = iterator.Current;
                    var caption = prefix + String.Join(string.Empty, charIndices.Select(x => (char)(x + 65)));
                    CurrentObjects[caption] = element;
                    addedKeys.Add(caption);

                    charIndices[charCount - 1] += 1;
                    var i = charCount - 1;
                    while (charIndices[i] >= CharacterCount)
                    {
                        charIndices[i] = 0;
                        --i;
                        charIndices[i] += 1;
                    }
                }
            }
            return addedKeys;
        }

        public IEnumerable<KeyValuePair<string, IUIElement>> GetFilteredOut(char c, int index)
        {
            return CurrentObjects.Where(pair => pair.Key.Length > index && pair.Key[index] != c);
        }

    }
}
