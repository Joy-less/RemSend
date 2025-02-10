using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RemSend.SourceGeneratorHelpers;

public static class SymbolExtensions {
    public static string FullName(this ISymbol Symbol) {
        string? ns = Symbol.NamespaceOrNull();
        return ns is null ? $"global::{Symbol.Name}" : $"{ns}.{Symbol.Name}";
    }
    public static string? NamespaceOrNull(this ISymbol Symbol) {
        return Symbol.ContainingNamespace.IsGlobalNamespace ? null : string.Join(".", Symbol.ContainingNamespace.ConstituentNamespaces);
    }
    public static (string? NamespaceDeclaration, string? NamespaceClosure, string? NamespaceIndent) GetNamespaceDeclaration(this ISymbol Symbol, string Indent = "    ") {
        string? Namespace = Symbol.NamespaceOrNull();
        return Namespace is null
            ? (null, null, null)
            : ($"namespace {Namespace} {{\n", "}\n", Indent);
    }
    public static INamedTypeSymbol? OuterType(this ISymbol Symbol) {
        return Symbol.ContainingType?.OuterType() ?? Symbol as INamedTypeSymbol;
    }
    public static string ClassDef(this INamedTypeSymbol Symbol) {
        return Symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
    }
    public static string? ClassPath(this INamedTypeSymbol Symbol) {
        return Symbol.DeclaringSyntaxReferences.FirstOrDefault()?.SyntaxTree?.FilePath;
    }
    public static string GeneratePartialClass(this INamedTypeSymbol Symbol, string Content, IEnumerable<string>? Usings = null) {
        (string? NamespaceDeclaration, string? NamespaceClosure, string? NamespaceIndent) = Symbol.GetNamespaceDeclaration();

        return $$"""
            {{string.Join("\n", (Usings ?? []).Select(Using => $"using {Using};"))}}
            {{NamespaceDeclaration?.Trim()}}
            {{NamespaceIndent}}partial{{(Symbol.IsRecord ? " record" : "")}} {{(Symbol.IsValueType ? "struct" : "class")}} {{Symbol.ClassDef()}} {
            {{NamespaceIndent}}    {{string.Join($"\n{NamespaceIndent}    ", Content.SplitLines())}}
            {{NamespaceIndent}}}
            {{NamespaceClosure?.Trim()}}
            """.TrimStart();
    }
    public static bool InheritsFrom(this ITypeSymbol Symbol, string Type) {
        INamedTypeSymbol? BaseType = Symbol.BaseType;
        while (BaseType is not null) {
            if (BaseType.Name == Type) {
                return true;
            }
            BaseType = BaseType.BaseType;
        }
        return false;
    }
    public static string GetDeclaredAccessibility(this ISymbol Symbol) {
        return SyntaxFacts.GetText(Symbol.DeclaredAccessibility);
    }
}
