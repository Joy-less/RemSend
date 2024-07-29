#nullable enable

using System.Threading;
using MemoryPack;

namespace RemSend;

[MemoryPackable]
internal sealed partial class RemPacket(string TargetPath, string MethodName, byte[][] PackedArguments) {
    public string TargetPath = TargetPath;
    public string MethodName = MethodName;
    public byte[][] PackedArguments = PackedArguments;
    public ulong PacketId = Interlocked.Increment(ref LastPacketId);

    private static ulong LastPacketId;
}