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
    /// The <see cref="RemAttribute"/> defined on <see cref="WaitSomeTime()"/>.
    /// </summary>
    /// <remarks>
    /// Todo: use the changed values of this attribute if it's changed.
    /// </remarks>
    public RemAttribute WaitSomeTimeRemAttribute { get; set; } = new() {
        Access = RemAccess.None,
        CallLocal = false,
        Mode = RemMode.Reliable,
        Channel = 0,
    };
    
    /// <summary>
    /// Remotely calls <see cref="WaitSomeTime()"/> on the given peer.<br/>
    /// Set <paramref name="PeerId"/> to 0 to broadcast to all peers.<br/>
    /// Set <paramref name="PeerId"/> to 1 to send to the authority.
    /// </summary>
    public void SendWaitSomeTime(int PeerId) {
        // Create arguments pack
        WaitSomeTimeSendPack ArgumentsPack = new();
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
            mode: MultiplayerPeer.TransferModeEnum.Reliable,
            channel: WaitSomeTimeRemAttribute.Channel
        );
    }
    
    /// <summary>
    /// Remotely calls <see cref="WaitSomeTime()"/> on each peer.
    /// </summary>
    public void SendWaitSomeTime(IEnumerable<int>? PeerIds) {
        // Skip if no peers
        if (PeerIds is null || !PeerIds.Any()) {
            return;
        }
        
        // Create arguments pack
        WaitSomeTimeSendPack ArgumentsPack = new();
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
                mode: MultiplayerPeer.TransferModeEnum.Reliable,
                channel: WaitSomeTimeRemAttribute.Channel
            );
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal event Action<WaitSomeTimeResultPack>? OnReceiveWaitSomeTimeResult;
    
    /// <summary>
    /// Remotely calls <see cref="WaitSomeTime()"/> on the given peer and awaits the return value.<br/>
    /// Set <paramref name="PeerId"/> to 0 to broadcast to all peers.<br/>
    /// Set <paramref name="PeerId"/> to 1 to send to the authority.
    /// </summary>
    public async System.Threading.Tasks.Task RequestWaitSomeTime(int PeerId, double Timeout) {
        // Generate request ID
        Guid RequestId = Guid.NewGuid();
    
        // Create arguments pack
        WaitSomeTimeRequestPack ArgumentsPack = new(RequestId);
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
            mode: MultiplayerPeer.TransferModeEnum.Reliable,
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
            await ResultAwaiter.Task.WaitAsync(TimeSpan.FromSeconds(Timeout));
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
            // Deserialize send arguments pack
            WaitSomeTimeSendPack DeserializedArgumentsPack = MemoryPackSerializer.Deserialize<WaitSomeTimeSendPack>(RemPacket.ArgumentsPack);
        
            // Call target method
            _ = WaitSomeTime();
        }
        // Request
        else if (RemPacket.Type is RemPacketType.Request) {
            // Deserialize request arguments pack
            WaitSomeTimeRequestPack DeserializedArgumentsPack = MemoryPackSerializer.Deserialize<WaitSomeTimeRequestPack>(RemPacket.ArgumentsPack);
    
            // Call target method
            await WaitSomeTime();
    
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
                mode: MultiplayerPeer.TransferModeEnum.Reliable,
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
    internal record struct WaitSomeTimeSendPack() {
        // Formatter
        internal sealed class Formatter : MemoryPackFormatter<WaitSomeTimeSendPack> {
            public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> Writer, scoped ref WaitSomeTimeSendPack Value) {
                
            }
            public override void Deserialize(ref MemoryPackReader Reader, scoped ref WaitSomeTimeSendPack Value) {
                Value = new() {
                    
                };
            }
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal record struct WaitSomeTimeRequestPack(Guid RequestId) {
        // Formatter
        internal sealed class Formatter : MemoryPackFormatter<WaitSomeTimeRequestPack> {
            public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> Writer, scoped ref WaitSomeTimeRequestPack Value) {
                Writer.WriteValue(Value.@RequestId);
            }
            public override void Deserialize(ref MemoryPackReader Reader, scoped ref WaitSomeTimeRequestPack Value) {
                Value = new() {
                    @RequestId = Reader.ReadValue<Guid>()!,
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
