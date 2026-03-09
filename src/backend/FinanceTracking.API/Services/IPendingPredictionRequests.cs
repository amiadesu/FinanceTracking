using FinanceTracking.API.DTOs;

namespace FinanceTracking.API.Services;

public interface IPendingPredictionRequests
{
    (string CorrelationId, Task<PredictionResponse> Task) Register(
        TimeSpan timeout,
        CancellationToken cancellationToken = default);
    bool TryComplete(string correlationId, PredictionResponse response);
}