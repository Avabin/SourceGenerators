namespace SourceGenerators.Orleans;

public static class SurrogateTemplate
{
    private const string Template = """
    using System;

namespace {Namespace};

[GenerateSerializer]
public struct {Name}Surrogate
{
    {Fields}
}

[RegisterConverter]
public sealed class {Name}SurrogateConverter : IConverter<{Name}, {Name}Surrogate>
{
    public {Name} ConvertFromSurrogate(in {Name}Surrogate surrogate) 
    {
        {ConvertFromSurrogate}
    }

    public {Name}Surrogate ConvertToSurrogate(in {Name} value) 
    {
        {ConvertToSurrogate}
    }
}
""";

    public static string GetSurrogate(string ns, string name, string fields, string convertFromSurrogate, string convertToSurrogate) => Template
        .Replace("{Namespace}", ns)
        .Replace("{Name}", name)
        .Replace("{Fields}", fields)
        .Replace("{ConvertFromSurrogate}", convertFromSurrogate)
        .Replace("{ConvertToSurrogate}", convertToSurrogate);
}