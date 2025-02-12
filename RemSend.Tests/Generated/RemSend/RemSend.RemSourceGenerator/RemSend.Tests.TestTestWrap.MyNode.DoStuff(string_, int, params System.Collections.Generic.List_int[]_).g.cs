#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Godot;
using MemoryPack;

namespace RemSend.Tests;

partial class TestTestWrap {
    partial class MyNode {    
        /// <summary>
        /// Remotely calls <see cref="DoStuff(string?, int, System.Collections.Generic.List{int[]})"/> on the given peer.<br/>
        /// Set <paramref name="PeerId"/> to 0 to broadcast to all peers.<br/>
        /// Set <paramref name="PeerId"/> to 1 to send to the authority.
        /// </summary>
        public void SendDoStuff(int PeerId, string? Arg, [System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] params System.Collections.Generic.List<int[]> Arg22) {
            // Serialize node path
            Span<byte> _NodePathBytes = Encoding.UTF8.GetBytes(GetPath());
            // Serialize method name
            Span<byte> _MethodNameBytes = [83, 101, 110, 100, 68, 111, 83, 116, 117, 102, 102];
            // Serialize arguments
            Span<byte> ArgBytes = MemoryPackSerializer.Serialize(Arg);
            Span<byte> Arg22Bytes = MemoryPackSerializer.Serialize(Arg22);
        
            // Combine packet
            Span<byte> _Packet = [
                .. BitConverter.GetBytes(_MethodNameBytes.Length), .. _MethodNameBytes,
                .. BitConverter.GetBytes(_NodePathBytes.Length), .. _NodePathBytes,
                .. BitConverter.GetBytes(ArgBytes.Length), .. ArgBytes,
                .. BitConverter.GetBytes(Arg22Bytes.Length), .. Arg22Bytes,
            ];
        
            // Send packet to single peer ID
            ((SceneMultiplayer)Multiplayer).SendBytes(
                bytes: _Packet,
                id: PeerId,
                mode: MultiplayerPeer.TransferModeEnum.UnreliableOrdered,
                channel: 1234
            );
        }
        
        /// <summary>
        /// Remotely calls <see cref="DoStuff(string?, int, System.Collections.Generic.List{int[]})"/> on each peer.
        /// </summary>
        public void SendDoStuff(IEnumerable<int>? PeerIds, string? Arg, [System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] params System.Collections.Generic.List<int[]> Arg22) {
            // Skip if no peers
            if (PeerIds is null || !PeerIds.Any()) {
                return;
            }
        
            // Serialize node path
            Span<byte> _NodePathBytes = Encoding.UTF8.GetBytes(GetPath());
            // Serialize method name
            Span<byte> _MethodNameBytes = [83, 101, 110, 100, 68, 111, 83, 116, 117, 102, 102];
            // Serialize arguments
            Span<byte> ArgBytes = MemoryPackSerializer.Serialize(Arg);
            Span<byte> Arg22Bytes = MemoryPackSerializer.Serialize(Arg22);
            
            // Combine packet components
            Span<byte> _Packet = [
                .. BitConverter.GetBytes(_MethodNameBytes.Length), .. _MethodNameBytes,
                .. BitConverter.GetBytes(_NodePathBytes.Length), .. _NodePathBytes,
                .. BitConverter.GetBytes(ArgBytes.Length), .. ArgBytes,
                .. BitConverter.GetBytes(Arg22Bytes.Length), .. Arg22Bytes,
            ];
            
            // Send call data to multiple peer IDs
            foreach (int PeerId in PeerIds) {
                ((SceneMultiplayer)Multiplayer).SendBytes(
                    bytes: _Packet,
                    id: PeerId,
                    mode: MultiplayerPeer.TransferModeEnum.UnreliableOrdered,
                    channel: 1234
                );
            }
        }
        
        private void SendDoStuffHandler(int SenderId, Span<byte> Packet) {
            /*((SceneMultiplayer)Multiplayer).PeerPacket += (SenderId, Packet) => {
                GD.Print($"received {Packet.Length} bytes from {SenderId}");
            };*/
        
            // Deserialize arguments
            var Arg = MemoryPackSerializer.Deserialize<string?>(ArgBytes)!;
            var Arg22 = MemoryPackSerializer.Deserialize<System.Collections.Generic.List<int[]>>(Arg22Bytes)!;
        
            // Call target method
            DoStuff(Arg, _SenderId, Arg22);
        }
        
        private void SendHandler(int SenderId, Span<byte> Packet) {
            // Deserialize node path
            NodePath NodePath = Encoding.UTF8.GetString(EatComponent(ref Packet));
            // Deserialize method name
            string MethodName = Encoding.UTF8.GetString(EatComponent(ref Packet));
        
            // Find node
            Node Node = GetNode(NodePath);
            // Find handler method
            if (MethodName == "SendDoStuff") {
                Node.SendDoStuffHandler(SenderId, Packet);
            }
        }
        
        private static Span<byte> EatComponent(ref Span<byte> Packet) {
            // Eat component length
            int Length = BitConverter.ToInt32(Packet[..sizeof(int)]);
            Packet = Packet[sizeof(int)..];
            // Eat component content
            Span<byte> Content = Packet[..Length];
            Packet = Packet[Length..];
            // Return component content
            return Content;
        }
    }    
}
