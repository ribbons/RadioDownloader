@echo off

rem Batch file to publish the Radio Downloader and BBCLA plugin to the 'bin' folder.

call "C:\Program Files\Microsoft Visual Studio 8\SDK\v2.0\Bin\sdkvars.bat"

msbuild /t:Clean
msbuild /p:Configuration=Release

pause