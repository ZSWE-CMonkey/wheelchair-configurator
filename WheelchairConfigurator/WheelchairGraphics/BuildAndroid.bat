@echo off

REM for android we will build only release mode

REM Change if location of ndk is different :>
set ANDROID_NDK=C:\Android-NDK\android-ndk-r27d

set PATH=C:\ninja-build;%PATH%

set ANDROID_PLATFORM=android-21

set ABIS=armeabi-v7a arm64-v8a x86 x86_64

for %%A in (%ABIS%) do (

    echo ===============================
    echo Building for %%A
    echo ===============================

    cmake -G "Ninja" -S . -B out-android\%%A ^
        -DCMAKE_TOOLCHAIN_FILE=%ANDROID_NDK%\build\cmake\android.toolchain.cmake ^
        -DANDROID_ABI=%%A ^
        -DANDROID_PLATFORM=%ANDROID_PLATFORM%

    cmake --build out-android\%%A --config Release

)

echo.
echo All builds finished!