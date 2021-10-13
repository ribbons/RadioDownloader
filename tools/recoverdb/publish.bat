@echo off

rem Copyright © 2014 Matt Robinson
rem
rem SPDX-License-Identifier: GPL-3.0-or-later

rem Load any user / machine specific variable values
if exist ../../build/publish-win-vars.bat call ../../build/publish-win-vars.bat

rem Check the SDK is installed
FOR /F "skip=2 tokens=2,*" %%A IN ('reg query "HKLM\SOFTWARE\Microsoft\Microsoft SDKs\Windows\v7.1" /v InstallationFolder 2^> nul') DO set sdklocation=%%B
if not exist "%sdklocation%" goto nosdk

rem Set up a Release build environment
setlocal ENABLEEXTENSIONS
setlocal ENABLEDELAYEDEXPANSION
call "%sdklocation%\Bin\setenv.cmd" /Release

start /W %windir%\SysWOW64\iexpress /N recoverdb.sed
if ERRORLEVEL 1 goto failed

PING 1.1.1.1 -n 1 -w 1000 >NUL

signtool sign /t http://timestamp.verisign.com/scripts/timstamp.dll /d "Recover Radio Downloader Database" ^
              %signparams% recoverdb.exe
if ERRORLEVEL 1 goto signfailed

goto :EOF

:nosdk

echo The Microsoft Windows SDK for Windows 7 does not appear to be installed

pause
goto :EOF

:signfailed

echo.
echo Warning: Failed to sign the binary.
echo If you have multiple code signing certificates set the variable 'signparams' to
echo the required signtool selection switch in the file ..\build\publish-win-vars.bat

pause
goto :EOF

:failed

echo.
echo Build failed - review above output for more details

pause
