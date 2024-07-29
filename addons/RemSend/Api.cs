#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Lq = System.Linq.Expressions;

namespace RemSend;

public static class Api {
    /// <summary>
    /// Call a remote procedure on every peer.
    /// </summary>
    public static void Rem(Lq.Expression<Action> CallExpression) {
        RemSend.Singleton.Rem((Lq.MethodCallExpression)CallExpression.Body);
    }
    /// <summary>
    /// Call a remote procedure on the given peers.
    /// </summary>
    public static void Rem(IEnumerable<int> PeerIds, Lq.Expression<Action> CallExpression) {
        RemSend.Singleton.Rem(PeerIds, (Lq.MethodCallExpression)CallExpression.Body);
    }
    /// <summary>
    /// Call a remote procedure on the given peer.
    /// </summary>
    public static void Rem(int PeerId, Lq.Expression<Action> CallExpression) {
        Rem([PeerId], CallExpression);
    }

    /// <summary>
    /// Call a remote procedure on every peer and await the result.
    /// </summary>
    public static async Task<T> RemWait<T>(Lq.Expression<Func<T>> CallExpression, CancellationToken CancelToken = default) {
        return await RemSend.Singleton.RemWait<T>((Lq.MethodCallExpression)CallExpression.Body, CancelToken);
    }
    /// <summary>
    /// Call a remote procedure on the given peers and await the result.
    /// </summary>
    public static async Task<T> RemWait<T>(IEnumerable<int> PeerIds, Lq.Expression<Func<T>> CallExpression, CancellationToken CancelToken = default) {
        return await RemSend.Singleton.RemWait<T>(PeerIds, (Lq.MethodCallExpression)CallExpression.Body, CancelToken);
    }
    /// <summary>
    /// Call a remote procedure on the given peer and await the result.
    /// </summary>
    public static async Task<T> RemWait<T>(int PeerId, Lq.Expression<Func<T>> CallExpression, CancellationToken CancelToken = default) {
        return await RemWait([PeerId], CallExpression, CancelToken);
    }

    /// <summary>
    /// Call a remote procedure on every peer and await the result.
    /// </summary>
    public static async Task<T> RemWait<T>(Lq.Expression<Func<Task<T>>> CallExpression, CancellationToken CancelToken = default) {
        return await RemSend.Singleton.RemWait<T>((Lq.MethodCallExpression)CallExpression.Body, CancelToken);
    }
    /// <summary>
    /// Call a remote procedure on the given peers and await the result.
    /// </summary>
    public static async Task<T> RemWait<T>(IEnumerable<int> PeerIds, Lq.Expression<Func<Task<T>>> CallExpression, CancellationToken CancelToken = default) {
        return await RemSend.Singleton.RemWait<T>(PeerIds, (Lq.MethodCallExpression)CallExpression.Body, CancelToken);
    }
    /// <summary>
    /// Call a remote procedure on the given peer and await the result.
    /// </summary>
    public static async Task<T> RemWait<T>(int PeerId, Lq.Expression<Func<Task<T>>> CallExpression, CancellationToken CancelToken = default) {
        return await RemWait([PeerId], CallExpression, CancelToken);
    }
}