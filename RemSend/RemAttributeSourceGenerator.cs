using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using RemSend.SourceGeneratorHelpers;

namespace RemSend;

[Generator]
internal class RemAttributeSourceGenerator : SourceGeneratorForDeclaredMethodWithAttribute<RemAttribute> {
    protected override (string? GeneratedCode, DiagnosticDetail? Error) GenerateCode(Compilation Compilation, SyntaxNode Node, IMethodSymbol Symbol, AttributeData Attribute, AnalyzerConfigOptions Options) {
        RemAttribute RemAttribute = ReconstructRemAttribute(Attribute);

        // Method names
        string SendMethodName = $"Send{Symbol.Name}";
        string SendHandlerMethodName = $"{SendMethodName}Handler";
        // Parameter names
        string PeerIdParameterName = "PeerId";
        string PeerIdsParameterName = "PeerIds";
        string SenderIdParameterName = "SenderId";
        // Local names
        string ArgumentsPackLocalName = "_ArgumentsPack";
        string SerializedArgumentsPackLocalName = "_SerializedArgumentsPack";
        string PacketLocalName = "_Packet";
        string SerializedPacketLocalName = "_SerializedPacket";
        // Type names
        string ArgumentsPackTypeName = $"{SendMethodName}Pack";
        string ArgumentsPackTypeFormatterName = $"{ArgumentsPackTypeName}Formatter";

        // Access modifiers
        string AccessModifier = Symbol.GetDeclaredAccessibility();

        // Filter parameters by category
        List<IParameterSymbol> PseudoParameters = [.. Symbol.Parameters.Where(Parameter => Parameter.HasAttribute<SenderAttribute>())];
        List <IParameterSymbol> RemoteParameters = [.. Symbol.Parameters.Except(PseudoParameters)];

        // Parameter definitions
        List<string> SendMethodParameters = [.. RemoteParameters.Select(Parameter => $"{Parameter.StringifyAttributes()}{Parameter}")];
        List<string> SendMethodOneParameters = [.. SendMethodParameters
            .Prepend($"int {PeerIdParameterName}")];
        List<string> SendMethodMultiParameters = [.. SendMethodParameters
            .Prepend($"IEnumerable<int>? {PeerIdsParameterName}")];

        // Arguments for locally calling target RemAttribute method
        List<string> SendTargetMethodArguments = [.. RemoteParameters.Select(Parameter => Parameter.Name)];
        // Pass pseudo parameters
        foreach (IParameterSymbol Parameter in PseudoParameters) {
            if (Parameter.HasAttribute<SenderAttribute>()) {
                SendTargetMethodArguments.Insert(Parameter.Ordinal, SenderIdParameterName);
            }
        }

        // Method definitions
        string MethodDefinitions = $$"""
            /// <summary>
            /// Remotely calls {{Symbol.GenerateSeeXml()}} on the given peer.<br/>
            /// Set <paramref name="{{PeerIdParameterName}}"/> to 0 to broadcast to all peers.<br/>
            /// Set <paramref name="{{PeerIdParameterName}}"/> to 1 to send to the authority.
            /// </summary>
            {{AccessModifier}} void {{SendMethodName}}({{string.Join(", ", SendMethodOneParameters)}}) {
                // Create arguments pack
                {{ArgumentsPackTypeName}} {{ArgumentsPackLocalName}} = new({{string.Join(", ", RemoteParameters.Select(Parameter => Parameter.Name))}});
                // Serialize arguments pack
                byte[] {{SerializedArgumentsPackLocalName}} = MemoryPackSerializer.Serialize({{ArgumentsPackLocalName}});

                // Create packet
                RemPacket {{PacketLocalName}} = new(this.GetPath(), "{{SendMethodName}}", {{SerializedArgumentsPackLocalName}});
                // Serialize packet
                byte[] {{SerializedPacketLocalName}} = MemoryPackSerializer.Serialize({{PacketLocalName}});

                // Send packet to peer ID
                ((SceneMultiplayer)this.Multiplayer).SendBytes(
                    bytes: {{SerializedPacketLocalName}},
                    id: {{PeerIdParameterName}},
                    mode: MultiplayerPeer.TransferModeEnum.{{RemAttribute.Mode}},
                    channel: {{RemAttribute.Channel}}
                );
            }

            /// <summary>
            /// Remotely calls {{Symbol.GenerateSeeXml()}} on each peer.
            /// </summary>
            {{AccessModifier}} void {{SendMethodName}}({{string.Join(", ", SendMethodMultiParameters)}}) {
                // Skip if no peers
                if ({{PeerIdsParameterName}} is null || !{{PeerIdsParameterName}}.Any()) {
                    return;
                }

                // Create arguments pack
                {{ArgumentsPackTypeName}} {{ArgumentsPackLocalName}} = new({{string.Join(", ", RemoteParameters.Select(Parameter => Parameter.Name))}});
                // Serialize arguments pack
                byte[] {{SerializedArgumentsPackLocalName}} = MemoryPackSerializer.Serialize({{ArgumentsPackLocalName}});
            
                // Create packet
                {{nameof(RemPacket)}} {{PacketLocalName}} = new(this.GetPath(), "{{SendMethodName}}", {{SerializedArgumentsPackLocalName}});
                // Serialize packet
                byte[] {{SerializedPacketLocalName}} = MemoryPackSerializer.Serialize({{PacketLocalName}});
                
                // Send packet to each peer ID
                foreach (int {{PeerIdParameterName}} in {{PeerIdsParameterName}}) {
                    ((SceneMultiplayer)this.Multiplayer).SendBytes(
                        bytes: {{SerializedPacketLocalName}},
                        id: {{PeerIdParameterName}},
                        mode: MultiplayerPeer.TransferModeEnum.{{RemAttribute.Mode}},
                        channel: {{RemAttribute.Channel}}
                    );
                }
            }

            private void {{SendHandlerMethodName}}(int {{SenderIdParameterName}}, {{nameof(RemPacket)}} {{PacketLocalName}}) {
                // Deserialize arguments pack
                {{ArgumentsPackTypeName}} {{ArgumentsPackLocalName}} = MemoryPackSerializer.Deserialize<{{ArgumentsPackTypeName}}>({{PacketLocalName}}.{{nameof(RemPacket.ArgumentsPack)}});
                
                // Extract arguments
                {{string.Join("\n    ", RemoteParameters.Select(Parameter => $"{Parameter.Type} {Parameter.Name} = {ArgumentsPackLocalName}.{Parameter.Name};"))}}

                // Call target method
                {{Symbol.Name}}({{string.Join(", ", SendTargetMethodArguments)}});
            }

            private record struct {{ArgumentsPackTypeName}}({{string.Join(", ", SendMethodParameters)}});

            private sealed class {{ArgumentsPackTypeFormatterName}} : MemoryPackFormatter<{{ArgumentsPackTypeName}}> {
                public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> Writer, scoped ref {{ArgumentsPackTypeName}} Value) {
                    {{string.Join("\n        ", RemoteParameters.Select(Parameter => $"Writer.WriteValue(Value.@{Parameter.Name});"))}}
                }
                public override void Deserialize(ref MemoryPackReader Reader, scoped ref {{ArgumentsPackTypeName}} Value) {
                    Value = new() {
                        {{string.Join("\n            ", RemoteParameters.Select(Parameter => $"@{Parameter.Name} = Reader.ReadValue<{Parameter.Type}>()!,"))}}
                    };
                }
            }
            """;

        // Using statements
        List<string> Usings = [
            "System",
            "System.Collections.Generic",
            "System.Linq",
            "System.Text",
            "System.ComponentModel",
            "Godot",
            "MemoryPack",
        ];

        // Generated source
        string GeneratedSource = $"""
            #nullable enable

            {Symbol.ContainingType.GeneratePartialType(MethodDefinitions, Usings)}
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