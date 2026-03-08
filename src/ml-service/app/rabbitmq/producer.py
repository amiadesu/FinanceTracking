import aio_pika
import json
import logging
from app.core.config import settings

logger = logging.getLogger(__name__)

_connection: aio_pika.RobustConnection | None = None

async def get_channel():
    global _connection
    if _connection is None or _connection.is_closed:
        _connection = await aio_pika.connect_robust(settings.RABBITMQ_URL)
    return await _connection.channel()

async def publish_results(results: list[dict], correlation_id: str = None, reply_queue: str = None):
    """
    Publishes the prediction results back to the main API server.
    """
    try:
        channel = await get_channel()
            
        payload = {"results": results}
        if correlation_id:
            payload["correlationId"] = correlation_id
            
        message_body = json.dumps(payload, ensure_ascii=False).encode()
        
        target_queue = reply_queue or settings.REPLY_QUEUE_NAME
        
        message = aio_pika.Message(
            body=message_body,
            content_type="application/json",
            correlation_id=correlation_id
        )
        
        await channel.default_exchange.publish(
            message,
            routing_key=target_queue,
        )
        logger.info(f"Published {len(results)} predictions to {target_queue}")
            
    except Exception as e:
        logger.error(f"Failed to publish results to RabbitMQ: {e}")