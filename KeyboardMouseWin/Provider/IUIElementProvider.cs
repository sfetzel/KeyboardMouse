namespace KeyboardMouseWin.Provider
{
    public interface IUIElementProvider
    {
        // Todo: add children to IUIElement, add INotifyPropertyChanged
        // Give methods a cancellationtoken.
        // Methods should traverse the tree until no children are found.
        // when token is cancelled, the traverse should be canceled.
        public (IUIElement, Task) GetActiveWindowElement(CancellationToken token, Action<IEnumerable<IUIElement>>? onChildrenAdded);

        // this method will do the same as above, just change the children property
        // of the provided element. the action can be cancelled.
        public Task AddDescendants(IUIElement element, CancellationToken token, Action<IEnumerable<IUIElement>>? onChildrenAdded);
    }
}
