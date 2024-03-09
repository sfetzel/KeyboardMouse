
namespace KeyboardMouseWin.Service
{
    public interface IElementLookupService
    {
        Task<IEnumerable<IUIElement>> CaptionUiElementsAsync(IEnumerable<IUIElement> startingElements, Action<IEnumerable<IUIElement>>? elementsAddedAction = null, CancellationToken ct = default);
    }
}