@echo off

rem Batch file to publish Radio Downloader and the plugins to the 'bin' folder.

set sdklocation=%programfiles%\Microsoft SDKs\Windows\v7.0
if not exist "%sdklocation%" goto nosdk

rem Required to run the SDK setenv script
setlocal ENABLEEXTENSIONS
setlocal ENABLEDELAYEDEXPANSION

rem Set up an x86 Release build environment
call "%sdklocation%\Bin\setenv.cmd" /Release /x86
if ERRORLEVEL 1 goto failed

msbuild /p:Configuration=Release /t:Clean
if ERRORLEVEL 1 goto failed

msbuild /p:Configuration=Release
if ERRORLEVEL 1 goto failed

goto exit

:nosdk

echo The Microsoft Windows SDK for Windows 7 does not appear to be installed
echo Please install it and then try running this script again

goto exit

:failed

echo.
echo Publish failed - review above output for more details

:exit

pause