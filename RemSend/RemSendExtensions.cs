namespace RemSend;

/// <summary>
/// A set of extension methods for use with RemSend.
/// </summary>
public static class RemSendExtensions {
    /// <summary>
    /// Waits for the task to complete and calls <paramref name="Callback"/> on an arbitrary thread.
    /// </summary>
    public static Task ContinueWith(this Task Task, Action Callback) {
        return Task.ContinueWith(Task => Callback());
    }
    /// <summary>
    /// Waits for the task to complete and calls <paramref name="Callback"/> on the original thread.
    /// </summary>
    public static Task ContinueWithOnSameThread(this Task Task, Action Callback) {
        return Task.ContinueWith(Task => Callback(), TaskScheduler.FromCurrentSynchronizationContext());
    }
    /// <inheritdoc cref="ContinueWith(Task, Action)"/>
    public static Task ContinueWith<T>(this Task<T> Task, Action<T> Callback) {
        return Task.ContinueWith(Task => Callback(Task.Result));
    }
    /// <inheritdoc cref="ContinueWithOnSameThread(Task, Action)"/>
    public static Task ContinueWithOnSameThread<T>(this Task<T> Task, Action<T> Callback) {
        return Task.ContinueWith(Task => Callback(Task.Result), TaskScheduler.FromCurrentSynchronizationContext());
    }
}