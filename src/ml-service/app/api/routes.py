import logging
from fastapi import APIRouter, HTTPException
from pydantic import BaseModel
from app.models.predictor import predictor

logger = logging.getLogger(__name__)
logging.basicConfig(level=logging.INFO)

router = APIRouter()

class SingularRequest(BaseModel):
    text: str

class BatchRequest(BaseModel):
    texts: list[str]

@router.get("/categories")
async def get_categories():
    return {"categories": predictor.get_categories()}

@router.post("/predict")
async def predict_singular(request: SingularRequest):
    if not request.text.strip():
        logger.warning("API received an empty text prediction request.")
        raise HTTPException(status_code=400, detail="Text cannot be empty")
    
    logger.info(f"API received product for prediction: '{request.text}'")
    
    prediction = predictor.predict([request.text])[0]
    
    logger.info(f"API prediction result: '{prediction}'")
    return {"text": request.text, "category": prediction}

@router.post("/predict/batch")
async def predict_batch(request: BatchRequest):
    if not request.texts:
        logger.warning("API received a batch prediction request with an empty list.")
        raise HTTPException(status_code=400, detail="Texts list cannot be empty")
    
    logger.info(f"API received a batch of {len(request.texts)} products. First item: '{request.texts[0]}'")
    
    predictions = predictor.predict(request.texts)
    
    results = [{"text": t, "category": p} for t, p in zip(request.texts, predictions)]  
    
    logger.info("API batch prediction complete.")
    return {"results": results}