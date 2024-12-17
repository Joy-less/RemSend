#nullable enable

using System;

namespace RemSend;

[AttributeUsage(AttributeTargets.Method)]
public class RemAttribute(RemAccess Access = RemAccess.None) : Attribute {
    /// <summary>
    /// The access permissions for the remote method.
    /// </summary>
    public RemAccess Access { get; set; } = Access;
    /// <summary>
    /// If <see langword="true"/>, the remote method is called on the local peer when calling on all peers.
    /// </summary>
    public bool CallLocal { get; set; } = false;
    /// <summary>
    /// The send mode for remote method calls.
    /// </summary>
    public RemMode Mode { get; set; } = RemMode.Reliable;
    /// <summary>
    /// The transfer channel number for remote method calls.<br/>
    /// Changing this can reduce congestion between unrelated calls (e.g. chatting and attacking).
    /// </summary>
    /// <remarks>The supported channels are 0 to <inheritdoc cref="RemSend.MaxChannel" path="//value"/>.</remarks>
    public int Channel { get; set; } = 0;
}