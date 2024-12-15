#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MemoryPack;

using Lq = System.Linq.Expressions;

namespace RemSend;

internal static class Extensions {
    /// <summary>
    /// Gets the constant value of the expression or compiles it to a delegate and invokes it.
    /// </summary>
    public static object? Evaluate(this Lq.Expression? Expression) {
        if (Expression is null) {
            return null;
        }
        else if (Expression is Lq.ConstantExpression ConstantExpression) {
            return ConstantExpression.Value;
        }
        else {
            return Lq.Expression.Lambda(Expression).Compile().DynamicInvoke();
        }
    }
    /// <summary>
    /// Gets the constant value of the expressions or compiles them to delegates and invokes them.
    /// </summary>
    public static object?[] Evaluate(this IEnumerable<Lq.Expression?> Expressions) {
        return Expressions.Select(Evaluate).ToArray();
    }
    /// <summary>
    /// Serialises the arguments based on the parameter types.
    /// </summary>
    public static byte[][] PackArguments(this IList<object?> Arguments, IList<ParameterInfo> Parameters) {
        byte[][] PackedArguments = new byte[Arguments.Count][];
        for (int Index = 0; Index < Arguments.Count; Index++) {
            try {
                PackedArguments[Index] = MemoryPackSerializer.Serialize(Parameters[Index].ParameterType, Arguments[Index]);
            }
            catch (MemoryPackSerializationException) {
                throw new Exception($"Failed to serialise argument '{Parameters[Index].Name}' (register '{Parameters[Index].ParameterType}' with MemoryPack).");
            }
        }
        return PackedArguments;
    }
    /// <summary>
    /// Deserialises the arguments based on the parameter types.
    /// </summary>
    public static object?[] UnpackArguments(this IList<byte[]> PackedArguments, IList<ParameterInfo> Parameters) {
        object?[] Arguments = new object[PackedArguments.Count];
        for (int Index = 0; Index < PackedArguments.Count; Index++) {
            Arguments[Index] = MemoryPackSerializer.Deserialize(Parameters[Index].ParameterType, PackedArguments[Index]);
        }
        return Arguments;
    }
}