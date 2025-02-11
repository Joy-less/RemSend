using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RemSend.Tests;

public partial class Program {
    public static void Main() {
        new TestTestWrap.MyNode().DoStuff("4", 0, []);
    }
}

public partial class TestTestWrap {
    public partial class MyNode : Godot.Node {
        /// <summary>
        /// Test.
        /// </summary>
        [Rem(RemAccess.Any, Channel: 1234, Mode = RemMode.UnreliableOrdered)]
        public void DoStuff(string? Arg, [Sender] int SenderId, [NotNullWhen(true)] params List<int[]> Arg22) {

        }
    }
}