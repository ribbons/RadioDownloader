@echo off

rem This file is part of Radio Downloader.
rem Copyright © 2007-2014 by the authors - see the AUTHORS file for details.
rem
rem This program is free software: you can redistribute it and/or modify it under the terms of the GNU General
rem Public License as published by the Free Software Foundation, either version 3 of the License, or (at your
rem option) any later version.
rem
rem This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
rem implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public
rem License for more details.
rem
rem You should have received a copy of the GNU General Public License along with this program.  If not, see
rem <http://www.gnu.org/licenses/>.

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
