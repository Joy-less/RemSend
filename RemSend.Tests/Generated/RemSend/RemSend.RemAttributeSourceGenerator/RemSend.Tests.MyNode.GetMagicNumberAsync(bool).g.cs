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
    /// The <see cref="RemAttribute"/> defined on <see cref="GetMagicNumberAsync(bool)"/>.<br/>
    /// The properties of this attribute can be changed to reconfigure the remote method.
    /// </summary>
    public RemAttribute GetMagicNumberAsyncRemAttribute { get; set; } = new() {
        Access = RemAccess.Any,
        CallLocal = false,
        Mode = RemMode.Reliable,
        Channel = 0,
    };
    
    /// <summary>
    /// Remotely calls <see cref="GetMagicNumberAsync(bool)"/> on the given peer using the given packet.<br/>
    /// Set <paramref name="PeerId"/> to 0 to broadcast to all eligible peers.<br/>
    /// Set <paramref name="PeerId"/> to 1 to send to the authority.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal void SendCoreGetMagicNumberAsync(int PeerId, RemPacket RemPacket, byte[] SerializedRemPacket) {
        // Send packet to local peer
        if (PeerId is 0 || PeerId == this.Multiplayer.GetUniqueId()) {
            if (GetMagicNumberAsyncRemAttribute.CallLocal) {
                // Call remote method locally
                ReceiveGetMagicNumberAsync(0, RemPacket);
    
                // Don't send remotely unless broadcasting
                if (PeerId is not 0) {
                    return;
                }
            }
            else {
                // Ensure authorized to call locally
                if (PeerId is not 0) {
                    throw new MethodAccessException("Not authorized to call on the local peer");
                }
            }
        }
        
        // Send packet to remote peer
        RemSendService.SendPacket(PeerId, this, GetMagicNumberAsyncRemAttribute, SerializedRemPacket);
    }
    
    /// <summary>
    /// Remotely calls <see cref="GetMagicNumberAsync(bool)"/> on the given peer.<br/>
    /// Set <paramref name="PeerId"/> to 0 to broadcast to all eligible peers.<br/>
    /// Set <paramref name="PeerId"/> to 1 to send to the authority.
    /// </summary>
    public void SendGetMagicNumberAsync(int PeerId, bool Dummy) {
        // Create send packet
        RemPacket RemPacket = RemSendService.CreatePacket(RemPacketType.Send, this.GetPath(), nameof(MyNode.GetMagicNumberAsync), new GetMagicNumberAsyncSendPack(@Dummy));
        // Serialize send packet
        byte[] SerializedRemPacket = MemoryPackSerializer.Serialize(RemPacket);
    
        // Send packet to peer
        SendCoreGetMagicNumberAsync(PeerId, RemPacket, SerializedRemPacket);
    }
    
    /// <summary>
    /// Remotely calls <see cref="GetMagicNumberAsync(bool)"/> on each peer.
    /// </summary>
    public void SendGetMagicNumberAsync(IEnumerable<int>? PeerIds, bool Dummy) {
        // Skip if no peers
        if (PeerIds is null || !PeerIds.Any()) {
            return;
        }
    
        // Create send packet
        RemPacket RemPacket = RemSendService.CreatePacket(RemPacketType.Send, this.GetPath(), nameof(MyNode.GetMagicNumberAsync), new GetMagicNumberAsyncSendPack(@Dummy));
        // Serialize send packet
        byte[] SerializedRemPacket = MemoryPackSerializer.Serialize(RemPacket);
        
        // Send packet to each peer
        foreach (int PeerId in PeerIds) {
            SendCoreGetMagicNumberAsync(PeerId, RemPacket, SerializedRemPacket);
        }
    }
    
    /// <summary>
    /// Remotely calls <see cref="GetMagicNumberAsync(bool)"/> on all eligible peers.
    /// </summary>
    public void BroadcastGetMagicNumberAsync(bool Dummy) {
        SendGetMagicNumberAsync(0, @Dummy);
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal event Action<int, GetMagicNumberAsyncResultPack>? OnReceiveGetMagicNumberAsyncResult;
    
    /// <summary>
    /// Remotely calls <see cref="GetMagicNumberAsync(bool)"/> on the given peer and awaits the return value.<br/>
    /// Set <paramref name="PeerId"/> to 1 to send to the authority.
    /// </summary>
    public async System.Threading.Tasks.Task<ushort> RequestGetMagicNumberAsync(int PeerId, TimeSpan Timeout, bool Dummy) {
        // Generate request ID
        Guid RequestId = Guid.NewGuid();
    
        // Create request packet
        RemPacket RemPacket = RemSendService.CreatePacket(RemPacketType.Request, this.GetPath(), nameof(MyNode.GetMagicNumberAsync), new GetMagicNumberAsyncRequestPack(RequestId, @Dummy));
        // Serialize request packet
        byte[] SerializedRemPacket = MemoryPackSerializer.Serialize(RemPacket);
    
        // Create result listener
        TaskCompletionSource<ushort> ResultAwaiter = new();
        void ResultCallback(int SenderId, GetMagicNumberAsyncResultPack ResultPack) {
            if (SenderId == PeerId && ResultPack.RequestId == RequestId) {
                ResultAwaiter.TrySetResult(ResultPack.ReturnValue);
            }
        }
        try {
            // Add result listener
            OnReceiveGetMagicNumberAsyncResult += ResultCallback;
            // Send packet to peer
            SendCoreGetMagicNumberAsync(PeerId, RemPacket, SerializedRemPacket);
            // Await result
            ushort ReturnValue = await ResultAwaiter.Task.WaitAsync(Timeout);
            // Return result
            return ReturnValue;
        }
        finally {
            // Remove result listener
            OnReceiveGetMagicNumberAsyncResult -= ResultCallback;
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal async void ReceiveGetMagicNumberAsync(int SenderId, RemPacket RemPacket) {
        // Send
        if (RemPacket.Type is RemPacketType.Send) {
            // Verify access
            RemSendService.VerifyAccess(GetMagicNumberAsyncRemAttribute.Access, SenderId, this.Multiplayer.GetUniqueId());
            
            // Deserialize arguments pack
            GetMagicNumberAsyncSendPack DeserializedArgumentsPack = MemoryPackSerializer.Deserialize<GetMagicNumberAsyncSendPack>(RemPacket.ArgumentsPack);
            
            // Call target method
            await GetMagicNumberAsync(DeserializedArgumentsPack.@Dummy);
        }
        // Request
        else if (RemPacket.Type is RemPacketType.Request) {
            // Deserialize arguments pack
            GetMagicNumberAsyncRequestPack DeserializedArgumentsPack = MemoryPackSerializer.Deserialize<GetMagicNumberAsyncRequestPack>(RemPacket.ArgumentsPack);
    
            // Call target method
            ushort ReturnValue = await GetMagicNumberAsync(DeserializedArgumentsPack.@Dummy);
    
            // Create result packet
            RemPacket ResultRemPacket = RemSendService.CreatePacket(RemPacketType.Result, this.GetPath(), nameof(MyNode.GetMagicNumberAsync), new GetMagicNumberAsyncResultPack(DeserializedArgumentsPack.RequestId, ReturnValue));
            // Serialize result packet
            byte[] SerializedResultRemPacket = MemoryPackSerializer.Serialize(ResultRemPacket);
    
            // Send result packet back to sender
            RemSendService.SendPacket(SenderId, this, GetMagicNumberAsyncRemAttribute, SerializedResultRemPacket);
        }
        // Result
        else if (RemPacket.Type is RemPacketType.Result) {
            // Deserialize result arguments pack
            GetMagicNumberAsyncResultPack DeserializedArgumentsPack = MemoryPackSerializer.Deserialize<GetMagicNumberAsyncResultPack>(RemPacket.ArgumentsPack);
            
            // Invoke receive event
            OnReceiveGetMagicNumberAsyncResult?.Invoke(SenderId, DeserializedArgumentsPack);
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal record struct GetMagicNumberAsyncSendPack(bool Dummy) {
        // Formatter
        internal sealed class Formatter : MemoryPackFormatter<GetMagicNumberAsyncSendPack> {
            public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> Writer, scoped ref GetMagicNumberAsyncSendPack Value) {
                Writer.WriteValue(Value.@Dummy);
            }
            public override void Deserialize(ref MemoryPackReader Reader, scoped ref GetMagicNumberAsyncSendPack Value) {
                Value = new() {
                    @Dummy = Reader.ReadValue<bool>()!,
                };
            }
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal record struct GetMagicNumberAsyncRequestPack(Guid RequestId, bool Dummy) {
        // Formatter
        internal sealed class Formatter : MemoryPackFormatter<GetMagicNumberAsyncRequestPack> {
            public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> Writer, scoped ref GetMagicNumberAsyncRequestPack Value) {
                Writer.WriteValue(Value.@RequestId);
                Writer.WriteValue(Value.@Dummy);
            }
            public override void Deserialize(ref MemoryPackReader Reader, scoped ref GetMagicNumberAsyncRequestPack Value) {
                Value = new() {
                    @RequestId = Reader.ReadValue<Guid>()!,
                    @Dummy = Reader.ReadValue<bool>()!,
                };
            }
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal record struct GetMagicNumberAsyncResultPack(Guid RequestId, ushort ReturnValue) {
        // Formatter
        internal sealed class Formatter : MemoryPackFormatter<GetMagicNumberAsyncResultPack> {
            public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> Writer, scoped ref GetMagicNumberAsyncResultPack Value) {
                Writer.WriteValue(Value.@RequestId);
                Writer.WriteValue(Value.@ReturnValue);
            }
            public override void Deserialize(ref MemoryPackReader Reader, scoped ref GetMagicNumberAsyncResultPack Value) {
                Value = new() {
                    @RequestId = Reader.ReadValue<Guid>()!,
                    @ReturnValue = Reader.ReadValue<ushort>()!,
                };
            }
        }
    }
}
