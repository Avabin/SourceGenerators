using System.Reactive.Linq;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace TestViewModels;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    private RoutingState _router = null!;
    public void RegisterRouter(RoutingState router)
    {
        _router = router;
    }

    public IObservable<IRoutableViewModel> Push<TViewModel>() where TViewModel : IRoutableViewModel
    {
        var vm = _serviceProvider.GetRequiredService<TViewModel>();
        return _router.Navigate.Execute(vm).ObserveOn(RxApp.MainThreadScheduler).SubscribeOn(RxApp.MainThreadScheduler);
    }
    
    public IObservable<IRoutableViewModel?> Pop() => 
        _router.NavigateBack.Execute().SubscribeOn(RxApp.MainThreadScheduler);
}