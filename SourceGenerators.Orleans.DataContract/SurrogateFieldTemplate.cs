namespace SourceGenerators.Orleans.DataContract;

public static class SurrogateFieldTemplate
{
    private const string Template = """[Id({Id})] public {Type} {Name} { get; set; }""";
    

    public static string GetField(int id, string name, string type) => Template
        .Replace("{Id}", id.ToString("D"))
        .Replace("{Name}", name)
        .Replace("{Type}", type);
}