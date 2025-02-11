using Godot;
using RemSend;

public partial class Main : Node {
    public override async void _Ready() {
        //SendSayHello(0, 3);

        // Server
        if (OS.HasFeature("server")) {
            CreateServer(12345);

            await ToSignal(Multiplayer, MultiplayerApi.SignalName.PeerConnected);
            
            while (Multiplayer.MultiplayerPeer.GetAvailablePacketCount() <= 0) {
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }

            GD.Print("got something");

            //Multiplayer.MultiplayerPeer.GetPacketPeer();
            byte[] Packet = Multiplayer.MultiplayerPeer.GetPacket();
            GD.Print(Packet.Length);
        }
        // Client
        else {
            CreateClient("localhost", 12345);

            await ToSignal(Multiplayer, MultiplayerApi.SignalName.ConnectedToServer);

            GD.Print("sending");

            Multiplayer.MultiplayerPeer.SetTargetPeer(1);
            Multiplayer.MultiplayerPeer.PutPacket([1, 2, 5]);
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
    }
    private void CreateClient(string Address, int Port) {
        ENetMultiplayerPeer Peer = new();
        Peer.CreateClient(Address, Port);
        Multiplayer.MultiplayerPeer = Peer;
    }
}