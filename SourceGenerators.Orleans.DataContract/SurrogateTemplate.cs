using System.Text;
using Microsoft.CodeAnalysis;

namespace SourceGenerators.Orleans.DataContract;

public static class SurrogateTemplate
{
    private const string Template = 
"""
using System;
using Orleans;

namespace {Namespace};

[GenerateSerializer]
public struct {Name}
{
{Fields}
}
""";

    public static string GetSurrogate(string ns, string name, IEnumerable<IPropertySymbol> props) => Template
        .Replace("{Namespace}", ns)
        .Replace("{Name}", name)
        .Replace("{Fields}", GetFields(props));
    
    private static string GetFields(IEnumerable<IPropertySymbol> props)
    {
        var sb = new StringBuilder();
        var counter = 0;
        foreach (var property in props)
        {
            // check if property has [DataMember] attribute with Order property
            var order = property.GetAttributes()
                .Where(a => a.AttributeClass.IsDataMemberAttribute())
                .Select(a => a.NamedArguments.FirstOrDefault(na => na.Key == "Order").Value.Value)
                .OfType<int?>()
                .SingleOrDefault();
            if (order.HasValue)
            {
                counter = order.Value;
            }
            else
            {
                order = counter++;
            }
            sb.AppendLine(SurrogateFieldTemplate.GetField(order.Value, property.Name, property.Type.ToDisplayString()));
        }

        return sb.ToString();
    }
}