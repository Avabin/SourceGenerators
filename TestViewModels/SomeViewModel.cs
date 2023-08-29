using ReactiveUI;

namespace TestViewModels;

public class SomeViewModel : ViewModel, IRoutableViewModel
{
    public string? UrlPathSegment { get; } = "SomeViewModel";
    public IScreen HostScreen { get; }

    public SomeViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}