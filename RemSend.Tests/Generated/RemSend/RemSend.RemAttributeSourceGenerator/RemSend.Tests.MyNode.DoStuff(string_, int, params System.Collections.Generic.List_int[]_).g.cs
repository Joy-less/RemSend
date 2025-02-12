#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Godot;
using MemoryPack;

namespace RemSend.Tests;

partial class MyNode {
    /// <summary>
    /// Remotely calls <see cref="DoStuff(string?, int, System.Collections.Generic.List{int[]})"/> on the given peer.<br/>
    /// Set <paramref name="PeerId"/> to 0 to broadcast to all peers.<br/>
    /// Set <paramref name="PeerId"/> to 1 to send to the authority.
    /// </summary>
    public void SendDoStuff(int PeerId, string? Arg, [System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] params System.Collections.Generic.List<int[]> Arg22) {
        // Create arguments pack
        SendDoStuffPack _ArgumentsPack = new(Arg, Arg22);
        // Serialize arguments pack
        byte[] _SerializedArgumentsPack = MemoryPackSerializer.Serialize(_ArgumentsPack);
    
        // Create packet
        RemPacket _Packet = new(this.GetPath(), "SendDoStuff", _SerializedArgumentsPack);
        // Serialize packet
        byte[] _SerializedPacket = MemoryPackSerializer.Serialize(_Packet);
    
        // Send packet to peer ID
        ((SceneMultiplayer)this.Multiplayer).SendBytes(
            bytes: _SerializedPacket,
            id: PeerId,
            mode: MultiplayerPeer.TransferModeEnum.UnreliableOrdered,
            channel: 1234
        );
    }
    
    /// <summary>
    /// Remotely calls <see cref="DoStuff(string?, int, System.Collections.Generic.List{int[]})"/> on each peer.
    /// </summary>
    public void SendDoStuff(IEnumerable<int>? PeerIds, string? Arg, [System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] params System.Collections.Generic.List<int[]> Arg22) {
        // Skip if no peers
        if (PeerIds is null || !PeerIds.Any()) {
            return;
        }
    
        // Create arguments pack
        SendDoStuffPack _ArgumentsPack = new(Arg, Arg22);
        // Serialize arguments pack
        byte[] _SerializedArgumentsPack = MemoryPackSerializer.Serialize(_ArgumentsPack);
        
        // Create packet
        RemPacket _Packet = new(this.GetPath(), "SendDoStuff", _SerializedArgumentsPack);
        // Serialize packet
        byte[] _SerializedPacket = MemoryPackSerializer.Serialize(_Packet);
        
        // Send packet to each peer ID
        foreach (int PeerId in PeerIds) {
            ((SceneMultiplayer)this.Multiplayer).SendBytes(
                bytes: _SerializedPacket,
                id: PeerId,
                mode: MultiplayerPeer.TransferModeEnum.UnreliableOrdered,
                channel: 1234
            );
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal void SendDoStuffHandler(int SenderId, RemPacket _Packet) {
        // Deserialize arguments pack
        SendDoStuffPack _ArgumentsPack = MemoryPackSerializer.Deserialize<SendDoStuffPack>(_Packet.ArgumentsPack);
        
        // Call target method
        DoStuff(_ArgumentsPack.Arg, SenderId, _ArgumentsPack.Arg22);
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    private record struct SendDoStuffPack(string? Arg, [System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] params System.Collections.Generic.List<int[]> Arg22);
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    private sealed class SendDoStuffPackFormatter : MemoryPackFormatter<SendDoStuffPack> {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> Writer, scoped ref SendDoStuffPack Value) {
            Writer.WriteValue(Value.@Arg);
            Writer.WriteValue(Value.@Arg22);
        }
        public override void Deserialize(ref MemoryPackReader Reader, scoped ref SendDoStuffPack Value) {
            Value = new() {
                @Arg = Reader.ReadValue<string?>()!,
                @Arg22 = Reader.ReadValue<System.Collections.Generic.List<int[]>>()!,
            };
        }
    }
}
