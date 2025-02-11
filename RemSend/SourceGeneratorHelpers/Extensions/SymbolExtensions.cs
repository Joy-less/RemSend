using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RemSend.SourceGeneratorHelpers;

public static class SymbolExtensions {
    public static string GetDeclaredAccessibility(this ISymbol Symbol) {
        return SyntaxFacts.GetText(Symbol.DeclaredAccessibility);
    }
    public static string GeneratePartialType(this INamedTypeSymbol Symbol, string Content, IEnumerable<string>? Usings = null, string Indent = "    ") {
        string PartialType = "";
        int Depth = 0;
        void Append(string String) {
            PartialType += string.Concat(Enumerable.Repeat(Indent, Depth));
            PartialType += String;
        }

        if (Usings is not null) {
            Append(string.Join("\n", Usings.Select(Using => $"using {Using};")));
            Append("\n\n");
        }

        if (!Symbol.ContainingNamespace.IsGlobalNamespace) {
            Append($"namespace {Symbol.ContainingNamespace};");
            Append("\n\n");
        }

        Stack<INamedTypeSymbol> ContainingTypes = [];
        INamedTypeSymbol? CurrentContainingType = Symbol;
        while (CurrentContainingType is not null) {
            ContainingTypes.Push(CurrentContainingType);
            CurrentContainingType = CurrentContainingType.ContainingType;
        }

        foreach (INamedTypeSymbol ContainingType in ContainingTypes) {
            string TypeKeywords =
                (Symbol.IsRefLikeType ? "ref " : "")
                + "partial "
                + (Symbol.IsRecord ? "record " : "")
                + (Symbol.IsValueType ? "struct" : "class");

            Append($"{TypeKeywords} {ContainingType.Name} {{");
            Append("\n");
            Depth++;
        }

        foreach (string ContentLine in Content.SplitLines()) {
            Append(ContentLine);
            Append("\n");
        }

        foreach (INamedTypeSymbol _ in ContainingTypes) {
            Depth--;
            Append("}");
            Append("\n");
        }

        return PartialType;
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