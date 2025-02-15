#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AttributeSourceGenerators;

public abstract class SourceGeneratorForFieldWithAttribute<TAttribute> : SourceGeneratorForMemberWithAttribute<TAttribute, FieldDeclarationSyntax, IFieldSymbol>
    where TAttribute : Attribute {

    protected override SyntaxNode GetSyntaxNode(FieldDeclarationSyntax Node)
        => Node.Declaration.Variables.Single();
}
