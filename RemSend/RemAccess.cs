namespace RemSend;

public enum RemAccess {
    /// <summary>
    /// No peer can call this remote method.
    /// </summary>
    None,
    /// <summary>
    /// The authority can call this remote method.
    /// </summary>
    Authority,
    /// <summary>
    /// Peers can call this remote method on the authority.
    /// </summary>
    Peer,
    /// <summary>
    /// Peers can call this remote method on other peers.
    /// </summary>
    Any,
}