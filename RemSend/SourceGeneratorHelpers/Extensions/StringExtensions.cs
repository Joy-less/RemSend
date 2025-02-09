namespace RemSend.SourceGeneratorHelpers;

public static class StringExtensions {
    private static readonly string[] LineSeparators = ["\r\n", "\r", "\n"];

    public static string[] SplitLines(this string String, StringSplitOptions Options = StringSplitOptions.None) {
        return String.Split(LineSeparators, Options);
    }
}