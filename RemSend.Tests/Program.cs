﻿using MemoryPack;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RemSend.Tests;

public partial class Program {
    public static void Main() {
        /*Console.WriteLine("[" + string.Join(", ", MemoryPackSerializer.Serialize((Guid?)null)) + "]");
        Console.WriteLine("[" + string.Join(", ", MemoryPackSerializer.Serialize((Guid?)Guid.NewGuid())) + "]");
        Console.WriteLine("[" + string.Join(", ", MemoryPackSerializer.Serialize(Guid.Empty)) + "]");
        Console.WriteLine("[" + string.Join(", ", MemoryPackSerializer.Serialize(Guid.NewGuid())) + "]");
        //Console.WriteLine("[" + string.Join(", ", MemoryPackSerializer.Serialize(((Guid?)null, (Guid?)null))) + "]");
        //Console.WriteLine("[" + string.Join(", ", MemoryPackSerializer.Serialize(((Guid?)Guid.NewGuid(), (Guid?)Guid.NewGuid()))) + "]");*/
    }
}

public partial class MyNode : Godot.Node {
    /*[Rem(RemAccess.Any, Channel: 1234, Mode = RemMode.UnreliableOrdered)]
    public void DoStuff(string? Arg, [Sender] int SenderId, [NotNullWhen(true)] params List<int[]> Arg22) {
        
    }*/

    [Rem(RemAccess.Any)]
    public ushort GetMagicNumber(bool Dummy) {
        return 7;
    }
}