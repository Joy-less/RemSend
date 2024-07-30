#nullable enable

using System;

namespace RemSend;

[AttributeUsage(AttributeTargets.Method)]
public class RemAttribute(RemAccess Access = RemAccess.None) : Attribute {
    /// <summary>
    /// The access permissions for the remote procedure.
    /// </summary>
    public RemAccess Access {get; set;} = Access;
    /// <summary>
    /// If <see langword="true"/>, the remote procedure is called on the local peer when calling on all peers.
    /// </summary>
    public bool CallLocal {get; set;} = false;
    /// <summary>
    /// The send method for remote procedure calls.
    /// </summary>
    public TransferMode Mode {get; set;} = TransferMode.Reliable;
}