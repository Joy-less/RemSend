#nullable enable

using System.Threading;
using MemoryPack;

namespace RemSend;

[MemoryPackable]
internal readonly partial struct RemPacket(string TargetPath, string MethodName, byte[][] PackedArguments) {
    public string TargetPath { get; } = TargetPath;
    public string MethodName { get; } = MethodName;
    public byte[][] PackedArguments { get; } = PackedArguments;
    public long PacketId { get; } = Interlocked.Increment(ref LastPacketId);

    private static long LastPacketId;
}