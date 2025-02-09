namespace RemSend;

public enum RemAccess {
    /// <summary>
    /// No peer can call this remote method.
    /// </summary>
    None,
    /// <summary>
    /// The authority can call this remote method on any peer.
    /// </summary>
    Authority,
    /// <summary>
    /// Any peer can call this remote method on the authority.
    /// </summary>
    PeerToAuthority,
    /// <summary>
    /// Any peer can call this remote method on any peer.
    /// </summary>
    Any,
}