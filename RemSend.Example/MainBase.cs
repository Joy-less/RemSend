using Godot;
using RemSend;

public partial class MainBase : Node {
    [Rem(RemAccess.Any)]
    public void SayInherited() {
        GD.Print("Inheritance works");
    }
}