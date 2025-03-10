﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace RemSend.Tests;

public partial class Program {
    public static void Main() {
    }
}

public partial class MyNode : Godot.Node {
    [Rem(RemAccess.Any)]
    public ushort GetMagicNumber(bool Dummy) {
        return 7;
    }

    [Rem(RemAccess.Any)]
    public async Task<ushort> GetMagicNumberAsync(bool Dummy) {
        await Task.Delay(0);
        return 7;
    }

    [Rem]
    public async Task WaitSomeTime(bool Dummy, [Sender] int Id) {
        await Task.Delay(10);
    }

    [Rem(RemAccess.Any, Channel: 1234, Mode = RemMode.UnreliableOrdered)]
    private void SillyExample(string? Arg, [Sender] int SenderId, [NotNullWhen(true)] params List<int[]> Arg22) {

    }
}