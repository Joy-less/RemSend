using Godot;
using RemSend;

public partial class Main : Node {
    public override void _Ready() {
        SendSayHello(0, 3);
    }

    [Rem(RemAccess.Any, CallLocal: true)]
    public void SayHello(int Times) {
        for (int Counter = 0; Counter < Times; Counter++) {
            GD.Print("Hello!");
        }
    }
}