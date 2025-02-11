#nullable enable

using Godot;
using System.ComponentModel;
using MemoryPack;
namespace RemSend.Tests {
    partial class MyNode {
        /// 
        public void SendDoStuff(int? PeerId, string Arg, [System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] params System.Collections.Generic.List<int[]> Arg22) {
            // Serialize arguments
            SendDoStuffPack ArgumentsPack = new(Arg, Arg22);
            byte[] SerializedArgumentsPack = MemoryPackSerializer.Serialize(ArgumentsPack);
        
            // Broadcast RPC to all peers
            if (PeerId is null) {
                Rpc("SendDoStuffRpc", SerializedArgumentsPack);
            }
            // Send RPC to one peer
            else {
                RpcId(PeerId.Value, "SendDoStuffRpc", SerializedArgumentsPack);
            }
        }
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferChannel = 1234, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered)]
        public void SendDoStuffRpc(byte[] SerializedArguments) {
            // Deserialize arguments
            SendDoStuffPack ArgumentsPack = MemoryPackSerializer.Deserialize<SendDoStuffPack>(SerializedArguments);
        
            // Call target method
            DoStuff(ArgumentsPack.Arg, ArgumentsPack.Arg22);
        }
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        private record struct SendDoStuffPack(string Arg, System.Collections.Generic.List<int[]> Arg22);
    }
}