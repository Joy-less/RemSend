#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace AttributeSourceGenerators;

public record DiagnosticDetail(string Title, string Message, string? Id = null, string? Category = null);