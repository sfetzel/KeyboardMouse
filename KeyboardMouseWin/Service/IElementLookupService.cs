
namespace KeyboardMouseWin.Service
{
    public interface IElementLookupService
    {
        Task CaptionUiElementsAsync(IEnumerable<IUIElement> startingElements, Action<IEnumerable<IUIElement>>? elementsAddedAction = null, CancellationToken ct = default);
    }
}