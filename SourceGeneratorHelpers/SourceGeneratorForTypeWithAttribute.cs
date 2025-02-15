using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RemSend.SourceGeneratorHelpers;

public abstract class SourceGeneratorForTypeWithAttribute<TAttribute> : SourceGeneratorForMemberWithAttribute<TAttribute, TypeDeclarationSyntax, INamedTypeSymbol>
    where TAttribute : Attribute {
}
