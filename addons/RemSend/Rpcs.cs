#nullable enable

using System.Collections.Generic;
using System.Collections.Frozen;
using Godot;

using RpcMode = Godot.MultiplayerApi.RpcMode;
using TransferModeEnum = Godot.MultiplayerPeer.TransferModeEnum;

namespace RemSend;

public partial class RemSend {
    private static readonly FrozenDictionary<RemMode, StringName[]> TransferRpcs = new Dictionary<RemMode, StringName[]>() {
        [RemMode.Reliable] = [
            MethodName.ReliablePacketRpc,
            MethodName.ReliablePacketRpc1,
            MethodName.ReliablePacketRpc2,
            MethodName.ReliablePacketRpc3,
        ],
        [RemMode.UnreliableOrdered] = [
            MethodName.UnreliableOrderedPacketRpc,
            MethodName.UnreliableOrderedPacketRpc1,
            MethodName.UnreliableOrderedPacketRpc2,
            MethodName.UnreliableOrderedPacketRpc3,
        ],
        [RemMode.Unreliable] = [
            MethodName.UnreliablePacketRpc,
            MethodName.UnreliablePacketRpc1,
            MethodName.UnreliablePacketRpc2,
            MethodName.UnreliablePacketRpc3,
        ],
    }.ToFrozenDictionary();
    private static readonly StringName[] ResponseTransferRpcs = [
        MethodName.PacketResponseRpc,
        MethodName.PacketResponseRpc1,
        MethodName.PacketResponseRpc2,
        MethodName.PacketResponseRpc3,
    ];

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