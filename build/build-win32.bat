@echo off

rem Batch file to build and Radio Downloader and providers

set sdklocation=%programfiles%\Microsoft SDKs\Windows\v7.1
if not exist "%sdklocation%" goto nosdk

rem Required to run the SDK setenv script
setlocal ENABLEEXTENSIONS
setlocal ENABLEDELAYEDEXPANSION

rem Set up an x86 Release build environment
call "%sdklocation%\Bin\setenv.cmd" /Release /x86
if ERRORLEVEL 1 exit /B 1

rem Build Radio Downloader and the providers

msbuild /p:Configuration=Release /p:Platform=win32 /t:Clean "../Radio Downloader.sln"
if ERRORLEVEL 1 exit /B 1

msbuild /p:Configuration=Release /p:Platform=win32 "../Radio Downloader.sln"
if ERRORLEVEL 1 exit /B 1

rem Run FxCop on the built assemblies
"%ProgramFiles(x86)%\Microsoft FxCop 10.0\FxCopCmd.exe" "/project:../Radio Downloader.FxCop" "/out:../obj/FxCopViolations.xml"
if ERRORLEVEL 1 exit /B 1

goto :EOF

:nosdk

echo The Microsoft Windows SDK for Windows 7 does not appear to be installed
exit /B 1
