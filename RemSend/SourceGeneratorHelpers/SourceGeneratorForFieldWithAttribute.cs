using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RemSend.SourceGeneratorHelpers;

public abstract class SourceGeneratorForFieldWithAttribute<TAttribute> : SourceGeneratorForMemberWithAttribute<TAttribute, FieldDeclarationSyntax, IFieldSymbol>
    where TAttribute : Attribute {

    protected override SyntaxNode GetSyntaxNode(FieldDeclarationSyntax Node)
        => Node.Declaration.Variables.Single();
}
