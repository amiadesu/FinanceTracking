using FinanceTracking.API.Constants;
using FinanceTracking.API.DTOs;
using FinanceTracking.API.Services;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace FinanceTracking.API.Handlers;

public class PredictionResponseHandler
{
    private readonly IPendingPredictionRequests _pending;
    private readonly ILogger<PredictionResponseHandler> _logger;

    public PredictionResponseHandler(
        IPendingPredictionRequests pending,
        ILogger<PredictionResponseHandler> logger)
    {
        _pending = pending;
        _logger = logger;
    }

    public void Handle(PredictionResponse response, Envelope envelope)
    {
        if (!envelope.Headers.TryGetValue(MLMessagingConstants.CorrelationIdHeader, out var correlationId)
            || string.IsNullOrEmpty(correlationId))
        {
            _logger.LogWarning(
                "Received PredictionResponse with no '{Header}' header — cannot route reply to a waiting caller.",
                MLMessagingConstants.CorrelationIdHeader);
            return;
        }

        if (!_pending.TryComplete(correlationId, response))
        {
            _logger.LogWarning(
                "Received PredictionResponse for CorrelationId {CorrelationId} but no pending request was found " +
                "(likely already timed out or cancelled).",
                correlationId);
        }
    }
}