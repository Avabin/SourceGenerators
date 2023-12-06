using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerators.Orleans.DataContract;

public static class Attributes
{
    public static bool IsGenerateSurrogatesAndConvertersAttribute(this AttributeSyntax? attribute) => attribute?.Name.ToString().StartsWith("GenerateSurrogatesAndConverters", StringComparison.Ordinal) ?? false;
    public static bool IsDataContractAttribute(this ISymbol? attribute) => attribute is ITypeSymbol { Name: "DataContractAttribute" };
    public static bool IsDataContractAttribute(this AttributeData? attribute) => attribute?.AttributeClass.IsDataContractAttribute() ?? false;
    public static bool IsDataMemberAttribute(this ISymbol? attribute) => attribute is ITypeSymbol { Name: "DataMemberAttribute" };
    public static bool IsDataMemberAttribute(this AttributeData? attribute) => attribute?.AttributeClass.IsDataMemberAttribute() ?? false;
}