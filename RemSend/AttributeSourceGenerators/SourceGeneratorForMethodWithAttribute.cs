#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AttributeSourceGenerators;

public abstract class SourceGeneratorForMethodWithAttribute<TAttribute> : SourceGeneratorForMemberWithAttribute<TAttribute, MethodDeclarationSyntax, IMethodSymbol>
    where TAttribute : Attribute {
}