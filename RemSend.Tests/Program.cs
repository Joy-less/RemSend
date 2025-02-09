namespace RemSend.Tests;

public partial class Program {
    public static void Main() {
        new Program().DoStuff();
    }

    [Rem(RemAccess.Any)]
    public void DoStuff() {
        SendDoStuff();
    }
}