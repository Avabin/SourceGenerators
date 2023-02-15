using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace SourceGenerators.ReactiveCommands;

public static class ReactiveCommandTemplate
{
    private static readonly Regex TaskRegex = new(@"Task<(?<TResult>.*)>");
    private const string ClassTemplate = """
                            using System;
                            using System.Reactive;
                            using System.Threading.Tasks;
                            using ReactiveUI;
                            namespace {Namespace}
                            {
                                public partial class {ClassName}
                                {
                                    {Properties}
                                }
                            }
                            """;

    // async reactive command with one parameter and return type
    private const string CommandFromAsyncWithParamAndReturnTemplate = """
public ReactiveUI.ReactiveCommand<{TParam}, {TResult}> {MethodName}Command => ReactiveUI.ReactiveCommand.CreateFromTask<{TParam}, {TResult}>({MethodName}Async);
""";

    // async reactive command with one parameter (no return type)
    private const string CommandFromAsyncWithParamNoReturnTemplate = """
public ReactiveUI.ReactiveCommand<{TParam}, Unit> {MethodName}Command => ReactiveUI.ReactiveCommand.CreateFromTask<{TParam}>({MethodName}Async);
""";
    // async reactive command without params (no param, but with return type)
    private const string CommandFromAsyncNoParamWithReturnTemplate = """
public ReactiveUI.ReactiveCommand<Unit, {TResult}> {MethodName}Command => ReactiveUI.ReactiveCommand.CreateFromTask({MethodName}Async);
""";
    // ReactiveCommand without params (no param, no return type)
    private const string CommandFromNoParamNoReturnTemplate = """
public ReactiveUI.ReactiveCommand<Unit, Unit> {MethodName}Command => ReactiveUI.ReactiveCommand.Create({MethodName});
""";
    // ReactiveCommand with params (no return type) no async
    private const string CommandFromParamNoReturnTemplate = """
public ReactiveUI.ReactiveCommand<{TParam}, Unit> {MethodName}Command => ReactiveUI.ReactiveCommand.Create<{TParam}, Unit>({MethodName});
""";
    // return-only reactive command
    private const string CommandFromNoParamWithReturnTemplate = """
public ReactiveUI.ReactiveCommand<Unit, {TResult}> {MethodName}Command => ReactiveUI.ReactiveCommand.Create({MethodName});
""";
    // ReactiveCommand with param and return type
    private const string CommandFromWithParamAndReturnTemplate = """
public ReactiveUI.ReactiveCommand<{TParam}, {TResult}> {MethodName}Command => ReactiveUI.ReactiveCommand.Create<{TParam}, {TResult}>({MethodName});
""";

    /// <summary>
    /// Render a property for a ReactiveCommand
    /// </summary>
    /// <param name="methodName">Source method name</param>
    /// <param name="tParam">Method parameter type</param>
    /// <param name="tResult">Method return type</param>
    /// <returns>Rendered property</returns>
    public static string RenderProperty(string methodName, string tParam, string tResult)
    {
        var isAsync = methodName.EndsWith("Async"); // check if method is async
        var hasParams = tParam is not ("Unit" or ""); // check if method has params
        var hasResult = tResult is not ("Unit" or "" or "Task<Unit>"); // check if method has return type
        methodName = isAsync ? methodName.Substring(0, methodName.Length - 5) : methodName; // remove Async suffix
        if (isAsync && hasResult)
        {
            var match = TaskRegex.Match(tResult);
            tResult = match.Success // get return type from Task<> 
                ? match.Groups["TResult"].Value  // if Task<> is used
                : "Unit"; // if Task is used
        }

        var template = (isAsync, hasParams, hasResult) switch
        {
            (true, true, true) => CommandFromAsyncWithParamAndReturnTemplate,
            (true, true, false) => CommandFromAsyncWithParamNoReturnTemplate,
            (true, false, true) => CommandFromAsyncNoParamWithReturnTemplate,
            (true, false, false) => CommandFromNoParamNoReturnTemplate,
            (false, true, false) => CommandFromParamNoReturnTemplate,
            (false, false, true) => CommandFromNoParamWithReturnTemplate,
            (false, true, true) => CommandFromWithParamAndReturnTemplate,
            _ => ""
        };
        if (template is "")
            Debugger.Break(); // this should never happen
        return new StringBuilder(template) // replace placeholders
            .Replace("{TParam}", tParam)
            .Replace("{TResult}", tResult)
            .Replace("{MethodName}", methodName)
            .ToString();
    }
    
    /// <summary>
    /// Render a class with ReactiveCommand properties
    /// </summary>
    /// <param name="ns">Class namespace (must be the same as the source class)</param>
    /// <param name="className">Source class name</param>
    /// <param name="properties">Properties to render</param>
    /// <returns>Rendered class</returns>
    public static string RenderClass(string ns, string className, IEnumerable<string> properties)
    {
        // measure indent using class template
        // indent is the space between the line start and the {Properties} placeholder
        // template is multiline, so we need to find the line with the placeholder
        var indentLength = ClassTemplate.Split(Environment.NewLine.ToArray())
            .First(line => line.Contains("{Properties}"))
            .IndexOf("{Properties}", StringComparison.Ordinal);
        var indent = new string(' ', indentLength); // create indent string
        // indent all properties and trim the first property as it's already indented
        var propertiesIndented = properties // for each property
            .Select(prop => indent + prop) // indent
            .Select((prop, i) => i == 0 ? prop.Trim() : prop); // trim first property
        
        var propsSb = new StringBuilder();
        foreach (var prop in propertiesIndented) // construct class body
            propsSb.AppendLine(prop);
        
        return new StringBuilder(ClassTemplate) // replace placeholders
            .Replace("{Namespace}", ns)
            .Replace("{ClassName}", className)
            .Replace("{Properties}", propsSb.ToString())
            .ToString();
    }
}