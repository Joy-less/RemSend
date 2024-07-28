# Rem Send

A Remote Procedure Call framework for Godot C#.

## Features

- Call RPCs with static typing
- Return values from RPCs
- Send variant-incompatible values (with MemoryPack)
- Extra access enum (peer -> authority)

## Dependencies
- [MemoryPack](https://github.com/Cysharp/MemoryPack)

## Examples

```cs
[Rem(RemAccess.Peer)]
public void SayWordsRem(List<string> Words) {
    foreach (string Word in Words) {
        GD.Print(Word);
    }
}

Rem(1, () => SayWordsRem(["cat", "dog"])); // The method name and arguments are extracted from the expression.
```

```cs
[Rem(RemAccess.Peer)]
public int GetNumber() {
    return 5;
}

int Number = await RemWait(1, () => GetNumber());
```

## Limitations

- Uses reflection (may be slow, incompatible with trimming, incompatible with GDScript)
- Optional parameters must be passed explicitly (due to a current limitation with Linq Expressions)
- Only supports one transfer channel