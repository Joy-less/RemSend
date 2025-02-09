using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using GeneratorContext = Microsoft.CodeAnalysis.IncrementalGeneratorInitializationContext;

namespace RemSend.SourceGeneratorHelpers;

public abstract class SourceGeneratorForDeclaredMemberWithAttribute<TAttribute, TDeclarationSyntax> : IIncrementalGenerator
    where TAttribute : Attribute
    where TDeclarationSyntax : MemberDeclarationSyntax {

    private static readonly string AttributeType = typeof(TAttribute).Name;
    private static readonly string AttributeName = Regex.Replace(AttributeType, "Attribute$", "", RegexOptions.Compiled);

    private const string GeneratedFilenameExtension = ".g.cs";

    private static readonly char[] InvalidFileNameChars = [
        '\"', '<', '>', '|', '\0',
        (char)1, (char)2, (char)3, (char)4, (char)5, (char)6, (char)7, (char)8, (char)9, (char)10,
        (char)11, (char)12, (char)13, (char)14, (char)15, (char)16, (char)17, (char)18, (char)19, (char)20,
        (char)21, (char)22, (char)23, (char)24, (char)25, (char)26, (char)27, (char)28, (char)29, (char)30,
        (char)31, ':', '*', '?', '\\', '/'
    ];

    protected virtual IEnumerable<(string Name, string Source)> StaticSources => [];

    public void Initialize(GeneratorContext Context) {
        foreach ((string Name, string Source) in StaticSources) {
            Context.RegisterPostInitializationOutput(Context => Context.AddSource(Name + GeneratedFilenameExtension, Source));
        }

        var SyntaxProvider = Context.SyntaxProvider.CreateSyntaxProvider(IsSyntaxTarget, GetSyntaxTarget);
        var CompilationProvider = Context.CompilationProvider.Combine(SyntaxProvider.Collect()).Combine(Context.AnalyzerConfigOptionsProvider);
        Context.RegisterImplementationSourceOutput(CompilationProvider, (Context, Provider) => OnExecute(Context, Provider.Left.Left, Provider.Left.Right, Provider.Right));

        static bool IsSyntaxTarget(SyntaxNode Node, CancellationToken CancelToken) {
            return Node is TDeclarationSyntax Type && HasAttributeType();

            bool HasAttributeType() {
                foreach (AttributeListSyntax AttributeList in Type.AttributeLists) {
                    foreach (AttributeSyntax Attribute in AttributeList.Attributes) {
                        if (Attribute.Name.ToString() == AttributeName) {
                            return true;
                        }
                    }
                }
                return false;
            }
        }
        static TDeclarationSyntax GetSyntaxTarget(GeneratorSyntaxContext Context, CancellationToken CancelToken) {
            return (TDeclarationSyntax)Context.Node;
        }
        void OnExecute(SourceProductionContext Context, Compilation Compilation, ImmutableArray<TDeclarationSyntax> Nodes, AnalyzerConfigOptionsProvider Options) {
            foreach (TDeclarationSyntax Node in Nodes.Distinct()) {
                if (Context.CancellationToken.IsCancellationRequested) {
                    return;
                }

                SemanticModel Model = Compilation.GetSemanticModel(Node.SyntaxTree);
                ISymbol? Symbol = Model.GetDeclaredSymbol(GetNode(Node));
                if (Symbol is null) {
                    continue;
                }
                AttributeData? Attribute = Symbol.GetAttributes().SingleOrDefault(Attribute => Attribute.AttributeClass?.Name == AttributeType);
                if (Attribute is null) {
                    continue;
                }

                (string? GeneratedCode, DiagnosticDetail? Error) = SafeGenerateCode(Compilation, Node, Symbol, Attribute, Options.GlobalOptions);

                if (GeneratedCode is null) {
                    DiagnosticDescriptor Descriptor = new(Error!.Id ?? AttributeName, Error.Title, Error.Message, Error.Category ?? "Usage", DiagnosticSeverity.Error, true);
                    Diagnostic Diagnostic = Diagnostic.Create(Descriptor, Attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation());
                    Context.ReportDiagnostic(Diagnostic);
                    continue;
                }

                Context.AddSource(GenerateFilename(Symbol), GeneratedCode);
            }
        }
    }

    protected abstract (string? GeneratedCode, DiagnosticDetail? Error) GenerateCode(Compilation Compilation, SyntaxNode Node, ISymbol Symbol, AttributeData Attribute, AnalyzerConfigOptions Options);
    private (string? GeneratedCode, DiagnosticDetail? Error) SafeGenerateCode(Compilation Compilation, SyntaxNode Node, ISymbol Symbol, AttributeData Attribute, AnalyzerConfigOptions Options) {
        try {
            return GenerateCode(Compilation, Node, Symbol, Attribute, Options);
        }
        catch (Exception Ex) {
            return (null, new DiagnosticDetail("Internal Error", Ex.Message));
        }
    }

    protected virtual string GenerateFilename(ISymbol Symbol) {
        return string.Join("_", Symbol.ToString().Split(InvalidFileNameChars)) + GeneratedFilenameExtension;
    }
    protected virtual SyntaxNode GetNode(TDeclarationSyntax Node) {
        return Node;
    }

    protected static bool TryGetAttributeArgument<T>(AttributeData AttributeData, string ArgumentName, out T Value) {
        // Try to get named argument
        foreach (KeyValuePair<string, TypedConstant> NamedArgument in AttributeData.NamedArguments) {
            if (NamedArgument.Key == ArgumentName) {
                Value = (T)NamedArgument.Value.Value!;
                return true;
            }
        }
        // Try to get positional argument
        if (AttributeData.AttributeConstructor is not null) {
            foreach (IParameterSymbol Parameter in AttributeData.AttributeConstructor.Parameters) {
                if (Parameter.Name == ArgumentName) {
                    Value = (T)AttributeData.ConstructorArguments[Parameter.Ordinal].Value!;
                    return true;
                }
            }
        }
        // Argument not found
        Value = default!;
        return false;
    }
    protected static T GetAttributeArgument<T>(AttributeData AttributeData, string ArgumentName, T? DefaultValue = default) {
        return TryGetAttributeArgument(AttributeData, ArgumentName, out T Value) ? Value : DefaultValue!;
    }
}