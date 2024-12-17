#nullable enable

using System;
using System.Collections.Generic;
using Godot;

using RpcMode = Godot.MultiplayerApi.RpcMode;
using TransferModeEnum = Godot.MultiplayerPeer.TransferModeEnum;

namespace RemSend;

public partial class RemSend {
    /// <value>3</value>
    public const int MaxChannel = 3;

    public static StringName GetTransferRpc(RemMode RemMode, int Channel) {
        return (RemMode, Channel) switch {
            (RemMode.Reliable, 0) => MethodName.ReliablePacketRpc,
            (RemMode.Reliable, 1) => MethodName.ReliablePacketRpc1,
            (RemMode.Reliable, 2) => MethodName.ReliablePacketRpc2,
            (RemMode.Reliable, 3) => MethodName.ReliablePacketRpc3,
            (RemMode.UnreliableOrdered, 0) => MethodName.UnreliableOrderedPacketRpc,
            (RemMode.UnreliableOrdered, 1) => MethodName.UnreliableOrderedPacketRpc1,
            (RemMode.UnreliableOrdered, 2) => MethodName.UnreliableOrderedPacketRpc2,
            (RemMode.UnreliableOrdered, 3) => MethodName.UnreliableOrderedPacketRpc3,
            (RemMode.Unreliable, 0) => MethodName.UnreliablePacketRpc,
            (RemMode.Unreliable, 1) => MethodName.UnreliablePacketRpc1,
            (RemMode.Unreliable, 2) => MethodName.UnreliablePacketRpc2,
            (RemMode.Unreliable, 3) => MethodName.UnreliablePacketRpc3,
            _ => throw new InvalidOperationException($"Remote call channel out of range (0 to {MaxChannel}): {Channel}")
        };
    }
    public static StringName GetResponseTransferRpc(int Channel) {
        return Channel switch {
            0 => MethodName.PacketResponseRpc,
            1 => MethodName.PacketResponseRpc1,
            2 => MethodName.PacketResponseRpc2,
            3 => MethodName.PacketResponseRpc3,
            _ => throw new InvalidOperationException($"Remote call channel out of range (0 to {MaxChannel}): {Channel}")
        };
    }

#region Channel 0 (Main)
    [Rpc(RpcMode.AnyPeer, TransferMode = TransferModeEnum.Reliable, TransferChannel = 0)]
    public void ReliablePacketRpc(byte[] PackedRemPacket) {
        ReceivePacket(PackedRemPacket);
    }
    [Rpc(RpcMode.AnyPeer, TransferMode = TransferModeEnum.UnreliableOrdered, TransferChannel = 0)]
    public void UnreliableOrderedPacketRpc(byte[] PackedRemPacket) {
        ReceivePacket(PackedRemPacket);
    }
    [Rpc(RpcMode.AnyPeer, TransferMode = TransferModeEnum.Unreliable, TransferChannel = 0)]
    public void UnreliablePacketRpc(byte[] PackedRemPacket) {
        ReceivePacket(PackedRemPacket);
    }
    [Rpc(RpcMode.AnyPeer, TransferMode = TransferModeEnum.Reliable, TransferChannel = 0)]
    public void PacketResponseRpc(long PacketId, byte[] PackedReturnValue) {
        ResponseAwaiters.GetValueOrDefault(PacketId)?.TrySetResult(PackedReturnValue);
    }
#endregion

#region Channel 1
    [Rpc(RpcMode.AnyPeer, TransferMode = TransferModeEnum.Reliable, TransferChannel = 1)]
    public void ReliablePacketRpc1(byte[] PackedRemPacket) {
        ReliablePacketRpc(PackedRemPacket);
    }
    [Rpc(RpcMode.AnyPeer, TransferMode = TransferModeEnum.UnreliableOrdered, TransferChannel = 1)]
    public void UnreliableOrderedPacketRpc1(byte[] PackedRemPacket) {
        UnreliableOrderedPacketRpc(PackedRemPacket);
    }
    [Rpc(RpcMode.AnyPeer, TransferMode = TransferModeEnum.Unreliable, TransferChannel = 1)]
    public void UnreliablePacketRpc1(byte[] PackedRemPacket) {
        UnreliablePacketRpc(PackedRemPacket);
    }
    [Rpc(RpcMode.AnyPeer, TransferMode = TransferModeEnum.Reliable, TransferChannel = 1)]
    public void PacketResponseRpc1(long PacketId, byte[] PackedReturnValue) {
        PacketResponseRpc(PacketId, PackedReturnValue);
    }
#endregion

#region Channel 2
    [Rpc(RpcMode.AnyPeer, TransferMode = TransferModeEnum.Reliable, TransferChannel = 2)]
    public void ReliablePacketRpc2(byte[] PackedRemPacket) {
        ReliablePacketRpc(PackedRemPacket);
    }
    [Rpc(RpcMode.AnyPeer, TransferMode = TransferModeEnum.UnreliableOrdered, TransferChannel = 2)]
    public void UnreliableOrderedPacketRpc2(byte[] PackedRemPacket) {
        UnreliableOrderedPacketRpc(PackedRemPacket);
    }
    [Rpc(RpcMode.AnyPeer, TransferMode = TransferModeEnum.Unreliable, TransferChannel = 2)]
    public void UnreliablePacketRpc2(byte[] PackedRemPacket) {
        UnreliablePacketRpc(PackedRemPacket);
    }
    [Rpc(RpcMode.AnyPeer, TransferMode = TransferModeEnum.Reliable, TransferChannel = 2)]
    public void PacketResponseRpc2(long PacketId, byte[] PackedReturnValue) {
        PacketResponseRpc(PacketId, PackedReturnValue);
    }
#endregion

#region Channel 3
    [Rpc(RpcMode.AnyPeer, TransferMode = TransferModeEnum.Reliable, TransferChannel = 3)]
    public void ReliablePacketRpc3(byte[] PackedRemPacket) {
        ReliablePacketRpc(PackedRemPacket);
    }
    [Rpc(RpcMode.AnyPeer, TransferMode = TransferModeEnum.UnreliableOrdered, TransferChannel = 3)]
    public void UnreliableOrderedPacketRpc3(byte[] PackedRemPacket) {
        UnreliableOrderedPacketRpc(PackedRemPacket);
    }
    [Rpc(RpcMode.AnyPeer, TransferMode = TransferModeEnum.Unreliable, TransferChannel = 3)]
    public void UnreliablePacketRpc3(byte[] PackedRemPacket) {
        UnreliablePacketRpc(PackedRemPacket);
    }
    [Rpc(RpcMode.AnyPeer, TransferMode = TransferModeEnum.Reliable, TransferChannel = 3)]
    public void PacketResponseRpc3(long PacketId, byte[] PackedReturnValue) {
        PacketResponseRpc(PacketId, PackedReturnValue);
    }
#endregion

}