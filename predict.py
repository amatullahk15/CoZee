import cv2

from detector import FurnitureDetector
from direction import get_direction
from rule_engine import RuleEngine

detector = FurnitureDetector()

engine = RuleEngine()

image_path = input("Enter Image Path : ")

room = input("Enter Room Type : ").lower()


image, furniture = detector.detect(image_path)

if image is None:

    print("Image not found.")

    exit()

if len(furniture) == 0:

    print("No furniture detected.")

    exit()

height, width = image.shape[:2]

print("\nDetected Furniture\n")

for i, item in enumerate(furniture):

    direction = get_direction(

        item["x"],
        item["y"],
        width,
        height

    )

    print(

        str(i+1)+".",

        item["name"],

        "(" + direction + ")"

    )

choice = int(input("\nChoose Furniture Number : ")) - 1

selected = furniture[choice]

direction = get_direction(

    selected["x"],
    selected["y"],
    width,
    height

)

result = engine.check(

    room,

    selected["name"],

    direction

)

print("\n")

print("="*50)

print("Furniture :",selected["name"])

print("Room :",room)

print("Current Direction :",direction)

print("Compatibility :",result["score"],"%")

print("Status :",result["status"])

print("Suggestion :",result["suggestion"])

print("="*50)

x1,y1,x2,y2 = selected["box"]

cv2.rectangle(

    image,

    (x1,y1),

    (x2,y2),

    (0,255,0),

    3

)

cv2.putText(

    image,

    selected["name"],

    (x1,y1-10),

    cv2.FONT_HERSHEY_SIMPLEX,

    0.8,

    (0,255,0),

    2

)

cv2.imshow("Selected Furniture",image)

cv2.waitKey(0)

cv2.destroyAllWindows()