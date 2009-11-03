@echo off

rem Batch file to publish Radio Downloader and the plugins to the 'bin' folder.

rem Required to run the SDK setenv script
setlocal ENABLEEXTENSIONS
setlocal ENABLEDELAYEDEXPANSION

rem Set up an x86 Release build environment
call "%programfiles%\Microsoft SDKs\Windows\v7.0\Bin\setenv.cmd" /Release /x86
if ERRORLEVEL 1 goto failed

msbuild /t:Clean
if ERRORLEVEL 1 goto failed

msbuild
if ERRORLEVEL 1 goto failed

goto exit

:failed

echo.
echo Publish failed - review above output for more details

:exit

pause