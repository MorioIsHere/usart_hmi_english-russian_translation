@echo off
setlocal EnableExtensions
REM ============================================================
REM ACTR.dll Decryption Script (x64)
REM Decrypts ACTR.dll from the USART HMI installation.
REM Note: no paths are expanded inside ( ... ) blocks, because
REM "Program Files (x86)" contains ")" and would break cmd parsing.
REM ============================================================

echo ============================================================
echo ACTR.dll Decryption Script
echo ============================================================
echo.

set "SCRIPT_DIR=%~dp0"
set "TOOLS_DIR=%SCRIPT_DIR%..\tools"
set "DECRYPTOR=%TOOLS_DIR%\ACTRDecryptor.exe"
set "DECRYPTOR_BUILD=%TOOLS_DIR%\bin\Release\net9.0-windows\win-x64\ACTRDecryptor.exe"

REM Prefer a locally built exe if present, otherwise the pre-built one
if exist "%DECRYPTOR_BUILD%" set "DECRYPTOR=%DECRYPTOR_BUILD%"
if not exist "%DECRYPTOR%" goto :err_decryptor

REM --- Find ACTR.dll ---
set "ACTR_FILE="
if exist "C:\Program Files (x86)\USART HMI\ACTR.dll" set "ACTR_FILE=C:\Program Files (x86)\USART HMI\ACTR.dll"
if not defined ACTR_FILE if exist "C:\Program Files\USART HMI\ACTR.dll" set "ACTR_FILE=C:\Program Files\USART HMI\ACTR.dll"
if defined ACTR_FILE goto :found_actr
echo ACTR.dll was not found in Program Files.
set /p "ACTR_FILE=Enter full path to ACTR.dll: "
set "ACTR_FILE=%ACTR_FILE:"=%"
if not exist "%ACTR_FILE%" goto :err_actr
:found_actr

echo Decryptor: "%DECRYPTOR%"
echo ACTR.dll:  "%ACTR_FILE%"
echo.

REM "decode" argument -> the exe does not pause on its own
"%DECRYPTOR%" decode "%ACTR_FILE%"
if errorlevel 1 goto :err_failed

echo.
echo ============================================================
echo Decryption complete!
echo ============================================================
echo The output folder is shown above ("decrypted" next to ACTR.dll,
echo or Documents\USART_HMI_decrypted without admin rights).
echo.
echo Next steps:
echo   1. Open hmitype.dll from that folder in dnSpy
echo   2. Modify the Language() method (docs\LANGUAGE_METHOD.md)
echo   3. Save the module
echo   4. Run install_dlls.bat as administrator
goto :end

:err_decryptor
echo ERROR: ACTRDecryptor.exe not found: "%DECRYPTOR%"
goto :end
:err_actr
echo ERROR: file not found: "%ACTR_FILE%"
goto :end
:err_failed
echo.
echo ERROR: Decryption failed!
goto :end

:end
echo.
pause
endlocal
