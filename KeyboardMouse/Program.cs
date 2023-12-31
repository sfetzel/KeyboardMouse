// See https://aka.ms/new-console-template for more information
using Gma.System.MouseKeyHook;
using System.Windows.Automation;

Console.WriteLine("Hello, World!");

var root = AutomationElement.RootElement;

var globalHook = Hook.GlobalEvents();

globalHook.KeyPress += GlobalHook_KeyPress;

void GlobalHook_KeyPress(object? sender, System.Windows.Forms.KeyPressEventArgs e)
{
    Console.WriteLine($"Key pressed: {e.KeyChar}");
}

void EnumerateElements(AutomationElement element, int depth = 0)
{
    if(depth > 30)
    {
        return;
    }
    var result = element.FindAll(TreeScope.Children, Condition.TrueCondition);

    foreach (AutomationElement child in result)
    {
        if (!child.Current.IsOffscreen)
        {
            Console.WriteLine(child.Current.ItemType);
            Console.WriteLine(child.Current.Name);
            EnumerateElements(child, depth + 1);
        }
    }
}

EnumerateElements(root, 0);