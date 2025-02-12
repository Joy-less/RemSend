#nullable enable

using System;
using System.Text;
using System.ComponentModel;
using Godot;
using MemoryPack;

namespace RemSend;

public static class RemSendService {
    public static void Setup(Node Root, SceneMultiplayer Multiplayer) {
        Multiplayer.PeerPacket += (SenderId, PacketBytes) => {
            ReceivePacket(Root, Multiplayer, (int)SenderId, PacketBytes);
        };
    }

    private static void ReceivePacket(Node Root, SceneMultiplayer Multiplayer, int SenderId, ReadOnlySpan<byte> PacketBytes) {
        // Deserialize packet
        RemPacket Packet = MemoryPackSerializer.Deserialize<RemPacket>(PacketBytes);

        // Find target node
        Node Node = Root.GetNode(Multiplayer.RootPath).GetNode(Packet.NodePath);
        // Find target handler method
        if (Node is @RemSend.Tests.MyNode @MyNode) {
            if (Packet.MethodName is "DoStuff") {
                @MyNode.SendDoStuffHandler((int)SenderId, Packet);
            }
        }
    }
}