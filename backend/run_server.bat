@echo off
REM ============================================================
REM  CoZee AI Backend Launcher
REM  Run this file to start the FastAPI server.
REM  Both your PC and your Android phone must be on the same Wi-Fi.
REM ============================================================

cd /d "%~dp0"

echo.
echo  Checking dependencies...
python -m pip install -r requirements.txt --quiet

echo.
echo  Your LAN IP addresses (use one of these in Unity's VastuApiClient):
ipconfig | findstr /i "IPv4"

echo.
echo  Starting CoZee API server on http://0.0.0.0:8000
echo  Press Ctrl+C to stop.
echo.

python -m uvicorn api:app --host 0.0.0.0 --port 8000 --log-level info

pause
