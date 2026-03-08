import joblib
import json
from pathlib import Path
from app.core.config import settings
from app.utils.preprocessing import normalize_ukrainian_text

class ModelPredictor:
    def __init__(self):
        self.model_path = Path(settings.MODEL_PATH)
        self.categories_path = Path(settings.CATEGORIES_PATH)
        
        self.model = None
        self.categories = []

    def load(self):
        if not self.model_path.exists():
            raise FileNotFoundError(f"Model missing at {self.model_path}.")
        
        self.model = joblib.load(self.model_path)
        
        if self.categories_path.exists():
            with open(self.categories_path, "r", encoding="utf-8") as f:
                self.categories = json.load(f)

    def predict(self, texts: list[str]) -> list[str]:
        if self.model is None:
            self.load()
            
        cleaned_texts = [normalize_ukrainian_text(text) for text in texts]
        
        return self.model.predict(cleaned_texts).tolist()

    def get_categories(self) -> list[str]:
        if not self.categories:
            self.load()
        return self.categories

predictor = ModelPredictor()