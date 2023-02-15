using System.Reactive;
using System.Reactive.Linq;
using SourceGenerators.ReactiveCommands.Core;

namespace GenerationTest;

public partial class ViewModel
{
    [ReactiveCommand]
    private Task<int> GetLengthAsync(string s) => Task.FromResult(s.Length);
    
    [ReactiveCommand]
    private Task DoSomethingAsync() => Task.CompletedTask;
    
    [ReactiveCommand]
    private string GetName() => "John";
    
    [ReactiveCommand]
    private string GetGreeting(string name) => "hello " + name;
    public ViewModel()
    {
        // GetLengthCommand.Select(x => $"The length is {x}").Subscribe(Console.WriteLine);
        // DoSomethingCommand.Select(x => $"Something").Subscribe(Console.WriteLine);
        // GetNameCommand.Select(x => $"Name is {x}").Subscribe(Console.WriteLine);
        // GetGreetingCommand.Subscribe(Console.WriteLine);
    }
}