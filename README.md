<img src="https://raw.githubusercontent.com/Joy-less/RemSend/main/Assets/Icon.png" width="300" />

# Rem Send

A Remote Procedure Call framework for Godot C# using source generators.

## Features

- Call source-generated RPCs with static typing and optimal performance.
- Request and return values from RPCs.
- Send variant-incompatible values with MemoryPack.
- Extra access enum (peer -> authority).
- Created for use in a real [MMORPG](https://youtu.be/4ptBKI0cGhI).

## Setup

1. Install RemSend through NuGet.
2. Connect RemSend to your MultiplayerApi by using the following code:
```cs
RemSendService.Setup((SceneMultiplayer)Multiplayer, GetTree().Root);
```

## Examples

```cs
[Rem(RemAccess.Any)]
public void SayWords(List<string> Words) {
    foreach (string Word in Words) {
        GD.Print(Word);
    }
}

// Broadcast SayWords to all peers
SendSayWords(0, ["cat", "dog"]);
```

```cs
[Rem(RemAccess.PeerToAuthority)]
public int GetNumber() {
    return 5;
}

// Send GetNumber to authority and await result for up to 10 seconds
int Number = await RequestGetNumber(1, TimeSpan.FromSeconds(10));
```

## Notes

- Since RemSend uses `SceneMultiplayer.SendBytes` and `SceneMultiplayer.PeerPacket`, you can't use them together with RemSend. However, you can still use RPCs.

## Special Thanks

- [GodotSharp.SourceGenerators](https://github.com/Cat-Lips/GodotSharp.SourceGenerators) for help with generating source for members with attributes.