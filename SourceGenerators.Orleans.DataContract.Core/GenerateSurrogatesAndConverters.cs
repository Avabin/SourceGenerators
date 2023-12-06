namespace SourceGenerators.Orleans.DataContract.Core;

/// <summary>
/// Represents an attribute that is used to generate surrogates and converters for a given assembly containing data contracts.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public sealed class GenerateSurrogatesAndConvertersAttribute : Attribute
{
    /// <summary>
    /// Gets the type withing the assembly that will be scanned for data contracts.
    /// </summary>
    /// <value>
    /// The type contained within the assembly that will be scanned for data contracts.
    /// </value>
    public Type AssemblyType { get; }

    /// <summary>
    /// Represents an attribute used to generate surrogates and converters for a given assembly containing data contracts.
    /// </summary>
    /// <param name="assemblyType">The type for which assembly will be scanned for data contracts.</param>
    public GenerateSurrogatesAndConvertersAttribute(Type assemblyType)
    {
        AssemblyType = assemblyType;
    }
}