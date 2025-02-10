using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using RemSend.SourceGeneratorHelpers;

namespace RemSend;

[Generator]
internal class RemSourceGenerator : SourceGeneratorForDeclaredMethodWithAttribute<RemAttribute> {
    protected override (string? GeneratedCode, DiagnosticDetail? Error) GenerateCode(Compilation Compilation, SyntaxNode Node, IMethodSymbol Symbol, AttributeData Attribute, AnalyzerConfigOptions Options) {
        RemAttribute RemAttribute = ReconstructAttribute(Attribute);

        // Method names
        string SendMethodName = $"Send{Symbol.Name}";
        string SendRpcMethodName = $"{SendMethodName}Rpc";
        // Type names
        string PackTypeName = $"{SendMethodName}Pack";
        // Parameter names
        string PeerIdParameterName = "PeerId";
        string SerializedArgumentsParameterName = "SerializedArguments";
        // Local variable names
        string ArgumentsPackLocalName = "ArgumentsPack";
        string SerializedArgumentsPackLocalName = "SerializedArgumentsPack";

        // Parameter definitions
        IEnumerable<string> SendMethodParameters = Symbol.Parameters.Select(Parameter => Parameter.ToString())
            .Prepend($"int? {PeerIdParameterName}");
        IEnumerable<string> PackStructParameters = Symbol.Parameters.Select(Parameter => $"{Parameter.Type} {Parameter.Name}");
        // Arguments
        IEnumerable<string> SendMethodArguments = Symbol.Parameters.Select(Parameter => Parameter.Name);

        // Attributes
        string NotBrowsableAttribute = "[EditorBrowsable(EditorBrowsableState.Never)]";

        // Method definitions
        string SendMethodDefinition = $$"""
            /// {{Symbol.GetDocumentationCommentXml()}}
            public void {{SendMethodName}}({{string.Join(", ", SendMethodParameters)}}) {
                // Serialize arguments
                {{PackTypeName}} {{ArgumentsPackLocalName}} = new({{string.Join(", ", SendMethodArguments)}});
                byte[] {{SerializedArgumentsPackLocalName}} = MemoryPackSerializer.Serialize({{ArgumentsPackLocalName}});

                // Broadcast RPC to all peers
                if ({{PeerIdParameterName}} is null) {
                    Rpc("{{SendRpcMethodName}}", {{SerializedArgumentsPackLocalName}});
                }
                // Send RPC to one peer
                else {
                    RpcId(PeerId.Value, "{{SendRpcMethodName}}", {{SerializedArgumentsPackLocalName}});
                }
            }
            """;
        string SendRpcMethodDefinition = $$"""
            {{NotBrowsableAttribute}}
            [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = {{(RemAttribute.CallLocal ? "true" : "false")}}, TransferChannel = {{RemAttribute.Channel}}, TransferMode = MultiplayerPeer.TransferModeEnum.{{RemAttribute.Mode}})]
            public void {{SendRpcMethodName}}(byte[] {{SerializedArgumentsParameterName}}) {
                // Deserialize arguments
                {{PackTypeName}} {{ArgumentsPackLocalName}} = MemoryPackSerializer.Deserialize<{{PackTypeName}}>({{SerializedArgumentsParameterName}});

                // Call target method
                {{Symbol.Name}}({{string.Join(", ", Symbol.Parameters.Select(Parameter => $"{ArgumentsPackLocalName}.{Parameter.Name}"))}});
            }
            """;

        // Type definitions
        string PackStructDefinition = $$"""
            {{NotBrowsableAttribute}}
            private record struct {{PackTypeName}}({{string.Join(", ", PackStructParameters)}});
            """;

        // Using statements
        IEnumerable<string> Usings = ["Godot", "System.ComponentModel", "MemoryPack"];

        // Generated source
        string GeneratedSource = $"""
            #nullable enable

            {Symbol.ContainingType.GeneratePartialType($"""
                {SendMethodDefinition}

                {SendRpcMethodDefinition}

                {PackStructDefinition}
                """, Usings)}
            """;
        return (GeneratedSource, null);
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