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
    /// Remotely calls <see cref="WaitSomeTime(bool, int)"/> on the given peer.<br/>
    /// Set <paramref name="PeerId"/> to 0 to broadcast to all peers.<br/>
    /// Set <paramref name="PeerId"/> to 1 to send to the authority.
    /// </summary>
    public void SendWaitSomeTime(int PeerId, bool X) {
        // Create arguments pack
        WaitSomeTimeSendPack ArgumentsPack = new(@X);
        // Serialize arguments pack
        byte[] SerializedArgumentsPack = MemoryPackSerializer.Serialize(ArgumentsPack);
        
        // Create packet
        RemPacket RemPacket = new(RemPacketType.Message, this.GetPath(), nameof(RemSend.Tests.MyNode.WaitSomeTime), SerializedArgumentsPack);
        // Serialize packet
        byte[] SerializedRemPacket = MemoryPackSerializer.Serialize(RemPacket);
        
        // Send packet to peer ID
        ((SceneMultiplayer)this.Multiplayer).SendBytes(
            bytes: SerializedRemPacket,
            id: PeerId,
            mode: RemSendService.RemModeToTransferModeEnum(WaitSomeTimeRemAttribute.Mode),
            channel: WaitSomeTimeRemAttribute.Channel
        );
    
        // Also call target method locally
        if (PeerId is 0 && WaitSomeTimeRemAttribute.CallLocal) {
            _ = WaitSomeTime(@X, 0);
        }
    }
    
    /// <summary>
    /// Remotely calls <see cref="WaitSomeTime(bool, int)"/> on each peer.
    /// </summary>
    public void SendWaitSomeTime(IEnumerable<int>? PeerIds, bool X) {
        // Skip if no peers
        if (PeerIds is null || !PeerIds.Any()) {
            return;
        }
        
        // Create arguments pack
        WaitSomeTimeSendPack ArgumentsPack = new(@X);
        // Serialize arguments pack
        byte[] SerializedArgumentsPack = MemoryPackSerializer.Serialize(ArgumentsPack);
        
        // Create packet
        RemPacket RemPacket = new(RemPacketType.Message, this.GetPath(), nameof(RemSend.Tests.MyNode.WaitSomeTime), SerializedArgumentsPack);
        // Serialize packet
        byte[] SerializedRemPacket = MemoryPackSerializer.Serialize(RemPacket);
        
        // Send packet to each peer ID
        foreach (int PeerId in PeerIds) {
            ((SceneMultiplayer)this.Multiplayer).SendBytes(
                bytes: SerializedRemPacket,
                id: PeerId,
                mode: RemSendService.RemModeToTransferModeEnum(WaitSomeTimeRemAttribute.Mode),
                channel: WaitSomeTimeRemAttribute.Channel
            );
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal event Action<WaitSomeTimeResultPack>? OnReceiveWaitSomeTimeResult;
    
    /// <summary>
    /// Remotely calls <see cref="WaitSomeTime(bool, int)"/> on the given peer and awaits the return value.<br/>
    /// Set <paramref name="PeerId"/> to 0 to broadcast to all peers.<br/>
    /// Set <paramref name="PeerId"/> to 1 to send to the authority.
    /// </summary>
    public async System.Threading.Tasks.Task RequestWaitSomeTime(int PeerId, TimeSpan Timeout, bool X) {
        // Generate request ID
        Guid RequestId = Guid.NewGuid();
    
        // Create arguments pack
        WaitSomeTimeRequestPack ArgumentsPack = new(RequestId, @X);
        // Serialize arguments pack
        byte[] SerializedArgumentsPack = MemoryPackSerializer.Serialize(ArgumentsPack);
        
        // Create packet
        RemPacket RemPacket = new(RemPacketType.Request, this.GetPath(), nameof(RemSend.Tests.MyNode.WaitSomeTime), SerializedArgumentsPack);
        // Serialize packet
        byte[] SerializedRemPacket = MemoryPackSerializer.Serialize(RemPacket);
        
        // Send packet to peer ID
        ((SceneMultiplayer)this.Multiplayer).SendBytes(
            bytes: SerializedRemPacket,
            id: PeerId,
            mode: RemSendService.RemModeToTransferModeEnum(WaitSomeTimeRemAttribute.Mode),
            channel: WaitSomeTimeRemAttribute.Channel
        );
    
        // Create result listener
        TaskCompletionSource ResultAwaiter = new();
        void ResultCallback(WaitSomeTimeResultPack ResultPack) {
            if (ResultPack.RequestId == RequestId) {
                ResultAwaiter.TrySetResult();
            }
        }
        try {
            // Add result listener
            OnReceiveWaitSomeTimeResult += ResultCallback;
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
        // Message
        if (RemPacket.Type is RemPacketType.Message) {
            // Verify access
            RemSendService.VerifyAccess(WaitSomeTimeRemAttribute.Access, SenderId, this.Multiplayer.GetUniqueId());
            
            // Deserialize arguments pack
            WaitSomeTimeSendPack DeserializedArgumentsPack = MemoryPackSerializer.Deserialize<WaitSomeTimeSendPack>(RemPacket.ArgumentsPack);
            
            // Call target method
            WaitSomeTime(DeserializedArgumentsPack.@X, SenderId);
        }
        // Request
        else if (RemPacket.Type is RemPacketType.Request) {
            // Deserialize request arguments pack
            WaitSomeTimeRequestPack DeserializedArgumentsPack = MemoryPackSerializer.Deserialize<WaitSomeTimeRequestPack>(RemPacket.ArgumentsPack);
    
            // Call target method
            await WaitSomeTime(DeserializedArgumentsPack.@X, SenderId);
    
            // Create arguments pack
            WaitSomeTimeResultPack ArgumentsPack = new(DeserializedArgumentsPack.RequestId);
            // Serialize arguments pack
            byte[] SerializedArgumentsPack = MemoryPackSerializer.Serialize(ArgumentsPack);
            
            // Create packet
            RemPacket ResultRemPacket = new(RemPacketType.Result, this.GetPath(), nameof(RemSend.Tests.MyNode.WaitSomeTime), SerializedArgumentsPack);
            // Serialize packet
            byte[] SerializedResultRemPacket = MemoryPackSerializer.Serialize(ResultRemPacket);
            
            // Send packet back to sender ID
            ((SceneMultiplayer)this.Multiplayer).SendBytes(
                bytes: SerializedResultRemPacket,
                id: SenderId,
                mode: RemSendService.RemModeToTransferModeEnum(WaitSomeTimeRemAttribute.Mode),
                channel: WaitSomeTimeRemAttribute.Channel
            );
        }
        // Result
        else if (RemPacket.Type is RemPacketType.Result) {
            // Deserialize result arguments pack
            WaitSomeTimeResultPack DeserializedArgumentsPack = MemoryPackSerializer.Deserialize<WaitSomeTimeResultPack>(RemPacket.ArgumentsPack);
            
            // Invoke receive event
            OnReceiveWaitSomeTimeResult?.Invoke(DeserializedArgumentsPack);
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal record struct WaitSomeTimeSendPack(bool X) {
        // Formatter
        internal sealed class Formatter : MemoryPackFormatter<WaitSomeTimeSendPack> {
            public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> Writer, scoped ref WaitSomeTimeSendPack Value) {
                Writer.WriteValue(Value.@X);
            }
            public override void Deserialize(ref MemoryPackReader Reader, scoped ref WaitSomeTimeSendPack Value) {
                Value = new() {
                    @X = Reader.ReadValue<bool>()!,
                };
            }
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal record struct WaitSomeTimeRequestPack(Guid RequestId, bool X) {
        // Formatter
        internal sealed class Formatter : MemoryPackFormatter<WaitSomeTimeRequestPack> {
            public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> Writer, scoped ref WaitSomeTimeRequestPack Value) {
                Writer.WriteValue(Value.@RequestId);
                Writer.WriteValue(Value.@X);
            }
            public override void Deserialize(ref MemoryPackReader Reader, scoped ref WaitSomeTimeRequestPack Value) {
                Value = new() {
                    @RequestId = Reader.ReadValue<Guid>()!,
                    @X = Reader.ReadValue<bool>()!,
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
