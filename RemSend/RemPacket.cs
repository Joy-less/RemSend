namespace RemSend;

public record struct RemPacket(string NodePath, string MethodName, byte[] ArgumentsPack);