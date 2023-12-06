using System.Diagnostics;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerators.Orleans.DataContract;

[Generator]
public class DataContractAssemblyGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        if (!Debugger.IsAttached)
        {
            // uncomment this line to debug the generator
            // Debugger.Launch();
        }
    }

    public void Execute(GeneratorExecutionContext context)
    {
        // get all classes
        var classes = context.Compilation.SyntaxTrees
            .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>());

        // get marker classes with GenerateSurrogatesAndConvertersAttribute
        var markerClasses = classes
            .Where(c => c.AttributeLists.Any(a =>
                a.Attributes.Any(at => at.Name.ToString().StartsWith("GenerateSurrogatesAndConverters"))));


        foreach (var markerClass in markerClasses)
        {
            var @namespace = markerClass.Ancestors().OfType<NamespaceDeclarationSyntax>()
                .Concat(markerClass.Ancestors().OfType<FileScopedNamespaceDeclarationSyntax>()
                    .Cast<BaseNamespaceDeclarationSyntax>()).FirstOrDefault();
            if (@namespace is null)
            {
                continue;
            }
            
            var ns = @namespace.Name.ToString();
            
            // get all GenerateSurrogatesAndConvertersAttribute from marker class
            var attributes = markerClass.AttributeLists
                .SelectMany(a => a.Attributes)
                .Where(a => a.Name.ToString().StartsWith("GenerateSurrogatesAndConverters"))
                .ToArray();

            foreach (var attribute in attributes)
            {
                // get full type name from attribute
                var expression = attribute.ArgumentList?.Arguments.FirstOrDefault()?.Expression;
                if (expression is not TypeOfExpressionSyntax typeOfExpression)
                {
                    continue;
                }
                
                var simpleTypeName = typeOfExpression.Type.ToString();
                
                // get assembly from type name
                var assemblySymbol = context.Compilation.SourceModule.ReferencedAssemblySymbols.FirstOrDefault(a => a.TypeNames.Contains(simpleTypeName));
                if (assemblySymbol is null)
                {
                    continue;
                }
                
                // get all types marked with [DataContract] attribute from found assembly symbol
                
                var dataContracts = assemblySymbol.GlobalNamespace.GetNamespaceMembers()
                    .SelectMany(n => n.GetTypeMembers())
                    .Where(t => t.GetAttributes().Any(a => a.IsDataContractAttribute()))
                    .ToArray();
                
                foreach (var dataContractType in dataContracts)
                {
                    var dataContractNamespace = dataContractType.ContainingNamespace.ToDisplayString();
                    var className = dataContractType.Name;
                    var surrogateClassName = $"{className}Surrogate";

                    var classProps = dataContractType.GetMembers().OfType<IPropertySymbol>().ToArray();
                
                
                    // generate surrogate class
                    var source = SurrogateTemplate.GetSurrogate(ns, surrogateClassName, classProps);
                
                    // add source to compilation
                    context.AddSource($"{surrogateClassName}.g.cs", source);
                    
                    
                    // generate surrogate converter class
                    var converterSource = SurrogateConverterTemplate.GetConverter(ns, dataContractNamespace, className, surrogateClassName, classProps);
                    
                    // add source to compilation
                    context.AddSource($"{surrogateClassName}Converter.g.cs", converterSource);
                }
            }
        }
    }
}