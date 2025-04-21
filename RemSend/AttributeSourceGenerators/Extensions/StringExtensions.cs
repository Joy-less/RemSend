#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Microsoft.CodeAnalysis.CSharp;

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
    public static string SanitizeFileName(this string FileName, char ReplacementChar = '_') {
        // Symbols not allowed in generated source hint names
        FileName = FileName.Replace('@', ReplacementChar);
        FileName = FileName.Replace('<', ReplacementChar);
        FileName = FileName.Replace('>', ReplacementChar);
        // Symbols not allowed in any file name
        return Path.GetInvalidFileNameChars().Aggregate(FileName, (String, InvalidChar) => String.Replace(InvalidChar, ReplacementChar));
    }
    public static string SanitizeIdentifier(this string Identifier, char ReplacementChar = '_') {
        for (int Index = 0; Index < Identifier.Length; Index++) {
            bool IsCharValid = Index is 0
                ? SyntaxFacts.IsIdentifierStartCharacter(Identifier[Index])
                : SyntaxFacts.IsIdentifierPartCharacter(Identifier[Index]);

            if (!IsCharValid) {
                Identifier = Identifier[..Index] + ReplacementChar + Identifier[(Index + 1)..];
            }
        }
        return Identifier;
    }
}