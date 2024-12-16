#nullable enable

namespace RemSend;

public enum RemMode {
    /// <summary>
    /// Packets will be resent until they are received. Later packets will wait for earlier packets.<br/>
    /// This is the best mode for important data like using an ability or sending a chat message.<br/>
    /// This mode may add a large overhead in poor network conditions, so use sparingly.
    /// </summary>
    Reliable,
    /// <summary>
    /// Lost packets will not be resent. Later packets will wait for earlier packets.<br/>
    /// This is the best mode for regularly-updated data like player movement.<br/>
    /// This mode may add an overhead in poor network conditions, so use sparingly.
    /// </summary>
    UnreliableOrdered,
    /// <summary>
    /// Lost packets will not be resent. Later packets may arrive before earlier packets.<br/>
    /// This is the best mode for unimportant data like special effects.
    /// </summary>
    Unreliable,
}