@echo off
rem ============================================================
rem DongBoGitHub.bat
rem Dat file nay vao thu muc KHO GITHUB (thu muc da clone echo-lab-escape-room).
rem Moi lan sua game trong Unity, dong Unity, bam dup file nay de chep ban moi sang kho,
rem roi mo GitHub Desktop de Commit va Push.
rem
rem Neu thu muc project Unity cua ban khac, sua dong SRC ben duoi.
rem ============================================================

set "SRC=C:\Users\admin\PhysicsEscapeRoom"
set "DST=%~dp0"

if not exist "%SRC%\Assets" (
    echo LOI: Khong thay thu muc Assets trong "%SRC%".
    echo Hay sua dong SRC trong file nay cho dung duong dan project Unity.
    pause
    exit /b 1
)

echo Nguon : %SRC%
echo Dich  : %DST%
echo.
echo Se dong bo 3 thu muc: Assets, Packages, ProjectSettings
echo (file thua o dich se bi xoa de khop voi nguon).
echo.
pause

for %%F in (Assets Packages ProjectSettings) do (
    echo.
    echo === Dang chep %%F ===
    robocopy "%SRC%\%%F" "%DST%%%F" /MIR /NFL /NDL /NJH /NJS /NP
    if errorlevel 8 (
        echo LOI khi chep %%F
        pause
        exit /b 1
    )
)

echo.
echo XONG! Bay gio mo GitHub Desktop, kiem tra tab Changes, Commit va Push origin.
echo (Co the bo tick file DongBoGitHub.bat de khong dua no len GitHub.)
pause
