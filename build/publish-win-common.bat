@echo off

rem This file is part of Radio Downloader.
rem Copyright © 2007-2013 by the authors - see the AUTHORS file for details.
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

rem Make sure we are running from the directory this is located in
cd /D "%~dp0"

rem Load any user / machine specific variable values
if exist publish-win-vars.bat call publish-win-vars.bat

rem Check the platform has been passed to this script and is known
if "%~1" == "" goto noplatform
if "%~1" == "x86" set platname=win32
if "%~1" == "x64" set platname=win64
if "%platname%" == "" goto badplatform

rem Check the SDK is installed
FOR /F "skip=2 tokens=2,*" %%A IN ('reg query "HKLM\SOFTWARE\Microsoft\Microsoft SDKs\Windows\v7.1" /v InstallationFolder 2^> nul') DO set sdklocation=%%B
if not exist "%sdklocation%" goto nosdk

rem Set up a Release build environment
setlocal ENABLEEXTENSIONS
setlocal ENABLEDELAYEDEXPANSION
call "%sdklocation%\Bin\setenv.cmd" /Release /%~1

rem Build Radio Downloader and the provider
call build-win-common.bat %1
if ERRORLEVEL 1 goto failed

rem Sign Radio Downloader and the provider
set timestampserver=http://timestamp.verisign.com/scripts/timstamp.dll
signtool sign /t %timestampserver% %signparams% "..\bin\%platname%\Radio Downloader.exe" ^
                                                "..\bin\%platname%\PodcastProvider.dll"
if ERRORLEVEL 1 set signfailed=1

rem Unregister HKCU JScript and VBScript which cause problems with installer validation

reg delete "HKCU\SOFTWARE\Classes\CLSID\{B54F3741-5B07-11CF-A4B0-00AA004A55E8}" /f
reg delete "HKCU\SOFTWARE\Classes\CLSID\{F414C260-6AC0-11CF-B6D1-00AA00BBBB58}" /f
reg delete "HKCU\SOFTWARE\Classes\Wow6432Node\CLSID\{B54F3741-5B07-11CF-A4B0-00AA004A55E8}" /f
reg delete "HKCU\SOFTWARE\Classes\Wow6432Node\CLSID\{F414C260-6AC0-11CF-B6D1-00AA00BBBB58}" /f

rem Clean and build the installer

msbuild /p:Configuration=Release /p:Platform=%~1 /t:Clean "..\installer\Radio Downloader.wixproj"
if ERRORLEVEL 1 goto failed

msbuild /p:Configuration=Release /p:Platform=%~1 "..\installer\Radio Downloader.wixproj"
if ERRORLEVEL 1 goto failed

rem Sign the installer

signtool sign /t %timestampserver% /d "Radio Downloader" %signparams% ^
              "..\installer\Radio_Downloader-%platname%.msi"
if ERRORLEVEL 1 set signfailed=1

if not "%signfailed%" == "" goto signfailed

goto :EOF

:noplatform

echo The platform must be passed as a parameter when running this script

pause
goto :EOF

:badplatform

echo The platform %~1 is not known

pause
goto :EOF

:nosdk

echo The Microsoft Windows SDK for Windows 7 does not appear to be installed
echo Please install it and then try running this script again

pause
goto :EOF

:signfailed

echo.
echo Warning: Failed to sign one or more of the binaries or installer.
echo If you have multiple code signing certificates set the variable 'signparams' to
echo the required signtool selection switch in the file publish-win-vars.bat

pause
goto :EOF

:failed

echo.
echo Publish failed - review above output for more details

pause
