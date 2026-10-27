from __future__ import annotations

import io
import pickle
from pathlib import Path
from typing import Optional

import numpy as np
import pandas as pd
from fastapi import FastAPI, File, Form, HTTPException, UploadFile
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field
from PIL import Image
from sklearn.cluster import KMeans

BASE_DIR = Path(__file__).resolve().parent
with (BASE_DIR / "interior_model.pkl").open("rb") as f:
    model = pickle.load(f)
with (BASE_DIR / "encoders.pkl").open("rb") as f:
    encoders = pickle.load(f)

FEATURES = [
    "dominant_color_1", "dominant_color_2", "dominant_color_3",
    "style", "room_type", "space", "lighting",
]

app = FastAPI(
    title="CoZee Furniture Colour Recommendation API",
    description="Predictions from the trained CoZee Random Forest model.",
    version="1.0.0",
)
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # Restrict before production deployment.
    allow_credentials=False,
    allow_methods=["*"],
    allow_headers=["*"],
)


class RecommendationRequest(BaseModel):
    dominant_color_1: str = Field(..., description="First dominant room colour")
    dominant_color_2: str = Field(..., description="Second dominant room colour")
    dominant_color_3: str = Field(..., description="Third dominant room colour")
    style: str = Field(..., description="Interior style")
    room_type: str = Field(..., description="Bedroom, dining, living, or office")
    space: str = Field(..., description="Small, medium, or large")
    lighting: str = Field(..., description="Bright, dark, dim, or warm")


def _normalise(value: str) -> str:
    return value.strip().lower().replace("-", "_")


def _validate_category(field: str, value: str) -> str:
    value = _normalise(value)
    encoder = encoders.get(field)
    if encoder is None or value not in set(encoder.classes_):
        allowed = list(encoder.classes_) if encoder is not None else []
        raise HTTPException(
            status_code=422,
            detail=f"Unsupported value for '{field}': '{value}'. Allowed values: {allowed}",
        )
    return value


def _predict(values: dict[str, str]) -> dict:
    encoded = {}
    for feature in FEATURES:
        value = _validate_category(feature, values[feature])
        encoded[feature] = int(encoders[feature].transform([value])[0])

    row = pd.DataFrame([[encoded[f] for f in FEATURES]], columns=FEATURES)
    prediction_code = int(model.predict(row)[0])
    predicted_colour = str(
        encoders["recommended_furniture_color"].inverse_transform([prediction_code])[0]
    )

    probabilities = {}
    if hasattr(model, "predict_proba"):
        for class_code, probability in zip(model.classes_, model.predict_proba(row)[0]):
            colour = str(
                encoders["recommended_furniture_color"].inverse_transform([int(class_code)])[0]
            )
            probabilities[colour] = round(float(probability), 4)

    return {
        "recommended_furniture_color": predicted_colour,
        "confidence": probabilities.get(predicted_colour),
        "probabilities": probabilities,
        "input": values,
        "model": "CoZee Random Forest",
    }


@app.get("/")
def root():
    return {"message": "CoZee colour recommendation API", "docs": "/docs", "health": "/health"}


@app.get("/health")
def health():
    return {"status": "ok", "model_loaded": model is not None, "features": FEATURES}


@app.post("/recommend")
def recommend(request: RecommendationRequest):
    values = request.model_dump() if hasattr(request, "model_dump") else request.dict()
    return _predict(values)


def _rgb_to_category(rgb: tuple[float, float, float]) -> str:
    # The model's dataset uses these seven coarse room-colour categories.
    prototypes = {
        "beige": (210, 190, 150),
        "black": (25, 25, 25),
        "blue": (65, 100, 175),
        "brown": (120, 78, 48),
        "cream": (242, 230, 200),
        "grey": (128, 128, 128),
        "white": (242, 242, 242),
    }
    rgb_arr = np.array(rgb, dtype=float)
    return min(
        prototypes,
        key=lambda name: float(np.linalg.norm(rgb_arr - np.array(prototypes[name], dtype=float))),
    )


def _image_features(image_bytes: bytes) -> tuple[list[str], str]:
    try:
        image = Image.open(io.BytesIO(image_bytes)).convert("RGB")
    except Exception as exc:
        raise HTTPException(status_code=400, detail="Could not read the uploaded image.") from exc

    image.thumbnail((256, 256))
    pixels = np.asarray(image, dtype=np.uint8).reshape(-1, 3)
    if len(pixels) < 3:
        raise HTTPException(status_code=400, detail="Image is too small to analyse.")

    sample = pixels
    if len(sample) > 12000:
        rng = np.random.default_rng(42)
        sample = sample[rng.choice(len(sample), 12000, replace=False)]
    unique_sample = np.unique(sample, axis=0)
    cluster_count = min(3, len(unique_sample))
    km = KMeans(n_clusters=cluster_count, random_state=42, n_init=10)
    labels = km.fit_predict(sample)
    counts = np.bincount(labels, minlength=cluster_count)
    ordered = np.argsort(counts)[::-1]
    dominant = [_rgb_to_category(tuple(km.cluster_centers_[idx])) for idx in ordered]
    # The model requires exactly three colour features. If a very flat image has
    # fewer than three distinct clusters, repeat the most common detected colour.
    while len(dominant) < 3:
        dominant.append(dominant[-1])

    mean_rgb = pixels.mean(axis=0)
    brightness = float(np.mean(pixels @ np.array([0.299, 0.587, 0.114])))
    warm_cast = mean_rgb[0] > mean_rgb[2] * 1.10
    if brightness < 65:
        lighting = "dark"
    elif brightness < 120:
        lighting = "dim"
    elif warm_cast:
        lighting = "warm"
    else:
        lighting = "bright"
    return dominant, lighting


@app.post("/recommend/image")
async def recommend_from_image(
    image: UploadFile = File(...),
    style: str = Form(...),
    room_type: str = Form(...),
    space: str = Form(...),
    lighting: Optional[str] = Form(None),
):
    image_bytes = await image.read()
    dominant, detected_lighting = _image_features(image_bytes)
    values = {
        "dominant_color_1": dominant[0],
        "dominant_color_2": dominant[1],
        "dominant_color_3": dominant[2],
        "style": style,
        "room_type": room_type,
        "space": space,
        "lighting": lighting or detected_lighting,
    }
    result = _predict(values)
    result["image_analysis"] = {
        "dominant_colors": dominant,
        "estimated_lighting": detected_lighting,
        "lighting_used": values["lighting"],
        "note": "Colours and lighting from the image are estimates; room type, style, and space are supplied by the app.",
    }
    return result
