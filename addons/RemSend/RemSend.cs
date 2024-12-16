#nullable enable
#pragma warning disable CS8618

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using Godot;
using MemoryPack;

using Lq = System.Linq.Expressions;
using RpcMode = Godot.MultiplayerApi.RpcMode;

namespace RemSend;

public partial class RemSend : Node {
    public static RemSend Singleton { get; private set; }

    private readonly ConcurrentDictionary<long, TaskCompletionSource<byte[]>> ResponseAwaiters = [];

    private const BindingFlags Bindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy;

    public override void _EnterTree() {
        Singleton = this;
    }
    public override void _Ready() {
        GodotMemoryPackFormatters.RegisterTypes();
    }

    internal long BroadcastRem(Lq.MethodCallExpression CallExpression) {
        return SendPacket(CallExpression, (byte[] PackedPacket, StringName TransferRpcName) => {
            // Transfer packet to all peers
            Rpc(TransferRpcName, PackedPacket);
            // Maybe call locally
            return true;
        });
    }
    internal long SendRem(IEnumerable<int> PeerIds, Lq.MethodCallExpression CallExpression) {
        return SendPacket(CallExpression, (byte[] PackedPacket, StringName TransferRpcName) => {
            // Transfer packet to given peers
            foreach (int PeerId in PeerIds) {
                RpcId(PeerId, TransferRpcName, PackedPacket);
            }
            // Never call locally
            return false;
        });
    }
    internal async Task<T> BroadcastRemAwaitResponse<T>(Lq.MethodCallExpression CallExpression, double Timeout, CancellationToken CancelToken = default) {
        return await AwaitResponseAsync<T>(BroadcastRem(CallExpression), Timeout, CancelToken);
    }
    internal async Task<T> SendRemAwaitResponse<T>(IEnumerable<int> PeerIds, Lq.MethodCallExpression CallExpression, double Timeout, CancellationToken CancelToken = default) {
        return await AwaitResponseAsync<T>(SendRem(PeerIds, CallExpression), Timeout, CancelToken);
    }

    private static long SendPacket(Lq.MethodCallExpression Expression, Func<byte[], StringName, bool> CallTransferRpc) {
        // Get target node from method call expression
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
        // Pack packet
        byte[] PackedPacket = MemoryPackSerializer.Serialize(Packet);

        // Ensure transfer mode is implemented
        if (!TransferRpcs.TryGetValue(RemAttribute.Mode, out StringName[]? TransferRpcsForMode)) {
            throw new NotImplementedException($"Remote call mode not implemented: {RemAttribute.Mode}");
        }
        // Ensure transfer channel is within supported range
        if (RemAttribute.Channel < 0 || RemAttribute.Channel >= TransferRpcsForMode.Length) {
            throw new InvalidOperationException($"Remote call channel out of range (0 to {TransferRpcsForMode.Length - 1}): {RemAttribute.Channel}");
        }
        // Get transfer RPC from attribute
        StringName TransferRpc = TransferRpcsForMode[RemAttribute.Channel];

        // Transfer packet
        if (CallTransferRpc(PackedPacket, TransferRpc)) {
            // Call RPC locally
            if (RemAttribute.CallLocal) {
                ReceivePacket(PackedPacket);
            }
        }
        // Return packet ID to await response
        return Packet.PacketId;
    }
    private static async void ReceivePacket(byte[] PackedPacket) {
        // Deserialise packet
        RemPacket Packet = MemoryPackSerializer.Deserialize<RemPacket>(PackedPacket)!;

        // Get peer IDs
        int RemoteId = Singleton.Multiplayer.GetRemoteSenderId();
        int LocalId = Singleton.Multiplayer.GetUniqueId();

        // Get target from path
        Node Target = Singleton.GetNodeOrNull(Packet.TargetPath)
            ?? throw new Exception($"Remote node '{Packet.TargetPath}' not found: '{Packet.MethodName}'");
        // Get method from node
        if (Target.GetType().GetMethod(Packet.MethodName, Bindings) is not MethodInfo Method) {
            throw new Exception($"Remote method not found: '{Packet.MethodName}'");
        }

        // Ensure remote method has attribute
        if (Method.GetCustomAttribute<RemAttribute>() is not RemAttribute RemAttribute) {
            throw new Exception($"Remote method has no {typeof(RemAttribute).Name}: '{Packet.MethodName}'");
        }
        // Ensure remote method is accessible
        switch (RemAttribute.Access) {
            case RemAccess.Authority:
                if (RemoteId is not (0 or 1)) {
                    throw new Exception($"Remote method cannot be called by non-authority: '{Packet.MethodName}'");
                }
                break;
            case RemAccess.Peer:
                if (LocalId is not 1) {
                    throw new Exception($"Remote method cannot be called on non-authority: '{Packet.MethodName}'");
                }
                break;
            case RemAccess.Any:
                break;
            case RemAccess.None or _:
                throw new Exception($"Remote method cannot be called: '{Packet.MethodName}'");
        }

        // Unpack arguments
        object?[] Arguments = Packet.PackedArguments.UnpackArguments(Method.GetParameters());
        // Invoke method with arguments
        Type ReturnType = Method.ReturnType;
        object? ReturnValue = Method.Invoke(Target, Arguments);

        // Method returns void
        if (ReturnType == typeof(void)) {
            // Don't return value
            return;
        }
        // Method returns task
        else if (ReturnValue is Task Task) {
            // Await task
            await Task;

            // Method returns task without value
            if (ReturnType == typeof(Task)) {
                // Return dummy value (instead of VoidTaskResult)
                ReturnType = typeof(byte);
                ReturnValue = (byte)0;
            }
            // Method returns task with value
            else {
                // Return task value
                PropertyInfo TaskResultProperty = Task.GetType().GetProperty(nameof(Task<object>.Result))!;
                ReturnType = TaskResultProperty.PropertyType;
                ReturnValue = TaskResultProperty.GetValue(Task);
            }
        }

        // Ensure reponse transfer channel is within supported range
        if (RemAttribute.Channel < 0 || RemAttribute.Channel >= ResponseTransferRpcs.Length) {
            throw new InvalidOperationException($"Remote call channel out of range (0 to {ResponseTransferRpcs.Length - 1}): {RemAttribute.Channel}");
        }
        // Get reponse transfer RPC from attribute
        StringName ResponseTransferRpc = ResponseTransferRpcs[RemAttribute.Channel];

        // Rpc return value
        Singleton.RpcId(RemoteId, ResponseTransferRpc, Packet.PacketId, MemoryPackSerializer.Serialize(ReturnType, ReturnValue));
    }
    private async Task<T> AwaitResponseAsync<T>(long PacketId, double Timeout, CancellationToken CancelToken = default) {
        // Add response awaiter
        TaskCompletionSource<byte[]> ResponseAwaiter = ResponseAwaiters.GetOrAdd(PacketId, PacketId => new());
        try {
            // Await response
            byte[] ReturnValue = await ResponseAwaiter.Task.WaitAsync(TimeSpan.FromSeconds(Timeout), CancelToken);
            // Return value
            return MemoryPackSerializer.Deserialize<T>(ReturnValue)!;
        }
        finally {
            // Free response awaiter
            ResponseAwaiters.TryRemove(PacketId, out _);
        }
    }
}