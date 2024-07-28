using TransferMode = Godot.MultiplayerPeer.TransferModeEnum;

namespace RemSend;

[AttributeUsage(AttributeTargets.Method)]
public class RemAttribute(RemAccess Access = RemAccess.None) : Attribute {
    public RemAccess Access {get; init;} = Access;
    public bool CallLocal {get; init;} = false;
    public TransferMode Mode {get; init;} = TransferMode.Reliable;
}