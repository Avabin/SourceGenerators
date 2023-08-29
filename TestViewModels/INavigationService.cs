using ReactiveUI;

namespace TestViewModels;

public interface INavigationService
{
    void RegisterRouter(RoutingState router);
    
    IObservable<IRoutableViewModel> Push<TViewModel>() where TViewModel : IRoutableViewModel;
    IObservable<IRoutableViewModel?> Pop();
}