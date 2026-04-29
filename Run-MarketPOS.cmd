@echo off
setlocal
cd /d "%~dp0"

echo Closing any already-running MarketPOS window...
taskkill /IM MarketPOS.exe /F >nul 2>nul

echo Building MarketPOS...
dotnet build MarketPOS.slnx
if errorlevel 1 (
    echo.
    echo Build failed. Read the error above.
    pause
    exit /b 1
)

echo.
echo Starting MarketPOS...
start "" "%~dp0bin\Debug\net8.0-windows\MarketPOS.exe"
exit /b 0
