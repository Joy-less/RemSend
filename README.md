<img src="https://raw.githubusercontent.com/Joy-less/RemSend/main/Assets/Icon.png" width="300" />

# Rem Send

[![NuGet](https://img.shields.io/nuget/v/RemSend.svg)](https://www.nuget.org/packages/RemSend)

A Remote Procedure Call framework for Godot C# using source generators.

## Features

- Call source-generated RPCs with static typing and optimal performance.
- Request and return values from RPCs.
- Send variant-incompatible values with MemoryPack.
- Extra access enum (peer -> authority).
- Fully compatible with async / Tasks.
- Created for use in a real [MMORPG](https://youtu.be/4ptBKI0cGhI).

## Setup

1. Install RemSend through NuGet.
2. Connect RemSend to your `MultiplayerApi` by using the following code:
```cs
RemSendService.Setup((SceneMultiplayer)Multiplayer, GetTree().Root);
```

## Examples

Sending a method call to a remote peer:

```cs
[Rem(RemAccess.Any)]
public void SayWords(List<string> Words) {
    foreach (string Word in Words) {
        GD.Print(Word);
    }
}

// Send SayWords to authority
SendSayWords(1, ["cat", "dog"]);
// Broadcast SayWords to all peers
BroadcastSayWords(["cat", "dog"]);
```

Requesting a result from a peer:

```cs
[Rem(RemAccess.PeerToAuthority)]
public int GetNumber() {
    return 5;
}

// Send GetNumber to authority and await result for up to 10 seconds
int Number = await RequestGetNumber(1, TimeSpan.FromSeconds(10));
```

Getting the remote sender's peer ID:

```cs
[Rem(RemAccess.Any)]
public void RemoteHug([Sender] int SenderId) {
    if (SenderId == 1) {
        GD.Print("Thank you authority.");
    }
    else if (SenderId == Multiplayer.GetUniqueId()) {
        GD.Print("*depression*");
    }
    else {
        GD.Print("Thank you random peer.");
    }
}

// Send RemoteHug to authority
SendRemoteHug(1);
```

## Notes

- Since RemSend uses `SceneMultiplayer.SendBytes` and `SceneMultiplayer.PeerPacket`, using them with RemSend is not recommended. However, you can still use RPCs.
- RemSend does not support GDScript or C++. It can only be used with C#. Other languages can use RPCs instead.

## Special Thanks

- [GodotSharp.SourceGenerators](https://github.com/Cat-Lips/GodotSharp.SourceGenerators) for help with source generation for members with attributes.