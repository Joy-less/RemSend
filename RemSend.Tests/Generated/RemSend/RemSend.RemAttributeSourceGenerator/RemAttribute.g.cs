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
            if (_Packet.MethodName is "DoStuff") {
                @MyNode.SendDoStuffHandler(SenderId, _Packet);
            }
        }
    }

    static RemSendService() {
        // Register MemoryPack formatters
    }
}