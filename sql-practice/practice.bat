@echo off
chcp 65001 >nul
cd /d "%~dp0"

where sqlite3 >nul 2>nul
if errorlevel 1 (
    echo sqlite3 が見つかりません。WINDOWS_SETUP.md の手順でインストールしてください。
    pause
    exit /b 1
)

if not exist practice.db (
    echo practice.db がまだありません。先に「1. 環境構築」を実行してください。
    pause
    exit /b 1
)

echo SQLを自由に試せます。終了するには .quit と入力してください。
echo.
sqlite3 -header -column practice.db
