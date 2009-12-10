@echo off

rem Batch file to build and sign and Radio Downloader and provider, and build and sign the installer

set sdklocation=%programfiles%\Microsoft SDKs\Windows\v7.0
if not exist "%sdklocation%" goto nosdk

set timestampserver=http://timestamp.verisign.com/scripts/timstamp.dll

rem Required to run the SDK setenv script
setlocal ENABLEEXTENSIONS
setlocal ENABLEDELAYEDEXPANSION

rem Set up an x86 Release build environment
call "%sdklocation%\Bin\setenv.cmd" /Release /x86
if ERRORLEVEL 1 goto failed

rem Clean and build Radio Downloader and the provider

msbuild /p:Configuration=Release /t:Clean
if ERRORLEVEL 1 goto failed

msbuild /p:Configuration=Release
if ERRORLEVEL 1 goto failed

rem Sign Radio Downloader and the provider

signtool sign /t %timestampserver% "bin\Radio Downloader.exe" "bin\PodcastProvider.dll"
if ERRORLEVEL 1 set signfailed=1

rem Clean and build the installer

msbuild /p:Configuration=Release /t:Clean "installer/Radio Downloader.wixproj"
if ERRORLEVEL 1 goto failed

msbuild /p:Configuration=Release "installer/Radio Downloader.wixproj"
if ERRORLEVEL 1 goto failed

rem Sign the installer

signtool sign /t %timestampserver% /d "Radio Downloader" "installer\Radio Downloader.msi"
if ERRORLEVEL 1 set signfailed=1

if not "%signfailed%" == "" goto signfailed

goto exit

:nosdk

echo The Microsoft Windows SDK for Windows 7 does not appear to be installed
echo Please install it and then try running this script again

pause

goto exit

:signfailed

echo.
echo Warning: Failed to sign one or more of the binaries or installer
echo Check that you have a code signing certificate installed and try again.

pause

goto exit

:failed

echo.
echo Publish failed - review above output for more details

pause

:exit
