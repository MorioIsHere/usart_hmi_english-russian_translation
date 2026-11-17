@echo off
setlocal EnableExtensions
REM ============================================================
REM Installation Verification Script
REM Note: no paths are expanded inside ( ... ) blocks, because
REM "Program Files (x86)" contains ")" and would break cmd parsing.
REM ============================================================

echo ============================================================
echo USART HMI Translation System - Verification
echo ============================================================
echo.

set "DEVEL_DIR=C:\devel"
set /a ERROR_COUNT=0

set "USART_DIR="
if exist "C:\Program Files (x86)\USART HMI\USART HMI.exe" set "USART_DIR=C:\Program Files (x86)\USART HMI"
if not defined USART_DIR if exist "C:\Program Files\USART HMI\USART HMI.exe" set "USART_DIR=C:\Program Files\USART HMI"

echo [1] USART HMI Installation Directory
if defined USART_DIR echo   [OK] Found: "%USART_DIR%"
if defined USART_DIR goto :check_dlls
echo   [ERROR] USART HMI not found in Program Files
set /a ERROR_COUNT+=1
goto :check_devel

:check_dlls
echo.
echo [2] Critical DLL Files
for %%D in (hmitype.dll achmiface.dll rsapp.dll HMIFORM.dll TFTEDIT.dll TFTRUN.dll hmioldapp.dll) do call :check_dll "%%D"

:check_devel
echo.
echo [3] Translation Directory
if exist "%DEVEL_DIR%\" echo   [OK] Found: %DEVEL_DIR%
if exist "%DEVEL_DIR%\" goto :check_translation
echo   [ERROR] Not found: %DEVEL_DIR%
set /a ERROR_COUNT+=1

:check_translation
echo.
echo [4] Translation File
if not exist "%DEVEL_DIR%\hmi_translation.txt" goto :no_translation
set "LINE_COUNT=0"
for /f %%A in ('type "%DEVEL_DIR%\hmi_translation.txt" ^| find /c /v ""') do set "LINE_COUNT=%%A"
echo   [OK] Found: %DEVEL_DIR%\hmi_translation.txt (%LINE_COUNT% lines)
goto :check_log
:no_translation
echo   [ERROR] Not found: %DEVEL_DIR%\hmi_translation.txt
set /a ERROR_COUNT+=1

:check_log
echo.
echo [5] Log File (optional)
if not exist "%DEVEL_DIR%\hmi.log" goto :no_log
set "LOG_COUNT=0"
for /f %%A in ('type "%DEVEL_DIR%\hmi.log" ^| find /c /v ""') do set "LOG_COUNT=%%A"
echo   [OK] Found: %DEVEL_DIR%\hmi.log (%LOG_COUNT% untranslated entries)
goto :summary
:no_log
echo   [INFO] %DEVEL_DIR%\hmi.log will be created on first run

:summary
echo.
echo ============================================================
if %ERROR_COUNT% equ 0 goto :all_ok
echo   [ERROR] Found %ERROR_COUNT% problem(s).
echo   Run install_dlls.bat as administrator to fix the installation.
goto :end
:all_ok
echo   [OK] All checks passed! Start USART HMI as usual.
goto :end

REM ============================================================
:check_dll
if exist "%USART_DIR%\%~1" echo   [OK] %~1
if exist "%USART_DIR%\%~1" goto :eof
echo   [ERROR] Missing: %~1
set /a ERROR_COUNT+=1
goto :eof

:end
echo ============================================================
echo.
pause
endlocal
