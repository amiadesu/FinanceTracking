import os
from pydantic_settings import BaseSettings

class Settings(BaseSettings):
    PROJECT_NAME: str = "ML-Microservice"
    
    RABBITMQ_URL: str = os.getenv("RABBITMQ_URL")
    QUEUE_NAME: str = os.getenv("ML_QUEUE_NAME")
    REPLY_QUEUE_NAME: str = os.getenv("ML_REPLY_QUEUE_NAME")
    
    MODEL_PATH: str = "app/models/exports/model.joblib"
    CATEGORIES_PATH: str = "app/models/exports/categories.json"

settings = Settings()