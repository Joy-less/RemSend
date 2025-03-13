﻿#nullable enable

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
    /// Remotely calls <see cref="GetMagicNumberAsync(bool)"/> on the given peer.<br/>
    /// Set <paramref name="PeerId"/> to 0 to broadcast to all peers.<br/>
    /// Set <paramref name="PeerId"/> to 1 to send to the authority.
    /// </summary>
    public void SendGetMagicNumberAsync(int PeerId, bool Dummy) {
        // Create send packet
        byte[] SerializedRemPacket = RemSendService.SerializePacket(RemPacketType.Send, this.GetPath(), nameof(MyNode.GetMagicNumberAsync), new GetMagicNumberAsyncSendPack(@Dummy));
        
        // Send packet to peer
        RemSendService.SendPacket(PeerId, this, GetMagicNumberAsyncRemAttribute, SerializedRemPacket);
    
        // Also call target method locally
        if (PeerId is 0 && GetMagicNumberAsyncRemAttribute.CallLocal) {
            _ = GetMagicNumberAsync(@Dummy);
        }
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
        byte[] SerializedRemPacket = RemSendService.SerializePacket(RemPacketType.Send, this.GetPath(), nameof(MyNode.GetMagicNumberAsync), new GetMagicNumberAsyncSendPack(@Dummy));
        
        // Send packet to each peer
        foreach (int PeerId in PeerIds) {
            RemSendService.SendPacket(PeerId, this, GetMagicNumberAsyncRemAttribute, SerializedRemPacket);
        }
    }
    
    /// <summary>
    /// Remotely calls <see cref="GetMagicNumberAsync(bool)"/> on all peers.
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
        byte[] SerializedRemPacket = RemSendService.SerializePacket(RemPacketType.Request, this.GetPath(), nameof(MyNode.GetMagicNumberAsync), new GetMagicNumberAsyncRequestPack(RequestId, @Dummy));
        
        // Send packet to peer
        RemSendService.SendPacket(PeerId, this, GetMagicNumberAsyncRemAttribute, SerializedRemPacket);
    
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
    
            // Serialize result packet
            byte[] SerializedRemPacket = RemSendService.SerializePacket(RemPacketType.Result, this.GetPath(), nameof(MyNode.GetMagicNumberAsync), new GetMagicNumberAsyncResultPack(DeserializedArgumentsPack.RequestId, ReturnValue));
            
            // Send result packet back to sender
            RemSendService.SendPacket(SenderId, this, GetMagicNumberAsyncRemAttribute, SerializedRemPacket);
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
