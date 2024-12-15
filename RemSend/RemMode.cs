namespace RemSend;

public enum RemMode {
    /// <inheritdoc cref="Godot.MultiplayerPeer.TransferModeEnum.Unreliable"/>
    Unreliable,
    /// <inheritdoc cref="Godot.MultiplayerPeer.TransferModeEnum.UnreliableOrdered"/>
    UnreliableOrdered,
    /// <inheritdoc cref="Godot.MultiplayerPeer.TransferModeEnum.Reliable"/>
    Reliable,
}