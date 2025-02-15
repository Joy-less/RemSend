using System;
using System.Threading.Tasks;
using Godot;
using RemSend;

public partial class Main : Node {
    public override async void _Ready() {
        // Server
        if (OS.HasFeature("server")) {
            CreateServer(12345);
        }
        // Client
        else {
            CreateClient("localhost", 12345);

            await ToSignal(Multiplayer, MultiplayerApi.SignalName.ConnectedToServer);

            SendSayHello(1, 4);
            SendSayHello(1);

            await RequestWaitASecond(1, TimeSpan.FromSeconds(10));

            GD.Print(await RequestAreYouTheServer(1, TimeSpan.FromSeconds(10)));

            GD.Print(await RequestGiveNineAfterASecond(1, TimeSpan.FromSeconds(10)));
        }
    }

    [Rem(RemAccess.Any, CallLocal: true)]
    public void SayHello([Sender] int SenderId, int Times = 1) {
        GD.Print($"{nameof(SayHello)} called by {SenderId}");

        for (int Counter = 0; Counter < Times; Counter++) {
            GD.Print("Hello!");
        }
    }
    [Rem(RemAccess.PeerToAuthority)]
    public bool AreYouTheServer() {
        return OS.HasFeature("server");
    }
    [Rem(RemAccess.Any)]
    public async Task WaitASecond() {
        await Task.Delay(TimeSpan.FromSeconds(1));
    }
    [Rem(RemAccess.Any)]
    public async Task<sbyte> GiveNineAfterASecond() {
        await Task.Delay(TimeSpan.FromSeconds(1));
        return 9;
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