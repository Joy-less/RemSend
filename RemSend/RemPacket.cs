namespace RemSend;

public record struct RemPacket(RemPacketType Type, string NodePath, string MethodName, byte[] ArgumentsPack);

public enum RemPacketType : byte {
    Message = 1,
    Request = 2,
    Result = 3,
}