using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RemSend.SourceGeneratorHelpers;

public abstract class SourceGeneratorForPropertyWithAttribute<TAttribute> : SourceGeneratorForMemberWithAttribute<TAttribute, PropertyDeclarationSyntax, IPropertySymbol>
    where TAttribute : Attribute {
}