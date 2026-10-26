from ultralytics import YOLO
import cv2

class FurnitureDetector:

    def __init__(self):
        self.model = YOLO("yolov8n.pt")

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