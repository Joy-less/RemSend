using Microsoft.CodeAnalysis;

namespace RemSend.SourceGeneratorHelpers;

public static class AttributeExtensions {
    public static string StringifyAttributes(this IEnumerable<AttributeData> Attributes) {
        if (!Attributes.Any()) {
            return "";
        }
        return $"[{string.Join(", ", Attributes)}] ";
    }
}