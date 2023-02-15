

// ReSharper disable UnusedTypeParameter

namespace SourceGenerators.ReactiveCommands.Core;
/// <summary>
/// Attribute to mark a method to generate a ReactiveCommand
/// Example:
/// <code>public async Task OneWayAsync();</code>
/// will generate:
/// <code><![CDATA[ public ReactiveUI.ReactiveCommand<Unit, Unit> OneWayCommand => ReactiveUI.ReactiveCommand.CreateFromTask(OneWayAsync); ]]></code>
///
/// Supports async and sync method with or without parameter and with or without return type
/// Async suffix is automatically removed
/// Currently only supports Task and <![CDATA[Task<T>]]> return types for async methods
///
/// Example:
/// <code><![CDATA[ public async Task<SomeImportantResponse> RequestAsync(SomeImportantRequest request); ]]></code>
/// will generate:
/// <code><![CDATA[ public ReactiveUI.ReactiveCommand<SomeImportantRequest, SomeImportantResponse> RequestCommand => ReactiveUI.ReactiveCommand.CreateFromTask<SomeImportantRequest, SomeImportantResponse>(RequestAsync); ]]></code>
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ReactiveCommandAttribute : Attribute
{
    
}