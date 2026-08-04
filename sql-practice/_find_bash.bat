@echo off
rem Git Bash (bash.exe) の場所を探して %BASH% にセットする共通部品。
rem 他の .bat から call で読み込んで使う。

where bash >nul 2>nul
if not errorlevel 1 (
    set "BASH=bash"
    goto :eof
)

if exist "C:\Program Files\Git\bin\bash.exe" (
    set "BASH=C:\Program Files\Git\bin\bash.exe"
    goto :eof
)

if exist "C:\Program Files (x86)\Git\bin\bash.exe" (
    set "BASH=C:\Program Files (x86)\Git\bin\bash.exe"
    goto :eof
)

echo Git Bash ^(bash.exe^) が見つかりませんでした。
echo Git for Windows がインストールされているか確認してください。
echo https://git-scm.com/download/win
set "BASH="
goto :eof
