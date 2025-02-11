using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RemSend.SourceGeneratorHelpers;

public static class SymbolExtensions {
    /*public static string FullName(this ISymbol Symbol) {
        if (Symbol.NamespaceOrNull() is not string Namespace) {
            return $"global::{Symbol.Name}";
        }
        return $"{Namespace}.{Symbol.Name}";
    }*/
    public static string? GetNamespaceOrNull(this ISymbol Symbol) {
        return Symbol.ContainingNamespace.IsGlobalNamespace ? null : string.Join(".", Symbol.ContainingNamespace.ConstituentNamespaces);
    }
    public static (string? NamespaceDeclaration, string? NamespaceClosure, string? NamespaceIndent) GetNamespaceDeclaration(this ISymbol Symbol, string Indent = "    ") {
        if (Symbol.GetNamespaceOrNull() is not string Namespace) {
            return (null, null, null);
        }
        return ($"namespace {Namespace} {{", "}", Indent);
    }
    public static INamedTypeSymbol? GetOuterType(this ISymbol Symbol) {
        return Symbol.ContainingType?.GetOuterType() ?? Symbol as INamedTypeSymbol;
    }
    public static string GetDeclaredAccessibility(this ISymbol Symbol) {
        return SyntaxFacts.GetText(Symbol.DeclaredAccessibility);
    }
    public static string GeneratePartialType(this INamedTypeSymbol Symbol, string Content, IEnumerable<string>? Usings = null, string Indent = "    ") {
        (string? NamespaceDeclaration, string? NamespaceClosure, string? NamespaceIndent) = Symbol.GetNamespaceDeclaration();

        string TypeModifiers =
            (Symbol.IsRefLikeType ? "ref " : "")
            + "partial "
            + (Symbol.IsRecord ? "record " : "")
            + (Symbol.IsValueType ? "struct" : "class");

        return $$"""
            {{string.Join("\n", (Usings ?? []).Select(Using => $"using {Using};"))}}
            {{NamespaceDeclaration}}
            {{NamespaceIndent}}{{TypeModifiers}} {{Symbol.Name}} {
            {{NamespaceIndent}}{{Indent}}{{string.Join($"\n{NamespaceIndent}{Indent}", Content.SplitLines())}}
            {{NamespaceIndent}}}
            {{NamespaceClosure}}
            """.TrimStart();
    }
    public static string GenerateSeeXml(this IMethodSymbol Symbol) {
        // Add method name
        string Content = Symbol.Name;
        // Add type parameters
        if (Symbol.TypeParameters.Length != 0) {
            Content += "{" + string.Join(", ", Symbol.TypeParameters.Select(TypeParameter => TypeParameter.Name)) + "}";
        }
        // Add arguments
        Content += "(" + string.Join(", ", Symbol.Parameters.Select(Parameter => Parameter.Type.ToString().Replace('<', '{').Replace('>', '}'))) + ")";
        // Add see tag
        return $"<see cref=\"{Content}\"/>";
    }
}