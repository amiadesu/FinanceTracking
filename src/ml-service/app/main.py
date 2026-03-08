import asyncio
import logging
from contextlib import asynccontextmanager
from fastapi import FastAPI
from app.api import routes
from app.models.predictor import predictor
from app.rabbitmq.consumer import consume

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s - %(name)s - %(levelname)s - %(message)s"
)
logger = logging.getLogger(__name__)

@asynccontextmanager
async def lifespan(app: FastAPI):
    logger.info("Initializing ML Service...")
    
    try:
        predictor.load()
        logger.info("Model and categories successfully loaded into memory.")
    except Exception as e:
        logger.error(f"Failed to load model artifacts: {e}")
        raise e
    
    rabbitmq_task = asyncio.create_task(consume())
    
    yield
    
    logger.info("Shutting down ML Service...")
    rabbitmq_task.cancel()
    try:
        await rabbitmq_task
    except asyncio.CancelledError:
        logger.info("RabbitMQ consumer task cancelled successfully.")

app = FastAPI(
    title="FinanceTracker ML Service",
    lifespan=lifespan
)

app.include_router(routes.router)