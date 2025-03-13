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
    /// The <see cref="RemAttribute"/> defined on <see cref="GetMagicNumber(bool)"/>.<br/>
    /// The properties of this attribute can be changed to reconfigure the remote method.
    /// </summary>
    public RemAttribute GetMagicNumberRemAttribute { get; set; } = new() {
        Access = RemAccess.Any,
        CallLocal = false,
        Mode = RemMode.Reliable,
        Channel = 0,
    };
    
    /// <summary>
    /// Remotely calls <see cref="GetMagicNumber(bool)"/> on the given peer.<br/>
    /// Set <paramref name="PeerId"/> to 0 to broadcast to all peers.<br/>
    /// Set <paramref name="PeerId"/> to 1 to send to the authority.
    /// </summary>
    public void SendGetMagicNumber(int PeerId, bool Dummy) {
        // Create send packet
        byte[] SerializedRemPacket = RemSendService.SerializePacket(RemPacketType.Send, this.GetPath(), nameof(MyNode.GetMagicNumber), new GetMagicNumberSendPack(@Dummy));
        
        // Send packet to peer ID
        ((SceneMultiplayer)this.Multiplayer).SendBytes(
            bytes: SerializedRemPacket,
            id: PeerId,
            mode: RemSendService.RemModeToTransferModeEnum(GetMagicNumberRemAttribute.Mode),
            channel: GetMagicNumberRemAttribute.Channel
        );
    
        // Also call target method locally
        if (PeerId is 0 && GetMagicNumberRemAttribute.CallLocal) {
            GetMagicNumber(@Dummy);
        }
    }
    
    /// <summary>
    /// Remotely calls <see cref="GetMagicNumber(bool)"/> on each peer.
    /// </summary>
    public void SendGetMagicNumber(IEnumerable<int>? PeerIds, bool Dummy) {
        // Skip if no peers
        if (PeerIds is null || !PeerIds.Any()) {
            return;
        }
    
        // Create send packet
        byte[] SerializedRemPacket = RemSendService.SerializePacket(RemPacketType.Send, this.GetPath(), nameof(MyNode.GetMagicNumber), new GetMagicNumberSendPack(@Dummy));
        
        // Send packet to each peer ID
        foreach (int PeerId in PeerIds) {
            ((SceneMultiplayer)this.Multiplayer).SendBytes(
                bytes: SerializedRemPacket,
                id: PeerId,
                mode: RemSendService.RemModeToTransferModeEnum(GetMagicNumberRemAttribute.Mode),
                channel: GetMagicNumberRemAttribute.Channel
            );
        }
    }
    
    /// <summary>
    /// Remotely calls <see cref="GetMagicNumber(bool)"/> on all peers.
    /// </summary>
    public void BroadcastGetMagicNumber(bool Dummy) {
        SendGetMagicNumber(0, @Dummy);
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal event Action<int, GetMagicNumberResultPack>? OnReceiveGetMagicNumberResult;
    
    /// <summary>
    /// Remotely calls <see cref="GetMagicNumber(bool)"/> on the given peer and awaits the return value.<br/>
    /// Set <paramref name="PeerId"/> to 1 to send to the authority.
    /// </summary>
    public async System.Threading.Tasks.Task<ushort> RequestGetMagicNumber(int PeerId, TimeSpan Timeout, bool Dummy) {
        // Generate request ID
        Guid RequestId = Guid.NewGuid();
    
        // Create request packet
        byte[] SerializedRemPacket = RemSendService.SerializePacket(RemPacketType.Request, this.GetPath(), nameof(MyNode.GetMagicNumber), new GetMagicNumberRequestPack(RequestId, @Dummy));
        
        // Send packet to peer ID
        ((SceneMultiplayer)this.Multiplayer).SendBytes(
            bytes: SerializedRemPacket,
            id: PeerId,
            mode: RemSendService.RemModeToTransferModeEnum(GetMagicNumberRemAttribute.Mode),
            channel: GetMagicNumberRemAttribute.Channel
        );
    
        // Create result listener
        TaskCompletionSource<ushort> ResultAwaiter = new();
        void ResultCallback(int SenderId, GetMagicNumberResultPack ResultPack) {
            if (SenderId == PeerId && ResultPack.RequestId == RequestId) {
                ResultAwaiter.TrySetResult(ResultPack.ReturnValue);
            }
        }
        try {
            // Add result listener
            OnReceiveGetMagicNumberResult += ResultCallback;
            // Await result
            ushort ReturnValue = await ResultAwaiter.Task.WaitAsync(Timeout);
            // Return result
            return ReturnValue;
        }
        finally {
            // Remove result listener
            OnReceiveGetMagicNumberResult -= ResultCallback;
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal void ReceiveGetMagicNumber(int SenderId, RemPacket RemPacket) {
        // Send
        if (RemPacket.Type is RemPacketType.Send) {
            // Verify access
            RemSendService.VerifyAccess(GetMagicNumberRemAttribute.Access, SenderId, this.Multiplayer.GetUniqueId());
            
            // Deserialize arguments pack
            GetMagicNumberSendPack DeserializedArgumentsPack = MemoryPackSerializer.Deserialize<GetMagicNumberSendPack>(RemPacket.ArgumentsPack);
            
            // Call target method
            GetMagicNumber(DeserializedArgumentsPack.@Dummy);
        }
        // Request
        else if (RemPacket.Type is RemPacketType.Request) {
            // Deserialize arguments pack
            GetMagicNumberRequestPack DeserializedArgumentsPack = MemoryPackSerializer.Deserialize<GetMagicNumberRequestPack>(RemPacket.ArgumentsPack);
    
            // Call target method
            ushort ReturnValue = GetMagicNumber(DeserializedArgumentsPack.@Dummy);
    
            // Serialize result packet
            byte[] SerializedRemPacket = RemSendService.SerializePacket(RemPacketType.Result, this.GetPath(), nameof(MyNode.GetMagicNumber), new GetMagicNumberResultPack(DeserializedArgumentsPack.RequestId, ReturnValue));
            
            // Send result packet back to sender ID
            ((SceneMultiplayer)this.Multiplayer).SendBytes(
                bytes: SerializedRemPacket,
                id: SenderId,
                mode: RemSendService.RemModeToTransferModeEnum(GetMagicNumberRemAttribute.Mode),
                channel: GetMagicNumberRemAttribute.Channel
            );
        }
        // Result
        else if (RemPacket.Type is RemPacketType.Result) {
            // Deserialize result arguments pack
            GetMagicNumberResultPack DeserializedArgumentsPack = MemoryPackSerializer.Deserialize<GetMagicNumberResultPack>(RemPacket.ArgumentsPack);
            
            // Invoke receive event
            OnReceiveGetMagicNumberResult?.Invoke(SenderId, DeserializedArgumentsPack);
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal record struct GetMagicNumberSendPack(bool Dummy) {
        // Formatter
        internal sealed class Formatter : MemoryPackFormatter<GetMagicNumberSendPack> {
            public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> Writer, scoped ref GetMagicNumberSendPack Value) {
                Writer.WriteValue(Value.@Dummy);
            }
            public override void Deserialize(ref MemoryPackReader Reader, scoped ref GetMagicNumberSendPack Value) {
                Value = new() {
                    @Dummy = Reader.ReadValue<bool>()!,
                };
            }
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal record struct GetMagicNumberRequestPack(Guid RequestId, bool Dummy) {
        // Formatter
        internal sealed class Formatter : MemoryPackFormatter<GetMagicNumberRequestPack> {
            public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> Writer, scoped ref GetMagicNumberRequestPack Value) {
                Writer.WriteValue(Value.@RequestId);
                Writer.WriteValue(Value.@Dummy);
            }
            public override void Deserialize(ref MemoryPackReader Reader, scoped ref GetMagicNumberRequestPack Value) {
                Value = new() {
                    @RequestId = Reader.ReadValue<Guid>()!,
                    @Dummy = Reader.ReadValue<bool>()!,
                };
            }
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal record struct GetMagicNumberResultPack(Guid RequestId, ushort ReturnValue) {
        // Formatter
        internal sealed class Formatter : MemoryPackFormatter<GetMagicNumberResultPack> {
            public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> Writer, scoped ref GetMagicNumberResultPack Value) {
                Writer.WriteValue(Value.@RequestId);
                Writer.WriteValue(Value.@ReturnValue);
            }
            public override void Deserialize(ref MemoryPackReader Reader, scoped ref GetMagicNumberResultPack Value) {
                Value = new() {
                    @RequestId = Reader.ReadValue<Guid>()!,
                    @ReturnValue = Reader.ReadValue<ushort>()!,
                };
            }
        }
    }
}
