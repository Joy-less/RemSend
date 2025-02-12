using Godot;
using RemSend;

public partial class Main : Node {
    private SceneMultiplayer SceneMultiplayer => (SceneMultiplayer)Multiplayer;

    public override async void _Ready() {
        // Server
        if (OS.HasFeature("server")) {
            CreateServer(12345);

            SceneMultiplayer.PeerPacket += (long SenderId, byte[] Packet) => {
                GD.Print($"received {Packet.Length} bytes from {SenderId}");
            };
        }
        // Client
        else {
            CreateClient("localhost", 12345);

            await ToSignal(Multiplayer, MultiplayerApi.SignalName.ConnectedToServer);

            SendSayHello(1, 4);
            SendSayHello(1);
        }
    }

    [Rem(RemAccess.Any, CallLocal: true)]
    public void SayHello([Sender] int SenderId, int Times = 1) {
        GD.Print($"{nameof(SayHello)} called by {SenderId}");

        for (int Counter = 0; Counter < Times; Counter++) {
            GD.Print("Hello!");
        }
    }

    private void CreateServer(int Port) {
        ENetMultiplayerPeer Peer = new();
        Peer.CreateServer(Port);
        Multiplayer.MultiplayerPeer = Peer;
        RemSendService.Setup((SceneMultiplayer)Multiplayer, GetTree().Root);
    }
    private void CreateClient(string Address, int Port) {
        ENetMultiplayerPeer Peer = new();
        Peer.CreateClient(Address, Port);
        Multiplayer.MultiplayerPeer = Peer;
        RemSendService.Setup((SceneMultiplayer)Multiplayer, GetTree().Root);
    }
}