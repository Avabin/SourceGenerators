using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerators.Orleans;

[Generator]
public class SurrogatesSourceGenerator : ISourceGenerator
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
        // find all classes with [GenerateSurrogates] attribute
        
        var classes = context.Compilation.SyntaxTrees
            .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>());
        
        var classesWithSurrogates = classes
            .Where(c => c.AttributeLists.Any(a =>
                a.Attributes.Any(at => at.Name.ToString().StartsWith("GenerateSurrogates"))));

        foreach (var classWithSurrogates in classesWithSurrogates)
        {
            // read attribute `Namespace` property
            var ns = classWithSurrogates.AttributeLists
                .SelectMany(a => a.Attributes)
                .Where(a => a.Name.ToString().StartsWith("GenerateSurrogates"))
                .Select(a => a.ArgumentList.Arguments[0].Expression.ToString())
                .Single();
        }
    }
}