using KeyboardMouseWin.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardMouseWin.Test
{
    [TestClass]
    public class CaptionHelperTest
    {
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(26)]
        [DataRow(27)]
        [DataRow(27 * 27)]
        public void TestGetMinimumCharacterLength(int elementCount)
        {
            var characterLength = CaptionHelper.GetMinimumCharacterLength(elementCount);
            var elementCountFromCharacterLength = Math.Pow(CaptionHelper.CharacterCount, characterLength);
            Assert.IsTrue(elementCountFromCharacterLength >= elementCount);
        }
    }
}
