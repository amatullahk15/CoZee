import requests
import json
import sys

img_path = '../reference_image.png'
url = 'http://localhost:8000/analyze'

with open(img_path, 'rb') as f:
    files = {'image': ('room.jpg', f, 'image/jpeg')}
    data = {'room_type': 'livingroom'}
    r = requests.post(url, files=files, data=data, timeout=60)

print('HTTP STATUS:', r.status_code)
if r.ok:
    body = r.json()
    print('summary      :', body['summary'])
    print('overall_score:', body['overall_score'])
    print('image_size   :', body['image_width'], 'x', body['image_height'])
    print()
    for d in body['detections']:
        name = d['name']
        conf = round(d['confidence'] * 100)
        zone = d['image_zone']
        print(f'  DETECT: {name} ({conf}%) zone={zone}')
    print()
    for v in body['vastu_results']:
        furn = v['furniture']
        status = v['status']
        score = v['score']
        suggestion = v['suggestion']
        print(f'  VASTU: {furn} -> {status} ({score}%) | {suggestion}')
    print()
    print('direction_note:', body['direction_note'][:120])
    print()
    print('--- FULL JSON (verify Unity field names) ---')
    print(json.dumps(body, indent=2))
else:
    print('ERROR BODY:', r.text)
