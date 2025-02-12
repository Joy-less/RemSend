#nullable enable

using System;
using System.Text;
using System.ComponentModel;
using Godot;

namespace RemSend;

public static class RemSender {
    public static void Setup(Node Root, SceneMultiplayer Multiplayer) {
        Multiplayer.PeerPacket += (SenderId, Packet) => {
            ReceivePacket(Root, Multiplayer, (int)SenderId, Packet);
        };
    }

    public static void ReceivePacket(Node Root, SceneMultiplayer Multiplayer, int SenderId, Span<byte> Packet) {
        // Deserialize node path
        NodePath NodePath = Encoding.UTF8.GetString(DecodePacketComponent(ref Packet));
        // Deserialize method name
        string MethodName = Encoding.UTF8.GetString(DecodePacketComponent(ref Packet));

        // Find target node
        Node Node = Root.GetNode(Multiplayer.RootPath).GetNode(NodePath);
        // Find target handler method
        if (Node is @Main @Main) {
            if (MethodName is "SendSayHello") {
                @Main.SendSayHelloHandler((int)SenderId, Packet);
            }
        }
    }

    public static Span<byte> DecodePacketComponent(ref Span<byte> Packet) {
        // Read component length
        int Length = BitConverter.ToInt32(Packet[..sizeof(int)]);
        Packet = Packet[sizeof(int)..];
        // Read component content
        Span<byte> Content = Packet[..Length];
        Packet = Packet[Length..];
        // Return component content
        return Content;
    }
}