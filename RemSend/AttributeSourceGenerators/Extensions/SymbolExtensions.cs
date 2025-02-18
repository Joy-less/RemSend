#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AttributeSourceGenerators;

public static class SymbolExtensions {
    public static string GetDeclaredAccessibility(this ISymbol Symbol) {
        return SyntaxFacts.GetText(Symbol.DeclaredAccessibility);
    }
    public static bool IsType(this INamedTypeSymbol? Symbol, Type TypeToCheck) {
        return Symbol?.ToString() == TypeToCheck.ToString();
    }
    public static bool IsType<T>(this INamedTypeSymbol? Symbol) {
        return Symbol.IsType(typeof(T));
    }
    public static AttributeData? GetAttribute(this ISymbol Symbol, Type AttributeType) {
        return Symbol.GetAttributes().FirstOrDefault(Attribute => Attribute.AttributeClass.IsType(AttributeType));
    }
    public static AttributeData? GetAttribute<T>(this ISymbol Symbol) {
        return Symbol.GetAttribute(typeof(T));
    }
    public static bool HasAttribute(this ISymbol Symbol, Type AttributeType) {
        return Symbol.GetAttribute(AttributeType) is not null;
    }
    public static bool HasAttribute<T>(this ISymbol Symbol) {
        return Symbol.HasAttribute(typeof(T));
    }
    public static bool IsDerivedType(this ITypeSymbol Symbol, Type BaseType) {
        if (Symbol.ToString() == BaseType.FullName) {
            return true;
        }
        if (Symbol.BaseType is not null) {
            return Symbol.BaseType.IsDerivedType(BaseType);
        }
        return false;
    }
    public static bool IsDerivedType<T>(this ITypeSymbol Symbol) {
        return Symbol.IsDerivedType(typeof(T));
    }
    public static bool IsTask(this ITypeSymbol Symbol) {
        return Symbol.IsDerivedType<Task>() || Symbol.IsDerivedType<ValueTask>();
    }
    public static bool IsGenericTask(this ITypeSymbol Symbol) {
        return Symbol.IsTask() && Symbol is INamedTypeSymbol { IsGenericType: true };
    }
    public static bool IsNonGenericTask(this ITypeSymbol Symbol) {
        return Symbol.IsTask() && Symbol is not INamedTypeSymbol { IsGenericType: true };
    }
    public static INamedTypeSymbol GetReturnTypeAsTask(this IMethodSymbol Symbol, Compilation Compilation) {
        // Method returns task
        if (Symbol.ReturnType.IsTask()) {
            // Return task as-is
            return (INamedTypeSymbol)Symbol.ReturnType;
        }
        // Method returns void
        else if (Symbol.ReturnsVoid) {
            // Return non-generic task
            return Compilation.GetTypeByMetadataName(typeof(Task<>).FullName)!;
        }
        // Method returns value
        else {
            // Return generic task
            return Compilation.GetTypeByMetadataName(typeof(Task<>).FullName)!.Construct(Symbol.ReturnType);
        }
    }
    public static ITypeSymbol GetReturnTypeAsValue(this IMethodSymbol Symbol, Compilation Compilation) {
        // Method returns value
        if (!Symbol.ReturnType.IsTask()) {
            // Return value as-is
            return Symbol.ReturnType;
        }
        // Method returns non-generic task
        else if (Symbol.ReturnType.IsNonGenericTask()) {
            // Return void
            return Compilation.GetSpecialType(SpecialType.System_Void);
        }
        // Method returns generic task
        else {
            // Return value
            return ((INamedTypeSymbol)Symbol.ReturnType).TypeArguments[0];
        }
    }
    public static string AsIdentifier(this ISymbol? Symbol, char ReplacementChar = '_') {
        if (Symbol is null) {
            return ReplacementChar.ToString();
        }
        return Symbol.ToString().SanitizeIdentifier(ReplacementChar);
    }
    public static string GetParameterDeclaration(this IParameterSymbol Parameter) {
        // Attributes
        string AttributesDeclaration = "";
        ImmutableArray<AttributeData> Attributes = Parameter.GetAttributes();
        if (!Attributes.IsDefaultOrEmpty) {
            AttributesDeclaration = "[" + string.Join(", ", Attributes) + "] ";
        }

        // Default value
        string DefaultValueDeclaration = "";
        if (Parameter.HasExplicitDefaultValue) {
            DefaultValueDeclaration = " = " + SymbolDisplay.FormatPrimitive(Parameter.ExplicitDefaultValue!, quoteStrings: true, useHexadecimalNumbers: false);
        }

        // Combined result
        return $"{AttributesDeclaration}{Parameter}{DefaultValueDeclaration}";
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
            Append(ContentLine + "\n");
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