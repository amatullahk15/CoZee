"""
CoZee FastAPI Backend
=====================
Wraps the existing Python AI pipeline (detector.py, direction.py, rule_engine.py)
and exposes two endpoints:

  GET  /health          — liveness check
  POST /analyze         — accepts a room image + room_type, returns JSON

Design decisions:
- sys.path manipulation ensures the parent directory (project root) is on the
  import path so detector/direction/rule_engine import identically to predict.py.
- direction.py maps pixel position within the *image frame* to a compass label
  (e.g. "South-West").  This is NOT an absolute compass bearing from hardware.
  The API field is named "image_zone" to be explicit about this.  The Vastu rule
  engine uses the same spatial convention, so scores remain valid.
- opencv-python-headless is preferred for servers (no GUI); falls back to
  opencv-python if only that is installed.
- Uploaded images are decoded from bytes in memory — no temp-file I/O.
- The response schema is designed to be parseable by Unity's JsonUtility
  (flat structs, no nested generic dicts, all fields snake_case).
"""

import sys
import os

# Make the project root importable so detector/direction/rule_engine are found.
_ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
if _ROOT not in sys.path:
    sys.path.insert(0, _ROOT)

import io
import logging
import traceback
from typing import List

import cv2
import numpy as np
from fastapi import FastAPI, File, Form, UploadFile, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel

# ── Local imports (all from project root) ──────────────────────────────────
from detector import FurnitureDetector
from direction import get_direction
from rule_engine import RuleEngine

# ── Logging ────────────────────────────────────────────────────────────────
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(message)s",
)
log = logging.getLogger("cozee_api")

# ── Initialise ML models once at startup (not per request) ─────────────────
log.info("Loading FurnitureDetector (YOLOv8)…")
detector = FurnitureDetector()
log.info("FurnitureDetector ready.")

log.info("Loading RuleEngine (Vastu dataset)…")
engine = RuleEngine()
log.info("RuleEngine ready.")

# ── FastAPI app ────────────────────────────────────────────────────────────
app = FastAPI(
    title="CoZee AI Backend",
    description="Furniture detection and Vastu analysis API",
    version="1.0.0",
)

# Allow Unity WebRequest from any origin (LAN IP, localhost, etc.)
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["GET", "POST"],
    allow_headers=["*"],
)


# ── Response schemas (must be flat for Unity JsonUtility) ──────────────────

class DetectionItem(BaseModel):
    """Single detected furniture item."""
    name: str
    confidence: float
    # image_zone = compass label derived from bounding-box pixel position
    # within the image frame (West / East / North / South / Center / …).
    # NOT a hardware compass reading.  Named explicitly to avoid confusion.
    image_zone: str
    box_x1: int
    box_y1: int
    box_x2: int
    box_y2: int
    center_x: int
    center_y: int


class VastuItem(BaseModel):
    """Vastu score for one detected furniture item."""
    furniture: str
    image_zone: str
    score: int
    status: str
    suggestion: str


class AnalyzeResponse(BaseModel):
    """
    Full analysis response.
    All fields are primitives or lists of flat objects so Unity's
    JsonUtility / JsonHelper can deserialise them without custom converters.
    """
    room_type: str
    image_width: int
    image_height: int
    detections: List[DetectionItem]
    vastu_results: List[VastuItem]
    overall_score: int
    summary: str
    direction_note: str


class HealthResponse(BaseModel):
    status: str
    message: str


# ── Endpoints ──────────────────────────────────────────────────────────────

@app.get("/health", response_model=HealthResponse)
async def health():
    """Liveness check — call this first to confirm the server is up."""
    return HealthResponse(
        status="ok",
        message="CoZee AI backend is running. Detector and RuleEngine loaded.",
    )


@app.post("/analyze", response_model=AnalyzeResponse)
async def analyze(
    image: UploadFile = File(..., description="Room photo (JPEG/PNG)"),
    room_type: str = Form("livingroom", description="Room type: bedroom, kitchen, livingroom, etc."),
):
    """
    Analyse a room photo:
    1. Decode the uploaded image from bytes (no temp files).
    2. Run YOLOv8 furniture detection (FurnitureDetector).
    3. For each detection, compute the image-frame zone (direction.py).
    4. Run Vastu rule check (RuleEngine.check).
    5. Return structured JSON.

    Field names in the multipart form:
        image     — binary file field
        room_type — text field, defaults to "livingroom"
    """
    # ── 1. Decode image from memory ────────────────────────────────────────
    try:
        raw_bytes = await image.read()
        if not raw_bytes:
            raise HTTPException(status_code=400, detail="Uploaded image file is empty.")

        np_arr = np.frombuffer(raw_bytes, dtype=np.uint8)
        img = cv2.imdecode(np_arr, cv2.IMREAD_COLOR)
        if img is None:
            raise HTTPException(
                status_code=400,
                detail="Could not decode image. Send a valid JPEG or PNG.",
            )
    except HTTPException:
        raise
    except Exception as exc:
        log.error("Image decode error: %s", traceback.format_exc())
        raise HTTPException(status_code=400, detail=f"Image decode failed: {exc}")

    img_h, img_w = img.shape[:2]
    log.info("Received image %dx%d, room_type=%r", img_w, img_h, room_type)

    # ── 2. Save decoded image to a temp path for FurnitureDetector ────────
    # detector.detect() accepts an image path.  We write to a BytesIO-backed
    # temp numpy array — detector also accepts numpy arrays via predict(source=).
    # Use the numpy array path directly to avoid any file I/O.
    try:
        results_raw = detector.model.predict(source=img, verbose=False)
    except Exception as exc:
        log.error("YOLO inference error: %s", traceback.format_exc())
        raise HTTPException(status_code=500, detail=f"Inference failed: {exc}")

    # ── 3. Parse detections ────────────────────────────────────────────────
    furniture_items = []
    for result in results_raw:
        for box in result.boxes:
            conf = float(box.conf)
            if conf < 0.40:
                continue
            cls_idx = int(box.cls)
            name = detector.model.names[cls_idx]
            x1, y1, x2, y2 = map(int, box.xyxy[0])
            cx = (x1 + x2) // 2
            cy = (y1 + y2) // 2
            furniture_items.append({
                "name": name,
                "confidence": round(conf, 4),
                "x": cx,
                "y": cy,
                "box": (x1, y1, x2, y2),
            })

    log.info("Detected %d items above threshold.", len(furniture_items))

    # ── 4. Compute image-frame zone + Vastu score per item ────────────────
    detections_out: List[DetectionItem] = []
    vastu_out: List[VastuItem] = []

    for item in furniture_items:
        zone = get_direction(item["x"], item["y"], img_w, img_h)

        x1, y1, x2, y2 = item["box"]
        detections_out.append(DetectionItem(
            name=item["name"],
            confidence=item["confidence"],
            image_zone=zone,
            box_x1=x1,
            box_y1=y1,
            box_x2=x2,
            box_y2=y2,
            center_x=item["x"],
            center_y=item["y"],
        ))

        vastu_result = engine.check(room_type, item["name"], zone)
        vastu_out.append(VastuItem(
            furniture=item["name"],
            image_zone=zone,
            score=int(vastu_result["score"]),
            status=vastu_result["status"],
            suggestion=vastu_result["suggestion"],
        ))

    # ── 5. Overall score (mean of individual scores, 0 if no detections) ──
    overall = (
        round(sum(v.score for v in vastu_out) / len(vastu_out))
        if vastu_out else 0
    )

    excellent = sum(1 for v in vastu_out if v.status == "Excellent")
    poor = sum(1 for v in vastu_out if v.status == "Poor")
    average = sum(1 for v in vastu_out if v.status == "Average")

    if not vastu_out:
        summary = "No furniture detected. Try a clearer, well-lit room photo."
    else:
        summary = (
            f"{len(vastu_out)} item(s) detected. "
            f"{excellent} Excellent, {average} Average, {poor} Poor."
        )

    direction_note = (
        "image_zone is derived from the bounding-box position within the photo "
        "(e.g. left side of image = West). It is NOT a hardware compass reading. "
        "Use the Vastu dashboard direction selector to set your room's actual compass orientation."
    )

    response = AnalyzeResponse(
        room_type=room_type,
        image_width=img_w,
        image_height=img_h,
        detections=detections_out,
        vastu_results=vastu_out,
        overall_score=overall,
        summary=summary,
        direction_note=direction_note,
    )

    log.info("Response: overall_score=%d, summary=%r", overall, summary)
    return response


# ── Entry point ────────────────────────────────────────────────────────────
if __name__ == "__main__":
    import uvicorn
    uvicorn.run(
        "api:app",
        host="0.0.0.0",   # Bind to all interfaces so Android on LAN can reach it
        port=8000,
        reload=False,      # Do not use reload= True in production/device testing
        log_level="info",
    )
