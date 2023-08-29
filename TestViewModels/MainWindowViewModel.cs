using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SourceGenerators.ReactiveCommands.Core;

namespace TestViewModels;

public partial class MainWindowViewModel : ViewModel, IScreen
{
    private readonly INavigationService _navigationService;
    [Reactive] public int Count { get; set; }
    [Reactive] public string Greeting { get; set; } = "Hello World!";
    private const string GreetingTemplate = "Hello! You've pressed the button {0} times.";

    [ReactiveCommand]
    private void Increment() => Count++;

    protected override void OnActivate(CompositeDisposable d)
    {
        this.WhenValueChanged(x => x.Count)
            .Select(x => string.Format(GreetingTemplate, x))
            .BindTo(this, vm => vm.Greeting)
            .DisposeWith(d);
    }

    public MainWindowViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        _navigationService.RegisterRouter(Router);
    }

    [ReactiveCommand]
    private IObservable<IRoutableViewModel> NavigateToPhotoDetection()
    {
        return _navigationService.Push<SomeViewModel>();
    }
    
    [ReactiveCommand]
    private IObservable<IRoutableViewModel?> NavigateBack() => 
        _navigationService.Pop();

    public RoutingState Router { get; } = new();
}