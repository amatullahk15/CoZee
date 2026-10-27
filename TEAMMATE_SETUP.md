# CoZee – Teammate Setup Guide

This guide lets a new team member run the full CoZee Python AI + Unity Vastu integration from scratch.

---

## Prerequisites

| Requirement | Version / Notes |
|---|---|
| Python | 3.10 or 3.11 (3.12+ may have dependency issues) |
| pip | bundled with Python |
| Unity | 2022.3 LTS (same version as the project) |
| Unity modules | Android Build Support + Android SDK/NDK Tools |
| Git | any recent version |
| Android phone | for on-device testing (optional) |

> **Note on model weights:** `backend/yolov8n.pt` (6 MB) and `vastu_complete_dataset.xlsx`  
> are committed to the repo. `Assets/Models/renovision.onnx` (~99 MB) is also committed.  
> If the push/clone is very slow, that is normal for the ONNX file.

---

## Step 1 – Clone the Repository

```bash
git clone https://github.com/amatullahk15/CoZee.git
cd CoZee
```

---

## Step 2 – Set Up the Python Backend

```bash
cd backend
pip install -r requirements.txt
```

This installs FastAPI, Uvicorn, Ultralytics (YOLOv8), OpenCV, Pandas, openpyxl, and Pillow.  
Installation may take 2–5 minutes the first time.

### Start the server

**Windows – double-click:**
```
backend\run_server.bat
```

**Or from a terminal:**
```bash
cd backend
python -m uvicorn api:app --host 0.0.0.0 --port 8000 --reload
```

The server is ready when you see:
```
INFO:     Uvicorn running on http://0.0.0.0:8000
```

### Test the backend (optional)

```bash
# Health check
curl http://localhost:8000/health

# Or run the smoke test
python backend/test_analyze.py
```

You can also open `http://localhost:8000/docs` in a browser to use the Swagger UI.

---

## Step 3 – Find Your PC's LAN IP Address

The Android device and your PC must be on the **same Wi-Fi network**.

**Windows:**
```
ipconfig
```
Look for **Wi-Fi → IPv4 Address**, e.g. `192.168.1.42`.  
This is your `<YOUR_PC_IP>`.

> `localhost` and `127.0.0.1` do NOT work on a physical Android device.

---

## Step 4 – Open the Unity Project

1. Open **Unity Hub**.
2. Click **Add → Add project from disk** and select the `CoZee` folder.
3. Open the project with **Unity 2022.3 LTS**.
4. Let Unity reimport assets (may take a few minutes the first time).

---

## Step 5 – Configure the Server URL in Unity

The `VastuApiClient` component stores the server URL as a `[SerializeField]` — you set it  
**in the Unity Inspector**, not in code. This means each teammate can use their own IP  
without touching any shared file.

1. In the **Hierarchy**, find the **VastuApiClient** GameObject  
   *(or press Ctrl+F and search for it — it may be on the same GameObject as VastuAssistantManager).*
2. Select it and look at the **Inspector**.
3. Find the **Server Url** field.
4. Replace the placeholder with:  
   ```
   http://<YOUR_PC_IP>:8000
   ```
   Example: `http://192.168.1.42:8000`

> **Do not commit this change** — the IP is machine-specific and already set to a safe  
> placeholder in the source file.

---

## Step 6 – Android Build Settings (for on-device testing)

1. **File → Build Settings → Android** → Switch Platform.
2. **Player Settings → Other Settings:**
   - Internet Access: **Require**
   - Write Permission: **External (SDCard)**
3. The `AndroidManifest.xml` and `network_security_config.xml` in  
   `Assets/Plugins/Android/` are already configured for cleartext HTTP on LAN.  
   You do **not** need to edit them.
4. Connect your Android phone via USB, enable **Developer Options + USB Debugging**.
5. **Build And Run** (or build APK and sideload).

---

## Step 7 – Test the Integration

1. Start the Python backend (Step 2).
2. Confirm it is reachable from your PC: `curl http://localhost:8000/health`
3. If testing on Android, confirm from your phone's browser:  
   `http://<YOUR_PC_IP>:8000/health` should return `{"status":"ok"}`.
4. Open the Unity app (Play Mode or on device).
5. Navigate to the **Vastu** screen.
6. Tap **Scan with AI**.
7. The app will capture a frame, send it to `/analyze`, and display:
   - Detected furniture with confidence scores
   - Per-item Vastu scores and suggestions
   - Overall Vastu score and summary
   - A direction note (image zone, not a compass direction)

---

## Project Structure

```
CoZee/
├── Assets/
│   ├── Models/renovision.onnx          ← Sentis/ONNX inference model
│   ├── Plugins/Android/                ← AndroidManifest + network security config
│   └── Scripts/
│       ├── VastuApiClient.cs           ← HTTP client for POST /analyze
│       ├── Managers/
│       │   ├── AppManager.cs           ← auto-spawns VastuApiClient singleton
│       │   └── VastuAssistantManager.cs← displays AI results in chat
│       └── UI/
│           ├── VastuDashboardView.cs   ← score cards, scrollable chat
│           └── VastuScreenController.cs← wires Scan with AI button to API
├── backend/
│   ├── api.py                          ← FastAPI /health + /analyze endpoints
│   ├── requirements.txt                ← Python dependencies
│   ├── run_server.bat                  ← one-click Windows launcher
│   ├── test_analyze.py                 ← smoke test
│   └── yolov8n.pt                      ← YOLOv8 nano model weights (6 MB)
├── detector.py                         ← YOLO detection wrapper
├── rule_engine.py                      ← Vastu rule evaluation
├── direction.py                        ← image-zone helper
├── datasetloader.py                    ← loads vastu_complete_dataset.xlsx
├── vastu_complete_dataset.xlsx         ← Vastu rules dataset (21 KB)
└── TEAMMATE_SETUP.md                   ← this file
```

---

## Troubleshooting

| Problem | Fix |
|---|---|
| `pip install` fails on `ultralytics` | Upgrade pip: `pip install --upgrade pip` then retry |
| Server starts but `/analyze` returns 500 | Check terminal for Python traceback; ensure `yolov8n.pt` is in `backend/` |
| Unity can't reach the server on Android | Verify same Wi-Fi; use LAN IP not localhost; check Windows Firewall |
| Windows Firewall blocks port 8000 | Allow it: `netsh advfirewall firewall add rule name="CoZee" dir=in action=allow protocol=TCP localport=8000` |
| Unity shows "Server URL not configured" | Set the Server Url field in the Inspector (Step 5) |
| renovision.onnx missing after clone | The file is committed; re-run `git pull` or check Git LFS if enabled |

---

## Notes for the Backend Developer

- The backend auto-loads `yolov8n.pt` using an absolute path resolved relative to `detector.py` — this works regardless of working directory.
- `rule_engine.py` loads `vastu_complete_dataset.xlsx` from the project root — keep it there.
- `direction.py` uses image quadrant analysis, not a real compass — the `direction_note` field in the response explains this to the user.
- POST `/analyze` accepts multipart form fields: `image` (file) and `room_type` (string, default `"living_room"`).
