def get_direction(x, y, width, height):

    horizontal = ""
    vertical = ""

    if x < width / 3:
        horizontal = "West"

    elif x > 2 * width / 3:
        horizontal = "East"

    if y < height / 3:
        vertical = "North"

    elif y > 2 * height / 3:
        vertical = "South"

    if horizontal == "" and vertical == "":
        return "Center"

    if horizontal == "":
        return vertical

    if vertical == "":
        return horizontal

    return vertical + "-" + horizontal