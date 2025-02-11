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
        // Parameter names
        string PeerIdParameterName = "PeerId";

        // Argument literals
        IEnumerable<string> SendMethodArguments = Symbol.Parameters.Select(Parameter => Parameter.Name);
        IEnumerable<string> SendMethodPackedArguments = SendMethodArguments.Select(Argument => $"{Argument}Pack");
        // Parameter definitions
        IEnumerable<string> SendMethodParameters = Symbol.Parameters.Select(Parameter => $"{Parameter.GetAttributes().StringifyAttributes()}{Parameter}")
            .Prepend($"int? {PeerIdParameterName}");
        IEnumerable<string> SendRpcMethodParameters = SendMethodPackedArguments.Select(Argument => $"byte[] {Argument}");

        // Method definitions
        string SendMethodDefinition = $$"""
            /// <summary>
            /// Remotely calls {{Symbol.GenerateSeeCrefXml()}}.
            /// </summary>
            public void {{SendMethodName}}({{string.Join(", ", SendMethodParameters)}}) {
                // Serialize arguments
                {{string.Join("\n    ", SendMethodArguments.Select(Argument => $"byte[] {Argument}Pack = MemoryPackSerializer.Serialize({Argument});"))}}

                // Broadcast RPC to all peers
                if ({{PeerIdParameterName}} is null) {
                    Rpc("{{SendRpcMethodName}}", {{string.Join(", ", SendMethodPackedArguments)}});
                }
                // Send RPC to one peer
                else {
                    RpcId(PeerId.Value, "{{SendRpcMethodName}}", {{string.Join(", ", SendMethodPackedArguments)}});
                }
            }
            """;
        string SendRpcMethodDefinition = $$"""
            [EditorBrowsable(EditorBrowsableState.Never)]
            [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = {{(RemAttribute.CallLocal ? "true" : "false")}}, TransferChannel = {{RemAttribute.Channel}}, TransferMode = MultiplayerPeer.TransferModeEnum.{{RemAttribute.Mode}})]
            public void {{SendRpcMethodName}}({{string.Join(", ", SendRpcMethodParameters)}}) {
                // Deserialize arguments
                {{string.Join("\n    ", Symbol.Parameters.Select(Parameter => $"var {Parameter.Name} = MemoryPackSerializer.Deserialize<{Parameter.Type}>({Parameter.Name}Pack)!;"))}}

                // Call target method
                {{Symbol.Name}}({{string.Join(", ", Symbol.Parameters.Select(Parameter => $"{Parameter.Name}"))}});
            }
            """;

        // Using statements
        IEnumerable<string> Usings = ["System", "System.ComponentModel", "Godot", "MemoryPack"];

        // Generated source
        string GeneratedSource = $"""
            #nullable enable

            {Symbol.ContainingType.GeneratePartialType($"""
                {SendMethodDefinition}

                {SendRpcMethodDefinition}
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