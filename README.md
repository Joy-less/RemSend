<img src="https://raw.githubusercontent.com/Joy-less/RemSend/main/Assets/Icon.png" width="300" />

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

## Setup

1. Install MemoryPack through NuGet or by editing your `csproj` file:
```
<ItemGroup>
  <PackageReference Include="MemoryPack" Version="1.21.1" />
</ItemGroup>
```

2. Add the Rem Send addon to your project.

3. Create a node and attach the `RemSend.cs` script.