#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using MemoryPack;
using RemSend;

namespace RemSend.Tests;

partial class MyNode {
    /// <summary>
    /// The <see cref="RemAttribute"/> defined on <see cref="SillyExample(string?, int, System.Collections.Generic.List{int[]})"/>.<br/>
    /// The properties of this attribute can be changed to reconfigure the remote method.
    /// </summary>
    private RemAttribute SillyExampleRemAttribute { get; set; } = new() {
        Access = RemAccess.Any,
        CallLocal = false,
        Mode = RemMode.UnreliableOrdered,
        Channel = 1234,
    };
    
    /// <summary>
    /// Remotely calls <see cref="SillyExample(string?, int, System.Collections.Generic.List{int[]})"/> on the given peer.<br/>
    /// Set <paramref name="PeerId"/> to 0 to broadcast to all eligible peers.<br/>
    /// Set <paramref name="PeerId"/> to 1 to send to the authority.
    /// </summary>
    private void SendSillyExample(int PeerId, string? Arg, [System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] params System.Collections.Generic.List<int[]> Arg22) {
        // Create send packet
        byte[] SerializedRemPacket = RemSendService.SerializePacket(RemPacketType.Send, this.GetPath(), nameof(MyNode.SillyExample), new SillyExampleSendPack(@Arg, @Arg22));
    
        // Send packet to local peer
        if (PeerId is 0) {
            SillyExample(@Arg, 0, @Arg22);
        }
        else if (PeerId == this.Multiplayer.GetUniqueId()) {
            if (!SillyExampleRemAttribute.CallLocal) {
                throw new ArgumentException("Not authorized to call on the local peer", nameof(PeerId));
            }
            SillyExample(@Arg, 0, @Arg22);
            return;
        }
        
        // Send packet to remote peer
        RemSendService.SendPacket(PeerId, this, SillyExampleRemAttribute, SerializedRemPacket);
    }
    
    /// <summary>
    /// Remotely calls <see cref="SillyExample(string?, int, System.Collections.Generic.List{int[]})"/> on each peer.
    /// </summary>
    private void SendSillyExample(IEnumerable<int>? PeerIds, string? Arg, [System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] params System.Collections.Generic.List<int[]> Arg22) {
        // Skip if no peers
        if (PeerIds is null || !PeerIds.Any()) {
            return;
        }
    
        // Create send packet
        byte[] SerializedRemPacket = RemSendService.SerializePacket(RemPacketType.Send, this.GetPath(), nameof(MyNode.SillyExample), new SillyExampleSendPack(@Arg, @Arg22));
        
        // Send packet to each peer
        foreach (int PeerId in PeerIds) {
            // Send packet to local peer
            if (PeerId is 0) {
                SillyExample(@Arg, 0, @Arg22);
            }
            else if (PeerId == this.Multiplayer.GetUniqueId()) {
                if (!SillyExampleRemAttribute.CallLocal) {
                    throw new ArgumentException("Not authorized to call on the local peer", nameof(PeerId));
                }
                SillyExample(@Arg, 0, @Arg22);
                return;
            }
        
            // Send packet to remote peer
            RemSendService.SendPacket(PeerId, this, SillyExampleRemAttribute, SerializedRemPacket);
        }
    }
    
    /// <summary>
    /// Remotely calls <see cref="SillyExample(string?, int, System.Collections.Generic.List{int[]})"/> on all eligible peers.
    /// </summary>
    private void BroadcastSillyExample(string? Arg, [System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] params System.Collections.Generic.List<int[]> Arg22) {
        SendSillyExample(0, @Arg, @Arg22);
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal void ReceiveSillyExample(int _SenderId, RemPacket RemPacket) {
        // Send
        if (RemPacket.Type is RemPacketType.Send) {
            // Verify access
            RemSendService.VerifyAccess(SillyExampleRemAttribute.Access, _SenderId, this.Multiplayer.GetUniqueId());
            
            // Deserialize arguments pack
            SillyExampleSendPack DeserializedArgumentsPack = MemoryPackSerializer.Deserialize<SillyExampleSendPack>(RemPacket.ArgumentsPack);
            
            // Call target method
            SillyExample(DeserializedArgumentsPack.@Arg, _SenderId, DeserializedArgumentsPack.@Arg22);
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal record struct SillyExampleSendPack(string? Arg, [System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] params System.Collections.Generic.List<int[]> Arg22) {
        // Formatter
        internal sealed class Formatter : MemoryPackFormatter<SillyExampleSendPack> {
            public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> Writer, scoped ref SillyExampleSendPack Value) {
                Writer.WriteValue(Value.@Arg);
                Writer.WriteValue(Value.@Arg22);
            }
            public override void Deserialize(ref MemoryPackReader Reader, scoped ref SillyExampleSendPack Value) {
                Value = new() {
                    @Arg = Reader.ReadValue<string?>()!,
                    @Arg22 = Reader.ReadValue<System.Collections.Generic.List<int[]>>()!,
                };
            }
        }
    }
}
