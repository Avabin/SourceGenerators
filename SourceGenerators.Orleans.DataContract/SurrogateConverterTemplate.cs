using Microsoft.CodeAnalysis;

namespace SourceGenerators.Orleans.DataContract;

public static class SurrogateConverterTemplate
{
    private const string Template = 
        """
        using System;
        using Orleans;
        using {SourceNamespace};

        namespace {Namespace};

        [RegisterConverter]
        public sealed class {Name}Converter : IConverter<{SourceNamespace}.{Name}, {Namespace}.{Name}Surrogate>
        {
            public {SourceNamespace}.{Name} ConvertFromSurrogate(in {Namespace}.{Name}Surrogate surrogate)
            {
                {ConvertFromSurrogate}
            }
        
            public {Namespace}.{Name}Surrogate ConvertToSurrogate(in {SourceNamespace}.{Name} value)
            {
                {ConvertToSurrogate}
            }
        }
        """;
    
    public static string GetConverter(string ns, string sourceNs, string name, string surrogateName, IEnumerable<IPropertySymbol> dataProperties)
    {
        var props = dataProperties as IPropertySymbol[] ?? dataProperties.ToArray();
        return Template
            .Replace("{Namespace}", ns)
            .Replace("{SourceNamespace}", sourceNs)
            .Replace("{Name}", name)
            .Replace("{ConvertFromSurrogate}",
                ConvertSurrogateBodyTemplate.GetConvertFromSurrogate(name, props))
            .Replace("{ConvertToSurrogate}", ConvertSurrogateBodyTemplate.GetConvertToSurrogate(surrogateName, props));
    }
}