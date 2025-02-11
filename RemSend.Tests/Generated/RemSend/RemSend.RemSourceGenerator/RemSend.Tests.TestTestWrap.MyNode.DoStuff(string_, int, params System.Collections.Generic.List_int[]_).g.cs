#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Godot;
using MemoryPack;

namespace RemSend.Tests;

partial class TestTestWrap {
    partial class MyNode {    
        /// <summary>
        /// Remotely calls <see cref="DoStuff(string?, int, System.Collections.Generic.List{int[]})"/>.<br/>
        /// Set <paramref name="PeerId"/> to 0 to broadcast to all peers.<br/>
        /// Set <paramref name="PeerId"/> to 1 to send to the authority.
        /// </summary>
        public void SendDoStuff(int PeerId, string? Arg, [System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] params System.Collections.Generic.List<int[]> Arg22) {
            // Serialize arguments
            byte[] ArgPack = MemoryPackSerializer.Serialize(Arg);
            byte[] Arg22Pack = MemoryPackSerializer.Serialize(Arg22);
        
            // Send RPC to specific peer
            RpcId(PeerId, "SendDoStuffRpc", ArgPack, Arg22Pack);
        }
        
        /// <summary>
        /// Remotely calls <see cref="DoStuff(string?, int, System.Collections.Generic.List{int[]})"/>.
        /// </summary>
        public void SendDoStuff(IEnumerable<int>? PeerIds, string? Arg, [System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] params System.Collections.Generic.List<int[]> Arg22) {
            // Skip if no peers
            if (PeerIds is null || !PeerIds.Any()) {
                return;
            }
        
            // Serialize arguments
            byte[] ArgPack = MemoryPackSerializer.Serialize(Arg);
            byte[] Arg22Pack = MemoryPackSerializer.Serialize(Arg22);
        
            // Send RPC to multiple peers
            foreach (int PeerId in PeerIds) {
                RpcId(PeerId, "SendDoStuffRpc", ArgPack, Arg22Pack);
            }
        }
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferChannel = 1234, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered)]
        public void SendDoStuffRpc(byte[] ArgPack, byte[] Arg22Pack) {
            // Deserialize arguments
            var Arg = MemoryPackSerializer.Deserialize<string?>(ArgPack)!;
            var Arg22 = MemoryPackSerializer.Deserialize<System.Collections.Generic.List<int[]>>(Arg22Pack)!;
        
            // Get sender peer ID
            int SenderId = Multiplayer.GetRemoteSenderId();
        
            // Call target method
            DoStuff(Arg, SenderId, Arg22);
        }
    }    
}
