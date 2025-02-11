using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using RemSend.SourceGeneratorHelpers;

namespace RemSend;

[Generator]
internal class RemSourceGenerator : SourceGeneratorForDeclaredMethodWithAttribute<RemAttribute> {
    protected override (string? GeneratedCode, DiagnosticDetail? Error) GenerateCode(Compilation Compilation, SyntaxNode Node, IMethodSymbol Symbol, AttributeData Attribute, AnalyzerConfigOptions Options) {
        RemAttribute RemAttribute = ReconstructRemAttribute(Attribute);

        // Method names
        string SendMethodName = $"Send{Symbol.Name}";
        string SendRpcMethodName = $"{SendMethodName}Rpc";
        // Parameter names
        string PeerIdParameterName = "PeerId";
        string PeerIdsParameterName = "PeerIds";
        // Local names
        string SenderIdLocalName = "SenderId";

        // Access modifiers
        string AccessModifier = Symbol.GetDeclaredAccessibility();

        // Filter parameters by category
        List<IParameterSymbol> PseudoParameters = [.. Symbol.Parameters.Where(Parameter => Parameter.HasAttribute<SenderAttribute>())];
        List <IParameterSymbol> RealParameters = [.. Symbol.Parameters.Except(PseudoParameters)];

        // Argument literals
        List<string> SendMethodArguments = [.. RealParameters.Select(Parameter => Parameter.Name)];
        List<string> SendMethodPackedArguments = [.. RealParameters.Select(Parameter => $"{Parameter.Name}Pack")];
        // Parameter definitions
        List<string> SendMethodParameters = [.. RealParameters.Select(Parameter => $"{Parameter.GetAttributes().StringifyAttributes()}{Parameter}")
            .Prepend($"int {PeerIdParameterName}")];
        List<string> SendMethodMultiParameters = [.. SendMethodParameters
            .Skip(1).Prepend($"IEnumerable<int>? {PeerIdsParameterName}")];
        List<string> SendRpcMethodParameters = [.. SendMethodPackedArguments.Select(Argument => $"byte[] {Argument}")];
        // Statements
        List<string> SerializeStatements = [.. RealParameters.Select(Parameter => $"byte[] {Parameter.Name}Pack = MemoryPackSerializer.Serialize({Parameter.Name});")];
        List<string> DeserializeStatements = [.. RealParameters.Select(Parameter => $"var {Parameter.Name} = MemoryPackSerializer.Deserialize<{Parameter.Type}>({Parameter.Name}Pack)!;")];

        // Pass pseudo parameters
        foreach (IParameterSymbol Parameter in Symbol.Parameters) {
            if (Parameter.HasAttribute<SenderAttribute>()) {
                SendMethodArguments.Insert(Parameter.Ordinal, SenderIdLocalName);
            }
        }

        // Method definitions
        string SendMethodDefinition = $$"""
            /// <summary>
            /// Remotely calls {{Symbol.GenerateSeeXml()}}.<br/>
            /// Set <paramref name="{{PeerIdParameterName}}"/> to 0 to broadcast to all peers.<br/>
            /// Set <paramref name="{{PeerIdParameterName}}"/> to 1 to send to the authority.
            /// </summary>
            {{AccessModifier}} void {{SendMethodName}}({{string.Join(", ", SendMethodParameters)}}) {
                // Serialize arguments
                {{string.Join("\n    ", SerializeStatements)}}

                // Send RPC to specific peer
                RpcId({{PeerIdParameterName}}, "{{SendRpcMethodName}}", {{string.Join(", ", SendMethodPackedArguments)}});
            }
            """;
        string SendMethodMultiDefinition = $$"""
            /// <summary>
            /// Remotely calls {{Symbol.GenerateSeeXml()}}.
            /// </summary>
            {{AccessModifier}} void {{SendMethodName}}({{string.Join(", ", SendMethodMultiParameters)}}) {
                // Skip if no peers
                if ({{PeerIdsParameterName}} is null || !{{PeerIdsParameterName}}.Any()) {
                    return;
                }

                // Serialize arguments
                {{string.Join("\n    ", SerializeStatements)}}

                // Send RPC to multiple peers
                foreach (int {{PeerIdParameterName}} in {{PeerIdsParameterName}}) {
                    RpcId({{PeerIdParameterName}}, "{{SendRpcMethodName}}", {{string.Join(", ", SendMethodPackedArguments)}});
                }
            }
            """;
        string SendRpcMethodDefinition = $$"""
            [EditorBrowsable(EditorBrowsableState.Never)]
            [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = {{(RemAttribute.CallLocal ? "true" : "false")}}, TransferChannel = {{RemAttribute.Channel}}, TransferMode = MultiplayerPeer.TransferModeEnum.{{RemAttribute.Mode}})]
            {{AccessModifier}} void {{SendRpcMethodName}}({{string.Join(", ", SendRpcMethodParameters)}}) {
                // Deserialize arguments
                {{string.Join("\n    ", DeserializeStatements)}}

                // Get sender peer ID
                int {{SenderIdLocalName}} = Multiplayer.GetRemoteSenderId();

                // Call target method
                {{Symbol.Name}}({{string.Join(", ", SendMethodArguments)}});
            }
            """;

        // Using statements
        List<string> Usings = [
            "System.Collections.Generic",
            "System.Linq",
            "System.ComponentModel",
            "Godot",
            "MemoryPack",
        ];

        // Generated source
        string GeneratedSource = $"""
            #nullable enable

            {Symbol.ContainingType.GeneratePartialType($"""
                {SendMethodDefinition}

                {SendMethodMultiDefinition}

                {SendRpcMethodDefinition}
                """, Usings)}
            """;
        return (GeneratedSource, null);
    }

    private static RemAttribute ReconstructRemAttribute(AttributeData AttributeData) {
        return new RemAttribute(
            Access: GetAttributeArgument(AttributeData, nameof(RemAttribute.Access), RemAccess.None),
            CallLocal: GetAttributeArgument(AttributeData, nameof(RemAttribute.CallLocal), false),
            Mode: GetAttributeArgument(AttributeData, nameof(RemAttribute.Mode), RemMode.Reliable),
            Channel: GetAttributeArgument(AttributeData, nameof(RemAttribute.Channel), 0)
        );
    }
}