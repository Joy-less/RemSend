﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RemSend.SourceGeneratorHelpers;

public abstract class SourceGeneratorForDeclaredFieldWithAttribute<TAttribute> : SourceGeneratorForDeclaredMemberWithAttribute<TAttribute, FieldDeclarationSyntax>
    where TAttribute : Attribute {

    protected abstract (string? GeneratedCode, DiagnosticDetail? Error) GenerateCode(Compilation Compilation, SyntaxNode Node, IFieldSymbol Symbol, AttributeData Attribute, AnalyzerConfigOptions Options);
    protected sealed override (string? GeneratedCode, DiagnosticDetail? Error) GenerateCode(Compilation Compilation, SyntaxNode Node, ISymbol Symbol, AttributeData Attribute, AnalyzerConfigOptions Options)
        => GenerateCode(Compilation, Node, (IFieldSymbol)Symbol, Attribute, Options);
    protected override SyntaxNode GetSyntaxNode(FieldDeclarationSyntax Node)
        => Node.Declaration.Variables.Single();
}
