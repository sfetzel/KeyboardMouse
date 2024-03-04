using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardMouseWin.TestApp
{
    public class TestViewModel
    {
        public static int ElementCount { get; } = 10;

        public static int Depth { get; } = 1;

        public List<TestObject> UiElements { get; set; } = Enumerable.Range(0, ElementCount).Select((index) => new TestObject(index, Depth)).ToList();
    }
}
