using System.Text;
using Microsoft.CodeAnalysis;

namespace SourceGenerators.Orleans.DataContract;

public static class ConvertSurrogateBodyTemplate
{
    private const string Template =
        """
        var result = new {ClassName}();
        {ConvertFromSurrogate}
        return result;
        """;

    public static string GetConvertFromSurrogate(string className, IEnumerable<IPropertySymbol> dataProperties)
    {
        var sb = new StringBuilder();
        foreach (var property in dataProperties)
        {
            sb.AppendLine($"result.{property.Name} = surrogate.{property.Name};");
        }
        
        return Template
            .Replace("{ClassName}", className)
            .Replace("{ConvertFromSurrogate}", sb.ToString());
    }
    
    public static string GetConvertToSurrogate(string surrogateClassName, IEnumerable<IPropertySymbol> surrogateProperties)
    {
        var sb = new StringBuilder();
        foreach (var property in surrogateProperties)
        {
            sb.AppendLine($"result.{property.Name} = value.{property.Name};");
        }
        
        return Template
            .Replace("{ClassName}", surrogateClassName)
            .Replace("{ConvertFromSurrogate}", sb.ToString());
    }
}