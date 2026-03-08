import aio_pika
import json
import logging
from app.models.predictor import predictor
from app.rabbitmq.producer import publish_results
from app.core.config import settings

logger = logging.getLogger(__name__)

async def process_message(message: aio_pika.IncomingMessage):
    async with message.process():
        try:
            body = json.loads(message.body.decode())
            texts = body.get("texts", [])
            
            correlation_id = message.correlation_id or body.get("correlationId")

            # Get Wolverine's temporary reply queue, fallback to static if not present
            reply_queue = message.reply_to or settings.REPLY_QUEUE_NAME
            
            if texts:
                logger.info(f"RabbitMQ consumer received {len(texts)} products for prediction.")
                
                predictions = predictor.predict(texts)
                
                results = [{"text": t, "category": p} for t, p in zip(texts, predictions)]
                
                await publish_results(results, correlation_id, reply_queue)
                
        except Exception as e:
            logger.error(f"Error processing message from RabbitMQ: {e}")

async def consume():
    try:
        connection = await aio_pika.connect_robust(settings.RABBITMQ_URL)
        
        channel = await connection.channel() 
        
        queue = await channel.declare_queue(settings.QUEUE_NAME, durable=True)
        
        await queue.consume(process_message)
        logger.info(f"[*] Waiting for messages in {settings.QUEUE_NAME}")
    except Exception as e:
        logger.error(f"Failed to connect to RabbitMQ: {e}")