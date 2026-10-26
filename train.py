from rule_engine import RuleEngine

engine = RuleEngine()

room = input("Enter Room Type : ")

furniture = input("Enter Furniture Name : ")

direction = input("Enter Current Direction : ")

result = engine.check(
    room,
    furniture,
    direction
)

print("\n")
print("=" * 40)

print("Furniture :", furniture)

print("Room :", room)

print("Current Direction :", direction)

print("Compatibility :", result["score"], "%")

print("Status :", result["status"])

print("Suggestion :", result["suggestion"])

print("=" * 40)