using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RemSend.Tests;

public partial class Program {
    public static void Main() {
    }
}

public partial class MyNode : Godot.Node {
    /*[Rem(RemAccess.Any, Channel: 1234, Mode = RemMode.UnreliableOrdered)]
    public void DoStuff(string? Arg, [Sender] int SenderId, [NotNullWhen(true)] params List<int[]> Arg22) {
        
    }*/

    [Rem(RemAccess.Any)]
    public int GetMagicNumber() {
        return 7;
    }
}