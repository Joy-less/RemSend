namespace RemSend.Tests;

public partial class Program {
    public static void Main() {
        new MyNode().DoStuff("4", []);
    }
}

public partial class MyNode : Godot.Node {
    /// <summary>
    /// Test.
    /// </summary>
    [Rem(RemAccess.Any, Channel: 1234, Mode = RemMode.UnreliableOrdered)]
    public void DoStuff(string Arg, System.Collections.Generic.List<int[]> Arg22) {
        
    }
}