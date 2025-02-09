using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RemSend.SourceGeneratorHelpers;

public abstract class SourceGeneratorForDeclaredTypeWithAttribute<TAttribute> : SourceGeneratorForDeclaredMemberWithAttribute<TAttribute, TypeDeclarationSyntax>
    where TAttribute : Attribute {

    protected abstract (string? GeneratedCode, DiagnosticDetail? Error) GenerateCode(Compilation Compilation, SyntaxNode Node, INamedTypeSymbol Symbol, AttributeData Attribute, AnalyzerConfigOptions Options);
    protected sealed override (string? GeneratedCode, DiagnosticDetail? Error) GenerateCode(Compilation Compilation, SyntaxNode Node, ISymbol Symbol, AttributeData Attribute, AnalyzerConfigOptions Options)
        => GenerateCode(Compilation, Node, (INamedTypeSymbol)Symbol, Attribute, Options);
}
