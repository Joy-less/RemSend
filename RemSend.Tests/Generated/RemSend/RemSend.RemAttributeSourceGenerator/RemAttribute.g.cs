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
            if (RemPacket.MethodName is nameof(RemSend.Tests.MyNode.GetMagicNumber)) {
                ((RemSend.Tests.MyNode)TargetNode).ReceiveGetMagicNumber(SenderId, RemPacket);
            }
        }
        if (TargetNode is RemSend.Tests.MyNode) {
            if (RemPacket.MethodName is nameof(RemSend.Tests.MyNode.WaitSomeTime)) {
                ((RemSend.Tests.MyNode)TargetNode).ReceiveWaitSomeTime(SenderId, RemPacket);
            }
        }
    }

    static RemSendService() {
        // Register MemoryPack formatters
        MemoryPackFormatterProvider.Register(new RemPacketFormatter());
        MemoryPackFormatterProvider.Register(new RemSend.Tests.MyNode.GetMagicNumberSendPack.Formatter());
        MemoryPackFormatterProvider.Register(new RemSend.Tests.MyNode.GetMagicNumberRequestPack.Formatter());
        MemoryPackFormatterProvider.Register(new RemSend.Tests.MyNode.GetMagicNumberResultPack.Formatter());
        MemoryPackFormatterProvider.Register(new RemSend.Tests.MyNode.WaitSomeTimeSendPack.Formatter());
        MemoryPackFormatterProvider.Register(new RemSend.Tests.MyNode.WaitSomeTimeRequestPack.Formatter());
        MemoryPackFormatterProvider.Register(new RemSend.Tests.MyNode.WaitSomeTimeResultPack.Formatter());
    }

    // Formatter for RemPacket because MemoryPack doesn't support .NET Standard 2.0
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
}