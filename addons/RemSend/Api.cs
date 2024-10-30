#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Lq = System.Linq.Expressions;

namespace RemSend;

public static class Api {
    private const double DefaultTimeout = 30;

    /// <summary>
    /// Calls a remote method on all peers.
    /// </summary>
    public static void Rem(Lq.Expression<Action> CallExpression) {
        RemSend.Singleton.BroadcastRem((Lq.MethodCallExpression)CallExpression.Body);
    }
    /// <summary>
    /// Calls a remote method on the given peers.
    /// </summary>
    public static void Rem(IEnumerable<int> PeerIds, Lq.Expression<Action> CallExpression) {
        RemSend.Singleton.SendRem(PeerIds, (Lq.MethodCallExpression)CallExpression.Body);
    }
    /// <summary>
    /// Calls a remote method on the given peer.
    /// </summary>
    public static void Rem(int PeerId, Lq.Expression<Action> CallExpression) {
        Rem([PeerId], CallExpression);
    }

    /// <summary>
    /// Calls a remote method on all peers and awaits the result.
    /// </summary>
    public static async Task<T> Rem<T>(Lq.Expression<Func<T>> CallExpression, double Timeout = DefaultTimeout, CancellationToken CancelToken = default) {
        return await RemSend.Singleton.BroadcastRemAwaitResponse<T>((Lq.MethodCallExpression)CallExpression.Body, Timeout, CancelToken);
    }
    /// <summary>
    /// Calls a remote method on the given peers and awaits the result.
    /// </summary>
    public static async Task<T> Rem<T>(IEnumerable<int> PeerIds, Lq.Expression<Func<T>> CallExpression, double Timeout = DefaultTimeout, CancellationToken CancelToken = default) {
        return await RemSend.Singleton.SendRemAwaitResponse<T>(PeerIds, (Lq.MethodCallExpression)CallExpression.Body, Timeout, CancelToken);
    }
    /// <summary>
    /// Calls a remote method on the given peer and awaits the result.
    /// </summary>
    public static async Task<T> Rem<T>(int PeerId, Lq.Expression<Func<T>> CallExpression, double Timeout = DefaultTimeout, CancellationToken CancelToken = default) {
        return await Rem([PeerId], CallExpression, Timeout, CancelToken);
    }

    /// <summary>
    /// Calls a remote asynchronous method on all peers and awaits the result.
    /// </summary>
    public static async Task<T> Rem<T>(Lq.Expression<Func<Task<T>>> CallExpression, double Timeout = DefaultTimeout, CancellationToken CancelToken = default) {
        return await RemSend.Singleton.BroadcastRemAwaitResponse<T>((Lq.MethodCallExpression)CallExpression.Body, Timeout, CancelToken);
    }
    /// <summary>
    /// Calls a remote asynchronous method on the given peers and awaits the result.
    /// </summary>
    public static async Task<T> Rem<T>(IEnumerable<int> PeerIds, Lq.Expression<Func<Task<T>>> CallExpression, double Timeout = DefaultTimeout, CancellationToken CancelToken = default) {
        return await RemSend.Singleton.SendRemAwaitResponse<T>(PeerIds, (Lq.MethodCallExpression)CallExpression.Body, Timeout, CancelToken);
    }
    /// <summary>
    /// Calls a remote asynchronous method on the given peer and awaits the result.
    /// </summary>
    public static async Task<T> Rem<T>(int PeerId, Lq.Expression<Func<Task<T>>> CallExpression, double Timeout = DefaultTimeout, CancellationToken CancelToken = default) {
        return await Rem([PeerId], CallExpression, Timeout, CancelToken);
    }

    /// <summary>
    /// Calls a remote asynchronous method on all peers and awaits execution.
    /// </summary>
    public static async Task Rem(Lq.Expression<Func<Task>> CallExpression, double Timeout = DefaultTimeout, CancellationToken CancelToken = default) {
        await RemSend.Singleton.BroadcastRemAwaitResponse<byte>((Lq.MethodCallExpression)CallExpression.Body, Timeout, CancelToken);
    }
    /// <summary>
    /// Calls a remote asynchronous method on the given peers and awaits execution.
    /// </summary>
    public static async Task Rem(IEnumerable<int> PeerIds, Lq.Expression<Func<Task>> CallExpression, double Timeout = DefaultTimeout, CancellationToken CancelToken = default) {
        await RemSend.Singleton.SendRemAwaitResponse<byte>(PeerIds, (Lq.MethodCallExpression)CallExpression.Body, Timeout, CancelToken);
    }
    /// <summary>
    /// Calls a remote asynchronous method on the given peer and awaits execution.
    /// </summary>
    public static async Task Rem(int PeerId, Lq.Expression<Func<Task>> CallExpression, double Timeout = DefaultTimeout, CancellationToken CancelToken = default) {
        await Rem([PeerId], CallExpression, Timeout, CancelToken);
    }

    /// <summary>
    /// Calls a remote method on all peers, awaits the result and invokes a callback.
    /// </summary>
    public static async void Rem<T>(Lq.Expression<Func<T>> CallExpression, Action<T> Callback, double Timeout = DefaultTimeout, CancellationToken CancelToken = default) {
        Callback(await Rem(CallExpression, Timeout, CancelToken));
    }
    /// <summary>
    /// Calls a remote method on the given peers, awaits the result and invokes a callback.
    /// </summary>
    public static async void Rem<T>(IEnumerable<int> PeerIds, Lq.Expression<Func<T>> CallExpression, Action<T> Callback, double Timeout = DefaultTimeout, CancellationToken CancelToken = default) {
        Callback(await Rem(PeerIds, CallExpression, Timeout, CancelToken));
    }
    /// <summary>
    /// Calls a remote method on the given peer, awaits the result and invokes a callback.
    /// </summary>
    public static void Rem<T>(int PeerId, Lq.Expression<Func<T>> CallExpression, Action<T> Callback, double Timeout = DefaultTimeout, CancellationToken CancelToken = default) {
        Rem([PeerId], CallExpression, Callback, Timeout, CancelToken);
    }

    /// <summary>
    /// Calls a remote asynchronous method on all peers, awaits the result and invokes a callback.
    /// </summary>
    public static async void Rem<T>(Lq.Expression<Func<Task<T>>> CallExpression, Action<T> Callback, double Timeout = DefaultTimeout, CancellationToken CancelToken = default) {
        Callback(await Rem(CallExpression, Timeout, CancelToken));
    }
    /// <summary>
    /// Calls a remote asynchronous method on the given peers, awaits the result and invokes a callback.
    /// </summary>
    public static async void Rem<T>(IEnumerable<int> PeerIds, Lq.Expression<Func<Task<T>>> CallExpression, Action<T> Callback, double Timeout = DefaultTimeout, CancellationToken CancelToken = default) {
        Callback(await Rem(PeerIds, CallExpression, Timeout, CancelToken));
    }
    /// <summary>
    /// Calls a remote asynchronous method on the given peer, awaits the result and invokes a callback.
    /// </summary>
    public static void Rem<T>(int PeerId, Lq.Expression<Func<Task<T>>> CallExpression, Action<T> Callback, double Timeout = DefaultTimeout, CancellationToken CancelToken = default) {
        Rem([PeerId], CallExpression, Callback, Timeout, CancelToken);
    }

    /// <summary>
    /// Calls a remote asynchronous method on all peers, awaits execution and invokes a callback.
    /// </summary>
    public static async void Rem(Lq.Expression<Func<Task>> CallExpression, Action Callback, double Timeout = DefaultTimeout, CancellationToken CancelToken = default) {
        await Rem(CallExpression, Timeout, CancelToken);
        Callback();
    }
    /// <summary>
    /// Calls a remote asynchronous method on the given peers, awaits execution and invokes a callback.
    /// </summary>
    public static async void Rem(IEnumerable<int> PeerIds, Lq.Expression<Func<Task>> CallExpression, Action Callback, double Timeout = DefaultTimeout, CancellationToken CancelToken = default) {
        await Rem(PeerIds, CallExpression, Timeout, CancelToken);
        Callback();
    }
    /// <summary>
    /// Calls a remote asynchronous method on the given peer, awaits execution and invokes a callback.
    /// </summary>
    public static void Rem(int PeerId, Lq.Expression<Func<Task>> CallExpression, Action Callback, double Timeout = DefaultTimeout, CancellationToken CancelToken = default) {
        Rem([PeerId], CallExpression, Callback, Timeout, CancelToken);
    }
}