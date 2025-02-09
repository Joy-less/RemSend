namespace RemSend.Tests;

public partial class Program {
    public static void Main() {
        new Program().DoStuff();
    }

    [Rem(RemAccess.Any, Channel: 123, Mode = RemMode.UnreliableOrdered)]
    public void DoStuff() {
        SendDoStuff();
    }
}