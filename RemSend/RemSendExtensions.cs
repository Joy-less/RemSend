namespace RemSend;

/// <summary>
/// A set of extension methods for use with RemSend.
/// </summary>
public static class RemSendExtensions {
    /// <inheritdoc cref="Task.ContinueWith(Action{Task})"/>
    public static Task ContinueWith(this Task Task, Action Callback) {
        return Task.ContinueWith(Task => Callback());
    }
    /// <inheritdoc cref="Task{T}.ContinueWith(Action{Task{T}})"/>
    public static Task ContinueWith<T>(this Task<T> Task, Action<T> Callback) {
        return Task.ContinueWith(Task => Callback(Task.Result));
    }
}