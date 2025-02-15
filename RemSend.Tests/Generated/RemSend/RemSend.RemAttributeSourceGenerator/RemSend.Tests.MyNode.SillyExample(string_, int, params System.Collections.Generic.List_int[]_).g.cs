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
    /// Set <paramref name="PeerId"/> to 0 to broadcast to all peers.<br/>
    /// Set <paramref name="PeerId"/> to 1 to send to the authority.
    /// </summary>
    private void SendSillyExample(int PeerId, string? Arg, [System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] params System.Collections.Generic.List<int[]> Arg22) {
        // Create arguments pack
        SillyExampleSendPack ArgumentsPack = new(@Arg, @Arg22);
        // Serialize arguments pack
        byte[] SerializedArgumentsPack = MemoryPackSerializer.Serialize(ArgumentsPack);
        
        // Create packet
        RemPacket RemPacket = new(RemPacketType.Send, this.GetPath(), nameof(RemSend.Tests.MyNode.SillyExample), SerializedArgumentsPack);
        // Serialize packet
        byte[] SerializedRemPacket = MemoryPackSerializer.Serialize(RemPacket);
        
        // Send packet to peer ID
        ((SceneMultiplayer)this.Multiplayer).SendBytes(
            bytes: SerializedRemPacket,
            id: PeerId,
            mode: RemSendService.RemModeToTransferModeEnum(SillyExampleRemAttribute.Mode),
            channel: SillyExampleRemAttribute.Channel
        );
    
        // Also call target method locally
        if (PeerId is 0 && SillyExampleRemAttribute.CallLocal) {
            SillyExample(@Arg, 0, @Arg22);
        }
    }
    
    /// <summary>
    /// Remotely calls <see cref="SillyExample(string?, int, System.Collections.Generic.List{int[]})"/> on each peer.
    /// </summary>
    private void SendSillyExample(IEnumerable<int>? PeerIds, string? Arg, [System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] params System.Collections.Generic.List<int[]> Arg22) {
        // Skip if no peers
        if (PeerIds is null || !PeerIds.Any()) {
            return;
        }
        
        // Create arguments pack
        SillyExampleSendPack ArgumentsPack = new(@Arg, @Arg22);
        // Serialize arguments pack
        byte[] SerializedArgumentsPack = MemoryPackSerializer.Serialize(ArgumentsPack);
        
        // Create packet
        RemPacket RemPacket = new(RemPacketType.Send, this.GetPath(), nameof(RemSend.Tests.MyNode.SillyExample), SerializedArgumentsPack);
        // Serialize packet
        byte[] SerializedRemPacket = MemoryPackSerializer.Serialize(RemPacket);
        
        // Send packet to each peer ID
        foreach (int PeerId in PeerIds) {
            ((SceneMultiplayer)this.Multiplayer).SendBytes(
                bytes: SerializedRemPacket,
                id: PeerId,
                mode: RemSendService.RemModeToTransferModeEnum(SillyExampleRemAttribute.Mode),
                channel: SillyExampleRemAttribute.Channel
            );
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal void ReceiveSillyExample(int SenderId, RemPacket RemPacket) {
        // Send
        if (RemPacket.Type is RemPacketType.Send) {
            // Verify access
            RemSendService.VerifyAccess(SillyExampleRemAttribute.Access, SenderId, this.Multiplayer.GetUniqueId());
            
            // Deserialize arguments pack
            SillyExampleSendPack DeserializedArgumentsPack = MemoryPackSerializer.Deserialize<SillyExampleSendPack>(RemPacket.ArgumentsPack);
            
            // Call target method
            SillyExample(DeserializedArgumentsPack.@Arg, SenderId, DeserializedArgumentsPack.@Arg22);
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
