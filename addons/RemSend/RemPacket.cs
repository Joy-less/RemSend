#nullable enable

using System.Threading;
using MemoryPack;

namespace RemSend;

[MemoryPackable]
internal partial record struct RemPacket(string TargetPath, string MethodName, byte[][] PackedArguments) {
    public string TargetPath { get; set; } = TargetPath;
    public string MethodName { get; set; } = MethodName;
    public byte[][] PackedArguments { get; set; } = PackedArguments;
    public long PacketId { get; set; } = Interlocked.Increment(ref LastPacketId);

    private static long LastPacketId;
}