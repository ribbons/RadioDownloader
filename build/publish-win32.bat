@echo off

rem Batch file to build and sign and Radio Downloader and provider, and build and sign the installer

set sdklocation=%programfiles%\Microsoft SDKs\Windows\v7.1
if not exist "%sdklocation%" goto nosdk

rem Required to run the SDK setenv script
setlocal ENABLEEXTENSIONS
setlocal ENABLEDELAYEDEXPANSION

rem Set up an x86 Release build environment
call "%sdklocation%\Bin\setenv.cmd" /Release /x86

set timestampserver=http://timestamp.verisign.com/scripts/timstamp.dll

call build-win32.bat
if ERRORLEVEL 1 goto failed

rem Sign Radio Downloader and the provider

signtool sign /t %timestampserver% "..\bin\win32\Radio Downloader.exe" "..\bin\win32\PodcastProvider.dll"
if ERRORLEVEL 1 set signfailed=1

rem Unregister HKCU JScript and VBScript which cause problems with installer validation

reg delete "HKCU\SOFTWARE\Classes\CLSID\{B54F3741-5B07-11CF-A4B0-00AA004A55E8}" /f
reg delete "HKCU\SOFTWARE\Classes\CLSID\{F414C260-6AC0-11CF-B6D1-00AA00BBBB58}" /f
reg delete "HKCU\SOFTWARE\Classes\Wow6432Node\CLSID\{B54F3741-5B07-11CF-A4B0-00AA004A55E8}" /f
reg delete "HKCU\SOFTWARE\Classes\Wow6432Node\CLSID\{F414C260-6AC0-11CF-B6D1-00AA00BBBB58}" /f

rem Clean and build the installer

msbuild /p:Configuration=Release /t:Clean "..\installer\Radio Downloader.wixproj"
if ERRORLEVEL 1 goto failed

msbuild /p:Configuration=Release "..\installer\Radio Downloader.wixproj"
if ERRORLEVEL 1 goto failed

rem Sign the installer

signtool sign /t %timestampserver% /d "Radio Downloader" "..\installer\Radio Downloader.msi"
if ERRORLEVEL 1 set signfailed=1

if not "%signfailed%" == "" goto signfailed

goto :EOF

:nosdk

echo The Microsoft Windows SDK for Windows 7 does not appear to be installed
echo Please install it and then try running this script again

pause

goto :EOF

:signfailed

echo.
echo Warning: Failed to sign one or more of the binaries or installer
echo Check that you have a code signing certificate installed and try again.

pause

goto :EOF

:failed

echo.
echo Publish failed - review above output for more details

pause
