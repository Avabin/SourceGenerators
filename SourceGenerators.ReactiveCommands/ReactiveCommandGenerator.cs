using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerators.ReactiveCommands;

[Generator]
public class ReactiveCommandGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
#if DEBUG
        if (!Debugger.IsAttached)
        {
            // uncomment this line to debug the generator
            Debugger.Launch();
        }
#endif
    }

    public void Execute(GeneratorExecutionContext context)
    {
        // get all classes
        var classes = context.Compilation.SyntaxTrees
            .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>());

        // get all classes with methods marked with [ReactiveCommand]
        var classesWithReactiveCommand = classes
            .Where(c => c.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .Any(m => m.AttributeLists.Any(a =>
                    a.Attributes.Any(at => at.Name.ToString().StartsWith("ReactiveCommand")))));

        // for every method marked with [ReactiveCommand<TParam, TResult>] generate a property of type ReactiveCommand<TParam,TResult>
        // with a name consisting of method name (without "Async", if present) and "Command" suffix
        // TParam and TResult are inferred from [ReactiveCommand<TParam, TResult>] attribute
        // example:
        // [ReactiveCommand<int, string>]
        // public async Task<string> SomeMethodAsync(int param) { ... }
        // should generate a property:
        // public ReactiveCommand<int, string> SomeMethodCommand { get; }


        // for every class with methods marked with [ReactiveCommand] generate a partial class with a property for each method
        foreach (var classWithReactiveCommand in classesWithReactiveCommand)
        {
            var className = classWithReactiveCommand.Identifier.Text;
            // get namespace
            var ns = classWithReactiveCommand.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().Single().Name
                .ToString();
            // get all methods marked with [ReactiveCommand]
            var methods = classWithReactiveCommand.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .Where(m => m.AttributeLists.Any(a =>
                    a.Attributes.Any(at => at.Name.ToString().StartsWith("ReactiveCommand"))));
            var properties = methods.Select(m => // for every method
            {
                var methodName = m.Identifier.Text; // get method name
                var tParam = m.ParameterList.Parameters.FirstOrDefault()?.Type?.ToString() ?? "Unit"; // get TParam
                var isAsync = methodName.EndsWith("Async"); // check if method is async
                var tResult = m.ReturnType?.ToString() ?? (isAsync ? "Task" : "Unit"); // get TResult and default to Task if method is async
                return ReactiveCommandTemplate.RenderProperty(methodName, tParam, tResult); // render property
            });
            var source = ReactiveCommandTemplate.RenderClass(ns, className, properties); // render class with properties
            context.AddSource($"{className}.ReactiveCommands.cs", source); // add source file to compilation
        }
    }
}