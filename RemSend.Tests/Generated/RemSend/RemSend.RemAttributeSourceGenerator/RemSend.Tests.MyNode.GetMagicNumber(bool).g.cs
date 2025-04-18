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
    /// Remotely calls <see cref="GetMagicNumber(bool)"/> on the given peer using the given packet.<br/>
    /// Set <paramref name="PeerId"/> to 0 to broadcast to all eligible peers.<br/>
    /// Set <paramref name="PeerId"/> to 1 to send to the authority.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal void SendCoreGetMagicNumber(int PeerId, RemPacket RemPacket, byte[] SerializedRemPacket) {
        // Send packet to local peer
        if (PeerId is 0 || PeerId == this.Multiplayer.GetUniqueId()) {
            if (GetMagicNumberRemAttribute.CallLocal) {
                // Call remote method locally
                ReceiveGetMagicNumber(this.Multiplayer.GetUniqueId(), RemPacket);
    
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
        RemSendService.SendPacket(PeerId, this, GetMagicNumberRemAttribute, SerializedRemPacket);
    }
    
    /// <summary>
    /// Remotely calls <see cref="GetMagicNumber(bool)"/> on the given peer.<br/>
    /// Set <paramref name="PeerId"/> to 0 to broadcast to all eligible peers.<br/>
    /// Set <paramref name="PeerId"/> to 1 to send to the authority.
    /// </summary>
    public void SendGetMagicNumber(int PeerId, bool Dummy) {
        // Create send packet
        RemPacket RemPacket = RemSendService.CreatePacket(RemPacketType.Send, this.GetPath(), nameof(MyNode.GetMagicNumber), new GetMagicNumberSendPack(@Dummy));
        // Serialize send packet
        byte[] SerializedRemPacket = MemoryPackSerializer.Serialize(RemPacket);
    
        // Send packet to peer
        SendCoreGetMagicNumber(PeerId, RemPacket, SerializedRemPacket);
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
        RemPacket RemPacket = RemSendService.CreatePacket(RemPacketType.Send, this.GetPath(), nameof(MyNode.GetMagicNumber), new GetMagicNumberSendPack(@Dummy));
        // Serialize send packet
        byte[] SerializedRemPacket = MemoryPackSerializer.Serialize(RemPacket);
        
        // Send packet to each peer
        foreach (int PeerId in PeerIds) {
            SendCoreGetMagicNumber(PeerId, RemPacket, SerializedRemPacket);
        }
    }
    
    /// <summary>
    /// Remotely calls <see cref="GetMagicNumber(bool)"/> on all eligible peers.
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
        RemPacket RemPacket = RemSendService.CreatePacket(RemPacketType.Request, this.GetPath(), nameof(MyNode.GetMagicNumber), new GetMagicNumberRequestPack(RequestId, @Dummy));
        // Serialize request packet
        byte[] SerializedRemPacket = MemoryPackSerializer.Serialize(RemPacket);
    
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
            // Send packet to peer
            SendCoreGetMagicNumber(PeerId, RemPacket, SerializedRemPacket);
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
    
            // Create result packet
            RemPacket ResultRemPacket = RemSendService.CreatePacket(RemPacketType.Result, this.GetPath(), nameof(MyNode.GetMagicNumber), new GetMagicNumberResultPack(DeserializedArgumentsPack.RequestId, ReturnValue));
            // Serialize result packet
            byte[] SerializedResultRemPacket = MemoryPackSerializer.Serialize(ResultRemPacket);
    
            // Send result packet back to sender
            SendCoreGetMagicNumber(SenderId, ResultRemPacket, SerializedResultRemPacket);
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
