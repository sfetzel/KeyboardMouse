using System.Linq;

namespace KeyboardMouseWin.TestApp
{
    public class TestObject
    {
        public int Index { get; set; }

        public List<TestObject> SubElements { get; set; }

        public TestObject(int index, int depth = 0)
        {
            Index = index;
            if(depth > 0)
            {
                SubElements = Enumerable.Range(0, TestViewModel.ElementCount).Select(index => new TestObject(index, depth - 1)).ToList();
            }
        }
    }
}