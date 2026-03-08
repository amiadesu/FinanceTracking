import joblib
import json
from sklearn.pipeline import Pipeline
from pathlib import Path
from app.core.config import settings
from app.utils.preprocessing import normalize_ukrainian_text

PREDICTION_CONFIDENCE_THRESHOLD = 0.75 # Threshold pick from train_model.ipynb

class ModelPredictor:
    def __init__(self):
        self.model_path = Path(settings.MODEL_PATH)
        self.categories_path = Path(settings.CATEGORIES_PATH)
        
        self.model: Pipeline = None
        self.categories = []

    def load(self):
        if not self.model_path.exists():
            raise FileNotFoundError(f"Model missing at {self.model_path}.")
        
        self.model = joblib.load(self.model_path)
        
        if self.categories_path.exists():
            with open(self.categories_path, "r", encoding="utf-8") as f:
                self.categories = json.load(f)

    def predict(self, texts: list[str]) -> list[str | None]:
        if self.model is None:
            self.load()

        if not texts:
            return []
        
        cleaned_texts = [normalize_ukrainian_text(text) for text in texts]

        probabilities = self.model.predict_proba(cleaned_texts)

        results = []
        for probs in probabilities:
            confidence = probs.max()
            if confidence >= PREDICTION_CONFIDENCE_THRESHOLD:
                class_index = probs.argmax()
                results.append(self.model.classes_[class_index])
            else:
                results.append(None)  # below threshold — no prediction

        return results

    def get_categories(self) -> list[str]:
        if not self.categories:
            self.load()
        return self.categories

predictor = ModelPredictor()