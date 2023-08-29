namespace SourceGenerators.Orleans.Core;

/// <summary>
/// Mark a type, to generate surrogates for all types found in namespace
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public class GenerateSurrogatesAttribute : Attribute
{
    public GenerateSurrogatesAttribute(string ns)
    {
        Namespace = ns;
    }

    public string Namespace { get; set; }
}