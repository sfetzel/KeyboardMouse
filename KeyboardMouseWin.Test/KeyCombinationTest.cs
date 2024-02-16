using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardMouseWin.Test
{
    [TestClass]
    public class KeyCombinationTest
    {
        [TestMethod]
        public void TestToString()
        {
            var keyCombination = new KeyCombination() { KeyCodes = new() { 12, 45, 9 } };
            var actual = keyCombination.ToString();
            var expected = "12,45,9";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestFromString()
        {
            var input = "12,9,, ,,8";
            var result = KeyCombination.FromString(input);
            Assert.AreEqual(3, result.KeyCodes.Count);
            Assert.IsTrue(result.KeyCodes.Contains(12));
            Assert.IsTrue(result.KeyCodes.Contains(9));
            Assert.IsTrue(result.KeyCodes.Contains(8));
        }
    }
}
