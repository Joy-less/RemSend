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
    /// The <see cref="RemAttribute"/> defined on <see cref="WaitSomeTime(bool, int)"/>.<br/>
    /// The properties of this attribute can be changed to reconfigure the remote method.
    /// </summary>
    public RemAttribute WaitSomeTimeRemAttribute { get; set; } = new() {
        Access = RemAccess.None,
        CallLocal = false,
        Mode = RemMode.Reliable,
        Channel = 0,
    };
    
    /// <summary>
    /// Remotely calls <see cref="WaitSomeTime(bool, int)"/> on the given peer using the given packet.<br/>
    /// Set <paramref name="PeerId"/> to 0 to broadcast to all eligible peers.<br/>
    /// Set <paramref name="PeerId"/> to 1 to send to the authority.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal void SendCoreWaitSomeTime(int PeerId, RemPacket RemPacket, byte[] SerializedRemPacket) {
        // Send packet to local peer
        if (PeerId is 0 || PeerId == this.Multiplayer.GetUniqueId()) {
            if (WaitSomeTimeRemAttribute.CallLocal) {
                // Call remote method locally
                ReceiveWaitSomeTime(0, RemPacket);
    
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
        RemSendService.SendPacket(PeerId, this, WaitSomeTimeRemAttribute, SerializedRemPacket);
    }
    
    /// <summary>
    /// Remotely calls <see cref="WaitSomeTime(bool, int)"/> on the given peer.<br/>
    /// Set <paramref name="PeerId"/> to 0 to broadcast to all eligible peers.<br/>
    /// Set <paramref name="PeerId"/> to 1 to send to the authority.
    /// </summary>
    public void SendWaitSomeTime(int PeerId, bool Dummy) {
        // Create send packet
        RemPacket RemPacket = RemSendService.CreatePacket(RemPacketType.Send, this.GetPath(), nameof(MyNode.WaitSomeTime), new WaitSomeTimeSendPack(@Dummy));
        // Serialize send packet
        byte[] SerializedRemPacket = MemoryPackSerializer.Serialize(RemPacket);
    
        // Send packet to peer
        SendCoreWaitSomeTime(PeerId, RemPacket, SerializedRemPacket);
    }
    
    /// <summary>
    /// Remotely calls <see cref="WaitSomeTime(bool, int)"/> on each peer.
    /// </summary>
    public void SendWaitSomeTime(IEnumerable<int>? PeerIds, bool Dummy) {
        // Skip if no peers
        if (PeerIds is null || !PeerIds.Any()) {
            return;
        }
    
        // Create send packet
        RemPacket RemPacket = RemSendService.CreatePacket(RemPacketType.Send, this.GetPath(), nameof(MyNode.WaitSomeTime), new WaitSomeTimeSendPack(@Dummy));
        // Serialize send packet
        byte[] SerializedRemPacket = MemoryPackSerializer.Serialize(RemPacket);
        
        // Send packet to each peer
        foreach (int PeerId in PeerIds) {
            SendCoreWaitSomeTime(PeerId, RemPacket, SerializedRemPacket);
        }
    }
    
    /// <summary>
    /// Remotely calls <see cref="WaitSomeTime(bool, int)"/> on all eligible peers.
    /// </summary>
    public void BroadcastWaitSomeTime(bool Dummy) {
        SendWaitSomeTime(0, @Dummy);
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal event Action<int, WaitSomeTimeResultPack>? OnReceiveWaitSomeTimeResult;
    
    /// <summary>
    /// Remotely calls <see cref="WaitSomeTime(bool, int)"/> on the given peer and awaits the return value.<br/>
    /// Set <paramref name="PeerId"/> to 1 to send to the authority.
    /// </summary>
    public async System.Threading.Tasks.Task RequestWaitSomeTime(int PeerId, TimeSpan Timeout, bool Dummy) {
        // Generate request ID
        Guid RequestId = Guid.NewGuid();
    
        // Create request packet
        RemPacket RemPacket = RemSendService.CreatePacket(RemPacketType.Request, this.GetPath(), nameof(MyNode.WaitSomeTime), new WaitSomeTimeRequestPack(RequestId, @Dummy));
        // Serialize request packet
        byte[] SerializedRemPacket = MemoryPackSerializer.Serialize(RemPacket);
    
        // Create result listener
        TaskCompletionSource ResultAwaiter = new();
        void ResultCallback(int SenderId, WaitSomeTimeResultPack ResultPack) {
            if (SenderId == PeerId && ResultPack.RequestId == RequestId) {
                ResultAwaiter.TrySetResult();
            }
        }
        try {
            // Add result listener
            OnReceiveWaitSomeTimeResult += ResultCallback;
            // Send packet to peer
            SendCoreWaitSomeTime(PeerId, RemPacket, SerializedRemPacket);
            // Await completion
            await ResultAwaiter.Task.WaitAsync(Timeout);
        }
        finally {
            // Remove result listener
            OnReceiveWaitSomeTimeResult -= ResultCallback;
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal async void ReceiveWaitSomeTime(int SenderId, RemPacket RemPacket) {
        // Send
        if (RemPacket.Type is RemPacketType.Send) {
            // Verify access
            RemSendService.VerifyAccess(WaitSomeTimeRemAttribute.Access, SenderId, this.Multiplayer.GetUniqueId());
            
            // Deserialize arguments pack
            WaitSomeTimeSendPack DeserializedArgumentsPack = MemoryPackSerializer.Deserialize<WaitSomeTimeSendPack>(RemPacket.ArgumentsPack);
            
            // Call target method
            await WaitSomeTime(DeserializedArgumentsPack.@Dummy, SenderId);
        }
        // Request
        else if (RemPacket.Type is RemPacketType.Request) {
            // Deserialize arguments pack
            WaitSomeTimeRequestPack DeserializedArgumentsPack = MemoryPackSerializer.Deserialize<WaitSomeTimeRequestPack>(RemPacket.ArgumentsPack);
    
            // Call target method
            await WaitSomeTime(DeserializedArgumentsPack.@Dummy, SenderId);
    
            // Create result packet
            RemPacket ResultRemPacket = RemSendService.CreatePacket(RemPacketType.Result, this.GetPath(), nameof(MyNode.WaitSomeTime), new WaitSomeTimeResultPack(DeserializedArgumentsPack.RequestId));
            // Serialize result packet
            byte[] SerializedResultRemPacket = MemoryPackSerializer.Serialize(ResultRemPacket);
    
            // Send result packet back to sender
            RemSendService.SendPacket(SenderId, this, WaitSomeTimeRemAttribute, SerializedResultRemPacket);
        }
        // Result
        else if (RemPacket.Type is RemPacketType.Result) {
            // Deserialize result arguments pack
            WaitSomeTimeResultPack DeserializedArgumentsPack = MemoryPackSerializer.Deserialize<WaitSomeTimeResultPack>(RemPacket.ArgumentsPack);
            
            // Invoke receive event
            OnReceiveWaitSomeTimeResult?.Invoke(SenderId, DeserializedArgumentsPack);
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal record struct WaitSomeTimeSendPack(bool Dummy) {
        // Formatter
        internal sealed class Formatter : MemoryPackFormatter<WaitSomeTimeSendPack> {
            public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> Writer, scoped ref WaitSomeTimeSendPack Value) {
                Writer.WriteValue(Value.@Dummy);
            }
            public override void Deserialize(ref MemoryPackReader Reader, scoped ref WaitSomeTimeSendPack Value) {
                Value = new() {
                    @Dummy = Reader.ReadValue<bool>()!,
                };
            }
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal record struct WaitSomeTimeRequestPack(Guid RequestId, bool Dummy) {
        // Formatter
        internal sealed class Formatter : MemoryPackFormatter<WaitSomeTimeRequestPack> {
            public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> Writer, scoped ref WaitSomeTimeRequestPack Value) {
                Writer.WriteValue(Value.@RequestId);
                Writer.WriteValue(Value.@Dummy);
            }
            public override void Deserialize(ref MemoryPackReader Reader, scoped ref WaitSomeTimeRequestPack Value) {
                Value = new() {
                    @RequestId = Reader.ReadValue<Guid>()!,
                    @Dummy = Reader.ReadValue<bool>()!,
                };
            }
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal record struct WaitSomeTimeResultPack(Guid RequestId) {
        // Formatter
        internal sealed class Formatter : MemoryPackFormatter<WaitSomeTimeResultPack> {
            public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> Writer, scoped ref WaitSomeTimeResultPack Value) {
                Writer.WriteValue(Value.@RequestId);
            }
            public override void Deserialize(ref MemoryPackReader Reader, scoped ref WaitSomeTimeResultPack Value) {
                Value = new() {
                    @RequestId = Reader.ReadValue<Guid>()!,
                };
            }
        }
    }
}
