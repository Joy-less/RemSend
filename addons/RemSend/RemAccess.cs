#nullable enable

namespace RemSend;

public enum RemAccess {
    /// <summary>
    /// No peer can call this remote procedure.
    /// </summary>
    None,
    /// <summary>
    /// The authority can call this remote procedure.
    /// </summary>
    Authority,
    /// <summary>
    /// Peers can call this remote procedure on the authority.
    /// </summary>
    Peer,
    /// <summary>
    /// Peers can call this remote procedure on other peers.
    /// </summary>
    Any,
}