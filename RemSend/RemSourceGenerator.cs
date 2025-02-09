using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using RemSend.SourceGeneratorHelpers;
using System.Text;

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
            """));

        return (SourceBuilder.ToString(), null);
    }

    private static RemAttribute ReconstructAttribute(AttributeData AttributeData) {
        // Create attribute from arguments
        RemAttribute Attribute = new(
            Access: (RemAccess)AttributeData.ConstructorArguments.ElementAtOrDefault(0).Value!,
            CallLocal: (bool)AttributeData.ConstructorArguments.ElementAtOrDefault(1).Value!,
            Mode: (RemMode)AttributeData.ConstructorArguments.ElementAtOrDefault(2).Value!,
            Channel: (int)AttributeData.ConstructorArguments.ElementAtOrDefault(3).Value!
        );
        // Update attribute with named arguments
        foreach (KeyValuePair<string, TypedConstant> NamedArgument in AttributeData.NamedArguments) {
            switch (NamedArgument.Key) {
                case nameof(Attribute.Access):
                    Attribute.Access = (RemAccess)NamedArgument.Value.Value!;
                    break;
                case nameof(Attribute.CallLocal):
                    Attribute.CallLocal = (bool)NamedArgument.Value.Value!;
                    break;
                case nameof(Attribute.Mode):
                    Attribute.Mode = (RemMode)NamedArgument.Value.Value!;
                    break;
                case nameof(Attribute.Channel):
                    Attribute.Channel = (int)NamedArgument.Value.Value!;
                    break;
            }
        }
        return Attribute;
    }
}