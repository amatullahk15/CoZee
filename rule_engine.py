from datasetloader import DatasetLoader


class RuleEngine:

    def __init__(self):

        loader = DatasetLoader("dataset/vastu_complete_dataset.xlsx")
        self.rooms = loader.load()

        self.furniture_database = {

            "livingroom": {

                "couch": {
                    "ideal": ["South-West", "South", "West"],
                    "avoid": ["North-East"]
                },

                "sofa": {
                    "ideal": ["South-West", "South", "West"],
                    "avoid": ["North-East"]
                },

                "tv": {
                    "ideal": ["South-East", "East"],
                    "avoid": ["North-West"]
                },

                "chair": {
                    "ideal": ["West", "South"],
                    "avoid": []
                }

            },

            "bedroom": {

                "bed": {
                    "ideal": ["South-West"],
                    "avoid": ["North-East"]
                },

                "wardrobe": {
                    "ideal": ["South-West", "West"],
                    "avoid": ["North-East"]
                },

                "mirror": {
                    "ideal": ["North", "East"],
                    "avoid": ["South"]
                }

            },

            "kitchen": {

                "stove": {
                    "ideal": ["South-East"],
                    "avoid": ["North-East"]
                },

                "sink": {
                    "ideal": ["North-East", "North"],
                    "avoid": ["South-East"]
                },

                "refrigerator": {
                    "ideal": ["South", "West", "South-West"],
                    "avoid": ["North-East"]
                }

            }

        }

    def extract_directions(self, text):

        directions = []

        possible = [

            "North-East",
            "North-West",
            "South-East",
            "South-West",
            "North",
            "South",
            "East",
            "West",
            "Center"

        ]

        text = str(text)

        for d in possible:

            if d.lower() in text.lower():

                directions.append(d)

        return directions

    def check(self, room, furniture, direction):

        room = room.lower().replace(" ", "")
        furniture = furniture.lower()

        # -----------------------------
        # First check furniture database
        # -----------------------------

        if room in self.furniture_database:

            if furniture in self.furniture_database[room]:

                rule = self.furniture_database[room][furniture]

                ideal = rule["ideal"]
                avoid = rule["avoid"]

                if direction in ideal:

                    return {

                        "score": 95,
                        "status": "Excellent",
                        "suggestion": f"{furniture.capitalize()} is correctly placed according to Vastu."

                    }

                elif direction in avoid:

                    return {

                        "score": 20,
                        "status": "Poor",
                        "suggestion": f"Move the {furniture} towards {ideal[0]}."

                    }

                else:

                    return {

                        "score": 60,
                        "status": "Average",
                        "suggestion": f"The recommended position for {furniture} is {', '.join(ideal)}."

                    }

        # -----------------------------------
        # Fallback to Excel dataset
        # -----------------------------------

        if room not in self.rooms:

            return {

                "score": 0,
                "status": "Room Not Found",
                "suggestion": "Room not present in dataset."

            }

        df = self.rooms[room]

        ideal = []
        avoid = []

        for _, row in df.iterrows():

            category = str(row["Rule Category"])
            recommendation = str(row["Recommendation"])

            if "Ideal Direction" in category:

                ideal = self.extract_directions(recommendation)

            if "Avoid Direction" in category:

                avoid = self.extract_directions(recommendation)

        if direction in ideal:

            return {

                "score": 95,
                "status": "Excellent",
                "suggestion": "Correct according to room Vastu."

            }

        elif direction in avoid:

            best = ideal[0] if len(ideal) > 0 else "Ideal Position"

            return {

                "score": 20,
                "status": "Poor",
                "suggestion": f"Move the furniture towards {best}."

            }

        else:

            best = ideal[0] if len(ideal) > 0 else "Ideal Position"

            return {

                "score": 60,
                "status": "Average",
                "suggestion": f"Recommended direction is {best}."

            }