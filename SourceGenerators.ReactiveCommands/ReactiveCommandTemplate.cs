using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace SourceGenerators.ReactiveCommands;

public static class ReactiveCommandTemplate
{
    private static readonly Regex TaskRegex = new(@"Task<(?<TResult>.*)>");
    // IObservable<T> regex
    private static readonly Regex ObservableRegex = new(@"IObservable<(?<TResult>.*)>");
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
    private const string FromAsyncWithParamAndReturnTemplate = "public ReactiveUI.ReactiveCommand<{TParam}, {TResult}> {MethodName}Command => ReactiveUI.ReactiveCommand.CreateFromTask<{TParam}, {TResult}>({MethodName}Async);";

    // async reactive command with one parameter (no return type)
    private const string FromAsyncWithParamNoReturnTemplate = "public ReactiveUI.ReactiveCommand<{TParam}, Unit> {MethodName}Command => ReactiveUI.ReactiveCommand.CreateFromTask<{TParam}>({MethodName}Async);";
    // async reactive command without params (no param, but with return type)
    private const string FromAsyncNoParamWithReturnTemplate = "public ReactiveUI.ReactiveCommand<Unit, {TResult}> {MethodName}Command => ReactiveUI.ReactiveCommand.CreateFromTask({MethodName}Async);";
    // async reactive command without params (no param, no return type)
    private const string FromAsyncNoParamNoReturnTemplate = "public ReactiveUI.ReactiveCommand<Unit, Unit> {MethodName}Command => ReactiveUI.ReactiveCommand.CreateFromTask({MethodName}Async);";
    
    // IObservable<Unit> reactive command without params (no param, no return type)
    private const string FromObservableNoParamNoReturnTemplate = "public ReactiveUI.ReactiveCommand<Unit, Unit> {MethodName}Command => ReactiveUI.ReactiveCommand.CreateFromObservable({MethodName});";
    
    // IObservable<TParam> reactive command with params (no return type)
    private const string FromObservableWithParamNoReturnTemplate = "public ReactiveUI.ReactiveCommand<{TParam}, Unit> {MethodName}Command => ReactiveUI.ReactiveCommand.CreateFromObservable<{TParam}>({MethodName});";
    
    // IObservable<TParam> reactive command with params and return type
    private const string FromObservableWithParamAndReturnTemplate = "public ReactiveUI.ReactiveCommand<{TParam}, {TResult}> {MethodName}Command => ReactiveUI.ReactiveCommand.CreateFromObservable<{TParam}, {TResult}>({MethodName});";
    
    // IObservable<TParam> reactive command with no params and return type
    private const string FromObservableNoParamWithReturnTemplate = "public ReactiveUI.ReactiveCommand<Unit, {TResult}> {MethodName}Command => ReactiveUI.ReactiveCommand.CreateFromObservable<{TResult}>({MethodName});";
    
    // ReactiveCommand without params (no param, no return type)
    private const string FromNoParamNoReturnTemplate = "public ReactiveUI.ReactiveCommand<Unit, Unit> {MethodName}Command => ReactiveUI.ReactiveCommand.Create({MethodName});";
    // ReactiveCommand with params (no return type) no async
    private const string FromParamNoReturnTemplate = "public ReactiveUI.ReactiveCommand<{TParam}, Unit> {MethodName}Command => ReactiveUI.ReactiveCommand.Create<{TParam}, Unit>({MethodName});";
    // return-only reactive command
    private const string FromNoParamWithReturnTemplate = "public ReactiveUI.ReactiveCommand<Unit, {TResult}> {MethodName}Command => ReactiveUI.ReactiveCommand.Create({MethodName});";
    // ReactiveCommand with param and return type
    private const string FromWithParamAndReturnTemplate = "public ReactiveUI.ReactiveCommand<{TParam}, {TResult}> {MethodName}Command => ReactiveUI.ReactiveCommand.Create<{TParam}, {TResult}>({MethodName});";

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
        var hasResult = tResult is not ("Unit" or "void" or "" or "Task<Unit>" or "Task"); // check if method has return type
        methodName = isAsync ? methodName.Substring(0, methodName.Length - 5) : methodName; // remove Async suffix
        if (isAsync && hasResult)
        {
            var match = TaskRegex.Match(tResult);
            tResult = match.Success // get return type from Task<> 
                ? match.Groups["TResult"].Value  // if Task<> is used
                : "Unit"; // if Task is used
        }
        
        var isIObservable = tResult.StartsWith("IObservable");
        if (isIObservable)
        {
            tResult = ObservableRegex.Match(tResult).Groups["TResult"].Value;
            if (tResult is "Unit")
            {
                hasResult = false;
            }
            else
            {
                hasResult = true;
            }
                
        }

        
        var template = (isIObservable, isAsync, hasParams, hasResult) switch
        {
            (false, true, true, true) => FromAsyncWithParamAndReturnTemplate,
            (false, true, true, false) => FromAsyncWithParamNoReturnTemplate,
            (false, true, false, true) => FromAsyncNoParamWithReturnTemplate,
            (false, true, false, false) => FromAsyncNoParamNoReturnTemplate,
            (false, false, false, false) => FromNoParamNoReturnTemplate,
            (false, false, true, false) => FromParamNoReturnTemplate,
            (false, false, false, true) => FromNoParamWithReturnTemplate,
            (false, false, true, true) => FromWithParamAndReturnTemplate,
            (true, false, false, false) => FromObservableNoParamNoReturnTemplate,
            (true, false, false, true) => FromObservableNoParamWithReturnTemplate,
            (true, false, true, false) => FromObservableWithParamNoReturnTemplate,
            (true, false, true, true) => FromObservableWithParamAndReturnTemplate,
            _ => throw new ArgumentOutOfRangeException()
        };
        if (template is "")
            Debugger.Break(); // this should never happen
        // if return type is 'void' then change it to 'Unit'
        if (tResult == "void")
            tResult = "Unit";
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
        var unixNewLine = new[] {"\n"};
        var windowsNewLine = new[] {"\r\n"};
        
        var isWindows = ClassTemplate.Contains(windowsNewLine[0]);
        
        var indentLength = (isWindows ? ClassTemplate.Split(windowsNewLine, StringSplitOptions.RemoveEmptyEntries)
            : ClassTemplate.Split(unixNewLine, StringSplitOptions.RemoveEmptyEntries)
        ).First(line => line.Contains("{Properties}")).IndexOf('{'); // find line with placeholder
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