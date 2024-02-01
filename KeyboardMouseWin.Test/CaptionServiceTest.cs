namespace KeyboardMouseWin.Test
{
    [TestClass]
    public class CaptionServiceTest
    {
        [TestMethod]
        public void TestAddObjects_WhenSmallList_ShouldUseOneLetter()
        {
            var element1 = new CaptionedElement();
            var element2 = new CaptionedElement();
            var elements = new[] { element1, element2 };
            var service = new CaptionService();
            service.AddObjects(elements);
            Assert.AreEqual(element1, service.CurrentObjects["A"]);
            Assert.AreEqual(element2, service.CurrentObjects["B"]);
        }

        [TestMethod]
        public void TestAddObjects_WhenListWith27Element_ShouldUseTwoLetters()
        {
            var elements = Enumerable.Range(0, 27).Select(_ => new CaptionedElement()).ToArray();
            var service = new CaptionService();
            service.AddObjects(elements);
            Assert.IsNotNull(service.CurrentObjects["BA"]);
        }

        [TestMethod]
        public void TestAddObjects_WhenVeryLargeList_ShouldUseFourLetters()
        {
            var elements = Enumerable.Range(0, 17576 + 1).Select(_ => new CaptionedElement()).ToArray();
            var service = new CaptionService();
            service.AddObjects(elements);
            Assert.IsNotNull(service.CurrentObjects["BAAA"]);
        }
    }
}