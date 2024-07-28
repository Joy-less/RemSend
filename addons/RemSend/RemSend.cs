#pragma warning disable CS8618

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using Godot;
using MemoryPack;

using Lq = System.Linq.Expressions;
using RpcMode = Godot.MultiplayerApi.RpcMode;
using TransferMode = Godot.MultiplayerPeer.TransferModeEnum;

namespace RemSend;

public partial class RemSend : Node {
    public static RemSend Singleton {get; private set;}

    private event Action<ulong, byte[]>? OnResponse;

    public override void _Ready() {
        Singleton = this;
        MemoryPackFormatters.RegisterCustomTypes();
    }

    internal ulong Rem(Lq.MethodCallExpression CallExpression) {
        (RemPacket Packet, RemAttribute RemAttribute, StringName RpcName) = CreatePacket(CallExpression);

        // Call remotely
        Rpc(RpcName, MemoryPackSerializer.Serialize(Packet));
        // Also call locally
        if (RemAttribute.CallLocal) {
            RpcId(Multiplayer.GetUniqueId(), RpcName, MemoryPackSerializer.Serialize(Packet));
        }

        return Packet.PacketId;
    }
    internal ulong Rem(IEnumerable<int> PeerIds, Lq.MethodCallExpression CallExpression) {
        (RemPacket Packet, RemAttribute RemAttribute, StringName RpcName) = CreatePacket(CallExpression);

        // Call remotely
        foreach (int PeerId in PeerIds) {
            RpcId(PeerId, RpcName, MemoryPackSerializer.Serialize(Packet));
        }
        // Also call locally
        if (RemAttribute.CallLocal) {
            int LocalId = Multiplayer.GetUniqueId();
            if (!PeerIds.Contains(LocalId)) {
                RpcId(LocalId, RpcName, MemoryPackSerializer.Serialize(Packet));
            }
        }
        
        return Packet.PacketId;
    }
    internal async Task<T> RemWait<T>(Lq.MethodCallExpression CallExpression, CancellationToken CancelToken = default) {
        return await AwaitResponseAsync<T>(Rem(CallExpression), CancelToken);
    }
    internal async Task<T> RemWait<T>(IEnumerable<int> PeerIds, Lq.MethodCallExpression CallExpression, CancellationToken CancelToken = default) {
        return await AwaitResponseAsync<T>(Rem(PeerIds, CallExpression), CancelToken);
    }

    [Rpc(RpcMode.AnyPeer, TransferMode = TransferMode.Reliable)]
    public void ReliablePacketRpc(byte[] PackedRemPacket) {
        ReceivePacket(PackedRemPacket);
    }
    [Rpc(RpcMode.AnyPeer, TransferMode = TransferMode.UnreliableOrdered)]
    public void UnreliableOrderedPacketRpc(byte[] PackedRemPacket) {
        ReceivePacket(PackedRemPacket);
    }
    [Rpc(RpcMode.AnyPeer, TransferMode = TransferMode.Unreliable)]
    public void UnreliablePacketRpc(byte[] PackedRemPacket) {
        ReceivePacket(PackedRemPacket);
    }
    [Rpc(RpcMode.AnyPeer, TransferMode = TransferMode.Reliable)]
    public void PacketResponseRpc(ulong PacketId, byte[] PackedReturnValue) {
        OnResponse?.Invoke(PacketId, PackedReturnValue);
    }

    private static StringName GetRpcForTransferMode(TransferMode TransferMode) {
        return TransferMode switch {
            TransferMode.Reliable => MethodName.ReliablePacketRpc,
            TransferMode.UnreliableOrdered => MethodName.UnreliableOrderedPacketRpc,
            TransferMode.Unreliable => MethodName.UnreliablePacketRpc,
            _ => throw new NotImplementedException($"Remote call transfer mode not implemented: {TransferMode}")
        };
    }
    private static (RemPacket Packet, RemAttribute RemAttribute, StringName RpcName) CreatePacket(Lq.MethodCallExpression Expression) {
        // Get target node from expression
        if (Expression.Object.Evaluate() is not Node Target) {
            throw new Exception($"Remote call method target must be Node (got '{Expression.Object?.GetType().Name ?? "null"}'): '{Expression.Method.Name}'");
        }
        // Get rem attribute
        RemAttribute RemAttribute = Expression.Method.GetCustomAttribute<RemAttribute>()
            ?? throw new Exception($"Remote call method must have {typeof(RemAttribute).Name}: '{Expression.Method.Name}'");
        // Get arguments from method call
        object?[] Arguments = Expression.Arguments.Evaluate();
        // Pack arguments
        byte[][] PackedArguments = Arguments.PackArguments(Expression.Method.GetParameters());
        // Create packet
        RemPacket Packet = new(Target.GetPath(), Expression.Method.Name, PackedArguments);
        // Get RPC name
        StringName RpcName = GetRpcForTransferMode(RemAttribute.Mode);
        // Return RPC data
        return (Packet, RemAttribute, RpcName);
    }
    private static async void ReceivePacket(byte[] PackedRemPacket) {
        // Deserialise packet
        RemPacket Packet = MemoryPackSerializer.Deserialize<RemPacket>(PackedRemPacket)!;

        // Get peer IDs
        int RemoteId = Singleton.Multiplayer.GetRemoteSenderId();
        int LocalId = Singleton.Multiplayer.GetUniqueId();

        // Get target from path
        Node Target = Singleton.GetNodeOrNull(Packet.TargetPath)
            ?? throw new Exception($"Remote node not found: '{Packet.TargetPath}' (method: '{Packet.MethodName}')");
        // Get method from node
        if (Target.GetType().GetMethod(Packet.MethodName) is not MethodInfo Method) {
            throw new Exception($"Remote method not found: '{Packet.MethodName}'");
        }

        // Ensure remote method has attribute
        if (Method.GetCustomAttribute<RemAttribute>() is not RemAttribute RemAttribute) {
            throw new Exception($"Remote method has no {typeof(RemAttribute).Name}: '{Packet.MethodName}'");
        }
        // Ensure remote method is accessible
        switch (RemAttribute.Access) {
            case RemAccess.None:
                throw new Exception($"Remote method cannot be called: '{Packet.MethodName}'");
            case RemAccess.Authority:
                if (RemoteId != MultiplayerPeer.TargetPeerServer) {
                    throw new Exception($"Remote method cannot be called by non-authority: '{Packet.MethodName}'");
                }
                break;
            case RemAccess.Peer:
                if (LocalId != MultiplayerPeer.TargetPeerServer) {
                    throw new Exception($"Remote method cannot be called on non-authority: '{Packet.MethodName}'");
                }
                break;
        }

        // Unpack arguments
        object?[] Arguments = Packet.PackedArguments.UnpackArguments(Method.GetParameters());
        // Invoke method with arguments
        object? ReturnValue = Method.Invoke(Target, Arguments);

        // Wait for return value unless void
        if (Method.ReturnType != typeof(void) && Method.ReturnType != typeof(Task)) {
            // Unwrap task result
            Type ReturnType = Method.ReturnType;
            if (ReturnValue is Task Task) {
                // Ensure task has return value
                if (Task.GetType().GetProperty(nameof(Task<object>.Result)) is PropertyInfo TaskResultProperty) {
                    // Await task
                    await Task;
                    // Get unwrapped return type and value
                    ReturnType = TaskResultProperty.PropertyType;
                    ReturnValue = TaskResultProperty.GetValue(Task);
                }
            }
            // Rpc return value
            byte[] PackedReturnValue = MemoryPackSerializer.Serialize(ReturnType, ReturnValue);
            Singleton.RpcId(RemoteId, MethodName.PacketResponseRpc, Packet.PacketId, PackedReturnValue);
        }
    }

    private async Task<T> AwaitResponseAsync<T>(ulong PacketId, CancellationToken CancelToken = default) {
        TaskCompletionSource<T> TaskSource = new();
        void ResponseListener(ulong Id, byte[] ReturnValue) {
            if (Id == PacketId) {
                TaskSource.TrySetResult(MemoryPackSerializer.Deserialize<T>(ReturnValue)!);
            }
        };
        OnResponse += ResponseListener;
        T ReturnValue = await TaskSource.Task.WaitAsync(CancelToken);
        OnResponse -= ResponseListener;
        return ReturnValue;
    }
}