#nullable enable

using System;
using System.Text;
using System.ComponentModel;
using Godot;
using MemoryPack;

namespace RemSend;

public static class RemSendService {
    public static void Setup(SceneMultiplayer Multiplayer, Node? Root = null) {
        // Default root node
        Root ??= ((SceneTree)Engine.GetMainLoop()).Root;
        // Listen for packets
        Multiplayer.PeerPacket += (SenderId, PacketBytes) => {
            HandlePacket(Multiplayer, Root, (int)SenderId, PacketBytes);
        };
    }

    private static void HandlePacket(SceneMultiplayer Multiplayer, Node Root, int SenderId, ReadOnlySpan<byte> PacketBytes) {
        // Deserialize packet
        RemPacket _Packet = MemoryPackSerializer.Deserialize<RemPacket>(PacketBytes);

        // Find target node
        Node _Node = Root.GetNode(Multiplayer.RootPath).GetNode(_Packet.NodePath);
        // Find target handler method
        if (_Node is @RemSend.Tests.MyNode @MyNode) {
            if (_Packet.MethodName is nameof(RemSend.Tests.MyNode.GetMagicNumber)) {
                @MyNode.ReceiveGetMagicNumber(SenderId, _Packet);
            }
        }
    }

    static RemSendService() {
        // Register MemoryPack formatters
        MemoryPackFormatterProvider.Register(new RemPacketFormatter());
        MemoryPackFormatterProvider.Register(new RemSend.Tests.MyNode.GetMagicNumberPackFormatter());
    }

    // Formatter for RemPacket because MemoryPack doesn't support .NET Standard 2.0
    private sealed class RemPacketFormatter: MemoryPackFormatter<RemPacket> {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> Writer, scoped ref RemPacket Value) {
            Writer.WriteValue(Value.NodePath);
            Writer.WriteValue(Value.MethodName);
            Writer.WriteValue(Value.ArgumentsPack);
        }
        public override void Deserialize(ref MemoryPackReader Reader, scoped ref RemPacket Value) {
            Value = new() {
                NodePath = Reader.ReadValue<string>()!,
                MethodName = Reader.ReadValue<string>()!,
                ArgumentsPack = Reader.ReadValue<byte[]>()!,
            };
        }
    }
}