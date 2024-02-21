using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KeyboardMouseWin.Test
{
    [TestClass]
    public class KeyCombinationTest
    {
        [TestMethod]
        public void TestToString()
        {
            var keyCombination = new KeyCombination() { Keys = new() { Key.Escape, Key.LeftCtrl, Key.A } };
            var actual = keyCombination.ToString();
            var expected = "Escape,LeftCtrl,A";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestFromString()
        {
            var input = "Escape,A,, ,,LeftCtrl";
            var result = KeyCombination.FromString(input);
            Assert.AreEqual(3, result.Keys.Count);
            Assert.IsTrue(result.Keys.Contains(Key.Escape));
            Assert.IsTrue(result.Keys.Contains(Key.A));
            Assert.IsTrue(result.Keys.Contains(Key.LeftCtrl));
        }
    }
}
