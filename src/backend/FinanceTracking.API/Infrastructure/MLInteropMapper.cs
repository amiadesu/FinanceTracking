using RabbitMQ.Client;
using Wolverine;
using Wolverine.RabbitMQ;
using System;
using Wolverine.RabbitMQ.Internal;
using FinanceTracking.API.Constants;

namespace FinanceTracking.API.Utils;

public class MLInteropMapper : IRabbitMqEnvelopeMapper
{
    private readonly string _replyQueueName;

    public MLInteropMapper(string replyQueueName)
    {
        _replyQueueName = replyQueueName;
    }

    public void MapEnvelopeToOutgoing(Envelope envelope, IBasicProperties outgoing)
    {
        // DeliveryOptions has no CorrelationId property, so the correlation ID
        // is carried as a named header and promoted to the AMQP correlationId
        // field here so that the Python consumer echoes it back unchanged.
        if (envelope.Headers.TryGetValue(MLMessagingConstants.CorrelationIdHeader, out var correlationId))
            outgoing.CorrelationId = correlationId;

        outgoing.MessageId = envelope.Id.ToString();
        outgoing.ReplyTo = _replyQueueName;
        outgoing.ContentType = "application/json";
    }

    public void MapIncomingToEnvelope(Envelope envelope, IReadOnlyBasicProperties incoming)
    {
        // Promote the AMQP correlationId back into the same header so that
        // PredictionResponseHandler can retrieve it without knowing about AMQP.
        if (!string.IsNullOrEmpty(incoming.CorrelationId))
            envelope.Headers[MLMessagingConstants.CorrelationIdHeader] = incoming.CorrelationId;

        envelope.MessageType = typeof(DTOs.PredictionResponse).FullName;
        envelope.ContentType = "application/json";
    }
}