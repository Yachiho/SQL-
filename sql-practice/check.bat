@echo off
chcp 65001 >nul
cd /d "%~dp0"
call _find_bash.bat
if not defined BASH (
    pause
    exit /b 1
)

set "DAY="
set /p DAY="採点する日番号を入力してください（例: 01。空欄でEnterなら01）: "
if "%DAY%"=="" set "DAY=01"

echo.
"%BASH%" check.sh %DAY%
echo.
pause
