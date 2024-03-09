using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KeyboardMouseWin.Utils
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
        public static Dictionary<string, T> CreateCaptions<T>(IEnumerable<T> elements, string prefix = "", Dictionary<string, T>? existingCaptions = null)
        {
            var elementCount = elements.Count();
            var elementsWithCaptions = new Dictionary<string, T>();
            var charCount = 0;
            if (elementCount > 0)
            {
                var existingDictionaryCharCount = existingCaptions != null && existingCaptions.Count > 0 ? existingCaptions?.Keys.Select(x => x.Length).Max() : 0;
                // The count of characters we need for captioning is 26^k > #elements.
                charCount = GetMinimumCharacterLength(elementCount + (existingCaptions?.Count ?? 0));
                IEnumerator<T> iterator;
                var charIndices = new int[charCount];
                if (existingDictionaryCharCount < charCount || existingCaptions == null)
                {
                    var elementsToCaption = elements;
                    if(existingCaptions != null)
                    {
                        elementsToCaption = elements.Concat(existingCaptions.Values);
                    }
                    iterator = elementsToCaption.GetEnumerator();
                }
                else
                {
                    elementsWithCaptions = existingCaptions;
                    iterator = elements.Except(existingCaptions.Values).GetEnumerator();
                    while (existingCaptions.ContainsKey(IndicesToString(prefix, charIndices)))
                    {
                        NextCaption(charCount, charIndices);
                    }
                }

                while (iterator.MoveNext())
                {
                    var element = iterator.Current;
                    string caption = IndicesToString(prefix, charIndices);
                    elementsWithCaptions[caption] = element;

                    NextCaption(charCount, charIndices);
                }
            }
            return elementsWithCaptions;
        }

        private static string IndicesToString(string prefix, int[] charIndices)
        {
            return prefix + string.Join(string.Empty, charIndices.Select(x => (char)(x + 65)));
        }

        private static void NextCaption(int charCount, int[] charIndices)
        {
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

        public static int GetMinimumCharacterLength(int elementCount)
        {
            return elementCount > 1 ? (int)Math.Ceiling(Math.Log10(elementCount) / Math.Log10(CharacterCount)) : 1;
        }
    }
}
