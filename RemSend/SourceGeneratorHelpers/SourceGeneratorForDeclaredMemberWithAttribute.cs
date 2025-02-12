using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RemSend.SourceGeneratorHelpers;

public abstract class SourceGeneratorForDeclaredMemberWithAttribute<TAttribute, TDeclarationSyntax> : IIncrementalGenerator
    where TAttribute : Attribute
    where TDeclarationSyntax : MemberDeclarationSyntax {

    private static readonly string AttributeType = typeof(TAttribute).Name;
    private static readonly string AttributeName = AttributeType.TrimSuffix("Attribute");

    protected virtual IEnumerable<(string Name, string Source)> StaticSources => [];

    public void Initialize(IncrementalGeneratorInitializationContext Context) {
        foreach ((string Name, string Source) in StaticSources) {
            Context.RegisterPostInitializationOutput(Context => Context.AddSource($"{Name}.g", Source));
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
                if (Model.GetDeclaredSymbol(GetSyntaxNode(Node)) is not ISymbol Symbol) {
                    continue;
                }
                if (Symbol.GetAttribute<TAttribute>() is not AttributeData Attribute) {
                    continue;
                }

                (string? GeneratedCode, DiagnosticDetail? Error) = SafeGenerateCode(Compilation, Node, Symbol, Attribute, Options.GlobalOptions);

                if (GeneratedCode is null) {
                    DiagnosticDescriptor Descriptor = new(Error!.Id ?? typeof(TAttribute).Name, Error.Title, Error.Message, Error.Category ?? "Usage", DiagnosticSeverity.Error, true);
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
        return $"{Symbol.ToString().SanitizeFileName()}.g";
    }
    protected virtual SyntaxNode GetSyntaxNode(TDeclarationSyntax Node) {
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