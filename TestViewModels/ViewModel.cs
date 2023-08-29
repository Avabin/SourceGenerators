using System.Reactive.Disposables;
using ReactiveUI;

namespace TestViewModels;

public abstract class ViewModel : ReactiveObject, IViewModel, IActivatableViewModel
{
    public ViewModelActivator Activator { get; } = new();

    protected ViewModel()
    {
        this.WhenActivated(OnActivate);
    }

    protected virtual void OnActivate(CompositeDisposable d)
    {
        
    }
}

public abstract class RoutableViewModel : ViewModel, IRoutableViewModel
{
    protected RoutableViewModel(IScreen screen) : base()
    {
        HostScreen = screen;
    }

    public abstract string? UrlPathSegment { get; }
    public IScreen HostScreen { get; }
}