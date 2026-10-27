# CoZee furniture colour recommendation model

This folder contains the trained Random Forest model and its label encoders, served through a small FastAPI backend. The API uses the original seven input features and predicts one of the trained furniture-colour labels (`beige`, `cream`, `grey`, `white`).

## Files

- `interior_model.pkl` — trained Random Forest classifier.
- `encoders.pkl` — fitted label encoders used by the model.
- `app.py` — API endpoints.
- `requirements.txt` — Python dependencies.
- `training_artifacts/` — processed training dataset and preprocessing script provided with the model.

## Run locally

Use Python 3.10 or 3.11 in a virtual environment where possible.

```bash
cd recommendation_api
python -m venv .venv
# Windows:
.venv\Scripts\activate
# macOS/Linux:
source .venv/bin/activate
python -m pip install -r requirements.txt
uvicorn app:app --host 0.0.0.0 --port 8000 --reload
```

Open `http://127.0.0.1:8000/docs` to try the endpoints.

## Endpoint A: submit extracted room features

`POST /recommend` with JSON:

```json
{
  "dominant_color_1": "beige",
  "dominant_color_2": "white",
  "dominant_color_3": "brown",
  "style": "modern",
  "room_type": "living",
  "space": "medium",
  "lighting": "bright"
}
```

Category values are case-insensitive. Use these values:

- Room colours: `beige`, `black`, `blue`, `brown`, `cream`, `grey`, `white`
- Style: `industrial`, `minimal`, `modern`, `scandinavian`, `traditional`
- Room type: `bedroom`, `dining`, `living`, `office`
- Space: `large`, `medium`, `small`
- Lighting: `bright`, `dark`, `dim`, `warm`

The response includes the predicted colour and the model's class probabilities. Probabilities are model scores, not guaranteed-calibrated confidence.

## Endpoint B: upload a room image

`POST /recommend/image` as `multipart/form-data` with:
- `image`: room photo
- `style`: interior style
- `room_type`: room type
- `space`: room size
- `lighting`: optional override (`bright`, `dark`, `dim`, or `warm`)

The API estimates dominant colours using K-Means and estimates lighting using image brightness/colour cast. These image features are heuristics mapped to the categories used by the training data; they are not a separate trained image model. Room type, style, and space are supplied by the app/user.

## Unity integration

Two scripts have been added under `Assets/Scripts/AI/`:

- `CozeeRecommendationClient.cs` sends feature JSON or uploads a room image.
- `FurnitureColourApplicator.cs` tints a selected furniture object's materials using the returned colour.

Attach `CozeeRecommendationClient` to a GameObject and set **Api Base Url** in the Inspector. Then call one of its methods from the UI flow. Example:

```csharp
recommendationClient.RecommendFromImage(
    roomImageBytes,
    "room.jpg",
    "modern",
    "living",
    "medium",
    "",
    response =>
    {
        colourApplicator.ApplyRecommendedColour(
            currentlySelectedFurniture,
            response.recommended_furniture_color
        );
    },
    error => Debug.LogError(error)
);
```

If you already have dominant room colours from your own room-scanning pipeline, call `RecommendFromFeatures(...)` instead.

### Set the API URL for the target device

- Unity Editor on the same computer: `http://127.0.0.1:8000`
- Android emulator: usually `http://10.0.2.2:8000`
- Physical phone: use the computer's LAN IP address, e.g. `http://192.168.1.10:8000`, and ensure both devices are on the same network and the firewall permits port 8000.

For production, deploy the API to an HTTPS host, restrict CORS, and configure Unity's platform/network security settings as needed. Do not put a private API key in the Unity client.

## Important model notes

- The API deliberately uses the included trained model and encoders rather than retraining a substitute.
- The supplied original `main.py` references `pipeline.py` and `room8.png`, which were not included in the uploaded ZIP. This API provides a separate working inference path using the saved model artifacts.
- The existing model can only recommend the four furniture colours represented in its training labels.
