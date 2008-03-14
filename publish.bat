@echo off

rem Batch file to publish the Radio Downloader and plugins to the 'bin' folder.

call "C:\Program Files\Microsoft SDKs\Windows\v6.1\Bin\setenv.cmd"

msbuild /t:Clean
msbuild /p:Configuration=Release

pause