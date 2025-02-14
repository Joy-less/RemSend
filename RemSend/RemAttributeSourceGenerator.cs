using Microsoft.CodeAnalysis;
using RemSend.SourceGeneratorHelpers;

namespace RemSend;

[Generator]
internal class RemAttributeSourceGenerator : SourceGeneratorForMethodWithAttribute<RemAttribute> {
    protected override (string? GeneratedCode, DiagnosticDetail? Error) GenerateCode(GenerateInput Input) {
        RemAttribute RemAttribute = ReconstructRemAttribute(Input.Attribute);

        // Method names
        string SendMethodName = $"Send{Input.Symbol.Name}";
        string RequestMethodName = $"Request{Input.Symbol.Name}";
        string ReceiveMethodName = $"Receive{Input.Symbol.Name}";
        // Parameter names
        string PeerIdParameterName = "PeerId";
        string PeerIdsParameterName = "PeerIds";
        string SenderIdParameterName = "SenderId";
        string TimeoutParameterName = "Timeout";
        // Local names
        string ArgumentsPackLocalName = EscapeLocalName("ArgumentsPack", Input.Symbol);
        string SerializedArgumentsPackLocalName = EscapeLocalName("SerializedArgumentsPack", Input.Symbol);
        string DeserializedArgumentsPackLocalName = EscapeLocalName("DeserializedArgumentsPack", Input.Symbol);
        string PacketLocalName = EscapeLocalName("RemPacket", Input.Symbol);
        string SerializedPacketLocalName = EscapeLocalName("SerializedRemPacket", Input.Symbol);
        string ResultPacketLocalName = EscapeLocalName("ResultRemPacket", Input.Symbol);
        string SerializedResultPacketLocalName = EscapeLocalName("SerializedResultRemPacket", Input.Symbol);
        string RequestIdLocalName = EscapeLocalName("RequestId", Input.Symbol);
        string ReturnValueLocalName = EscapeLocalName("ReturnValue", Input.Symbol);
        string ResultAwaiterLocalName = EscapeLocalName("ResultAwaiter", Input.Symbol);
        string ResultCallbackLocalName = EscapeLocalName("ResultCallback", Input.Symbol);
        string ResultPackLocalName = EscapeLocalName("ResultPack", Input.Symbol);
        // Type names
        string SendArgumentsPackTypeName = $"{Input.Symbol.Name}SendPack";
        string RequestArgumentsPackTypeName = $"{Input.Symbol.Name}RequestPack";
        string ResultArgumentsPackTypeName = $"{Input.Symbol.Name}ResultPack";
        string FormatterTypeName = "Formatter";
        // Property names
        string RequestIdPropertyName = "RequestId";
        string ReturnValuePropertyName = "ReturnValue";
        string AttributePropertyName = $"{Input.Symbol.Name}RemAttribute";
        // Event names
        string OnReceiveResultEventName = $"OnReceive{Input.Symbol.Name}Result";

        // Access modifiers
        string AccessModifier = Input.Symbol.GetDeclaredAccessibility();

        // Filter parameters by category
        List<IParameterSymbol> PseudoParameters = [.. Input.Symbol.Parameters.Where(Parameter => Parameter.HasAttribute<SenderAttribute>())];
        List<IParameterSymbol> RemoteParameters = [.. Input.Symbol.Parameters.Except(PseudoParameters)];

        // Handle task return values
        bool ReturnsTask = Input.Symbol.ReturnType.IsTask();
        bool ReturnsNonGenericTask = Input.Symbol.ReturnType.IsNonGenericTask();
        INamedTypeSymbol ReturnTypeAsTask = Input.Symbol.GetReturnTypeAsTask(Input.Compilation);
        ITypeSymbol ReturnTypeAsValue = Input.Symbol.GetReturnTypeAsValue(Input.Compilation);

        // Parameters
        List<string> SendMethodParameters = [.. RemoteParameters.Select(Parameter => Parameter.GetParameterDeclaration())];
        List<string> SendMethodOneParameters = [.. SendMethodParameters.Prepend($"int {PeerIdParameterName}")];
        List<string> SendMethodMultiParameters = [.. SendMethodParameters.Prepend($"IEnumerable<int>? {PeerIdsParameterName}")];
        List<string> RequestMethodParameters = [.. SendMethodParameters.Prepend($"double {TimeoutParameterName}").Prepend($"int {PeerIdParameterName}")];

        // Arguments
        List<string> SendArgumentsPackArguments = [.. RemoteParameters.Select(Parameter => $"@{Parameter.Name}")];
        List<string> RequestArgumentsPackArguments = [.. SendArgumentsPackArguments.Prepend(RequestIdLocalName)];

        // Properties
        List<string> SendArgumentsPackProperties = [.. SendMethodParameters];
        List<string> RequestArgumentsPackProperties = [.. SendArgumentsPackProperties.Prepend($"Guid {RequestIdPropertyName}")];
        List<string> ResultArgumentsPackProperties = ReturnsNonGenericTask ? [$"Guid {RequestIdPropertyName}"] : [$"Guid {RequestIdPropertyName}", $"{ReturnTypeAsValue} {ReturnValuePropertyName}"];

        // Arguments for locally calling target method
        List<string> SendTargetMethodArguments = [.. RemoteParameters.Select(Parameter => $"{DeserializedArgumentsPackLocalName}.{Parameter.Name}")];
        // Pass pseudo parameters
        foreach (IParameterSymbol Parameter in PseudoParameters) {
            if (Parameter.HasAttribute<SenderAttribute>()) {
                SendTargetMethodArguments.Insert(Parameter.Ordinal, SenderIdParameterName);
            }
        }

        // XML references
        string MethodSeeXml = Input.Symbol.GenerateSeeXml();

        // Definitions (methods, types)
        List<string> Definitions = [];
        // Attribute
        Definitions.Add($$"""
            /// <summary>
            /// The <see cref="{{nameof(RemAttribute)}}"/> defined on {{MethodSeeXml}}.
            /// </summary>
            /// <remarks>
            /// Todo: use the changed values of this attribute if it's changed.
            /// </remarks>
            {{AccessModifier}} {{nameof(RemAttribute)}} {{AttributePropertyName}} { get; set; } = new() {
                {{nameof(RemAttribute.Access)}} = {{nameof(RemAccess)}}.{{RemAttribute.Access}},
                {{nameof(RemAttribute.CallLocal)}} = {{(RemAttribute.CallLocal ? "true" : "false")}},
                {{nameof(RemAttribute.Mode)}} = {{nameof(RemMode)}}.{{RemAttribute.Mode}},
                {{nameof(RemAttribute.Channel)}} = {{RemAttribute.Channel}},
            };
            """);
        // Send One
        Definitions.Add($$"""
            /// <summary>
            /// Remotely calls {{MethodSeeXml}} on the given peer.<br/>
            /// Set <paramref name="{{PeerIdParameterName}}"/> to 0 to broadcast to all peers.<br/>
            /// Set <paramref name="{{PeerIdParameterName}}"/> to 1 to send to the authority.
            /// </summary>
            {{AccessModifier}} void {{SendMethodName}}({{string.Join(", ", SendMethodOneParameters)}}) {
                // Create arguments pack
                {{SendArgumentsPackTypeName}} {{ArgumentsPackLocalName}} = new({{string.Join(", ", SendArgumentsPackArguments)}});
                // Serialize arguments pack
                byte[] {{SerializedArgumentsPackLocalName}} = MemoryPackSerializer.Serialize({{ArgumentsPackLocalName}});
                
                // Create packet
                {{nameof(RemPacket)}} {{PacketLocalName}} = new({{nameof(RemPacketType)}}.{{nameof(RemPacketType.Message)}}, this.GetPath(), nameof({{Input.Symbol.ContainingType}}.{{Input.Symbol.Name}}), {{SerializedArgumentsPackLocalName}});
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
            """);
        // Send Multi
        Definitions.Add($$"""
            /// <summary>
            /// Remotely calls {{MethodSeeXml}} on each peer.
            /// </summary>
            {{AccessModifier}} void {{SendMethodName}}({{string.Join(", ", SendMethodMultiParameters)}}) {
                // Skip if no peers
                if ({{PeerIdsParameterName}} is null || !{{PeerIdsParameterName}}.Any()) {
                    return;
                }
                
                // Create arguments pack
                {{SendArgumentsPackTypeName}} {{ArgumentsPackLocalName}} = new({{string.Join(", ", SendArgumentsPackArguments)}});
                // Serialize arguments pack
                byte[] {{SerializedArgumentsPackLocalName}} = MemoryPackSerializer.Serialize({{ArgumentsPackLocalName}});
                
                // Create packet
                {{nameof(RemPacket)}} {{PacketLocalName}} = new({{nameof(RemPacketType)}}.{{nameof(RemPacketType.Message)}}, this.GetPath(), nameof({{Input.Symbol.ContainingType}}.{{Input.Symbol.Name}}), {{SerializedArgumentsPackLocalName}});
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
            """);
        // Request
        if (!Input.Symbol.ReturnsVoid) {
            Definitions.Add($$"""
                [EditorBrowsable(EditorBrowsableState.Never)]
                internal event Action<{{ResultArgumentsPackTypeName}}>? {{OnReceiveResultEventName}};
                """);
            Definitions.Add($$"""
                /// <summary>
                /// Remotely calls {{MethodSeeXml}} on the given peer and awaits the return value.<br/>
                /// Set <paramref name="{{PeerIdParameterName}}"/> to 0 to broadcast to all peers.<br/>
                /// Set <paramref name="{{PeerIdParameterName}}"/> to 1 to send to the authority.
                /// </summary>
                {{AccessModifier}} async {{ReturnTypeAsTask}} {{RequestMethodName}}({{string.Join(", ", RequestMethodParameters)}}) {
                    // Generate request ID
                    Guid {{RequestIdLocalName}} = Guid.NewGuid();

                    // Create arguments pack
                    {{RequestArgumentsPackTypeName}} {{ArgumentsPackLocalName}} = new({{string.Join(", ", RequestArgumentsPackArguments)}});
                    // Serialize arguments pack
                    byte[] {{SerializedArgumentsPackLocalName}} = MemoryPackSerializer.Serialize({{ArgumentsPackLocalName}});
                    
                    // Create packet
                    {{nameof(RemPacket)}} {{PacketLocalName}} = new({{nameof(RemPacketType)}}.{{nameof(RemPacketType.Request)}}, this.GetPath(), nameof({{Input.Symbol.ContainingType}}.{{Input.Symbol.Name}}), {{SerializedArgumentsPackLocalName}});
                    // Serialize packet
                    byte[] {{SerializedPacketLocalName}} = MemoryPackSerializer.Serialize({{PacketLocalName}});
                    
                    // Send packet to peer ID
                    ((SceneMultiplayer)this.Multiplayer).SendBytes(
                        bytes: {{SerializedPacketLocalName}},
                        id: {{PeerIdParameterName}},
                        mode: MultiplayerPeer.TransferModeEnum.{{RemAttribute.Mode}},
                        channel: {{RemAttribute.Channel}}
                    );

                    // Create result listener
                    TaskCompletionSource{{(ReturnsNonGenericTask ? "" : $"<{ReturnTypeAsValue}>")}} {{ResultAwaiterLocalName}} = new();
                    void {{ResultCallbackLocalName}}({{ResultArgumentsPackTypeName}} {{ResultPackLocalName}}) {
                        if ({{ResultPackLocalName}}.{{RequestIdPropertyName}} == {{RequestIdLocalName}}) {
                            {{ResultAwaiterLocalName}}.TrySetResult({{(ReturnsNonGenericTask ? "" : $"{ResultPackLocalName}.{ReturnValuePropertyName}")}});
                        }
                    }
                    try {
                        // Add result listener
                        {{OnReceiveResultEventName}} += {{ResultCallbackLocalName}};
                {{(ReturnsNonGenericTask
                    ? $$"""
                        // Await completion
                        await {{ResultAwaiterLocalName}}.Task.WaitAsync(TimeSpan.FromSeconds(Timeout));
                """
                    : $$"""
                        // Await result
                        {{ReturnTypeAsValue}} ReturnValue = await {{ResultAwaiterLocalName}}.Task.WaitAsync(TimeSpan.FromSeconds(Timeout));
                        // Return result
                        return ReturnValue;
                """)}}
                    }
                    finally {
                        // Remove result listener
                        {{OnReceiveResultEventName}} -= {{ResultCallbackLocalName}};
                    }
                }
                """);
        }
        // Receive (returns void)
        if (Input.Symbol.ReturnsVoid) {
            Definitions.Add($$"""
                [EditorBrowsable(EditorBrowsableState.Never)]
                internal void {{ReceiveMethodName}}(int {{SenderIdParameterName}}, {{nameof(RemPacket)}} {{PacketLocalName}}) {
                    // Deserialize arguments pack
                    {{SendArgumentsPackTypeName}} {{DeserializedArgumentsPackLocalName}} = MemoryPackSerializer.Deserialize<{{SendArgumentsPackTypeName}}>({{PacketLocalName}}.{{nameof(RemPacket.ArgumentsPack)}});
                    
                    // Call target method
                    {{Input.Symbol.Name}}({{string.Join(", ", SendTargetMethodArguments)}});
                }
                """);
        }
        // Receive (returns value)
        else {
            Definitions.Add($$"""
                [EditorBrowsable(EditorBrowsableState.Never)]
                internal {{(ReturnsTask ? "async " : "")}}void {{ReceiveMethodName}}(int {{SenderIdParameterName}}, {{nameof(RemPacket)}} {{PacketLocalName}}) {
                    // Message
                    if ({{PacketLocalName}}.{{nameof(RemPacket.Type)}} is {{nameof(RemPacketType)}}.{{nameof(RemPacketType.Message)}}) {
                        // Deserialize send arguments pack
                        {{SendArgumentsPackTypeName}} {{DeserializedArgumentsPackLocalName}} = MemoryPackSerializer.Deserialize<{{SendArgumentsPackTypeName}}>({{PacketLocalName}}.{{nameof(RemPacket.ArgumentsPack)}});
                    
                        // Call target method
                        {{(ReturnsTask ? "_ = " : "")}}{{Input.Symbol.Name}}({{string.Join(", ", SendTargetMethodArguments)}});
                    }
                    // Request
                    else if ({{PacketLocalName}}.{{nameof(RemPacket.Type)}} is {{nameof(RemPacketType)}}.{{nameof(RemPacketType.Request)}}) {
                        // Deserialize request arguments pack
                        {{RequestArgumentsPackTypeName}} {{DeserializedArgumentsPackLocalName}} = MemoryPackSerializer.Deserialize<{{RequestArgumentsPackTypeName}}>({{PacketLocalName}}.{{nameof(RemPacket.ArgumentsPack)}});

                        // Call target method
                        {{(ReturnsNonGenericTask ? "" : $"{ReturnTypeAsValue} {ReturnValueLocalName} = ")}}{{(ReturnsTask ? "await " : "")}}{{Input.Symbol.Name}}({{string.Join(", ", SendTargetMethodArguments)}});

                        // Create arguments pack
                        {{ResultArgumentsPackTypeName}} {{ArgumentsPackLocalName}} = new({{DeserializedArgumentsPackLocalName}}.{{RequestIdPropertyName}}{{(ReturnsNonGenericTask ? "" : $", {ReturnValueLocalName}")}});
                        // Serialize arguments pack
                        byte[] {{SerializedArgumentsPackLocalName}} = MemoryPackSerializer.Serialize({{ArgumentsPackLocalName}});
                    
                        // Create packet
                        {{nameof(RemPacket)}} {{ResultPacketLocalName}} = new({{nameof(RemPacketType)}}.{{nameof(RemPacketType.Result)}}, this.GetPath(), nameof({{Input.Symbol.ContainingType}}.{{Input.Symbol.Name}}), {{SerializedArgumentsPackLocalName}});
                        // Serialize packet
                        byte[] {{SerializedResultPacketLocalName}} = MemoryPackSerializer.Serialize({{ResultPacketLocalName}});
                
                        // Send packet back to sender ID
                        ((SceneMultiplayer)this.Multiplayer).SendBytes(
                            bytes: {{SerializedResultPacketLocalName}},
                            id: {{SenderIdParameterName}},
                            mode: MultiplayerPeer.TransferModeEnum.{{RemAttribute.Mode}},
                            channel: {{RemAttribute.Channel}}
                        );
                    }
                    // Result
                    else if ({{PacketLocalName}}.{{nameof(RemPacket.Type)}} is {{nameof(RemPacketType)}}.{{nameof(RemPacketType.Result)}}) {
                        // Deserialize result arguments pack
                        {{ResultArgumentsPackTypeName}} {{DeserializedArgumentsPackLocalName}} = MemoryPackSerializer.Deserialize<{{ResultArgumentsPackTypeName}}>({{PacketLocalName}}.{{nameof(RemPacket.ArgumentsPack)}});
                        
                        // Invoke receive event
                        {{OnReceiveResultEventName}}?.Invoke({{DeserializedArgumentsPackLocalName}});
                    }
                }
                """);
        }
        // Send Arguments Pack & Formatter
        Definitions.Add($$"""
            [EditorBrowsable(EditorBrowsableState.Never)]
            internal record struct {{SendArgumentsPackTypeName}}({{string.Join(", ", SendArgumentsPackProperties)}}) {
                // Formatter
                {{GenerateMemoryPackFormatterCode("internal", FormatterTypeName, SendArgumentsPackTypeName, "    ",
                    RemoteParameters.Select(Parameter => (Parameter.Name, Parameter.Type.ToString()))
                )}}
            }
            """);
        // Request Arguments Pack & Formatter
        if (!Input.Symbol.ReturnsVoid) {
            Definitions.Add($$"""
                [EditorBrowsable(EditorBrowsableState.Never)]
                internal record struct {{RequestArgumentsPackTypeName}}({{string.Join(", ", RequestArgumentsPackProperties)}}) {
                    // Formatter
                    {{GenerateMemoryPackFormatterCode("internal", FormatterTypeName, RequestArgumentsPackTypeName, "    ",
                        RemoteParameters.Select(Parameter => (Parameter.Name, Parameter.Type.ToString()))
                            .Prepend((RequestIdPropertyName, "Guid"))
                    )}}
                }
                """);
        }
        // Result Arguments Pack & Formatter
        if (!Input.Symbol.ReturnsVoid) {
            Definitions.Add($$"""
                [EditorBrowsable(EditorBrowsableState.Never)]
                internal record struct {{ResultArgumentsPackTypeName}}({{string.Join(", ", ResultArgumentsPackProperties)}}) {
                    // Formatter
                    {{GenerateMemoryPackFormatterCode("internal", FormatterTypeName, ResultArgumentsPackTypeName, "    ", ReturnsNonGenericTask
                        ? [(RequestIdPropertyName, "Guid")]
                        : [(RequestIdPropertyName, "Guid"), (ReturnValuePropertyName, ReturnTypeAsValue.ToString())]
                    )}}
                }
                """);
        }

        // Generated source
        string GeneratedSource = $"""
            #nullable enable

            using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.ComponentModel;
            using System.Threading.Tasks;
            using Godot;
            using MemoryPack;
            using RemSend;

            {Input.Symbol.ContainingType.GeneratePartialType(string.Join("\n\n", Definitions))}
            """;
        return (GeneratedSource, null);
    }
    protected override (string? GeneratedCode, DiagnosticDetail? Error) GenerateCode(IEnumerable<GenerateInput> Inputs) {
        // Method names
        string SetupMethodName = "Setup";
        string HandlePacketMethodName = "HandlePacket";
        string ReceiveMethodName = "Receive{0}";
        // Parameter names
        string RootNodeParameterName = "Root";
        string SceneMultiplayerParameterName = "Multiplayer";
        string SenderIdParameterName = "SenderId";
        string PacketBytesParameterName = "PacketBytes";
        // Local names
        string PacketLocalName = "RemPacket";
        string NodeLocalName = "TargetNode";
        // Type names
        string RemSendServiceTypeName = "RemSendService";
        string RemPacketFormatterTypeName = $"{nameof(RemPacket)}Formatter";
        string SendArgumentsPackTypeName = "{0}SendPack";
        string RequestArgumentsPackTypeName = "{0}RequestPack";
        string ResultArgumentsPackTypeName = "{0}ResultPack";
        string FormatterTypeName = "Formatter";

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
                    // Find target receive method
            {{string.Join("\n", Inputs.Select(Input => $$"""
                    if ({{NodeLocalName}} is {{Input.Symbol.ContainingType}}) {
                        if ({{PacketLocalName}}.{{nameof(RemPacket.MethodName)}} is nameof({{Input.Symbol.ContainingType}}.{{Input.Symbol.Name}})) {
                            (({{Input.Symbol.ContainingType}}){{NodeLocalName}}).{{string.Format(ReceiveMethodName, Input.Symbol.Name)}}({{SenderIdParameterName}}, {{PacketLocalName}});
                        }
                    }
            """))}}
                }

                static {{RemSendServiceTypeName}}() {
                    // Register MemoryPack formatters
                    MemoryPackFormatterProvider.Register(new {{RemPacketFormatterTypeName}}());
            {{string.Join("\n", Inputs.Select(Input => $$"""
                    MemoryPackFormatterProvider.Register(new {{Input.Symbol.ContainingType}}.{{string.Format(SendArgumentsPackTypeName, Input.Symbol.Name)}}.{{FormatterTypeName}}());
            {{(Input.Symbol.ReturnsVoid ? "" : $$"""
                    MemoryPackFormatterProvider.Register(new {{Input.Symbol.ContainingType}}.{{string.Format(RequestArgumentsPackTypeName, Input.Symbol.Name)}}.{{FormatterTypeName}}());
                    MemoryPackFormatterProvider.Register(new {{Input.Symbol.ContainingType }}.{{string.Format(ResultArgumentsPackTypeName, Input.Symbol.Name)}}.{{FormatterTypeName}}());
            """)}}
            """.TrimEnd()))}}
                }

                // Formatter for {{nameof(RemPacket)}} because MemoryPack doesn't support .NET Standard 2.0
                {{GenerateMemoryPackFormatterCode("private", RemPacketFormatterTypeName, nameof(RemPacket), "    ", [
                    (nameof(RemPacket.Type), nameof(RemPacketType)),
                    (nameof(RemPacket.NodePath), "string"),
                    (nameof(RemPacket.MethodName), "string"),
                    (nameof(RemPacket.ArgumentsPack), "byte[]"),
                ])}}
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
    private static string GenerateMemoryPackFormatterCode(string AccessModifier, string FormatterName, string TypeName, string Indent, IEnumerable<(string Name, string Type)> Properties) {
        return $$"""
            {{AccessModifier}} sealed class {{FormatterName}} : MemoryPackFormatter<{{TypeName}}> {
            {{Indent}}    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> Writer, scoped ref {{TypeName}} Value) {
            {{Indent}}        {{string.Join($"\n{Indent}        ", Properties.Select(Property => $"Writer.WriteValue(Value.@{Property.Name});"))}}
            {{Indent}}    }
            {{Indent}}    public override void Deserialize(ref MemoryPackReader Reader, scoped ref {{TypeName}} Value) {
            {{Indent}}        Value = new() {
            {{Indent}}            {{string.Join($"\n{Indent}            ", Properties.Select(Property => $"@{Property.Name} = Reader.ReadValue<{Property.Type}>()!,"))}}
            {{Indent}}        };
            {{Indent}}    }
            {{Indent}}}
            """;
    }
    private static string EscapeLocalName(string LocalName, IMethodSymbol MethodSymbol) {
        if (MethodSymbol.Parameters.Any(Parameter => Parameter.Name == LocalName)) {
            return EscapeLocalName("_" + LocalName, MethodSymbol);
        }
        if (MethodSymbol.TypeParameters.Any(Parameter => Parameter.Name == LocalName)) {
            return EscapeLocalName("_" + LocalName, MethodSymbol);
        }
        return LocalName;
    }
}