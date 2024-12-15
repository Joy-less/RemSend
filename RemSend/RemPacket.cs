using MemoryPack;

namespace RemSend;

[MemoryPackable]
internal sealed partial class RemPacket(string TargetPath, string MethodName, byte[][] PackedArguments) {
    public string TargetPath = TargetPath;
    public string MethodName = MethodName;
    public byte[][] PackedArguments = PackedArguments;
    public long PacketId = Interlocked.Increment(ref LastPacketId);

    private static long LastPacketId;
}