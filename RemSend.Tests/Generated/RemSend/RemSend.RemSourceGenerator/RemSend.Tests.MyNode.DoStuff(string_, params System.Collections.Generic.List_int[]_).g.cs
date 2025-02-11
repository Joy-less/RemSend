#nullable enable

using System;
using System.ComponentModel;
using Godot;
using MemoryPack;
namespace RemSend.Tests {
    partial class MyNode {
        /// <summary>
        /// Remotely calls <see cref="DoStuff(string?, System.Collections.Generic.List{int[]})"/>.
        /// </summary>
        public void SendDoStuff(int? PeerId, string? Arg, [System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] params System.Collections.Generic.List<int[]> Arg22) {
            // Serialize arguments
            byte[] ArgPack = MemoryPackSerializer.Serialize(Arg);
            byte[] Arg22Pack = MemoryPackSerializer.Serialize(Arg22);
        
            // Broadcast RPC to all peers
            if (PeerId is null) {
                Rpc("SendDoStuffRpc", ArgPack, Arg22Pack);
            }
            // Send RPC to one peer
            else {
                RpcId(PeerId.Value, "SendDoStuffRpc", ArgPack, Arg22Pack);
            }
        }
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferChannel = 1234, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered)]
        public void SendDoStuffRpc(byte[] ArgPack, byte[] Arg22Pack) {
            // Deserialize arguments
            var Arg = MemoryPackSerializer.Deserialize<string?>(ArgPack)!;
            var Arg22 = MemoryPackSerializer.Deserialize<System.Collections.Generic.List<int[]>>(Arg22Pack)!;
        
            // Call target method
            DoStuff(Arg, Arg22);
        }
    }
}