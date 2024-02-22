namespace KeyboardMouseWin.Provider
{
    public interface IUIElementProvider
    {
        public IEnumerable<IUIElement> GetElementsOfActiveWindow();

        public IEnumerable<IUIElement> GetSubElements(IUIElement element);
    }
}
