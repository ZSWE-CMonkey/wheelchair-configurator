@echo off

set ABIS=armeabi-v7a arm64-v8a x86 x86_64
set SCRIPT_DIR=%~dp0
set SRC_BASE=%SCRIPT_DIR%out-android
set DST_BASE=%SCRIPT_DIR%..\WheelchairConfigurator\Resources\libs

echo Exporting Android libraries to MAUI app...

for %%A in (%ABIS%) do (
    if not exist "%SRC_BASE%\%%A\WheelchairGraphics\libWheelchairGraphics.so" (
        echo WARNING: Missing %%A - run BuildAndroid.bat first
    ) else (
        xcopy /Y "%SRC_BASE%\%%A\WheelchairGraphics\libWheelchairGraphics.so" "%DST_BASE%\%%A\"
        echo Copied %%A
    )
)

echo Done.
