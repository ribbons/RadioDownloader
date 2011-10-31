@echo off

rem Batch file to build and Radio Downloader and providers

rem Check the platform has been passed to this script and is known
if "%~1" == "" goto noplatform
if "%~1" == "x86" set platname=win32
if "%~1" == "x64" set platname=win64
if "%platname%" == "" goto badplatform

rem Check the SDK is installed
set sdklocation=%programfiles%\Microsoft SDKs\Windows\v7.1
if not exist "%sdklocation%" goto nosdk

rem Set up a Release build environment
setlocal ENABLEEXTENSIONS
setlocal ENABLEDELAYEDEXPANSION
call "%sdklocation%\Bin\setenv.cmd" /Release /%~1

rem Build Radio Downloader and the providers
msbuild /p:Configuration=Release /p:Platform=%platname% /t:Clean "../Radio Downloader.sln"
if ERRORLEVEL 1 exit /B 1
msbuild /p:Configuration=Release /p:Platform=%platname% "../Radio Downloader.sln"
if ERRORLEVEL 1 exit /B 1

rem Run FxCop on the built assemblies
"%ProgramFiles(x86)%\Microsoft FxCop 10.0\FxCopCmd.exe" "/project:../Radio Downloader.FxCop" "/out:../obj/FxCopViolations.xml"
if ERRORLEVEL 1 exit /B 1

rem Make sure the FxCop violations file exists even when there are no violations
if not exist ..\obj\FxCopViolations.xml (
	echo ^<?xml version="1.0" encoding="utf-8"?^> > ..\obj\FxCopViolations.xml
	echo ^<FxCopReport /^> >> ..\obj\FxCopViolations.xml
)

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
