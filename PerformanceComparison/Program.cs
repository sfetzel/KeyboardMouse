// See https://aka.ms/new-console-template for more information
//using FlaUI.Core.Conditions;
//using FlaUI.UIA2;
//using FlaUI.UIA3;
using System.Diagnostics;
using UIAutomationClient;
using System.Linq;

//Console.WriteLine("Hello, World!");

//using (var flauiAutomation = new UIA3Automation())
//{
//    var sw = new Stopwatch();
//    sw.Start();
//    var desktop = flauiAutomation.GetDesktop();
//    var windows = desktop.FindAllChildren();
//    var vs = windows.First(x => x.Name.Contains("Visual Studio"));
//    // makes no difference if a condition is applied.
//    //var allChildren = vs.FindAllDescendants(); //factory => factory.ByControlType(FlaUI.Core.Definitions.ControlType.Button));
//    var allChildren = vs.FindAllDescendants();
//    vs.FindAllDescendants();
//    Console.WriteLine(allChildren.Length.ToString());
//    sw.Stop();
//    Console.WriteLine($"Took {sw.ElapsedMilliseconds}ms");
    
//}

CUIAutomation automation = new CUIAutomation();
var stopwatch = new Stopwatch();
stopwatch.Start();
var root = automation.GetRootElement();
var children = root.FindAll(TreeScope.TreeScope_Children, automation.CreateTrueCondition());
Console.WriteLine(children.Length);
for(var i = 0; i < children.Length; ++i)
{
    var child = children.GetElement(i);
    var name = child.CurrentName;
    if (name.Contains("Visual Studio"))
    {
        Console.WriteLine($"Found Visual studio: {name}");
        //var descendants = child.FindAllBuildCache(TreeScope.TreeScope_Descendants, automation.CreateTrueCondition(), automation.CreateCacheRequest());
        var descendants = child.FindAll(TreeScope.TreeScope_Descendants, automation.CreateTrueCondition());
        Console.WriteLine(descendants.Length);
    }
}
stopwatch.Stop();
Console.WriteLine($"Took {stopwatch.ElapsedMilliseconds}ms");