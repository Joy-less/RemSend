#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace AttributeSourceGenerators;

public static class StringExtensions {
    private static readonly string[] LineSeparators = ["\r\n", "\r", "\n", "\u2028", "\u2029"];

    public static string[] SplitLines(this string String, StringSplitOptions Options = StringSplitOptions.None) {
        return String.Split(LineSeparators, Options);
    }
    public static string TrimPrefix(this string String, string Prefix) {
        if (String.StartsWith(Prefix, StringComparison.Ordinal)) {
            return String[Prefix.Length..];
        }
        return String;
    }
    public static string TrimSuffix(this string String, string Suffix) {
        if (String.EndsWith(Suffix, StringComparison.Ordinal)) {
            return String[..^Suffix.Length];
        }
        return String;
    }
    public static string SanitizeFileName(this string FileName) {
        // Symbols not allowed in generated source hint names
        FileName = FileName.Replace('@', '_');
        // Symbols not allowed in any file name
        return Path.GetInvalidFileNameChars().Aggregate(FileName, (String, InvalidChar) => String.Replace(InvalidChar, '_'));
    }
}