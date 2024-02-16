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
        public HashSet<ushort> KeyCodes { get; set; } = new();

        public KeyCombination() { }

        public static KeyCombination FromString(string inputString)
        {
            var keyCodeStrings = inputString.Split(KeyCodeSeparator);
            return new()
            {
                KeyCodes = keyCodeStrings.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => Convert.ToUInt16(x)).ToHashSet()
            };

        }

        public bool IsPressed(HashSet<ushort> pressedKeys) => KeyCodes.IsSubsetOf(pressedKeys);

        public override string ToString() => String.Join(KeyCodeSeparator, KeyCodes);
    }
}
