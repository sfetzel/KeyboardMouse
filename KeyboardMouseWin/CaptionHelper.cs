using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KeyboardMouseWin
{
    public static class CaptionHelper
    {
        public static int CharacterCount { get; } = 26;

        /// <summary>
        /// Creates a unique caption with letters from A-Z for each element in the specified list.
        /// </summary>
        /// <typeparam name="T">The type of the objects to caption.</typeparam>
        /// <param name="elements">The elements to captions.</param>
        /// <param name="prefix">(Optional) A prefix which is added in front of each caption.</param>
        /// <returns>A dictionary which maps each unique caption to an element in the provided list.</returns>
        public static Dictionary<string, T> CreateCaptions<T>(IEnumerable<T> elements, string prefix = "")
        {
            var elementCount = elements.Count();
            var elementsWithCaptions = new Dictionary<string, T>();
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
                    elementsWithCaptions[caption] = element;

                    charIndices[charCount - 1] += 1;
                    var i = charCount - 1;
                    while (i >= 0 && charIndices[i] >= CharacterCount)
                    {
                        charIndices[i] = 0;
                        --i;
                        if (i >= 0)
                        {
                            charIndices[i] += 1;
                        }
                    }
                }
            }
            return elementsWithCaptions;
        }
    }
}
