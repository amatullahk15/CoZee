from ultralytics import YOLO
import cv2
import os

class FurnitureDetector:

    def __init__(self):
        # Resolve model path relative to this file so it works regardless of
        # the current working directory (important when api.py imports from root).
        _here = os.path.dirname(os.path.abspath(__file__))
        _candidates = [
            os.path.join(_here, "yolov8n.pt"),
            os.path.join(_here, "backend", "yolov8n.pt"),
        ]
        _model_path = next((p for p in _candidates if os.path.isfile(p)), "yolov8n.pt")
        self.model = YOLO(_model_path)

    def detect(self, image_path):

        image = cv2.imread(image_path)

        if image is None:
            print("Image not found!")
            return None, []

        results = self.model.predict(source=image)

        furniture = []

        for result in results:

            for box in result.boxes:

                cls = int(box.cls)
                conf = float(box.conf)

                if conf < 0.40:
                    continue

                name = self.model.names[cls]

                x1, y1, x2, y2 = map(int, box.xyxy[0])

                furniture.append({
                    "name": name,
                    "x": (x1 + x2) // 2,
                    "y": (y1 + y2) // 2,
                    "box": (x1, y1, x2, y2)
                })

        return image, furniture