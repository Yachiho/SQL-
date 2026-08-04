@echo off
chcp 65001 >nul
cd /d "%~dp0"
call _find_bash.bat
if not defined BASH (
    pause
    exit /b 1
)

echo practice.db を構築しています...
echo.
"%BASH%" setup.sh
echo.
pause
