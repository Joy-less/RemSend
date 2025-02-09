<img src="https://raw.githubusercontent.com/Joy-less/RemSend/main/Assets/Icon.png" width="300" />

# Rem Send

A Remote Procedure Call framework for Godot C# using source generators.

## Features

- Call RPCs with static typing.
- Return values from RPCs.
- Send variant-incompatible values (with MemoryPack).
- Extra access enum (peer -> authority).
- Created for use in a real [MMORPG](https://youtu.be/4ptBKI0cGhI).

## Dependencies
- [MemoryPack](https://github.com/Cysharp/MemoryPack)

## Examples

```cs
[Rem(RemAccess.PeerToAuthority)]
public void SayWords(List<string> Words) {
    foreach (string Word in Words) {
        GD.Print(Word);
    }
}

SendSayWords(["cat", "dog"]);
```

```cs
[Rem(RemAccess.PeerToAuthority)]
public int GetNumber() {
    return 5;
}

int Number = await RequestGetNumber();
```

## Special Thanks

- [GodotSharp.SourceGenerators](https://github.com/Cat-Lips/GodotSharp.SourceGenerators)