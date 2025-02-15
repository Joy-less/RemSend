using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RemSend.SourceGeneratorHelpers;

public abstract class SourceGeneratorForMethodWithAttribute<TAttribute> : SourceGeneratorForMemberWithAttribute<TAttribute, MethodDeclarationSyntax, IMethodSymbol>
    where TAttribute : Attribute {
}