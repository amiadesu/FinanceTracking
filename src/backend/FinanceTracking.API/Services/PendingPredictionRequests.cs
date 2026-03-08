using System.Collections.Concurrent;
using FinanceTracking.API.DTOs;

namespace FinanceTracking.API.Services;

public interface IPendingPredictionRequests
{
    /// <summary>
    /// Registers a new pending prediction request and returns the correlation ID
    /// to stamp on the outgoing RabbitMQ message, plus a Task that completes
    /// when the ML service reply arrives (or faults on timeout/cancellation).
    /// </summary>
    (string CorrelationId, Task<PredictionResponse> Task) Register(
        TimeSpan timeout,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Called by <see cref="Handlers.PredictionResponseHandler"/> when a reply
    /// arrives on the reply queue. Returns false if the entry was already
    /// removed (timed out or cancelled).
    /// </summary>
    bool TryComplete(string correlationId, PredictionResponse response);
}

public class PendingPredictionRequests : IPendingPredictionRequests
{
    private readonly ConcurrentDictionary<string, TaskCompletionSource<PredictionResponse>> _pending = new();

    public (string CorrelationId, Task<PredictionResponse> Task) Register(
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        var tcs = new TaskCompletionSource<PredictionResponse>(TaskCreationOptions.RunContinuationsAsynchronously);

        _pending[correlationId] = tcs;

        // Link the caller's cancellation token with a timeout so that the
        // pending entry is always removed — preventing memory leaks regardless
        // of whether the reply ever arrives.
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedCts.Token.Register(() =>
        {
            if (_pending.TryRemove(correlationId, out var pending))
                pending.TrySetCanceled();

            linkedCts.Dispose();
        });
        linkedCts.CancelAfter(timeout);

        return (correlationId, tcs.Task);
    }

    public bool TryComplete(string correlationId, PredictionResponse response)
    {
        if (_pending.TryRemove(correlationId, out var tcs))
        {
            tcs.TrySetResult(response);
            return true;
        }
        return false;
    }
}