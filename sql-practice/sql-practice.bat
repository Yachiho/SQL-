@echo off
chcp 65001 >nul
cd /d "%~dp0"

:menu
cls
echo ========================================
echo   SQL Practice
echo ========================================
echo   1. 環境構築（practice.db を作成）
echo   2. 採点する
echo   3. SQLを自由に試す（対話モード）
echo   4. 終了
echo ========================================
set "CHOICE="
set /p CHOICE="番号を選んでEnterキーを押してください: "

if "%CHOICE%"=="1" (
    call setup.bat
    goto menu
)
if "%CHOICE%"=="2" (
    call check.bat
    goto menu
)
if "%CHOICE%"=="3" (
    call practice.bat
    goto menu
)
if "%CHOICE%"=="4" (
    exit /b 0
)

echo 1〜4の番号を入力してください。
pause
goto menu
