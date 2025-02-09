using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using RemSend.SourceGeneratorHelpers;

namespace RemSend;

[Generator]
internal class RemSourceGenerator : SourceGeneratorForDeclaredMethodWithAttribute<RemAttribute> {
    protected override (string? GeneratedCode, DiagnosticDetail? Error) GenerateCode(Compilation Compilation, SyntaxNode Node, IMethodSymbol Symbol, AttributeData Attribute, AnalyzerConfigOptions Options) {
        RemAttribute RemAttribute = ReconstructAttribute(Attribute);

        StringBuilder SourceBuilder = new();

        SourceBuilder.Append(Symbol.ContainingType.GeneratePartialClass($$"""
            public void Send{{Symbol.Name}}() {
                Console.WriteLine("Hello, World!");
            }
            """, ["System"]));

        return (SourceBuilder.ToString(), null);
    }

    private static RemAttribute ReconstructAttribute(AttributeData AttributeData) {
        return new RemAttribute(
            Access: GetAttributeArgument(AttributeData, nameof(RemAttribute.Access), RemAccess.None),
            CallLocal: GetAttributeArgument(AttributeData, nameof(RemAttribute.CallLocal), false),
            Mode: GetAttributeArgument(AttributeData, nameof(RemAttribute.Mode), RemMode.Reliable),
            Channel: GetAttributeArgument(AttributeData, nameof(RemAttribute.Channel), 0)
        );
    }
}