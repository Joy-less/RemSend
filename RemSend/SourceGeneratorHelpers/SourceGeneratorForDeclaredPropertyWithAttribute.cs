using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RemSend.SourceGeneratorHelpers;

public abstract class SourceGeneratorForDeclaredPropertyWithAttribute<TAttribute> : SourceGeneratorForDeclaredMemberWithAttribute<TAttribute, PropertyDeclarationSyntax>
    where TAttribute : Attribute {

    protected abstract (string? GeneratedCode, DiagnosticDetail? Error) GenerateCode(Compilation Compilation, SyntaxNode Node, IPropertySymbol Symbol, AttributeData Attribute, AnalyzerConfigOptions Options);
    protected sealed override (string? GeneratedCode, DiagnosticDetail? Error) GenerateCode(Compilation Compilation, SyntaxNode Node, ISymbol Symbol, AttributeData Attribute, AnalyzerConfigOptions Options)
        => GenerateCode(Compilation, Node, (IPropertySymbol)Symbol, Attribute, Options);
}