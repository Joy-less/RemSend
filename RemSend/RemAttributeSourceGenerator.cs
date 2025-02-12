using Microsoft.CodeAnalysis;
using RemSend.SourceGeneratorHelpers;

namespace RemSend;

[Generator]
internal class RemAttributeSourceGenerator : SourceGeneratorForMethodWithAttribute<RemAttribute> {
    protected override (string? GeneratedCode, DiagnosticDetail? Error) GenerateCode(GenerateInput Input) {
        RemAttribute RemAttribute = ReconstructRemAttribute(Input.Attribute);

        // Method names
        string SendMethodName = $"Send{Input.Symbol.Name}";
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
        string SendArgumentsPackTypeName = $"{SendMethodName}Pack";
        string SendArgumentsPackFormatterTypeName = $"{SendArgumentsPackTypeName}Formatter";

        // Access modifiers
        string AccessModifier = Input.Symbol.GetDeclaredAccessibility();

        // Filter parameters by category
        List<IParameterSymbol> PseudoParameters = [.. Input.Symbol.Parameters.Where(Parameter => Parameter.HasAttribute<SenderAttribute>())];
        List <IParameterSymbol> RemoteParameters = [.. Input.Symbol.Parameters.Except(PseudoParameters)];

        // Parameter definitions
        List<string> SendMethodParameters = [.. RemoteParameters.Select(Parameter => $"{Parameter.StringifyAttributes()}{Parameter}")];
        List<string> SendMethodOneParameters = [.. SendMethodParameters
            .Prepend($"int {PeerIdParameterName}")];
        List<string> SendMethodMultiParameters = [.. SendMethodParameters
            .Prepend($"IEnumerable<int>? {PeerIdsParameterName}")];

        // Arguments for locally calling target method
        List<string> SendTargetMethodArguments = [.. RemoteParameters.Select(Parameter => $"{ArgumentsPackLocalName}.{Parameter.Name}")];
        // Pass pseudo parameters
        foreach (IParameterSymbol Parameter in PseudoParameters) {
            if (Parameter.HasAttribute<SenderAttribute>()) {
                SendTargetMethodArguments.Insert(Parameter.Ordinal, SenderIdParameterName);
            }
        }

        // XML references
        string MethodSeeXml = Input.Symbol.GenerateSeeXml();

        // Method definitions
        string MethodDefinitions = $$"""
            /// <summary>
            /// Remotely calls {{Input.Symbol.GenerateSeeXml()}} on the given peer.<br/>
            /// Set <paramref name="{{PeerIdParameterName}}"/> to 0 to broadcast to all peers.<br/>
            /// Set <paramref name="{{PeerIdParameterName}}"/> to 1 to send to the authority.
            /// </summary>
            {{AccessModifier}} void {{SendMethodName}}({{string.Join(", ", SendMethodOneParameters)}}) {
                // Create arguments pack
                {{SendArgumentsPackTypeName}} {{ArgumentsPackLocalName}} = new({{string.Join(", ", RemoteParameters.Select(Parameter => Parameter.Name))}});
                // Serialize arguments pack
                byte[] {{SerializedArgumentsPackLocalName}} = MemoryPackSerializer.Serialize({{ArgumentsPackLocalName}});
            
                // Create packet
                {{nameof(RemPacket)}} {{PacketLocalName}} = new(this.GetPath(), nameof({{SendMethodName}}), {{SerializedArgumentsPackLocalName}});
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
            /// Remotely calls {{MethodSeeXml}} on each peer.
            /// </summary>
            {{AccessModifier}} void {{SendMethodName}}({{string.Join(", ", SendMethodMultiParameters)}}) {
                // Skip if no peers
                if ({{PeerIdsParameterName}} is null || !{{PeerIdsParameterName}}.Any()) {
                    return;
                }

                // Create arguments pack
                {{SendArgumentsPackTypeName}} {{ArgumentsPackLocalName}} = new({{string.Join(", ", RemoteParameters.Select(Parameter => Parameter.Name))}});
                // Serialize arguments pack
                byte[] {{SerializedArgumentsPackLocalName}} = MemoryPackSerializer.Serialize({{ArgumentsPackLocalName}});
                
                // Create packet
                {{nameof(RemPacket)}} {{PacketLocalName}} = new(this.GetPath(), nameof({{SendMethodName}}), {{SerializedArgumentsPackLocalName}});
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

            [EditorBrowsable(EditorBrowsableState.Never)]
            internal void {{SendHandlerMethodName}}(int {{SenderIdParameterName}}, {{nameof(RemPacket)}} {{PacketLocalName}}) {
                // Deserialize arguments pack
                {{SendArgumentsPackTypeName}} {{ArgumentsPackLocalName}} = MemoryPackSerializer.Deserialize<{{SendArgumentsPackTypeName}}>({{PacketLocalName}}.{{nameof(RemPacket.ArgumentsPack)}});
                
                // Call target method
                {{Input.Symbol.Name}}({{string.Join(", ", SendTargetMethodArguments)}});
            }
            
            [EditorBrowsable(EditorBrowsableState.Never)]
            internal record struct {{SendArgumentsPackTypeName}}({{string.Join(", ", SendMethodParameters)}});
            
            [EditorBrowsable(EditorBrowsableState.Never)]
            internal sealed class {{SendArgumentsPackFormatterTypeName}} : MemoryPackFormatter<{{SendArgumentsPackTypeName}}> {
                public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> Writer, scoped ref {{SendArgumentsPackTypeName}} Value) {
                    {{string.Join("\n        ", RemoteParameters.Select(Parameter => $"Writer.WriteValue(Value.@{Parameter.Name});"))}}
                }
                public override void Deserialize(ref MemoryPackReader Reader, scoped ref {{SendArgumentsPackTypeName}} Value) {
                    Value = new() {
                        {{string.Join("\n            ", RemoteParameters.Select(Parameter => $"@{Parameter.Name} = Reader.ReadValue<{Parameter.Type}>()!,"))}}
                    };
                }
            }
            """;

        // Generated source
        string GeneratedSource = $"""
            #nullable enable

            using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;
            using System.ComponentModel;
            using Godot;
            using MemoryPack;
            using RemSend;

            {Input.Symbol.ContainingType.GeneratePartialType(MethodDefinitions)}
            """;
        return (GeneratedSource, null);
    }
    protected override (string? GeneratedCode, DiagnosticDetail? Error) GenerateCode(IEnumerable<GenerateInput> Inputs) {
        // Method names
        string SetupMethodName = "Setup";
        string HandlePacketMethodName = "HandlePacket";
        string SendMethodName = "Send{0}";
        string SendHandlerMethodName = $"{SendMethodName}Handler";
        // Parameter names
        string RootNodeParameterName = "Root";
        string SceneMultiplayerParameterName = "Multiplayer";
        string SenderIdParameterName = "SenderId";
        string PacketBytesParameterName = "PacketBytes";
        // Local names
        string PacketLocalName = "_Packet";
        string NodeLocalName = "_Node";
        // Type names
        string RemSendServiceTypeName = "RemSendService";
        string RemPacketFormatterTypeName = $"{nameof(RemPacket)}Formatter";
        string SendArgumentsPackTypeName = $"{SendMethodName}Pack";
        string SendArgumentsPackFormatterTypeName = $"{SendArgumentsPackTypeName}Formatter";

        // Generated source
        string GeneratedSource = $$"""
            #nullable enable

            using System;
            using System.Text;
            using System.ComponentModel;
            using Godot;
            using MemoryPack;

            namespace {{nameof(RemSend)}};

            public static class {{RemSendServiceTypeName}} {
                public static void {{SetupMethodName}}(SceneMultiplayer {{SceneMultiplayerParameterName}}, Node? {{RootNodeParameterName}} = null) {
                    // Default root node
                    {{RootNodeParameterName}} ??= ((SceneTree)Engine.GetMainLoop()).Root;
                    // Listen for packets
                    {{SceneMultiplayerParameterName}}.PeerPacket += ({{SenderIdParameterName}}, {{PacketBytesParameterName}}) => {
                        {{HandlePacketMethodName}}({{SceneMultiplayerParameterName}}, {{RootNodeParameterName}}, (int){{SenderIdParameterName}}, {{PacketBytesParameterName}});
                    };
                }

                private static void {{HandlePacketMethodName}}(SceneMultiplayer {{SceneMultiplayerParameterName}}, Node {{RootNodeParameterName}}, int {{SenderIdParameterName}}, ReadOnlySpan<byte> {{PacketBytesParameterName}}) {
                    // Deserialize packet
                    {{nameof(RemPacket)}} {{PacketLocalName}} = MemoryPackSerializer.Deserialize<{{nameof(RemPacket)}}>({{PacketBytesParameterName}});

                    // Find target node
                    Node {{NodeLocalName}} = {{RootNodeParameterName}}.GetNode({{SceneMultiplayerParameterName}}.RootPath).GetNode({{PacketLocalName}}.{{nameof(RemPacket.NodePath)}});
                    // Find target handler method
            {{string.Join("\n", Inputs.Select(Input => $$"""
                    if ({{NodeLocalName}} is @{{Input.Symbol.ContainingType}} @{{Input.Symbol.ContainingType.Name}}) {
                        if ({{PacketLocalName}}.{{nameof(RemPacket.MethodName)}} is nameof({{Input.Symbol.ContainingType}}.{{string.Format(SendMethodName, Input.Symbol.Name)}})) {
                            @{{Input.Symbol.ContainingType.Name}}.{{string.Format(SendHandlerMethodName, Input.Symbol.Name)}}({{SenderIdParameterName}}, {{PacketLocalName}});
                        }
                    }
            """))}}
                }

                static {{RemSendServiceTypeName}}() {
                    // Register MemoryPack formatters
                    MemoryPackFormatterProvider.Register(new {{RemPacketFormatterTypeName}}());
            {{string.Join("\n", Inputs.Select(Input => $$"""
                    MemoryPackFormatterProvider.Register(new {{Input.Symbol.ContainingType}}.{{string.Format(SendArgumentsPackFormatterTypeName, Input.Symbol.Name)}}());
            """))}}
                }

                // Formatter for {{nameof(RemPacket)}} because MemoryPack doesn't support .NET Standard 2.0
                private sealed class {{RemPacketFormatterTypeName}}: MemoryPackFormatter<{{nameof(RemPacket)}}> {
                    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> Writer, scoped ref {{nameof(RemPacket)}} Value) {
                        Writer.WriteValue(Value.{{nameof(RemPacket.NodePath)}});
                        Writer.WriteValue(Value.{{nameof(RemPacket.MethodName)}});
                        Writer.WriteValue(Value.{{nameof(RemPacket.ArgumentsPack)}});
                    }
                    public override void Deserialize(ref MemoryPackReader Reader, scoped ref {{nameof(RemPacket)}} Value) {
                        Value = new() {
                            {{nameof(RemPacket.NodePath)}} = Reader.ReadValue<string>()!,
                            {{nameof(RemPacket.MethodName)}} = Reader.ReadValue<string>()!,
                            {{nameof(RemPacket.ArgumentsPack)}} = Reader.ReadValue<byte[]>()!,
                        };
                    }
                }
            }
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