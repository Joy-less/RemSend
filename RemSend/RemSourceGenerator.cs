using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using RemSend.SourceGeneratorHelpers;
using System.Text;

namespace RemSend;

[Generator]
internal class RemSourceGenerator : SourceGeneratorForDeclaredMethodWithAttribute<RemAttribute> {
    protected override (string? GeneratedCode, DiagnosticDetail? Error) GenerateCode(Compilation Compilation, SyntaxNode Node, IMethodSymbol Symbol, AttributeData Attribute, AnalyzerConfigOptions Options) {
        RemAttribute RemAttribute = ReconstructRemAttribute(Attribute);

        // Method names
        string SendMethodName = $"Send{Symbol.Name}";
        string SendHandlerMethodName = $"{SendMethodName}Handler";
        // Parameter names
        string PeerIdParameterName = "PeerId";
        string PeerIdsParameterName = "PeerIds";
        // Local names
        string SenderIdLocalName = "_SenderId";
        string PacketLocalName = "_Packet";
        string NodePathBytesLocalName = "_NodePathBytes";
        string MethodNameBytesLocalName = "_MethodNameBytes";

        // Access modifiers
        string AccessModifier = Symbol.GetDeclaredAccessibility();

        // Filter parameters by category
        List<IParameterSymbol> PseudoParameters = [.. Symbol.Parameters.Where(Parameter => Parameter.HasAttribute<SenderAttribute>())];
        List <IParameterSymbol> RealParameters = [.. Symbol.Parameters.Except(PseudoParameters)];

        // Argument literals
        List<string> SendMethodArguments = [.. RealParameters.Select(Parameter => Parameter.Name)];
        List<string> SendMethodPackedArguments = [.. RealParameters.Select(Parameter => $"{Parameter.Name}Bytes")];
        // Parameter definitions
        List<string> BaseSendMethodParameters = [.. RealParameters.Select(Parameter => $"{Parameter.GetAttributes().StringifyAttributes()}{Parameter}")];
        List<string> SendMethodParameters = [.. BaseSendMethodParameters
            .Prepend($"int {PeerIdParameterName}")];
        List<string> SendMethodMultiParameters = [.. BaseSendMethodParameters
            .Prepend($"IEnumerable<int>? {PeerIdsParameterName}")];
        // Statements
        List<string> SerializeStatements = [.. RealParameters.Select(Parameter => $"Span<byte> {Parameter.Name}Bytes = MemoryPackSerializer.Serialize({Parameter.Name});")];
        List<string> CombinePacketExpressions = [.. SendMethodPackedArguments.Select(Argument => $".. BitConverter.GetBytes({Argument}.Length), .. {Argument},")
            .Prepend($".. BitConverter.GetBytes({NodePathBytesLocalName}.Length), .. {NodePathBytesLocalName},")
            .Prepend($".. BitConverter.GetBytes({MethodNameBytesLocalName}.Length), .. {MethodNameBytesLocalName},")];
        List<string> DeserializeStatements = [.. RealParameters.Select(Parameter => $"var {Parameter.Name} = MemoryPackSerializer.Deserialize<{Parameter.Type}>({Parameter.Name}Bytes)!;")];

        // Pass pseudo parameters
        foreach (IParameterSymbol Parameter in Symbol.Parameters) {
            if (Parameter.HasAttribute<SenderAttribute>()) {
                SendMethodArguments.Insert(Parameter.Ordinal, SenderIdLocalName);
            }
        }

        // Method definitions
        string MethodDefinitions = $$"""
            /// <summary>
            /// Remotely calls {{Symbol.GenerateSeeXml()}} on the given peer.<br/>
            /// Set <paramref name="{{PeerIdParameterName}}"/> to 0 to broadcast to all peers.<br/>
            /// Set <paramref name="{{PeerIdParameterName}}"/> to 1 to send to the authority.
            /// </summary>
            {{AccessModifier}} void {{SendMethodName}}({{string.Join(", ", SendMethodParameters)}}) {
                // Serialize node path
                Span<byte> {{NodePathBytesLocalName}} = Encoding.UTF8.GetBytes(GetPath());
                // Serialize method name
                Span<byte> {{MethodNameBytesLocalName}} = [{{string.Join(", ", Encoding.UTF8.GetBytes(SendMethodName))}}];
                // Serialize arguments
                {{string.Join("\n    ", SerializeStatements)}}

                // Combine packet
                Span<byte> {{PacketLocalName}} = [
                    {{string.Join("\n        ", CombinePacketExpressions)}}
                ];

                // Send packet to single peer ID
                ((SceneMultiplayer)Multiplayer).SendBytes(
                    bytes: {{PacketLocalName}},
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

                // Serialize node path
                Span<byte> {{NodePathBytesLocalName}} = Encoding.UTF8.GetBytes(GetPath());
                // Serialize method name
                Span<byte> {{MethodNameBytesLocalName}} = [{{string.Join(", ", Encoding.UTF8.GetBytes(SendMethodName))}}];
                // Serialize arguments
                {{string.Join("\n    ", SerializeStatements)}}
                
                // Combine packet components
                Span<byte> {{PacketLocalName}} = [
                    {{string.Join("\n        ", CombinePacketExpressions)}}
                ];
                
                // Send call data to multiple peer IDs
                foreach (int {{PeerIdParameterName}} in {{PeerIdsParameterName}}) {
                    ((SceneMultiplayer)Multiplayer).SendBytes(
                        bytes: {{PacketLocalName}},
                        id: {{PeerIdParameterName}},
                        mode: MultiplayerPeer.TransferModeEnum.{{RemAttribute.Mode}},
                        channel: {{RemAttribute.Channel}}
                    );
                }
            }

            private void {{SendHandlerMethodName}}(int SenderId, Span<byte> Packet) {
                /*((SceneMultiplayer)Multiplayer).PeerPacket += (SenderId, Packet) => {
                    GD.Print($"received {Packet.Length} bytes from {SenderId}");
                };*/

                // Deserialize arguments
                {{string.Join("\n    ", DeserializeStatements)}}

                // Call target method
                {{Symbol.Name}}({{string.Join(", ", SendMethodArguments)}});
            }

            private void SendHandler(int SenderId, Span<byte> Packet) {
                // Deserialize node path
                NodePath NodePath = Encoding.UTF8.GetString(EatComponent(ref Packet));
                // Deserialize method name
                string MethodName = Encoding.UTF8.GetString(EatComponent(ref Packet));

                // Find node
                Node Node = GetNode(NodePath);
                // Find handler method
                if (MethodName == "{{SendMethodName}}") {
                    Node.{{SendHandlerMethodName}}(SenderId, Packet);
                }
            }

            private static Span<byte> EatComponent(ref Span<byte> Packet) {
                // Eat component length
                int Length = BitConverter.ToInt32(Packet[..sizeof(int)]);
                Packet = Packet[sizeof(int)..];
                // Eat component content
                Span<byte> Content = Packet[..Length];
                Packet = Packet[Length..];
                // Return component content
                return Content;
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