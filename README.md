<img src="https://raw.githubusercontent.com/Joy-less/RemSend/main/Assets/Icon.png" width="300" />

# Rem Send

A Remote Procedure Call framework for Godot C#.

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
public int GetNumberRem() {
    return 5;
}

int Number = await Rem(1, () => GetNumberRem());
```

## Limitations

- Uses reflection (may be slow, incompatible with trimming, incompatible with GDScript)
- Optional parameters must be passed explicitly (due to a current limitation with Linq Expressions)
- Only supports 4 transfer channels (since they are implemented manually)

## Setup

1. Install MemoryPack through NuGet or by editing your `csproj` file:
```
<ItemGroup>
  <PackageReference Include="MemoryPack" Version="1.21.3" />
</ItemGroup>
```

2. Add the Rem Send addon to your project and build the project.

3. Create a `RemSend` node (or create a node and attach the `RemSend.cs` script).