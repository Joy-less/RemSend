#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Lq = System.Linq.Expressions;

namespace RemSend;

public static class Api {
    /// <summary>
    /// Calls a remote procedure on all peers.
    /// </summary>
    public static void Rem(Lq.Expression<Action> CallExpression) {
        RemSend.Singleton.Rem((Lq.MethodCallExpression)CallExpression.Body);
    }
    /// <summary>
    /// Calls a remote procedure on the given peers.
    /// </summary>
    public static void Rem(IEnumerable<int> PeerIds, Lq.Expression<Action> CallExpression) {
        RemSend.Singleton.Rem(PeerIds, (Lq.MethodCallExpression)CallExpression.Body);
    }
    /// <summary>
    /// Calls a remote procedure on the given peer.
    /// </summary>
    public static void Rem(int PeerId, Lq.Expression<Action> CallExpression) {
        Rem([PeerId], CallExpression);
    }

    /// <summary>
    /// Calls a remote procedure on all peers and awaits the result.
    /// </summary>
    public static async Task<T> Rem<T>(Lq.Expression<Func<T>> CallExpression, CancellationToken CancelToken = default) {
        return await RemSend.Singleton.Rem<T>((Lq.MethodCallExpression)CallExpression.Body, CancelToken);
    }
    /// <summary>
    /// Calls a remote procedure on the given peers and awaits the result.
    /// </summary>
    public static async Task<T> Rem<T>(IEnumerable<int> PeerIds, Lq.Expression<Func<T>> CallExpression, CancellationToken CancelToken = default) {
        return await RemSend.Singleton.Rem<T>(PeerIds, (Lq.MethodCallExpression)CallExpression.Body, CancelToken);
    }
    /// <summary>
    /// Calls a remote procedure on the given peer and awaits the result.
    /// </summary>
    public static async Task<T> Rem<T>(int PeerId, Lq.Expression<Func<T>> CallExpression, CancellationToken CancelToken = default) {
        return await Rem([PeerId], CallExpression, CancelToken);
    }

    /// <summary>
    /// Calls a remote asynchronous procedure on all peers and awaits the result.
    /// </summary>
    public static async Task<T> Rem<T>(Lq.Expression<Func<Task<T>>> CallExpression, CancellationToken CancelToken = default) {
        return await RemSend.Singleton.Rem<T>((Lq.MethodCallExpression)CallExpression.Body, CancelToken);
    }
    /// <summary>
    /// Calls a remote asynchronous procedure on the given peers and awaits the result.
    /// </summary>
    public static async Task<T> Rem<T>(IEnumerable<int> PeerIds, Lq.Expression<Func<Task<T>>> CallExpression, CancellationToken CancelToken = default) {
        return await RemSend.Singleton.Rem<T>(PeerIds, (Lq.MethodCallExpression)CallExpression.Body, CancelToken);
    }
    /// <summary>
    /// Calls a remote asynchronous procedure on the given peer and awaits the result.
    /// </summary>
    public static async Task<T> Rem<T>(int PeerId, Lq.Expression<Func<Task<T>>> CallExpression, CancellationToken CancelToken = default) {
        return await Rem([PeerId], CallExpression, CancelToken);
    }

    /// <summary>
    /// Calls a remote procedure on all peers, awaits the result and invokes a callback.
    /// </summary>
    public static async void Rem<T>(Lq.Expression<Func<T>> CallExpression, Action<T> Callback, CancellationToken CancelToken = default) {
        Callback(await Rem(CallExpression, CancelToken));
    }
    /// <summary>
    /// Calls a remote procedure on the given peers, awaits the result and invokes a callback.
    /// </summary>
    public static async void Rem<T>(IEnumerable<int> PeerIds, Lq.Expression<Func<T>> CallExpression, Action<T> Callback, CancellationToken CancelToken = default) {
        Callback(await Rem(PeerIds, CallExpression, CancelToken));
    }
    /// <summary>
    /// Calls a remote procedure on the given peer, awaits the result and invokes a callback.
    /// </summary>
    public static void Rem<T>(int PeerId, Lq.Expression<Func<T>> CallExpression, Action<T> Callback, CancellationToken CancelToken = default) {
        Rem([PeerId], CallExpression, Callback, CancelToken);
    }

    /// <summary>
    /// Calls a remote asynchronous procedure on all peers, awaits the result and invokes a callback.
    /// </summary>
    public static async void Rem<T>(Lq.Expression<Func<Task<T>>> CallExpression, Action<T> Callback, CancellationToken CancelToken = default) {
        Callback(await Rem(CallExpression, CancelToken));
    }
    /// <summary>
    /// Calls a remote asynchronous procedure on the given peers, awaits the result and invokes a callback.
    /// </summary>
    public static async void Rem<T>(IEnumerable<int> PeerIds, Lq.Expression<Func<Task<T>>> CallExpression, Action<T> Callback, CancellationToken CancelToken = default) {
        Callback(await Rem(PeerIds, CallExpression, CancelToken));
    }
    /// <summary>
    /// Calls a remote asynchronous procedure on the given peer, awaits the result and invokes a callback.
    /// </summary>
    public static void Rem<T>(int PeerId, Lq.Expression<Func<Task<T>>> CallExpression, Action<T> Callback, CancellationToken CancelToken = default) {
        Rem([PeerId], CallExpression, Callback, CancelToken);
    }
}