namespace RemSend;

/// <summary>
/// A serializable packet containing data for a remote method call.
/// </summary>
public record struct RemPacket(RemPacketType Type, string NodePath, string MethodName, byte[] ArgumentsPack);

/// <summary>
/// The type of a <see cref="RemPacket"/>.
/// </summary>
public enum RemPacketType : byte {
    /// <summary>
    /// A remote method call.
    /// </summary>
    Send = 1,
    /// <summary>
    /// A remote method call that expects a result back.
    /// </summary>
    Request = 2,
    /// <summary>
    /// A result for a request.
    /// </summary>
    Result = 3,
}