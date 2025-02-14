#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Godot;
using MemoryPack;
using RemSend;

namespace RemSend.Tests;

partial class MyNode {
    /// <summary>
    /// Remotely calls <see cref="GetMagicNumber()"/> on the given peer.<br/>
    /// Set <paramref name="PeerId"/> to 0 to broadcast to all peers.<br/>
    /// Set <paramref name="PeerId"/> to 1 to send to the authority.
    /// </summary>
    public void SendGetMagicNumber(int PeerId) {
        // Create arguments pack
        GetMagicNumberPack _ArgumentsPack = new();
        // Serialize arguments pack
        byte[] _SerializedArgumentsPack = MemoryPackSerializer.Serialize(_ArgumentsPack);
    
        // Create packet
        RemPacket _Packet = new(this.GetPath(), nameof(SendGetMagicNumber), _SerializedArgumentsPack);
        // Serialize packet
        byte[] _SerializedPacket = MemoryPackSerializer.Serialize(_Packet);
    
        // Send packet to peer ID
        ((SceneMultiplayer)this.Multiplayer).SendBytes(
            bytes: _SerializedPacket,
            id: PeerId,
            mode: MultiplayerPeer.TransferModeEnum.Reliable,
            channel: 0
        );
    }
    
    /// <summary>
    /// Remotely calls <see cref="GetMagicNumber()"/> on each peer.
    /// </summary>
    public void SendGetMagicNumber(IEnumerable<int>? PeerIds) {
        // Skip if no peers
        if (PeerIds is null || !PeerIds.Any()) {
            return;
        }
    
        // Create arguments pack
        GetMagicNumberPack _ArgumentsPack = new();
        // Serialize arguments pack
        byte[] _SerializedArgumentsPack = MemoryPackSerializer.Serialize(_ArgumentsPack);
        
        // Create packet
        RemPacket _Packet = new(this.GetPath(), nameof(SendGetMagicNumber), _SerializedArgumentsPack);
        // Serialize packet
        byte[] _SerializedPacket = MemoryPackSerializer.Serialize(_Packet);
        
        // Send packet to each peer ID
        foreach (int PeerId in PeerIds) {
            ((SceneMultiplayer)this.Multiplayer).SendBytes(
                bytes: _SerializedPacket,
                id: PeerId,
                mode: MultiplayerPeer.TransferModeEnum.Reliable,
                channel: 0
            );
        }
    }
    
    /// <summary>
    /// Remotely calls <see cref="GetMagicNumber()"/> on the given peer and awaits the return value.<br/>
    /// Set <paramref name="PeerId"/> to 0 to broadcast to all peers.<br/>
    /// Set <paramref name="PeerId"/> to 1 to send to the authority.
    /// </summary>
    public int RequestGetMagicNumber(int PeerId) {
        // Call send-one method
        SendGetMagicNumber(PeerId);
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal void ReceiveGetMagicNumber(int SenderId, RemPacket _Packet) {
        // Deserialize arguments pack
        GetMagicNumberPack _ArgumentsPack = MemoryPackSerializer.Deserialize<GetMagicNumberPack>(_Packet.ArgumentsPack);
        
        // Call target method
        GetMagicNumber();
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal record struct GetMagicNumberPack();
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class GetMagicNumberPackFormatter : MemoryPackFormatter<GetMagicNumberPack> {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> Writer, scoped ref GetMagicNumberPack Value) {
            
        }
        public override void Deserialize(ref MemoryPackReader Reader, scoped ref GetMagicNumberPack Value) {
            Value = new() {
                
            };
        }
    }
}
