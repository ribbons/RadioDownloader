@echo off

rem This file is part of Radio Downloader.
rem Copyright © 2007-2016 by the authors - see the AUTHORS file for details.
rem
rem This program is free software: you can redistribute it and/or modify
rem it under the terms of the GNU General Public License as published by
rem the Free Software Foundation, either version 3 of the License, or
rem (at your option) any later version.
rem
rem This program is distributed in the hope that it will be useful,
rem but WITHOUT ANY WARRANTY; without even the implied warranty of
rem MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
rem GNU General Public License for more details.
rem
rem You should have received a copy of the GNU General Public License
rem along with this program.  If not, see <http://www.gnu.org/licenses/>.

rem Make sure we are running from the directory this is located in
cd /D "%~dp0"

rem Check the platform has been passed to this script and is known
if "%~1" == "" goto noplatform
if "%~1" == "x86" set platname=win32
if "%~1" == "x64" set platname=win64
if "%platname%" == "" goto badplatform

if "%Configuration%_%TARGET_CPU%" == "%~1_Release" goto haveenv

rem Check the SDK is installed
FOR /F "skip=2 tokens=2,*" %%A IN ('reg query "HKLM\SOFTWARE\Microsoft\Microsoft SDKs\Windows\v7.1" /v InstallationFolder 2^> nul') DO set sdklocation=%%B
if not exist "%sdklocation%" goto nosdk

rem Set up a Release build environment
setlocal ENABLEEXTENSIONS
setlocal ENABLEDELAYEDEXPANSION
call "%sdklocation%\Bin\setenv.cmd" /Release /%~1

:haveenv

rem Build Radio Downloader and the providers
msbuild /p:Configuration=Release /p:Platform=%platname% /t:Clean "../Radio Downloader.sln"
if ERRORLEVEL 1 exit /B 1
msbuild /p:Configuration=Release /p:Platform=%platname% "../Radio Downloader.sln"
if ERRORLEVEL 1 exit /B 1

goto :EOF

:noplatform

echo The platform must be passed as a parameter when running this script
exit /B 1

:badplatform

echo The platform %~1 is not known
exit /B 1

:nosdk

echo The Microsoft Windows SDK for Windows 7 does not appear to be installed
exit /B 1
