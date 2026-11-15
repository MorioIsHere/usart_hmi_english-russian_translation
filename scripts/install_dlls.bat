@echo off
setlocal EnableExtensions
REM ============================================================
REM USART HMI Translation System - Installation Script
REM Installs decrypted DLLs and translation files.
REM Note: no paths are expanded inside ( ... ) blocks, because
REM "Program Files (x86)" contains ")" and would break cmd parsing.
REM ============================================================

echo ============================================================
echo USART HMI Translation System - Installation
echo ============================================================
echo.

REM --- Admin check ---
net session >nul 2>&1
if errorlevel 1 goto :err_admin
echo Administrator privileges: OK
echo.

set "SCRIPT_DIR=%~dp0"
set "DEVEL_DIR=C:\devel"
set "TRANSLATION_EN=%SCRIPT_DIR%..\translation\hmi_translation.txt"
set "TRANSLATION_RU=%SCRIPT_DIR%..\translation\hmi_translation_rus.txt"

REM --- Find USART HMI ---
set "USART_DIR="
if exist "C:\Program Files (x86)\USART HMI\USART HMI.exe" set "USART_DIR=C:\Program Files (x86)\USART HMI"
if not defined USART_DIR if exist "C:\Program Files\USART HMI\USART HMI.exe" set "USART_DIR=C:\Program Files\USART HMI"
if not defined USART_DIR goto :ask_usart
goto :found_usart
:ask_usart
echo USART HMI was not found in Program Files.
set /p "USART_DIR=Enter USART HMI folder path: "
set "USART_DIR=%USART_DIR:"=%"
if not exist "%USART_DIR%\" goto :err_usart
:found_usart
echo USART HMI folder: "%USART_DIR%"

REM --- Find decrypted DLLs ---
set "DECRYPTED_DIR="
if exist "%USART_DIR%\decrypted\hmitype.dll" set "DECRYPTED_DIR=%USART_DIR%\decrypted"
if not defined DECRYPTED_DIR if exist "%USERPROFILE%\Documents\USART_HMI_decrypted\hmitype.dll" set "DECRYPTED_DIR=%USERPROFILE%\Documents\USART_HMI_decrypted"
if not defined DECRYPTED_DIR if exist "E:\decrypted\hmitype.dll" set "DECRYPTED_DIR=E:\decrypted"
if not defined DECRYPTED_DIR goto :ask_decrypted
goto :found_decrypted
:ask_decrypted
echo Folder with decrypted DLLs (hmitype.dll) was not found automatically.
set /p "DECRYPTED_DIR=Enter path to the 'decrypted' folder: "
set "DECRYPTED_DIR=%DECRYPTED_DIR:"=%"
if not exist "%DECRYPTED_DIR%\hmitype.dll" goto :err_decrypted
:found_decrypted
echo Decrypted DLLs:  "%DECRYPTED_DIR%"
echo.

REM --- Make sure USART HMI is not running ---
tasklist /fi "imagename eq USART HMI.exe" 2>nul | find /i "USART HMI.exe" >nul
if not errorlevel 1 goto :err_running

REM --- Choose translation language ---
echo Translation language:
echo   1 - English
echo   2 - Russian
set "LANGSEL=1"
set /p "LANGSEL=Select 1 or 2 [1]: "
set "LANGSEL=%LANGSEL:~0,1%"
set "TRANSLATION_FILE=%TRANSLATION_EN%"
if "%LANGSEL%"=="2" set "TRANSLATION_FILE=%TRANSLATION_RU%"
if "%LANGSEL%"=="2" echo Selected: Russian
if not "%LANGSEL%"=="2" echo Selected: English
echo.

REM --- Step 1: translation file ---
echo Step 1: Installing translation file...
if not exist "%DEVEL_DIR%\" mkdir "%DEVEL_DIR%"
if not exist "%DEVEL_DIR%\" goto :err_devel
if not exist "%TRANSLATION_FILE%" goto :warn_translation
copy /y "%TRANSLATION_FILE%" "%DEVEL_DIR%\hmi_translation.txt" >nul
if errorlevel 1 goto :err_copy_translation
echo   Installed: %DEVEL_DIR%\hmi_translation.txt
goto :step2
:warn_translation
echo   WARNING: translation file not found: "%TRANSLATION_FILE%"

:step2
REM --- Step 2: backup ---
echo Step 2: Backing up original DLLs...
set "BACKUP_DIR=%USART_DIR%\backup_original"
if exist "%BACKUP_DIR%\" goto :backup_exists
mkdir "%BACKUP_DIR%"
copy /y "%USART_DIR%\*.dll" "%BACKUP_DIR%\" >nul 2>&1
echo   Backup created: "%BACKUP_DIR%"
goto :step3
:backup_exists
echo   Backup already exists, skipping: "%BACKUP_DIR%"

:step3
REM --- Step 3: copy DLLs ---
echo Step 3: Installing decrypted DLLs...
set /a COUNT=0
set /a FAILED=0
REM pushd + (*.dll): a path with ")" must not appear inside for ( ... )
pushd "%DECRYPTED_DIR%"
for %%F in (*.dll) do call :copy_one "%%~fF"
popd
echo   Installed: %COUNT% file(s), failed: %FAILED%
echo.

REM --- Step 4: verify ---
echo Step 4: Verifying...
set /a MISSING=0
for %%D in (hmitype.dll achmiface.dll rsapp.dll HMIFORM.dll TFTEDIT.dll TFTRUN.dll hmioldapp.dll) do call :check_one "%%D"
if exist "%DEVEL_DIR%\hmi_translation.txt" echo   OK: translation file
if not exist "%DEVEL_DIR%\hmi_translation.txt" echo   WARNING: translation file missing
echo.

if %FAILED% gtr 0 goto :done_with_errors
if %MISSING% gtr 0 goto :done_with_errors

echo ============================================================
echo Installation complete!
echo ============================================================
echo.
echo Start USART HMI as usual.
echo Untranslated text will be logged to: %DEVEL_DIR%\hmi.log
echo Add translations to: %DEVEL_DIR%\hmi_translation.txt
echo   (format: chinese text TAB translation, UTF-8)
echo.
echo To undo: copy all files from "%BACKUP_DIR%" back to "%USART_DIR%"
goto :end

:done_with_errors
echo ============================================================
echo Installation finished WITH ERRORS - see messages above.
echo ============================================================
goto :end

REM ============================================================
:copy_one
copy /y "%~1" "%USART_DIR%\" >nul 2>&1
if errorlevel 1 goto :copy_one_fail
echo   Installed: %~nx1
set /a COUNT+=1
goto :eof
:copy_one_fail
echo   ERROR: failed to copy %~nx1
set /a FAILED+=1
goto :eof

:check_one
if exist "%USART_DIR%\%~1" echo   OK: %~1
if exist "%USART_DIR%\%~1" goto :eof
echo   MISSING: %~1
set /a MISSING+=1
goto :eof

REM ============================================================
:err_admin
echo ERROR: Administrator privileges required!
echo Right-click install_dlls.bat and select "Run as administrator".
goto :end
:err_usart
echo ERROR: USART HMI folder not found: "%USART_DIR%"
goto :end
:err_decrypted
echo ERROR: hmitype.dll not found in: "%DECRYPTED_DIR%"
echo Run tools\ACTRDecryptor.exe first.
goto :end
:err_running
echo ERROR: USART HMI is running. Close it and run this script again.
goto :end
:err_devel
echo ERROR: cannot create folder %DEVEL_DIR%
goto :end
:err_copy_translation
echo ERROR: failed to copy translation file to %DEVEL_DIR%
goto :end

:end
echo.
pause
endlocal
