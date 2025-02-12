using System;
using System.Collections.Frozen;
using System.Text;
using System.Threading.Tasks;
using Godot;
using RemSend;

public partial class Main : Node {
    private SceneMultiplayer SceneMultiplayer => (SceneMultiplayer)Multiplayer;

    public override async void _Ready() {
        //SendSayHello(0, 3);

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

            await Task.Delay(TimeSpan.FromSeconds(0.5));

            SceneMultiplayer.SendBytes([1, 2, 3], 1, MultiplayerPeer.TransferModeEnum.Unreliable, 134);
        }
    }

    [Rem(RemAccess.Any, CallLocal: true)]
    public void SayHello([Sender] int SenderId, int Times) {
        for (int Counter = 0; Counter < Times; Counter++) {
            GD.Print("Hello!");
        }
    }

    private void CreateServer(int Port) {
        ENetMultiplayerPeer Peer = new();
        Peer.CreateServer(Port);
        Multiplayer.MultiplayerPeer = Peer;
        RemSender.Setup(GetTree().Root, (SceneMultiplayer)Multiplayer);
    }
    private void CreateClient(string Address, int Port) {
        ENetMultiplayerPeer Peer = new();
        Peer.CreateClient(Address, Port);
        Multiplayer.MultiplayerPeer = Peer;
        RemSender.Setup(GetTree().Root, (SceneMultiplayer)Multiplayer);
    }
}