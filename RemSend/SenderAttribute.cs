namespace RemSend;

/// <summary>
/// Apply this attribute to an <see langword="int"/> parameter to get the peer ID of the remote method sender.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class SenderAttribute() : Attribute {
}