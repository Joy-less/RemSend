#nullable enable

using System;
using System.Text;
using System.ComponentModel;
using Godot;
using MemoryPack;
using MemoryPack.Formatters;

namespace RemSend;

public static class RemSendService {
    public static void Setup(SceneMultiplayer Multiplayer, Node? Root = null) {
        // Default root node
        Root ??= ((SceneTree)Engine.GetMainLoop()).Root;
        // Listen for packets
        Multiplayer.PeerPacket += (SenderId, PacketBytes) => {
            ReceivePacket(Multiplayer, Root, (int)SenderId, PacketBytes);
        };
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static MultiplayerPeer.TransferModeEnum RemModeToTransferModeEnum(RemMode Mode) {
        return Mode switch {
            RemMode.Reliable => MultiplayerPeer.TransferModeEnum.Reliable,
            RemMode.UnreliableOrdered => MultiplayerPeer.TransferModeEnum.UnreliableOrdered,
            RemMode.Unreliable => MultiplayerPeer.TransferModeEnum.Unreliable,
            _ => throw new NotImplementedException()
        };
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static void VerifyAccess(RemAccess Access, int SenderId, int LocalId) {
        bool IsAuthorized = Access switch {
            RemAccess.None => false,
            RemAccess.Authority => SenderId is 1 or 0,
            RemAccess.PeerToAuthority => LocalId is 1,
            RemAccess.Any => true,
            _ => throw new NotImplementedException()
        };
        if (!IsAuthorized) {
            throw new MethodAccessException("Remote method call not authorized");
        }
    }

    private static void ReceivePacket(SceneMultiplayer Multiplayer, Node Root, int SenderId, ReadOnlySpan<byte> PacketBytes) {
        // Deserialize packet
        RemPacket RemPacket = MemoryPackSerializer.Deserialize<RemPacket>(PacketBytes);

        // Find target node
        Node TargetNode = Root.GetNode(Multiplayer.RootPath).GetNode(RemPacket.NodePath);
        // Find target receive method
        if (TargetNode is RemSend.Tests.MyNode) {
            if (RemPacket.MethodName is "GetMagicNumber") {
                ((RemSend.Tests.MyNode)TargetNode).ReceiveGetMagicNumber(SenderId, RemPacket);
            }
        }
        if (TargetNode is RemSend.Tests.MyNode) {
            if (RemPacket.MethodName is "GetMagicNumberAsync") {
                ((RemSend.Tests.MyNode)TargetNode).ReceiveGetMagicNumberAsync(SenderId, RemPacket);
            }
        }
        if (TargetNode is RemSend.Tests.MyNode) {
            if (RemPacket.MethodName is "WaitSomeTime") {
                ((RemSend.Tests.MyNode)TargetNode).ReceiveWaitSomeTime(SenderId, RemPacket);
            }
        }
        if (TargetNode is RemSend.Tests.MyNode) {
            if (RemPacket.MethodName is "SillyExample") {
                ((RemSend.Tests.MyNode)TargetNode).ReceiveSillyExample(SenderId, RemPacket);
            }
        }
    }

    static RemSendService() {
        RegisterMemoryPackFormatters();
    }

    private static void RegisterMemoryPackFormatters() {
        // RemSend types
        MemoryPackFormatterProvider.Register(new RemPacketFormatter());

        // RemSend generated types
        MemoryPackFormatterProvider.Register(new RemSend.Tests.MyNode.GetMagicNumberSendPack.Formatter());        
        MemoryPackFormatterProvider.Register(new RemSend.Tests.MyNode.GetMagicNumberRequestPack.Formatter());
        MemoryPackFormatterProvider.Register(new RemSend.Tests.MyNode.GetMagicNumberResultPack.Formatter());
        MemoryPackFormatterProvider.Register(new RemSend.Tests.MyNode.GetMagicNumberAsyncSendPack.Formatter());        
        MemoryPackFormatterProvider.Register(new RemSend.Tests.MyNode.GetMagicNumberAsyncRequestPack.Formatter());
        MemoryPackFormatterProvider.Register(new RemSend.Tests.MyNode.GetMagicNumberAsyncResultPack.Formatter());
        MemoryPackFormatterProvider.Register(new RemSend.Tests.MyNode.WaitSomeTimeSendPack.Formatter());        
        MemoryPackFormatterProvider.Register(new RemSend.Tests.MyNode.WaitSomeTimeRequestPack.Formatter());
        MemoryPackFormatterProvider.Register(new RemSend.Tests.MyNode.WaitSomeTimeResultPack.Formatter());
        MemoryPackFormatterProvider.Register(new RemSend.Tests.MyNode.SillyExampleSendPack.Formatter());

        // Godot types
        MemoryPackFormatterProvider.Register(new UnmanagedFormatter<Color>());
        MemoryPackFormatterProvider.Register(new NullableFormatter<Color>());
        MemoryPackFormatterProvider.Register(new UnmanagedArrayFormatter<Color>());
        MemoryPackFormatterProvider.Register(new UnmanagedFormatter<Vector2>());
        MemoryPackFormatterProvider.Register(new NullableFormatter<Vector2>());
        MemoryPackFormatterProvider.Register(new UnmanagedArrayFormatter<Vector2>());
        MemoryPackFormatterProvider.Register(new UnmanagedFormatter<Vector2I>());
        MemoryPackFormatterProvider.Register(new NullableFormatter<Vector2I>());
        MemoryPackFormatterProvider.Register(new UnmanagedArrayFormatter<Vector2I>());
        MemoryPackFormatterProvider.Register(new UnmanagedFormatter<Vector3>());
        MemoryPackFormatterProvider.Register(new NullableFormatter<Vector3>());
        MemoryPackFormatterProvider.Register(new UnmanagedArrayFormatter<Vector3>());
        MemoryPackFormatterProvider.Register(new UnmanagedFormatter<Vector3I>());
        MemoryPackFormatterProvider.Register(new NullableFormatter<Vector3I>());
        MemoryPackFormatterProvider.Register(new UnmanagedArrayFormatter<Vector3I>());
        MemoryPackFormatterProvider.Register(new UnmanagedFormatter<Vector4>());
        MemoryPackFormatterProvider.Register(new NullableFormatter<Vector4>());
        MemoryPackFormatterProvider.Register(new UnmanagedArrayFormatter<Vector4>());
        MemoryPackFormatterProvider.Register(new UnmanagedFormatter<Vector4I>());
        MemoryPackFormatterProvider.Register(new NullableFormatter<Vector4I>());
        MemoryPackFormatterProvider.Register(new UnmanagedArrayFormatter<Vector4I>());
        MemoryPackFormatterProvider.Register(new UnmanagedFormatter<Rect2>());
        MemoryPackFormatterProvider.Register(new NullableFormatter<Rect2>());
        MemoryPackFormatterProvider.Register(new UnmanagedArrayFormatter<Rect2>());
        MemoryPackFormatterProvider.Register(new UnmanagedFormatter<Rect2I>());
        MemoryPackFormatterProvider.Register(new NullableFormatter<Rect2I>());
        MemoryPackFormatterProvider.Register(new UnmanagedArrayFormatter<Rect2I>());
        MemoryPackFormatterProvider.Register(new UnmanagedFormatter<Aabb>());
        MemoryPackFormatterProvider.Register(new NullableFormatter<Aabb>());
        MemoryPackFormatterProvider.Register(new UnmanagedArrayFormatter<Aabb>());
        MemoryPackFormatterProvider.Register(new UnmanagedFormatter<Basis>());
        MemoryPackFormatterProvider.Register(new NullableFormatter<Basis>());
        MemoryPackFormatterProvider.Register(new UnmanagedArrayFormatter<Basis>());
        MemoryPackFormatterProvider.Register(new UnmanagedFormatter<Plane>());
        MemoryPackFormatterProvider.Register(new NullableFormatter<Plane>());
        MemoryPackFormatterProvider.Register(new UnmanagedArrayFormatter<Plane>());
        MemoryPackFormatterProvider.Register(new UnmanagedFormatter<Projection>());
        MemoryPackFormatterProvider.Register(new NullableFormatter<Projection>());
        MemoryPackFormatterProvider.Register(new UnmanagedArrayFormatter<Projection>());
        MemoryPackFormatterProvider.Register(new UnmanagedFormatter<Quaternion>());
        MemoryPackFormatterProvider.Register(new NullableFormatter<Quaternion>());
        MemoryPackFormatterProvider.Register(new UnmanagedArrayFormatter<Quaternion>());
        MemoryPackFormatterProvider.Register(new UnmanagedFormatter<Rid>());
        MemoryPackFormatterProvider.Register(new NullableFormatter<Rid>());
        MemoryPackFormatterProvider.Register(new UnmanagedArrayFormatter<Rid>());
        MemoryPackFormatterProvider.Register(new UnmanagedFormatter<Transform2D>());
        MemoryPackFormatterProvider.Register(new NullableFormatter<Transform2D>());
        MemoryPackFormatterProvider.Register(new UnmanagedArrayFormatter<Transform2D>());
        MemoryPackFormatterProvider.Register(new UnmanagedFormatter<Transform3D>());
        MemoryPackFormatterProvider.Register(new NullableFormatter<Transform3D>());
        MemoryPackFormatterProvider.Register(new UnmanagedArrayFormatter<Transform3D>());
        MemoryPackFormatterProvider.Register(new StringNameFormatter());
        MemoryPackFormatterProvider.Register(new ArrayFormatter<StringName>());
        MemoryPackFormatterProvider.Register(new NodePathFormatter());
        MemoryPackFormatterProvider.Register(new ArrayFormatter<NodePath>());
    }

    // Formatter for RemPacket (since MemoryPack doesn't support .NET Standard 2.0)
    private sealed class RemPacketFormatter : MemoryPackFormatter<RemPacket> {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> Writer, scoped ref RemPacket Value) {
            Writer.WriteValue(Value.@Type);
            Writer.WriteValue(Value.@NodePath);
            Writer.WriteValue(Value.@MethodName);
            Writer.WriteValue(Value.@ArgumentsPack);
        }
        public override void Deserialize(ref MemoryPackReader Reader, scoped ref RemPacket Value) {
            Value = new() {
                @Type = Reader.ReadValue<RemPacketType>()!,
                @NodePath = Reader.ReadValue<string>()!,
                @MethodName = Reader.ReadValue<string>()!,
                @ArgumentsPack = Reader.ReadValue<byte[]>()!,
            };
        }
    }

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