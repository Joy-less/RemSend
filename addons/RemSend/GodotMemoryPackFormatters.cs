#nullable enable

using Godot;
using MemoryPack;

namespace RemSend;

internal static class GodotMemoryPackFormatters {
    public static void RegisterTypes() {
        // Register types for non-generic serialisation
        MemoryPackFormatterProvider.Register(new Vector2Formatter());
        MemoryPackFormatterProvider.Register(new Vector3Formatter());
        MemoryPackFormatterProvider.Register(new Vector4Formatter());
        MemoryPackFormatterProvider.Register(new Vector2IFormatter());
        MemoryPackFormatterProvider.Register(new Vector3IFormatter());
        MemoryPackFormatterProvider.Register(new Vector4IFormatter());
        MemoryPackFormatterProvider.Register(new StringNameFormatter());
        MemoryPackFormatterProvider.Register(new NodePathFormatter());
    }
}

internal class Vector2Formatter : MemoryPackFormatter<Vector2> {
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> Writer, scoped ref Vector2 Value) {
        Writer.WriteValue(Value.X);
        Writer.WriteValue(Value.Y);
    }
    public override void Deserialize(ref MemoryPackReader Reader, scoped ref Vector2 Value) {
        Value = new Vector2(
            Reader.ReadValue<float>(),
            Reader.ReadValue<float>()
        );
    }
}
internal class Vector3Formatter : MemoryPackFormatter<Vector3> {
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> Writer, scoped ref Vector3 Value) {
        Writer.WriteValue(Value.X);
        Writer.WriteValue(Value.Y);
        Writer.WriteValue(Value.Z);
    }
    public override void Deserialize(ref MemoryPackReader Reader, scoped ref Vector3 Value) {
        Value = new Vector3(
            Reader.ReadValue<float>(),
            Reader.ReadValue<float>(),
            Reader.ReadValue<float>()
        );
    }
}
internal class Vector4Formatter : MemoryPackFormatter<Vector4> {
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> Writer, scoped ref Vector4 Value) {
        Writer.WriteValue(Value.X);
        Writer.WriteValue(Value.Y);
        Writer.WriteValue(Value.Z);
        Writer.WriteValue(Value.W);
    }
    public override void Deserialize(ref MemoryPackReader Reader, scoped ref Vector4 Value) {
        Value = new Vector4(
            Reader.ReadValue<float>(),
            Reader.ReadValue<float>(),
            Reader.ReadValue<float>(),
            Reader.ReadValue<float>()
        );
    }
}
internal class Vector2IFormatter : MemoryPackFormatter<Vector2I> {
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> Writer, scoped ref Vector2I Value) {
        Writer.WriteValue(Value.X);
        Writer.WriteValue(Value.Y);
    }
    public override void Deserialize(ref MemoryPackReader Reader, scoped ref Vector2I Value) {
        Value = new Vector2I(
            Reader.ReadValue<int>(),
            Reader.ReadValue<int>()
        );
    }
}
internal class Vector3IFormatter : MemoryPackFormatter<Vector3I> {
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> Writer, scoped ref Vector3I Value) {
        Writer.WriteValue(Value.X);
        Writer.WriteValue(Value.Y);
        Writer.WriteValue(Value.Z);
    }
    public override void Deserialize(ref MemoryPackReader Reader, scoped ref Vector3I Value) {
        Value = new Vector3I(
            Reader.ReadValue<int>(),
            Reader.ReadValue<int>(),
            Reader.ReadValue<int>()
        );
    }
}
internal class Vector4IFormatter : MemoryPackFormatter<Vector4I> {
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> Writer, scoped ref Vector4I Value) {
        Writer.WriteValue(Value.X);
        Writer.WriteValue(Value.Y);
        Writer.WriteValue(Value.Z);
        Writer.WriteValue(Value.W);
    }
    public override void Deserialize(ref MemoryPackReader Reader, scoped ref Vector4I Value) {
        Value = new Vector4I(
            Reader.ReadValue<int>(),
            Reader.ReadValue<int>(),
            Reader.ReadValue<int>(),
            Reader.ReadValue<int>()
        );
    }
}
internal class StringNameFormatter : MemoryPackFormatter<StringName> {
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> Writer, scoped ref StringName? Value) {
        Writer.WriteString(Value);
    }
    public override void Deserialize(ref MemoryPackReader Reader, scoped ref StringName? Value) {
        Value = Reader.ReadString()!;
    }
}
internal class NodePathFormatter : MemoryPackFormatter<NodePath> {
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> Writer, scoped ref NodePath? Value) {
        Writer.WriteString(Value);
    }
    public override void Deserialize(ref MemoryPackReader Reader, scoped ref NodePath? Value) {
        Value = Reader.ReadString()!;
    }
}