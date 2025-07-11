using Microsoft.CodeAnalysis;
using AttributeSourceGenerators;

namespace RemSend;

[Generator]
internal class RemAttributeSourceGenerator : SourceGeneratorForMethodWithAttribute<RemAttribute> {
    protected override (string? GeneratedCode, DiagnosticDetail? Error) GenerateCode(GenerateInput Input) {
        RemAttribute RemAttribute = ReconstructRemAttribute(Input.Attribute);

        // Method names
        string QualifiedMethodName = $"{Input.Symbol.ContainingType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}.{Input.Symbol.Name}";
        string SendCoreMethodName = $"SendCore{Input.Symbol.Name}";
        string SendMethodName = $"Send{Input.Symbol.Name}";
        string BroadcastMethodName = $"Broadcast{Input.Symbol.Name}";
        string RequestMethodName = $"Request{Input.Symbol.Name}";
        string ReceiveMethodName = $"Receive{Input.Symbol.Name}";
        string VerifyAccessMethodName = "VerifyAccess";
        string CreatePacketMethodName = "CreatePacket";
        string SendPacketMethodName = "SendPacket";
        // Local names
        string PeerIdLocalName = EscapeVariableName("PeerId", Input.Symbol);
        string PeerIdsLocalName = EscapeVariableName("PeerIds", Input.Symbol);
        string SenderIdLocalName = EscapeVariableName("SenderId", Input.Symbol);
        string TimeoutLocalName = EscapeVariableName("Timeout", Input.Symbol);
        string PacketLocalName = EscapeVariableName("RemPacket", Input.Symbol);
        string SerializedPacketLocalName = EscapeVariableName("SerializedRemPacket", Input.Symbol);
        string ResultPacketLocalName = EscapeVariableName("ResultRemPacket", Input.Symbol);
        string SerializedResultPacketLocalName = EscapeVariableName("SerializedResultRemPacket", Input.Symbol);
        string DeserializedArgumentsPackLocalName = EscapeVariableName("DeserializedArgumentsPack", Input.Symbol);
        string RequestIdLocalName = EscapeVariableName("RequestId", Input.Symbol);
        string ReturnValueLocalName = EscapeVariableName("ReturnValue", Input.Symbol);
        string ResultAwaiterLocalName = EscapeVariableName("ResultAwaiter", Input.Symbol);
        string ResultCallbackLocalName = EscapeVariableName("ResultCallback", Input.Symbol);
        string ResultPackLocalName = EscapeVariableName("ResultPack", Input.Symbol);
        // Type names
        string RemSendServiceTypeName = "RemSendService";
        string SendArgumentsPackTypeName = $"{Input.Symbol.Name}SendPack";
        string RequestArgumentsPackTypeName = $"{Input.Symbol.Name}RequestPack";
        string ResultArgumentsPackTypeName = $"{Input.Symbol.Name}ResultPack";
        string FormatterTypeName = "Formatter";
        // Property names
        string RequestIdPropertyName = "RequestId";
        string ReturnValuePropertyName = "ReturnValue";
        string RemAttributePropertyName = $"{Input.Symbol.Name}RemAttribute";
        // Event names
        string OnReceiveResultEventName = $"OnReceive{Input.Symbol.Name}Result";

        // Access modifiers
        string AccessModifier = Input.Symbol.GetDeclaredAccessibility();

        // Filter parameters by category
        List<IParameterSymbol> PseudoParameters = [.. Input.Symbol.Parameters.Where(Parameter => Parameter.HasAttribute<SenderAttribute>())];
        List<IParameterSymbol> RemoteParameters = [.. Input.Symbol.Parameters.Except(PseudoParameters)];

        // Handle task return values
        bool ReturnsTask = Input.Symbol.ReturnType.IsTaskType();
        bool ReturnsNonGenericTask = Input.Symbol.ReturnType.IsNonGenericTaskType();
        ITypeSymbol ReturnTypeAsTask = Input.Symbol.GetReturnTypeAsTask(Input.Compilation);
        ITypeSymbol ReturnTypeAsValue = Input.Symbol.GetReturnTypeAsValue(Input.Compilation);

        // Parameters
        List<string> SendMethodCoreParameters = [$"int {PeerIdLocalName}", $"{nameof(RemPacket)} {PacketLocalName}", $"byte[] {SerializedPacketLocalName}"];
        List<string> SendMethodParameters = [.. RemoteParameters.Select(Parameter => Parameter.GetParameterDeclaration())];
        List<string> SendMethodOneParameters = [$"int {PeerIdLocalName}", .. SendMethodParameters];
        List<string> SendMethodMultiParameters = [$"IEnumerable<int>? {PeerIdsLocalName}", .. SendMethodParameters];
        List<string> BroadcastMethodParameters = [.. SendMethodParameters];
        List<string> RequestMethodParameters = [$"int {PeerIdLocalName}", $"TimeSpan {TimeoutLocalName}", .. SendMethodParameters];

        // Arguments
        List<string> SendCoreArguments = [PeerIdLocalName, PacketLocalName, SerializedPacketLocalName];
        List<string> SendArgumentsPackArguments = [.. RemoteParameters.Select(Parameter => $"@{Parameter.Name}")];
        List<string> RequestArgumentsPackArguments = [RequestIdLocalName, .. SendArgumentsPackArguments];
        List<string> SendBroadcastArguments = ["0", .. RemoteParameters.Select(Parameter => $"@{Parameter.Name}")];
        List<string> RequestCallbackArguments = [PeerIdLocalName, TimeoutLocalName, .. RemoteParameters.Select(Parameter => $"@{Parameter.Name}")];

        // Properties
        List<string> SendArgumentsPackProperties = [.. SendMethodParameters];
        List<string> RequestArgumentsPackProperties = [$"Guid {RequestIdPropertyName}", .. SendArgumentsPackProperties];
        List<string> ResultArgumentsPackProperties = ReturnsNonGenericTask ? [$"Guid {RequestIdPropertyName}"] : [$"Guid {RequestIdPropertyName}", $"{ReturnTypeAsValue} {ReturnValuePropertyName}"];

        // Arguments for locally calling target method
        List<string> TargetMethodArguments = [.. RemoteParameters.Select(Parameter => $"{DeserializedArgumentsPackLocalName}.@{Parameter.Name}")];
        List<string> TargetMethodCallLocalArguments = [.. RemoteParameters.Select(Parameter => $"@{Parameter.Name}")];
        // Pass pseudo parameters
        foreach (IParameterSymbol Parameter in PseudoParameters) {
            if (Parameter.HasAttribute<SenderAttribute>()) {
                TargetMethodArguments.Insert(Parameter.Ordinal, SenderIdLocalName);
                TargetMethodCallLocalArguments.Insert(Parameter.Ordinal, "0");
            }
        }

        // XML references
        string MethodSeeXml = Input.Symbol.GenerateSeeXml();

        // Definitions (methods, types)
        List<string> Definitions = [];
        // Attribute
        Definitions.Add($$"""
            /// <summary>
            /// The <see cref="{{nameof(RemAttribute)}}"/> defined on {{MethodSeeXml}}.<br/>
            /// The properties of this attribute can be changed to reconfigure the remote method.
            /// </summary>
            {{AccessModifier}} {{nameof(RemAttribute)}} {{RemAttributePropertyName}} { get; set; } = new() {
                {{nameof(RemAttribute.Access)}} = {{nameof(RemAccess)}}.{{RemAttribute.Access}},
                {{nameof(RemAttribute.CallLocal)}} = {{(RemAttribute.CallLocal ? "true" : "false")}},
                {{nameof(RemAttribute.Mode)}} = {{nameof(RemMode)}}.{{RemAttribute.Mode}},
                {{nameof(RemAttribute.Channel)}} = {{RemAttribute.Channel}},
            };
            """);
        // Send One Core
        Definitions.Add($$"""
            /// <summary>
            /// Remotely calls {{MethodSeeXml}} on the given peer using the given packet.<br/>
            /// Set <paramref name="{{PeerIdLocalName}}"/> to 0 to broadcast to all eligible peers.<br/>
            /// Set <paramref name="{{PeerIdLocalName}}"/> to 1 to send to the authority.
            /// </summary>
            [EditorBrowsable(EditorBrowsableState.Never)]
            internal void {{SendCoreMethodName}}({{string.Join(", ", SendMethodCoreParameters)}}) {
                // Send packet to local peer
                if ({{PeerIdLocalName}} is 0 || {{PeerIdLocalName}} == this.Multiplayer.GetUniqueId()) {
                    if ({{RemAttributePropertyName}}.{{nameof(RemAttribute.CallLocal)}}) {
                        // Call remote method locally
                        {{ReceiveMethodName}}(this.Multiplayer.GetUniqueId(), {{PacketLocalName}});

                        // Don't send remotely unless broadcasting
                        if ({{PeerIdLocalName}} is not 0) {
                            return;
                        }
                    }
                    else {
                        // Ensure authorized to call locally
                        if ({{PeerIdLocalName}} is not 0) {
                            throw new {{nameof(MethodAccessException)}}("Not authorized to call on the local peer");
                        }
                    }
                }
                
                // Send packet to remote peer
                {{RemSendServiceTypeName}}.{{SendPacketMethodName}}({{PeerIdLocalName}}, this, {{RemAttributePropertyName}}, {{SerializedPacketLocalName}});
            }
            """);
        // Send One
        Definitions.Add($$"""
            /// <summary>
            /// Remotely calls {{MethodSeeXml}} on the given peer.<br/>
            /// Set <paramref name="{{PeerIdLocalName}}"/> to 0 to broadcast to all eligible peers.<br/>
            /// Set <paramref name="{{PeerIdLocalName}}"/> to 1 to send to the authority.
            /// </summary>
            {{AccessModifier}} void {{SendMethodName}}({{string.Join(", ", SendMethodOneParameters)}}) {
                // Create send packet
                {{nameof(RemPacket)}} {{PacketLocalName}} = {{RemSendServiceTypeName}}.{{CreatePacketMethodName}}({{nameof(RemPacketType)}}.{{nameof(RemPacketType.Send)}}, this.GetPath(), nameof({{QualifiedMethodName}}), new {{SendArgumentsPackTypeName}}({{string.Join(", ", SendArgumentsPackArguments)}}));
                // Serialize send packet
                byte[] {{SerializedPacketLocalName}} = MemoryPackSerializer.Serialize({{PacketLocalName}});

                // Send packet to peer
                {{SendCoreMethodName}}({{string.Join(", ", SendCoreArguments)}});
            }
            """);
        // Send Multi
        Definitions.Add($$"""
            /// <summary>
            /// Remotely calls {{MethodSeeXml}} on each peer.
            /// </summary>
            {{AccessModifier}} void {{SendMethodName}}({{string.Join(", ", SendMethodMultiParameters)}}) {
                // Skip if no peers
                if ({{PeerIdsLocalName}} is null || !{{PeerIdsLocalName}}.Any()) {
                    return;
                }

                // Create send packet
                {{nameof(RemPacket)}} {{PacketLocalName}} = {{RemSendServiceTypeName}}.{{CreatePacketMethodName}}({{nameof(RemPacketType)}}.{{nameof(RemPacketType.Send)}}, this.GetPath(), nameof({{QualifiedMethodName}}), new {{SendArgumentsPackTypeName}}({{string.Join(", ", SendArgumentsPackArguments)}}));
                // Serialize send packet
                byte[] {{SerializedPacketLocalName}} = MemoryPackSerializer.Serialize({{PacketLocalName}});
                
                // Send packet to each peer
                foreach (int {{PeerIdLocalName}} in {{PeerIdsLocalName}}) {
                    {{SendCoreMethodName}}({{string.Join(", ", SendCoreArguments)}});
                }
            }
            """);
        // Broadcast
        Definitions.Add($$"""
            /// <summary>
            /// Remotely calls {{MethodSeeXml}} on all eligible peers.
            /// </summary>
            {{AccessModifier}} void {{BroadcastMethodName}}({{string.Join(", ", BroadcastMethodParameters)}}) {
                {{SendMethodName}}({{string.Join(", ", SendBroadcastArguments)}});
            }
            """);
        // Request
        if (!Input.Symbol.ReturnsVoid) {
            // On Receive Result
            Definitions.Add($$"""
                [EditorBrowsable(EditorBrowsableState.Never)]
                internal event Action<int, {{ResultArgumentsPackTypeName}}>? {{OnReceiveResultEventName}};
                """);
            // Request
            Definitions.Add($$"""
                /// <summary>
                /// Remotely calls {{MethodSeeXml}} on the given peer and awaits the return value.<br/>
                /// Set <paramref name="{{PeerIdLocalName}}"/> to 1 to send to the authority.
                /// </summary>
                {{AccessModifier}} async {{ReturnTypeAsTask}} {{RequestMethodName}}({{string.Join(", ", RequestMethodParameters)}}) {
                    // Generate request ID
                    Guid {{RequestIdLocalName}} = Guid.NewGuid();

                    // Create request packet
                    {{nameof(RemPacket)}} {{PacketLocalName}} = {{RemSendServiceTypeName}}.{{CreatePacketMethodName}}({{nameof(RemPacketType)}}.{{nameof(RemPacketType.Request)}}, this.GetPath(), nameof({{QualifiedMethodName}}), new {{RequestArgumentsPackTypeName}}({{string.Join(", ", RequestArgumentsPackArguments)}}));
                    // Serialize request packet
                    byte[] {{SerializedPacketLocalName}} = MemoryPackSerializer.Serialize({{PacketLocalName}});

                    // Create result listener
                    TaskCompletionSource{{(ReturnsNonGenericTask ? "" : $"<{ReturnTypeAsValue}>")}} {{ResultAwaiterLocalName}} = new();
                    void {{ResultCallbackLocalName}}(int {{SenderIdLocalName}}, {{ResultArgumentsPackTypeName}} {{ResultPackLocalName}}) {
                        if ({{SenderIdLocalName}} == {{PeerIdLocalName}} && {{ResultPackLocalName}}.{{RequestIdPropertyName}} == {{RequestIdLocalName}}) {
                            {{ResultAwaiterLocalName}}.TrySetResult({{(ReturnsNonGenericTask ? "" : $"{ResultPackLocalName}.{ReturnValuePropertyName}")}});
                        }
                    }
                    try {
                        // Add result listener
                        {{OnReceiveResultEventName}} += {{ResultCallbackLocalName}};
                        // Send packet to peer
                        {{SendCoreMethodName}}({{string.Join(", ", SendCoreArguments)}});
                {{(ReturnsNonGenericTask
                    ? $$"""
                        // Await completion
                        await {{ResultAwaiterLocalName}}.Task.WaitAsync(Timeout);
                """
                    : $$"""
                        // Await result
                        {{ReturnTypeAsValue}} ReturnValue = await {{ResultAwaiterLocalName}}.Task.WaitAsync(Timeout);
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
                internal void {{ReceiveMethodName}}(int {{SenderIdLocalName}}, {{nameof(RemPacket)}} {{PacketLocalName}}) {
                    // Send
                    if ({{PacketLocalName}}.{{nameof(RemPacket.Type)}} is {{nameof(RemPacketType)}}.{{nameof(RemPacketType.Send)}}) {
                        // Verify access
                        {{RemSendServiceTypeName}}.{{VerifyAccessMethodName}}({{RemAttributePropertyName}}.{{nameof(RemAttribute.Access)}}, {{SenderIdLocalName}}, this.Multiplayer.GetUniqueId());
                        
                        // Deserialize arguments pack
                        {{SendArgumentsPackTypeName}} {{DeserializedArgumentsPackLocalName}} = MemoryPackSerializer.Deserialize<{{SendArgumentsPackTypeName}}>({{PacketLocalName}}.{{nameof(RemPacket.ArgumentsPack)}});
                        
                        // Call target method
                        {{Input.Symbol.Name}}({{string.Join(", ", TargetMethodArguments)}});
                    }
                }
                """);
        }
        // Receive (returns value)
        else {
            Definitions.Add($$"""
                [EditorBrowsable(EditorBrowsableState.Never)]
                internal {{(ReturnsTask ? "async " : "")}}void {{ReceiveMethodName}}(int {{SenderIdLocalName}}, {{nameof(RemPacket)}} {{PacketLocalName}}) {
                    // Send
                    if ({{PacketLocalName}}.{{nameof(RemPacket.Type)}} is {{nameof(RemPacketType)}}.{{nameof(RemPacketType.Send)}}) {
                        // Verify access
                        {{RemSendServiceTypeName}}.{{VerifyAccessMethodName}}({{RemAttributePropertyName}}.{{nameof(RemAttribute.Access)}}, {{SenderIdLocalName}}, this.Multiplayer.GetUniqueId());
                        
                        // Deserialize arguments pack
                        {{SendArgumentsPackTypeName}} {{DeserializedArgumentsPackLocalName}} = MemoryPackSerializer.Deserialize<{{SendArgumentsPackTypeName}}>({{PacketLocalName}}.{{nameof(RemPacket.ArgumentsPack)}});
                        
                        // Call target method
                        {{(ReturnsTask ? "await " : "")}}{{Input.Symbol.Name}}({{string.Join(", ", TargetMethodArguments)}});
                    }
                    // Request
                    else if ({{PacketLocalName}}.{{nameof(RemPacket.Type)}} is {{nameof(RemPacketType)}}.{{nameof(RemPacketType.Request)}}) {
                        // Deserialize arguments pack
                        {{RequestArgumentsPackTypeName}} {{DeserializedArgumentsPackLocalName}} = MemoryPackSerializer.Deserialize<{{RequestArgumentsPackTypeName}}>({{PacketLocalName}}.{{nameof(RemPacket.ArgumentsPack)}});

                        // Call target method
                        {{(ReturnsNonGenericTask ? "" : $"{ReturnTypeAsValue} {ReturnValueLocalName} = ")}}{{(ReturnsTask ? "await " : "")}}{{Input.Symbol.Name}}({{string.Join(", ", TargetMethodArguments)}});

                        // Create result packet
                        {{nameof(RemPacket)}} {{ResultPacketLocalName}} = {{RemSendServiceTypeName}}.{{CreatePacketMethodName}}({{nameof(RemPacketType)}}.{{nameof(RemPacketType.Result)}}, this.GetPath(), nameof({{QualifiedMethodName}}), new {{ResultArgumentsPackTypeName}}({{DeserializedArgumentsPackLocalName}}.{{RequestIdPropertyName}}{{(ReturnsNonGenericTask ? "" : $", {ReturnValueLocalName}")}}));
                        // Serialize result packet
                        byte[] {{SerializedResultPacketLocalName}} = MemoryPackSerializer.Serialize({{ResultPacketLocalName}});

                        // Send result packet back to sender
                        {{SendCoreMethodName}}({{SenderIdLocalName}}, {{ResultPacketLocalName}}, {{SerializedResultPacketLocalName}});
                    }
                    // Result
                    else if ({{PacketLocalName}}.{{nameof(RemPacket.Type)}} is {{nameof(RemPacketType)}}.{{nameof(RemPacketType.Result)}}) {
                        // Deserialize result arguments pack
                        {{ResultArgumentsPackTypeName}} {{DeserializedArgumentsPackLocalName}} = MemoryPackSerializer.Deserialize<{{ResultArgumentsPackTypeName}}>({{PacketLocalName}}.{{nameof(RemPacket.ArgumentsPack)}});
                        
                        // Invoke receive event
                        {{OnReceiveResultEventName}}?.Invoke({{SenderIdLocalName}}, {{DeserializedArgumentsPackLocalName}});
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
                    {{GenerateMemoryPackFormatterCode("internal", FormatterTypeName, RequestArgumentsPackTypeName, "    ", [
                        (RequestIdPropertyName, "Guid"),
                        .. RemoteParameters.Select(Parameter => (Parameter.Name, Parameter.Type.ToString()))
                    ])}}
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
        string ReceivePacketMethodName = "ReceivePacket";
        string ReceiveMethodName = "Receive{0}";
        string RemModeToTransferModeEnumMethodName = "RemModeToTransferModeEnum";
        string VerifyAccessMethodName = "VerifyAccess";
        string RegisterMemoryPackFormattersMethodName = "RegisterMemoryPackFormatters";
        string CreatePacketMethodName = "CreatePacket";
        string SendPacketMethodName = "SendPacket";
        // Local names
        string RootNodeLocalName = "Root";
        string SceneMultiplayerLocalName = "Multiplayer";
        string AccessLocalName = "Access";
        string SenderIdLocalName = "SenderId";
        string LocalIdLocalName = "LocalId";
        string NodePathLocalName = "NodePath";
        string MethodNameLocalName = "MethodName";
        string PacketTypeLocalName = "PacketType";
        string AttributeLocalName = "Attribute";
        string PeerIdLocalName = "PeerId";
        string PacketLocalName = "RemPacket";
        string SerializedPacketLocalName = "SerializedRemPacket";
        string TargetNodeLocalName = "TargetNode";
        string IsAuthorizedLocalName = "IsAuthorized";
        string ArgumentsPackLocalName = "ArgumentsPack";
        string SerializedArgumentsPackLocalName = "SerializedArgumentsPack";
        // Type names
        string RemSendServiceTypeName = "RemSendService";
        string RemPacketFormatterTypeName = $"{nameof(RemPacket)}Formatter";
        string SendArgumentsPackTypeName = "{0}SendPack";
        string RequestArgumentsPackTypeName = "{0}RequestPack";
        string ResultArgumentsPackTypeName = "{0}ResultPack";
        string FormatterTypeName = "Formatter";

        // MemoryPack formatters
        List<string> StructRegisterTypes = [
            "Color",
            "Vector2",
            "Vector2I",
            "Vector3",
            "Vector3I",
            "Vector4",
            "Vector4I",
            "Rect2",
            "Rect2I",
            "Aabb",
            "Basis",
            "Plane",
            "Projection",
            "Quaternion",
            "Rid",
            "Transform2D",
            "Transform3D",
        ];
        List<string> CustomRegisterTypes = [
            "StringName",
            "NodePath",
        ];

        // Generated source
        string GeneratedSource = $$"""
            #nullable enable

            using System;
            using System.Text;
            using System.ComponentModel;
            using Godot;
            using MemoryPack;
            using MemoryPack.Formatters;

            namespace {{nameof(RemSend)}};

            /// <summary>
            /// Contains global methods for {{nameof(RemSend)}}.
            /// </summary>
            public static class {{RemSendServiceTypeName}} {
                /// <summary>
                /// Starts listening for packets received from <paramref name="{{SceneMultiplayerLocalName}}"/>.
                /// </summary>
                public static void {{SetupMethodName}}(SceneMultiplayer {{SceneMultiplayerLocalName}}, Node? {{RootNodeLocalName}} = null) {
                    // Default root node
                    {{RootNodeLocalName}} ??= ((SceneTree)Engine.GetMainLoop()).Root;
                    // Listen for packets
                    {{SceneMultiplayerLocalName}}.PeerPacket += ({{SenderIdLocalName}}, {{SerializedPacketLocalName}}) => {
                        {{ReceivePacketMethodName}}({{SceneMultiplayerLocalName}}, {{RootNodeLocalName}}, (int){{SenderIdLocalName}}, {{SerializedPacketLocalName}});
                    };
                }
                
                /// <summary>
                /// Converts from <see cref="{{nameof(RemMode)}}"/> to <see cref="MultiplayerPeer"/>.
                /// </summary>
                [EditorBrowsable(EditorBrowsableState.Never)]
                internal static MultiplayerPeer.TransferModeEnum {{RemModeToTransferModeEnumMethodName}}({{nameof(RemMode)}} Mode) {
                    return Mode switch {
                        {{nameof(RemMode)}}.{{nameof(RemMode.Reliable)}} => MultiplayerPeer.TransferModeEnum.Reliable,
                        {{nameof(RemMode)}}.{{nameof(RemMode.UnreliableOrdered)}} => MultiplayerPeer.TransferModeEnum.UnreliableOrdered,
                        {{nameof(RemMode)}}.{{nameof(RemMode.Unreliable)}} => MultiplayerPeer.TransferModeEnum.Unreliable,
                        _ => throw new {{nameof(NotImplementedException)}}()
                    };
                }
                
                /// <summary>
                /// Throws if the call is unauthorized.
                /// </summary>
                [EditorBrowsable(EditorBrowsableState.Never)]
                internal static void {{VerifyAccessMethodName}}({{nameof(RemAccess)}} {{AccessLocalName}}, int {{SenderIdLocalName}}, int {{LocalIdLocalName}}) {
                    bool {{IsAuthorizedLocalName}} = {{AccessLocalName}} switch {
                        {{nameof(RemAccess)}}.{{nameof(RemAccess.None)}} => false,
                        {{nameof(RemAccess)}}.{{nameof(RemAccess.Authority)}} => {{SenderIdLocalName}} is 1 or 0,
                        {{nameof(RemAccess)}}.{{nameof(RemAccess.PeerToAuthority)}} => {{LocalIdLocalName}} is 1,
                        {{nameof(RemAccess)}}.{{nameof(RemAccess.Any)}} => true,
                        _ => throw new {{nameof(NotImplementedException)}}()
                    };
                    if (!{{IsAuthorizedLocalName}}) {
                        throw new {{nameof(MethodAccessException)}}("Remote method call not authorized");
                    }
                }
                
                /// <summary>
                /// Creates a packet for a remote method call.
                /// </summary>
                [EditorBrowsable(EditorBrowsableState.Never)]
                internal static {{nameof(RemPacket)}} {{CreatePacketMethodName}}<T>({{nameof(RemPacketType)}} {{PacketTypeLocalName}}, string {{NodePathLocalName}}, string {{MethodNameLocalName}}, in T {{ArgumentsPackLocalName}}) {
                    // Serialize arguments pack
                    byte[] {{SerializedArgumentsPackLocalName}} = MemoryPackSerializer.Serialize({{ArgumentsPackLocalName}});
                    // Create packet
                    {{nameof(RemPacket)}} {{PacketLocalName}} = new({{PacketTypeLocalName}}, {{NodePathLocalName}}, {{MethodNameLocalName}}, {{SerializedArgumentsPackLocalName}});
                    return {{PacketLocalName}};
                }
                
                /// <summary>
                /// Sends a serialized packet to a peer.
                /// </summary>
                [EditorBrowsable(EditorBrowsableState.Never)]
                internal static void {{SendPacketMethodName}}(int {{PeerIdLocalName}}, Node {{TargetNodeLocalName}}, {{nameof(RemAttribute)}} {{AttributeLocalName}}, byte[] {{SerializedPacketLocalName}}) {
                    ((SceneMultiplayer){{TargetNodeLocalName}}.Multiplayer).SendBytes(
                        bytes: {{SerializedPacketLocalName}},
                        id: {{PeerIdLocalName}},
                        mode: {{RemModeToTransferModeEnumMethodName}}({{AttributeLocalName}}.{{nameof(RemAttribute.Mode)}}),
                        channel: {{AttributeLocalName}}.{{nameof(RemAttribute.Channel)}}
                    );
                }
                
                /// <summary>
                /// Finds and calls the target method for the received packet.
                /// </summary>
                private static void {{ReceivePacketMethodName}}(SceneMultiplayer {{SceneMultiplayerLocalName}}, Node {{RootNodeLocalName}}, int {{SenderIdLocalName}}, ReadOnlySpan<byte> {{SerializedPacketLocalName}}) {
                    // Get actual local sender ID
                    if ({{SenderIdLocalName}} is 0) {
                        {{SenderIdLocalName}} = {{SceneMultiplayerLocalName}}.GetUniqueId();
                    }
                    
                    // Deserialize packet
                    {{nameof(RemPacket)}} {{PacketLocalName}} = MemoryPackSerializer.Deserialize<{{nameof(RemPacket)}}>({{SerializedPacketLocalName}});

                    // Find target node
                    Node {{TargetNodeLocalName}} = {{RootNodeLocalName}}.GetNode({{SceneMultiplayerLocalName}}.RootPath).GetNode({{PacketLocalName}}.{{nameof(RemPacket.NodePath)}});
                    // Find target receive method
            {{string.Join("\n", Inputs.GroupBy(Input => Input.Symbol.ContainingType, SymbolEqualityComparer.Default).Select(TargetNode => $$"""
                    if ({{TargetNodeLocalName}} is {{TargetNode.Key}} @{{TargetNode.Key.AsIdentifier()}}) {
            {{string.Join("\n", TargetNode.Select(Input => $$"""
                        if ({{PacketLocalName}}.{{nameof(RemPacket.MethodName)}} is "{{Input.Symbol.Name}}") {
                            @{{TargetNode.Key.AsIdentifier()}}.{{string.Format(ReceiveMethodName, Input.Symbol.Name)}}({{SenderIdLocalName}}, {{PacketLocalName}});
                        }
            """))}}
                    }
            """))}}
                }
                
                /// <summary>
                /// Initializes {{RemSendServiceTypeName}}.
                /// </summary>
                static {{RemSendServiceTypeName}}() {
                    RegisterMemoryPackFormatters();
                }

                /// <summary>
                /// Registers MemoryPack formatters for the types used in {{nameof(RemSend)}}.
                /// </summary>
                private static void {{RegisterMemoryPackFormattersMethodName}}() {
                    // RemSend types
                    MemoryPackFormatterProvider.Register(new {{RemPacketFormatterTypeName}}());

                    // RemSend generated types
            {{string.Join("\n", Inputs.Select(Input => $$"""
                    MemoryPackFormatterProvider.Register(new {{Input.Symbol.ContainingType}}.{{string.Format(SendArgumentsPackTypeName, Input.Symbol.Name)}}.{{FormatterTypeName}}());
            """
            + (Input.Symbol.ReturnsVoid ? "" : $$"""
                    
                    MemoryPackFormatterProvider.Register(new {{Input.Symbol.ContainingType}}.{{string.Format(RequestArgumentsPackTypeName, Input.Symbol.Name)}}.{{FormatterTypeName}}());
                    MemoryPackFormatterProvider.Register(new {{Input.Symbol.ContainingType}}.{{string.Format(ResultArgumentsPackTypeName, Input.Symbol.Name)}}.{{FormatterTypeName}}());
            """)))}}

                    // Godot types
            {{string.Join("\n", StructRegisterTypes.Select(StructType => $$"""
                    MemoryPackFormatterProvider.Register(new UnmanagedFormatter<{{StructType}}>());
                    MemoryPackFormatterProvider.Register(new NullableFormatter<{{StructType}}>());
                    MemoryPackFormatterProvider.Register(new UnmanagedArrayFormatter<{{StructType}}>());
            """))}}
            {{string.Join("\n", CustomRegisterTypes.Select(ClassType => $$"""
                    MemoryPackFormatterProvider.Register(new {{ClassType}}Formatter());
                    MemoryPackFormatterProvider.Register(new ArrayFormatter<{{ClassType}}>());
            """))}}
                }

                // Formatter for {{nameof(RemPacket)}} (since MemoryPack doesn't support .NET Standard 2.0)
                {{GenerateMemoryPackFormatterCode("private", RemPacketFormatterTypeName, nameof(RemPacket), "    ", [
                    (nameof(RemPacket.Type), nameof(RemPacketType)),
                    (nameof(RemPacket.NodePath), "string"),
                    (nameof(RemPacket.MethodName), "string"),
                    (nameof(RemPacket.ArgumentsPack), "byte[]"),
                ])}}

                // Formatter for StringName
                private class StringNameFormatter : MemoryPackFormatter<StringName> {
                    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> Writer, scoped ref StringName? Value) {
                        Writer.WriteString(Value);
                    }
                    public override void Deserialize(ref MemoryPackReader Reader, scoped ref StringName? Value) {
                        Value = Reader.ReadString()!;
                    }
                }

                // Formatter for NodePath
                private class NodePathFormatter : MemoryPackFormatter<NodePath> {
                    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> Writer, scoped ref NodePath? Value) {
                        Writer.WriteString(Value);
                    }
                    public override void Deserialize(ref MemoryPackReader Reader, scoped ref NodePath? Value) {
                        Value = Reader.ReadString()!;
                    }
                }
            }
            """;
        return (GeneratedSource, null);
    }

    private static RemAttribute ReconstructRemAttribute(AttributeData AttributeData) {
        return new RemAttribute(
            Access: GetAttributeArgument(AttributeData, nameof(RemAttribute.Access), default(RemAccess)),
            CallLocal: GetAttributeArgument(AttributeData, nameof(RemAttribute.CallLocal), default(bool)),
            Mode: GetAttributeArgument(AttributeData, nameof(RemAttribute.Mode), default(RemMode)),
            Channel: GetAttributeArgument(AttributeData, nameof(RemAttribute.Channel), default(int))
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
    private static string EscapeVariableName(string VariableName, IMethodSymbol MethodSymbol) {
        if (MethodSymbol.Name == VariableName) {
            return EscapeVariableName("_" + VariableName, MethodSymbol);
        }
        if (MethodSymbol.Parameters.Any(Parameter => Parameter.Name == VariableName)) {
            return EscapeVariableName("_" + VariableName, MethodSymbol);
        }
        if (MethodSymbol.TypeParameters.Any(Parameter => Parameter.Name == VariableName)) {
            return EscapeVariableName("_" + VariableName, MethodSymbol);
        }
        return VariableName;
    }
}