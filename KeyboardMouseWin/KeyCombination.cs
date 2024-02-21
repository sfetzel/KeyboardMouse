using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KeyboardMouseWin
{
    public class KeyCombination
    {
        public static char KeyCodeSeparator { get; } = ',';
        /// <summary>
        /// The codes of the keys for this key combination.
        /// </summary>
        public HashSet<Key> Keys { get; set; } = new();

        public KeyCombination() { }

        public static KeyCombination FromString(string inputString)
        {
            var keyStrings = inputString.Split(KeyCodeSeparator);
            
            return new()
            {
                Keys = keyStrings.Where(x => !string.IsNullOrWhiteSpace(x)).
                    Select(x => (Key)Enum.Parse(typeof(Key), x)).ToHashSet()
            };

        }

        public bool IsPressed(HashSet<Int32> pressedKeys) => Keys.Select(x => (int)x).ToHashSet().IsSubsetOf(pressedKeys);

        public bool IsPressed(HashSet<Key> pressedKeys) => Keys.IsSubsetOf(pressedKeys);

        public override string ToString() => String.Join(KeyCodeSeparator, Keys);
    }
}
